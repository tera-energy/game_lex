using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NativeWebSocket;

/// <summary>
/// 대결 모드 핵심 매니저.
/// - 매칭 (fn_join_or_create_battle)
/// - Supabase Realtime 구독 (battle_rooms postgres_changes 감지)
/// - 게이지 업데이트 (fn_battle_gauge_update)
/// - 하트비트 (fn_battle_heartbeat)
/// </summary>
public class TrBattleManager : MonoBehaviour
{
    static TrBattleManager _instance;
    public static TrBattleManager xInstance => _instance;

    // ── 방 상태 ────────────────────────────────────────────────
    public string RoomId      { get; private set; }
    public bool   IsPlayer1   { get; private set; }
    public string MyNickname  { get; private set; }
    public string OppNickname { get; private set; }

    // gauge_position: -100 ~ +100, 0=중앙, positive=player1 우세, negative=player2 우세
    int _gaugePosition = 0;
    public int GaugePosition => _gaugePosition;

    int _myCorrect;
    int _oppCorrect;
    int _prevOppCorrect = 0;
    bool _battleEnded = false;

    // 양쪽 플레이어 모두 준비 완료 플래그 (게임 시작 동기화용)
    public bool BothPlayersReady { get; private set; }

    // ── 이벤트 ────────────────────────────────────────────────
    public event Action<float>    OnGaugeChanged;       // (myRatio: 0~1, 0.5=중앙)
    public event Action<int, int> OnProgressChanged;   // (myCorrect, oppCorrect)
    public event Action<bool, int, int> OnBattleEnded; // (didIWin, myCorrect, oppCorrect)
    public event Action           OnMatchFound;         // 상대 발견
    public event Action           OnBothPlayersReady;   // 양쪽 모두 준비 완료 → 게임 시작 신호
    public event Action           OnOppBurgerCompleted; // 상대 햄버거 완성 시

    // ── WebSocket ──────────────────────────────────────────────
    WebSocket _ws;
    Coroutine _coHeartbeat;
    Coroutine _coMatchPoll;

    // Fix #1: WS 연결 완료 플래그 — OnOpen이 발화해야 true가 됨
    bool _wsConnected = false;

    // Fix #3: OnMessage는 DispatchMessageQueue()로 메인 스레드 보장,
    //         OnOpen/OnError/OnClose는 잠재적 백그라운드 스레드.
    //         SendText는 NativeWebSocket 내부적으로 thread-safe.
    //         단, Unity 오브젝트 접근은 OnMessage(메인스레드)에서만 수행.
    //         메인 스레드 디스패치가 필요한 작업은 _mainThreadQueue를 통해 처리.
    readonly Queue<Action> _mainThreadQueue = new Queue<Action>();

    const float HeartbeatInterval = 10f;
    const float MatchPollInterval = 2f;
    const float MatchTimeout      = 30f;

    // ── Unity ─────────────────────────────────────────────────
    void Awake()
    {
        if (_instance == null) _instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // 로비에서 매칭이 완료된 경우 즉시 Realtime 연결 시작
        if (TrMatchingSession.IsReady)
        {
            // 이전 Train/Challenge 게임의 _correctNum이 남아있을 수 있으므로 초기화
            if (GameManager.xInstance != null)
                GameManager.xInstance._correctNum = 0;

            RoomId      = TrMatchingSession.RoomId;
            IsPlayer1   = TrMatchingSession.IsPlayer1;
            MyNickname  = TrMatchingSession.MyNickname  ?? DatabaseManager._myDatas?.nickName ?? "나";
            OppNickname = TrMatchingSession.OppNickname ?? "상대";

            // 씬 전환으로 유실될 수 있는 seed를 여기서 재적용
            if (TrMatchingSession.BurgerSeed.HasValue)
                UnityEngine.Random.InitState(TrMatchingSession.BurgerSeed.Value);

            TrMatchingSession.Clear();

            OnMatchFound?.Invoke();
            StartCoroutine(yConnectAfterSession());
        }
    }

    IEnumerator yConnectAfterSession()
    {
        yield return StartCoroutine(yConnectRealtime());
        _coHeartbeat = StartCoroutine(yHeartbeat());
        StartCoroutine(ySubscribePuzzleEventsWithRetry());
        zSignalBattleStart();
    }

    void OnDestroy() => Disconnect();

    void Update()
    {
        // NativeWebSocket: non-WebGL 환경에서 메인 스레드 메시지 디스패치
#if !UNITY_WEBGL || UNITY_EDITOR
        _ws?.DispatchMessageQueue();
#endif
        // OnOpen 등 백그라운드 콜백에서 예약된 Unity API 작업 실행
        // Count 확인과 Dequeue를 같은 lock 안에서 처리해야 멀티스레드 안전
        while (true)
        {
            Action action;
            lock (_mainThreadQueue)
            {
                if (_mainThreadQueue.Count == 0) break;
                action = _mainThreadQueue.Dequeue();
            }
            action?.Invoke();
        }
    }

    // ── 매칭 시작 ──────────────────────────────────────────────
    public void zStartMatching()
    {
        StartCoroutine(yJoinOrCreate());
    }

    IEnumerator yJoinOrCreate()
    {
        bool isDone = false;
        string url  = SupabaseClient.RestUrl("rpc/fn_join_or_create_battle");
        string json = $"{{\"p_player_id\":\"{AuthManager._userId}\"}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: response =>
            {
                RoomId = response.Trim('"', '\n', '\r', ' ');
                isDone = true;
            },
            onError: err =>
            {
                Debug.LogError("[Battle] 매칭 실패: " + err);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);
        if (string.IsNullOrEmpty(RoomId)) yield break;

        _coMatchPoll = StartCoroutine(yPollUntilReady());
    }

    IEnumerator yPollUntilReady()
    {
        float elapsed = 0f;
        while (elapsed < MatchTimeout)
        {
            yield return new WaitForSeconds(MatchPollInterval);
            elapsed += MatchPollInterval;

            bool isDone  = false;
            bool isReady = false;
            // Fix #4: burger_seed=0 케이스 처리를 위해 nullable 방식 사용
            int?   seedParsed = null;

            string url = SupabaseClient.RestUrl("battle_rooms",
                $"id=eq.{RoomId}&select=status,player1_id,player2_id,gauge_position,burger_seed");

            yield return SupabaseClient.Get(url,
                onSuccess: response =>
                {
                    if (response.Contains("\"ready\"") || response.Contains("\"in_progress\""))
                        isReady = true;

                    IsPlayer1 = response.Contains($"\"player1_id\":\"{AuthManager._userId}\"");

                    // Fix #4: seed가 JSON에 존재하면(null 포함) 반드시 InitState 호출
                    if (yTryParseIntField(response, "burger_seed", out int seed))
                        seedParsed = seed;

                    isDone = true;
                },
                onError: err =>
                {
                    Debug.LogWarning("[Battle] 폴링 에러: " + err);
                    isDone = true;
                });

            yield return new WaitUntil(() => isDone);

            // Fix #4: seed가 파싱됐다면 0이어도 InitState를 반드시 적용
            if (seedParsed.HasValue)
                UnityEngine.Random.InitState(seedParsed.Value);

            if (isReady)
            {
                // 로비 직접 매칭 경로에서도 이전 게임 _correctNum 초기화
                if (GameManager.xInstance != null)
                    GameManager.xInstance._correctNum = 0;

                OnMatchFound?.Invoke();
                yield return StartCoroutine(yConnectRealtime());
                _coHeartbeat = StartCoroutine(yHeartbeat());
                StartCoroutine(ySubscribePuzzleEventsWithRetry());
                yield break;
            }
        }

        Debug.Log("[Battle] 매칭 타임아웃");
        StartCoroutine(yAbandonRoom());
    }

    // ── Realtime WebSocket ─────────────────────────────────────
    IEnumerator yConnectRealtime()
    {
        _wsConnected = false;

        string wsUrl = SupabaseClient.RealtimeUrl();
        _ws = new WebSocket(wsUrl);

        // Fix #1: OnOpen을 메인 스레드 큐를 통해 처리,
        //         _wsConnected 플래그를 안전하게 설정
        _ws.OnOpen += () =>
        {
            string topic   = $"realtime:public:battle_rooms:id=eq.{RoomId}";
            // Supabase Realtime v2 phx_join 메시지
            string joinMsg = "{"
                + $"\"topic\":\"{topic}\","
                + "\"event\":\"phx_join\","
                + "\"payload\":{"
                    + $"\"access_token\":\"{SupabaseClient.AccessToken}\","
                    + "\"config\":{"
                        + "\"broadcast\":{\"ack\":false},"
                        + "\"presence\":{\"key\":\"\"},"
                        + "\"postgres_changes\":[{"
                            + "\"event\":\"UPDATE\","
                            + "\"schema\":\"public\","
                            + "\"table\":\"battle_rooms\","
                            + $"\"filter\":\"id=eq.{RoomId}\""
                        + "}]"
                    + "}"
                + "},"
                + "\"ref\":\"1\""
                + "}";

            // SendText는 NativeWebSocket 내부에서 thread-safe하게 처리
            _ws.SendText(joinMsg);

            // _wsConnected 플래그는 메인 스레드에서 설정
            lock (_mainThreadQueue)
                _mainThreadQueue.Enqueue(() => _wsConnected = true);
        };

        // Fix #3: OnMessage는 DispatchMessageQueue() 덕분에 메인 스레드에서 발화
        _ws.OnMessage += bytes =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            yOnRealtimeMessage(msg);
        };

        _ws.OnError += err => Debug.LogError("[Battle] WS 에러: " + err);
        _ws.OnClose += code => Debug.Log("[Battle] WS 종료: " + code);

        yield return _ws.Connect();

        // Fix #1: Connect() yield 후에도 OnOpen이 아직 발화 전일 수 있음.
        //         최대 5초간 연결 완료 대기 (타임아웃 안전망)
        float waitTime = 0f;
        while (!_wsConnected && waitTime < 5f)
        {
            yield return null;
            waitTime += Time.deltaTime;
        }

        if (!_wsConnected)
            Debug.LogWarning("[Battle] WebSocket OnOpen 타임아웃 — join 메시지 미발송 가능성");
    }

    // ── Realtime 메시지 파싱 ───────────────────────────────────
    void yOnRealtimeMessage(string msg)
    {
        // Fix #2: Supabase Realtime v2 postgres_changes 이벤트 포맷:
        //   {"topic":"...","event":"postgres_changes","payload":{"data":{"type":"UPDATE","record":{...},...},...}}
        //
        // "event":"postgres_changes" 인지 먼저 확인,
        // phx_reply/phx_join 응답을 오탐하지 않도록 event 필드를 정확히 검사
        // phx_reply 에러 감지 (토큰 만료 또는 인증 실패 시 무음 장애 방지)
        if (msg.Contains("\"event\":\"phx_reply\"") && msg.Contains("\"status\":\"error\""))
        {
            Debug.LogWarning("[Battle] Realtime phx_reply 에러 — 토큰 갱신 후 재연결 시도: " + msg);
            StartCoroutine(yRefreshTokenAndReconnect());
            return;
        }

        if (!msg.Contains("\"event\":\"postgres_changes\"")) return;

        // payload.data 블록 추출
        int dataIdx = msg.IndexOf("\"data\":", StringComparison.Ordinal);
        if (dataIdx < 0) return;

        // data 오브젝트 내 "type":"UPDATE" 검증
        int dataBrace = msg.IndexOf('{', dataIdx);
        if (dataBrace < 0) return;

        // data 블록의 끝 탐색
        int depth    = 0;
        int dataEnd  = dataBrace;
        for (int i = dataBrace; i < msg.Length; i++)
        {
            if (msg[i] == '{') depth++;
            else if (msg[i] == '}')
            {
                depth--;
                if (depth == 0) { dataEnd = i; break; }
            }
        }
        string dataBlock = msg.Substring(dataBrace, dataEnd - dataBrace + 1);

        // Fix #2: type 필드로 UPDATE 이벤트만 처리 (INSERT/DELETE 배제)
        // Supabase Realtime JSON 포맷에 따라 "type":"UPDATE" 또는 "type": "UPDATE" 모두 허용
        if (!dataBlock.Contains("\"type\":\"UPDATE\"") && !dataBlock.Contains("\"type\": \"UPDATE\"")) return;

        // record 블록 추출
        int recIdx = dataBlock.IndexOf("\"record\":", StringComparison.Ordinal);
        if (recIdx < 0) return;

        int braceStart = dataBlock.IndexOf('{', recIdx);
        if (braceStart < 0) return;

        depth = 0;
        int braceEnd = braceStart;
        for (int i = braceStart; i < dataBlock.Length; i++)
        {
            if (dataBlock[i] == '{') depth++;
            else if (dataBlock[i] == '}')
            {
                depth--;
                if (depth == 0) { braceEnd = i; break; }
            }
        }
        string record = dataBlock.Substring(braceStart, braceEnd - braceStart + 1);

        _gaugePosition = yParseIntField(record, "gauge_position");
        int p1Cor      = yParseIntField(record, "player1_correct");
        int p2Cor      = yParseIntField(record, "player2_correct");
        string status  = yParseStrField(record, "status");
        string result  = yParseStrField(record, "result");

        _myCorrect  = IsPlayer1 ? p1Cor : p2Cor;
        _oppCorrect = IsPlayer1 ? p2Cor : p1Cor;

        // myRatio 변환: 0.5=중앙, 1.0=내가 완전 점령, 0.0=상대 완전 점령
        float myRatio = IsPlayer1
            ? (_gaugePosition + 100) / 200f
            : (100 - _gaugePosition) / 200f;

        OnGaugeChanged?.Invoke(myRatio);

        // 상대 햄버거 완성 감지
        if (_oppCorrect > _prevOppCorrect)
            OnOppBurgerCompleted?.Invoke();
        _prevOppCorrect = _oppCorrect;

        OnProgressChanged?.Invoke(_myCorrect, _oppCorrect);

        // 양쪽 플레이어 모두 준비 완료 신호 (게임 시작 동기화)
        // started_at까지 대기 후 발화 → 양쪽 동시 시작 보장
        if (status == "in_progress" && !BothPlayersReady)
        {
            string startedAt = yParseStrField(record, "started_at");
            if (!string.IsNullOrEmpty(startedAt))
                StartCoroutine(yWaitUntilStartTime(startedAt));
            else
                zFireBothPlayersReady();
        }

        // 게이지 한계 도달 → 서버 응답 대기 없이 클라이언트 즉시 판정
        if (!_battleEnded && Mathf.Abs(_gaugePosition) >= 100)
        {
            _battleEnded = true;
            bool didIWinByGauge = IsPlayer1 ? _gaugePosition > 0 : _gaugePosition < 0;
            Debug.Log($"[Battle] 게이지 한계 도달 — gauge:{_gaugePosition} isP1:{IsPlayer1} win:{didIWinByGauge} → OnBattleEnded 발화");
            OnBattleEnded?.Invoke(didIWinByGauge, _myCorrect, _oppCorrect);
            StartCoroutine(yReportGaugeResult());
            return;
        }

        if (status == "finished" && !string.IsNullOrEmpty(result))
        {
            if (_battleEnded) return;
            _battleEnded = true;

            string winnerId = yParseStrField(record, "winner_id");
            // draw는 별도 DRAW UI가 없으므로 LOSE 처리 (winnerId가 빈 문자열이면 나와 불일치)
            bool didIWin = winnerId == AuthManager._userId;
            Debug.Log($"[Battle] status=finished — result:{result} winnerId:{winnerId} didIWin:{didIWin} → OnBattleEnded 발화");
            OnBattleEnded?.Invoke(didIWin, _myCorrect, _oppCorrect);
        }
    }

    // ── 토큰 만료 대응: 갱신 후 Realtime 재연결 ────────────────
    IEnumerator yRefreshTokenAndReconnect()
    {
        // 중복 호출 방지: 이미 재연결 중이면 스킵
        if (!_wsConnected) yield break;
        _wsConnected = false;

        Debug.Log("[Battle] 토큰 갱신 중...");
        if (AuthManager.xInstance != null)
            yield return AuthManager.xInstance.zStartRefreshSession();

        // 기존 WS 정리
        if (_coHeartbeat != null) { StopCoroutine(_coHeartbeat); _coHeartbeat = null; }
        _ws?.Close();
        _ws = null;

        yield return new WaitForSeconds(0.5f);

        // 새 토큰으로 Realtime 재연결
        yield return StartCoroutine(yConnectRealtime());
        _coHeartbeat = StartCoroutine(yHeartbeat());
        Debug.Log("[Battle] Realtime 재연결 완료");
    }

    // ── 퍼즐 이벤트 구독 ──────────────────────────────────────
    IEnumerator ySubscribePuzzleEventsWithRetry()
    {
        float elapsed = 0f;
        while (elapsed < 5f)
        {
            var puzzle = TrPuzzleHamburger.xInstance;
            if (puzzle != null)
            {
                puzzle.OnBurgerCompleted += OnBurgerCompleted;
                puzzle.OnBurgerFailed    += OnBurgerFailed;
                yield break;
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
        Debug.LogError("[Battle] TrPuzzleHamburger 구독 실패 — 5초 대기 초과");
    }

    void yUnsubscribePuzzleEvents()
    {
        var puzzle = TrPuzzleHamburger.xInstance;
        if (puzzle == null) return;
        puzzle.OnBurgerCompleted -= OnBurgerCompleted;
        puzzle.OnBurgerFailed    -= OnBurgerFailed;
    }

    void OnBurgerCompleted() => StartCoroutine(yUpdateHP("completed"));
    void OnBurgerFailed()    => StartCoroutine(yUpdateHP("failed"));

    IEnumerator yUpdateHP(string action)
    {
        int ingredients = TrPuzzleHamburger.xInstance != null
            ? TrPuzzleHamburger.xInstance.NumViewIngredients
            : 6;

        bool isDone = false;
        string url  = SupabaseClient.RestUrl("rpc/fn_battle_gauge_update");
        string json = $"{{\"p_room_id\":\"{RoomId}\","
                    + $"\"p_player_id\":\"{AuthManager._userId}\","
                    + $"\"p_action\":\"{action}\","
                    + $"\"p_ingredients\":{ingredients}}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: _ => isDone = true,
            onError: err =>
            {
                Debug.LogError("[Battle] HP 업데이트 실패: " + err);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);

        // RPC 성공 후 즉시 게이지 조회 — Realtime 도착 전에 종료 감지
        if (!_battleEnded)
            yield return StartCoroutine(yCheckGaugeImmediate());
    }

    IEnumerator yCheckGaugeImmediate()
    {
        bool done = false;
        yield return SupabaseClient.Get(
            SupabaseClient.RestUrl("battle_rooms",
                $"id=eq.{RoomId}&select=gauge_position,player1_correct,player2_correct"),
            onSuccess: resp =>
            {
                if (yTryParseIntField(resp, "gauge_position", out int pos))
                {
                    _gaugePosition = pos;

                    int p1Cor = yParseIntField(resp, "player1_correct");
                    int p2Cor = yParseIntField(resp, "player2_correct");
                    _myCorrect  = IsPlayer1 ? p1Cor : p2Cor;
                    _oppCorrect = IsPlayer1 ? p2Cor : p1Cor;

                    float myRatio = IsPlayer1
                        ? (_gaugePosition + 100) / 200f
                        : (100 - _gaugePosition) / 200f;
                    OnGaugeChanged?.Invoke(myRatio);

                    // 상대 햄버거 완성 감지 (REST 즉시조회 경로)
                    if (_oppCorrect > _prevOppCorrect)
                        OnOppBurgerCompleted?.Invoke();
                    _prevOppCorrect = _oppCorrect;

                    OnProgressChanged?.Invoke(_myCorrect, _oppCorrect);

                    if (!_battleEnded && Mathf.Abs(_gaugePosition) >= 100)
                    {
                        _battleEnded = true;
                        bool win = IsPlayer1 ? _gaugePosition > 0 : _gaugePosition < 0;
                        Debug.Log($"[Battle] REST 즉시조회 게이지 한계 — gauge:{_gaugePosition} win:{win} → OnBattleEnded 발화");
                        OnBattleEnded?.Invoke(win, _myCorrect, _oppCorrect);
                        StartCoroutine(yReportGaugeResult());
                    }
                }
                done = true;
            },
            onError: _ => done = true);

        yield return new WaitUntil(() => done);
    }

    // ── 타임아웃 판정 ─────────────────────────────────────────
    /// <summary>
    /// 타이머 만료 시 클라이언트 호출. 게이지 현재 위치로 승패 판정 후 결과창 표시.
    /// </summary>
    public void zHandleTimeout()
    {
        if (_battleEnded) return;
        _battleEnded = true;

        // gauge_position: positive = player1 우세, negative = player2 우세
        // 내가 player1이면 gauge > 0 이 이기는 것, player2면 gauge < 0 이 이기는 것
        bool didIWin;
        if (_gaugePosition == 0)
            didIWin = false; // 정중앙이면 무승부 → LOSE 처리
        else
            didIWin = IsPlayer1 ? _gaugePosition > 0 : _gaugePosition < 0;

        OnBattleEnded?.Invoke(didIWin, _myCorrect, _oppCorrect);

        // 서버에도 타임아웃 결과 반영 (비동기, 결과창 표시를 막지 않음)
        StartCoroutine(yReportTimeout());
    }

    IEnumerator yReportTimeout()
    {
        if (string.IsNullOrEmpty(RoomId)) yield break;

        // winner_id 결정을 위해 방의 player1_id / player2_id 조회
        string player1Id = "", player2Id = "";
        bool fetchDone = false;
        string fetchUrl = SupabaseClient.RestUrl("battle_rooms",
            $"id=eq.{RoomId}&select=player1_id,player2_id");
        yield return SupabaseClient.Get(fetchUrl,
            onSuccess: resp =>
            {
                player1Id = yParseStrField(resp, "player1_id");
                player2Id = yParseStrField(resp, "player2_id");
                fetchDone = true;
            },
            onError: _ => fetchDone = true);
        yield return new WaitUntil(() => fetchDone);

        // Bug #1 수정: "player1" → "player1_win" (SQL 컨벤션 일치)
        // Bug #2 수정: winner_id 포함 (상대 클라이언트가 승패 판정에 사용)
        string result, winnerId;
        if (_gaugePosition == 0)
        {
            result   = "draw";
            winnerId = "";
        }
        else if (_gaugePosition > 0)
        {
            result   = "player1_win";
            winnerId = player1Id;
        }
        else
        {
            result   = "player2_win";
            winnerId = player2Id;
        }

        string url  = SupabaseClient.RestUrl("battle_rooms", $"id=eq.{RoomId}");
        string json = string.IsNullOrEmpty(winnerId)
            ? $"{{\"status\":\"finished\",\"result\":\"{result}\"}}"
            : $"{{\"status\":\"finished\",\"result\":\"{result}\",\"winner_id\":\"{winnerId}\"}}";

        yield return SupabaseClient.Patch(url, json,
            onSuccess: _ => Debug.Log("[Battle] 타임아웃 결과 서버 반영 완료"),
            onError: err => Debug.LogWarning("[Battle] 타임아웃 서버 반영 실패: " + err));
    }

    // ── 게이지 한계 도달 서버 반영 ────────────────────────────
    IEnumerator yReportGaugeResult()
    {
        if (string.IsNullOrEmpty(RoomId)) yield break;

        string player1Id = "", player2Id = "";
        bool fetchDone = false;
        yield return SupabaseClient.Get(
            SupabaseClient.RestUrl("battle_rooms", $"id=eq.{RoomId}&select=player1_id,player2_id"),
            onSuccess: resp => { player1Id = yParseStrField(resp, "player1_id"); player2Id = yParseStrField(resp, "player2_id"); fetchDone = true; },
            onError:   _    => fetchDone = true);
        yield return new WaitUntil(() => fetchDone);

        string result, winnerId;
        if (_gaugePosition >= 100)       { result = "player1_win"; winnerId = player1Id; }
        else if (_gaugePosition <= -100) { result = "player2_win"; winnerId = player2Id; }
        else                             { result = "draw";         winnerId = ""; }

        string url  = SupabaseClient.RestUrl("battle_rooms", $"id=eq.{RoomId}");
        string json = string.IsNullOrEmpty(winnerId)
            ? $"{{\"status\":\"finished\",\"result\":\"{result}\"}}"
            : $"{{\"status\":\"finished\",\"result\":\"{result}\",\"winner_id\":\"{winnerId}\"}}";

        yield return SupabaseClient.Patch(url, json,
            onSuccess: _ => Debug.Log("[Battle] 게이지 결과 서버 반영 완료"),
            onError: err => Debug.LogWarning("[Battle] 게이지 결과 서버 반영 실패: " + err));
    }

    // ── 게임 시작 신호 ─────────────────────────────────────────
    public void zSignalBattleStart()
    {
        StartCoroutine(yBattleStart());
    }

    IEnumerator yBattleStart()
    {
        bool isDone = false;
        bool didTransition = false;
        string url  = SupabaseClient.RestUrl("rpc/fn_battle_start");
        string json = $"{{\"p_room_id\":\"{RoomId}\",\"p_player_id\":\"{AuthManager._userId}\"}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: body =>
            {
                string trimmed = body != null ? body.Trim() : "";
                didTransition = (trimmed == "true");
                Debug.Log(didTransition
                    ? "[Battle] 내가 게임을 in_progress로 전환시킴 (첫 번째 호출)"
                    : "[Battle] 상대가 이미 게임을 시작시킴 (두 번째 호출)");
                isDone = true;
            },
            onError: err =>
            {
                Debug.LogError("[Battle] 게임 시작 신호 실패: " + err);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);

        // 두 번째 호출(false): Realtime 이벤트를 놓쳤을 수 있으므로 DB에서 started_at 직접 조회
        if (!didTransition && !BothPlayersReady)
            yield return StartCoroutine(yFetchAndWaitStartedAt());
        // 첫 번째 호출(true): Realtime이 started_at을 전달 → yOnRealtimeMessage에서 처리됨
    }

    // Realtime을 놓친 경우 폴백: DB에서 started_at 조회 후 대기
    IEnumerator yFetchAndWaitStartedAt()
    {
        bool done = false;
        string startedAt = null;

        yield return SupabaseClient.Get(
            SupabaseClient.RestUrl("battle_rooms", $"id=eq.{RoomId}&select=started_at"),
            onSuccess: resp => { startedAt = yParseStrField(resp, "started_at"); done = true; },
            onError: _ => done = true);

        yield return new WaitUntil(() => done);

        if (!string.IsNullOrEmpty(startedAt))
            yield return StartCoroutine(yWaitUntilStartTime(startedAt));
        else
            zFireBothPlayersReady(); // started_at이 없으면 즉시 시작 (안전망)
    }

    // ISO 8601 타임스탬프까지 대기 후 OnBothPlayersReady 발화
    IEnumerator yWaitUntilStartTime(string isoTimestamp)
    {
        if (!System.DateTime.TryParse(isoTimestamp, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime startTime))
        {
            Debug.LogWarning("[Battle] started_at 파싱 실패: " + isoTimestamp);
            zFireBothPlayersReady();
            yield break;
        }

        startTime = startTime.ToUniversalTime();

        // busy wait 대신 0.1초 간격 폴링 + 최대 10초 안전망
        float safetyLimit = 10f;
        while (System.DateTime.UtcNow < startTime && safetyLimit > 0f)
        {
            yield return new WaitForSeconds(0.1f);
            safetyLimit -= 0.1f;
        }

        zFireBothPlayersReady();
    }

    void zFireBothPlayersReady()
    {
        if (BothPlayersReady) return;
        BothPlayersReady = true;
        OnBothPlayersReady?.Invoke();
    }

    // ── 하트비트 ──────────────────────────────────────────────
    IEnumerator yHeartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(HeartbeatInterval);

            bool isDone = false;
            string url  = SupabaseClient.RestUrl("rpc/fn_battle_heartbeat");
            string json = $"{{\"p_room_id\":\"{RoomId}\",\"p_player_id\":\"{AuthManager._userId}\"}}";

            yield return SupabaseClient.Post(url, json,
                onSuccess: _ => isDone = true,
                onError: err =>
                {
                    Debug.LogWarning("[Battle] 하트비트 실패: " + err);
                    isDone = true;
                });

            yield return new WaitUntil(() => isDone);
        }
    }

    // ── 이탈 ──────────────────────────────────────────────────
    IEnumerator yAbandonRoom()
    {
        if (string.IsNullOrEmpty(RoomId)) yield break;

        bool isDone = false;
        string url  = SupabaseClient.RestUrl("battle_rooms", $"id=eq.{RoomId}");
        string json = "{\"status\":\"abandoned\"}";

        yield return SupabaseClient.Patch(url, json,
            onSuccess: _ => isDone = true,
            onError: _ => isDone = true);

        yield return new WaitUntil(() => isDone);
    }

    public void Disconnect()
    {
        yUnsubscribePuzzleEvents();
        if (_coHeartbeat != null) { StopCoroutine(_coHeartbeat); _coHeartbeat = null; }
        if (_coMatchPoll  != null) { StopCoroutine(_coMatchPoll);  _coMatchPoll  = null; }
        _ws?.Close();
        _ws = null;
        _wsConnected = false;
    }

    // ── JSON 파싱 헬퍼 ─────────────────────────────────────────

    // Fix #4: 성공/실패를 bool로 분리 — 값이 0이어도 파싱 성공을 알 수 있음
    static bool yTryParseIntField(string json, string key, out int result)
    {
        result = 0;
        string search = $"\"{key}\":";
        int idx = json.IndexOf(search, StringComparison.Ordinal);
        if (idx < 0) return false;
        idx += search.Length;
        while (idx < json.Length && json[idx] == ' ') idx++;
        // null 리터럴 처리 (DB에서 null이 내려오는 경우)
        if (idx + 4 <= json.Length && json.Substring(idx, 4) == "null") return false;
        int end = idx;
        while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
        if (end == idx) return false;
        return int.TryParse(json.Substring(idx, end - idx), out result);
    }

    // 기존 헬퍼 유지 (내부 사용, 파싱 실패 시 0 반환)
    static int yParseIntField(string json, string key)
    {
        yTryParseIntField(json, key, out int val);
        return val;
    }

    static string yParseStrField(string json, string key)
    {
        string search = $"\"{key}\":";
        int idx = json.IndexOf(search, StringComparison.Ordinal);
        if (idx < 0) return "";
        idx += search.Length;
        // 콜론 뒤 공백 허용 ("key": "value" 포맷 대응)
        while (idx < json.Length && json[idx] == ' ') idx++;
        if (idx >= json.Length || json[idx] != '"') return "";
        idx++; // 여는 따옴표 스킵

        // 백슬래시 이스케이프 처리: \" 를 만나면 닫는 따옴표로 오해하지 않음
        var sb = new System.Text.StringBuilder();
        while (idx < json.Length)
        {
            char c = json[idx];
            if (c == '\\' && idx + 1 < json.Length)
            {
                idx += 2; // 이스케이프 시퀀스 건너뜀
                continue;
            }
            if (c == '"') break;
            sb.Append(c);
            idx++;
        }
        return sb.ToString();
    }
}
