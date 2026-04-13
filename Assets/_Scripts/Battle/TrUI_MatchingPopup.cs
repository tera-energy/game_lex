using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

/// <summary>
/// 로비 씬에서 경쟁 모드 버튼 클릭 시 표시되는 매칭 대기 팝업.
/// 매칭 완료 → TrMatchingSession에 결과 저장 → BattleHamburger 씬 로드.
/// </summary>
public class TrUI_MatchingPopup : MonoBehaviour
{
    static TrUI_MatchingPopup _instance;
    public static TrUI_MatchingPopup xInstance => _instance;

    [SerializeField] GameObject      _goPopup;
    [SerializeField] RectTransform   _tfSpinner;
    [SerializeField] TextMeshProUGUI _txtStatus;

    const float MatchPollInterval = 2f;
    const float MatchTimeout      = 30f;

    Coroutine _coMatching;
    Coroutine _coNick;
    string    _roomId;
    Tween     _spinnerTween;
    bool      _isCancelled;

    static readonly System.Text.RegularExpressions.Regex _uuidRegex =
        new System.Text.RegularExpressions.Regex(
            @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    void Awake()
    {
        if (_instance == null) _instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start() { }

    // ── 외부 호출 ─────────────────────────────────────────────
    public void zShow()
    {
        if (_coMatching != null) return; // 이미 매칭 중
        _isCancelled = false;
        _goPopup.SetActive(true);
        yStartSpinner();
        _txtStatus.text = "상대를 찾는 중...";
        _coMatching = StartCoroutine(yMatch());
    }

    public void zOnClickCancel()
    {
        _isCancelled = true;
        if (_coMatching != null) { StopCoroutine(_coMatching); _coMatching = null; }
        if (_coNick     != null) { StopCoroutine(_coNick);     _coNick     = null; }
        yStopSpinner();
        _goPopup.SetActive(false);

        // 재매칭으로 진입한 경우 숨겨뒀던 로비를 페이드 인
        TrLobbyManager.xInstance?.zShowLobby();

        if (!string.IsNullOrEmpty(_roomId))
            StartCoroutine(yAbandon());
    }

    // ── 매칭 흐름 ─────────────────────────────────────────────
    IEnumerator yMatch()
    {
        // 1. 방 생성 or 입장
        bool isDone = false;
        string url  = SupabaseClient.RestUrl("rpc/fn_join_or_create_battle");
        string json = $"{{\"p_player_id\":\"{AuthManager._userId}\"}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: res => { _roomId = res.Trim('"', '\n', '\r', ' '); isDone = true; },
            onError: err =>
            {
                Debug.LogError("[Matching] 방 생성 실패: " + err);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);

        if (string.IsNullOrEmpty(_roomId))
        {
            yOnTimeout();
            yield break;
        }

        // 2. 상대 대기 폴링
        float elapsed = 0f;
        while (elapsed < MatchTimeout)
        {
            yield return new WaitForSeconds(MatchPollInterval);
            elapsed += MatchPollInterval;

            isDone = false;
            bool   isReady       = false;
            bool   isPlayer1     = false;
            int?   seedParsed    = null;
            string lastPollRes   = null;

            string pollUrl = SupabaseClient.RestUrl("battle_rooms",
                $"id=eq.{_roomId}&select=status,player1_id,player2_id,burger_seed");

            yield return SupabaseClient.Get(pollUrl,
                onSuccess: res =>
                {
                    lastPollRes = res;
                    if (res.Contains("\"ready\"") || res.Contains("\"in_progress\""))
                        isReady = true;

                    isPlayer1 = res.Contains($"\"player1_id\":\"{AuthManager._userId}\"");

                    if (yTryParseIntField(res, "burger_seed", out int seed))
                        seedParsed = seed;

                    isDone = true;
                },
                onError: err => { Debug.LogWarning("[Matching] 폴링 에러: " + err); isDone = true; });

            yield return new WaitUntil(() => isDone);

            if (isReady)
            {
                TrMatchingSession.RoomId     = _roomId;
                TrMatchingSession.IsPlayer1  = isPlayer1;
                TrMatchingSession.BurgerSeed = seedParsed;
                TrMatchingSession.MyNickname = DatabaseManager._myDatas?.nickName ?? "나";

                // 상대방 닉네임 조회 (UUID 검증 후 쿼리, 5초 실효 타임아웃)
                string oppId = yParseOppId(lastPollRes, isPlayer1);
                if (!string.IsNullOrEmpty(oppId))
                {
                    bool   nickDone = false;
                    string oppUrl   = SupabaseClient.RestUrl("users", $"id=eq.{oppId}&select=nickname");

                    // Get을 별도 코루틴으로 띄워야 while 루프가 실효 타임아웃으로 작동함
                    // 핸들 저장으로 취소 시 명시적 중단 가능
                    _coNick = StartCoroutine(yFetchOppNickname(oppUrl, nick =>
                    {
                        TrMatchingSession.OppNickname = nick;
                        nickDone = true;
                    }));


                    float deadline = Time.realtimeSinceStartup + 5f;
                    while (!nickDone && Time.realtimeSinceStartup < deadline)
                        yield return null;

                    if (!nickDone) TrMatchingSession.OppNickname = "상대";
                }
                else
                {
                    TrMatchingSession.OppNickname = "상대";
                }

                // 닉네임 조회 중 취소됐으면 씬 전환하지 않음
                if (_isCancelled) yield break;

                yOnMatchFound();
                yield break;
            }
        }

        yOnTimeout();
    }

    void yOnMatchFound()
    {
        yStopSpinner();
        _goPopup.SetActive(false);
        GameManager._type = TT.enumGameType.Battle;
        SceneManager.LoadScene(TrProjectSettings.strBATTLE);
    }

    void yOnTimeout()
    {
        yStopSpinner();
        _txtStatus.text = "상대를 찾지 못했어요.\n잠시 후 다시 시도해 주세요.";
        if (!string.IsNullOrEmpty(_roomId))
            StartCoroutine(yAbandon());

        DOVirtual.DelayedCall(2f, () =>
        {
            if (_goPopup != null) _goPopup.SetActive(false);
        });
    }

    IEnumerator yAbandon()
    {
        if (string.IsNullOrEmpty(_roomId)) yield break; // 중복 호출 방지
        string url  = SupabaseClient.RestUrl("battle_rooms", $"id=eq.{_roomId}");
        string json = "{\"status\":\"abandoned\"}";
        _roomId = null; // yield 전에 null 처리 → 재진입 차단
        yield return SupabaseClient.Patch(url, json, onSuccess: _ => { }, onError: _ => { });
    }

    // ── 스피너 ────────────────────────────────────────────────
    void yStartSpinner()
    {
        if (_tfSpinner == null) return;
        _spinnerTween?.Kill();
        _tfSpinner.localRotation = Quaternion.identity;
        _spinnerTween = _tfSpinner.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1);
    }

    void yStopSpinner()
    {
        _spinnerTween?.Kill();
        _spinnerTween = null;
    }

    // 닉네임 비동기 조회 (별도 코루틴 — 타임아웃 while과 병렬 실행됨)
    IEnumerator yFetchOppNickname(string url, System.Action<string> onResult)
    {
        yield return SupabaseClient.Get(url,
            onSuccess: res =>
            {
                string nick = yParseStringField(res, "nickname");
                onResult(string.IsNullOrEmpty(nick) ? "상대" : nick);
            },
            onError: _ => onResult("상대"));
        _coNick = null; // 완료 시 자가 초기화 (상태 일관성)
    }

    // ── JSON 헬퍼 ─────────────────────────────────────────────

    // "player1_id" / "player2_id" 중 내가 아닌 쪽 ID 반환 (UUID 형식 검증 포함)
    static string yParseOppId(string json, bool iAmPlayer1)
    {
        if (json == null) return null;
        string oppKey = iAmPlayer1 ? "player2_id" : "player1_id";
        string oppId  = yParseStringField(json, oppKey);
        if (oppId == null || !_uuidRegex.IsMatch(oppId)) return null;
        return oppId;
    }

    // JSON 문자열 필드 추출 (백슬래시 이스케이프 처리 포함)
    static string yParseStringField(string json, string key)
    {
        if (json == null) return null;
        string search = $"\"{key}\":\"";
        int idx = json.IndexOf(search, System.StringComparison.Ordinal);
        if (idx < 0) return null;
        idx += search.Length;

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
        return sb.Length > 0 ? sb.ToString() : null;
    }

    static bool yTryParseIntField(string json, string key, out int result)
    {
        result = 0;
        string search = $"\"{key}\":";
        int idx = json.IndexOf(search, System.StringComparison.Ordinal);
        if (idx < 0) return false;
        idx += search.Length;
        while (idx < json.Length && json[idx] == ' ') idx++;
        if (idx + 4 <= json.Length && json.Substring(idx, 4) == "null") return false;
        int end = idx;
        while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
        if (end == idx) return false;
        return int.TryParse(json.Substring(idx, end - idx), out result);
    }
}
