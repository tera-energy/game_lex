using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PLATFORM_IOS
using UnityEngine.iOS;
#endif

public class DatabaseManager : MonoBehaviour
{
    static DatabaseManager _instance;
    public static DatabaseManager xInstance { get { return _instance; } }

    public static int _maxWaitingTime = 30;

    [HideInInspector] public static List<int> _liMyScores;
    [HideInInspector] public List<TrTotalScore> _liTotalScores;
    [HideInInspector] public static TrUserData _myDatas;
    [HideInInspector] public bool _isSuccess;

    string _uid => AuthManager._userId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes() => _instance = null;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    static List<T> ParseArray<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<JsonWrapper<T>>("{\"items\":" + json + "}");
        return wrapper?.items ?? new List<T>();
    }

    #region Score

    public IEnumerator zGetDataMyScores()
    {
        bool isDone = false;
        string url = SupabaseClient.RestUrl("user_scores", $"user_id=eq.{_uid}");

        yield return SupabaseClient.Get(url,
            onSuccess: json =>
            {
                var list = ParseArray<SupabaseUserScores>(json);
                _liMyScores = new List<int>();
                if (list.Count > 0)
                {
                    var s = list[0];
                    _liMyScores.Add(s.score1);
                    _liMyScores.Add(s.score2);
                    _liMyScores.Add(s.score3);
                    _liMyScores.Add(s.score4);
                    _liMyScores.Add(s.score5);
                }
                isDone = true;
            },
            onError: err => { Debug.LogError("Failed get scores: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zGetDataTotalScores()
    {
        bool isDone = false;
        string url = SupabaseClient.RestUrl("users", "select=nickname,max_score&max_score=gt.0&order=max_score.desc&limit=50");

        yield return SupabaseClient.Get(url,
            onSuccess: json =>
            {
                var list = ParseArray<SupabaseTotalScore>(json);
                _liTotalScores = new List<TrTotalScore>();
                foreach (var item in list)
                    _liTotalScores.Add(new TrTotalScore { nickname = item.nickname, maxScore = item.max_score });
                isDone = true;
            },
            onError: err => { Debug.LogError("Failed get total scores: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zSetMaxScore()
    {
        bool isDone = false;
        string url  = SupabaseClient.RestUrl("users", $"id=eq.{_uid}");
        string json = $"{{\"max_score\":{_myDatas.maxScore}}}";

        yield return SupabaseClient.Patch(url, json,
            onSuccess: _ => isDone = true,
            onError: err => { Debug.LogError("Failed set max score: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zSetMyScores()
    {
        bool isDone = false;
        string url  = SupabaseClient.RestUrl("user_scores", "on_conflict=user_id");
        string json = $"{{\"user_id\":\"{_uid}\"," +
                      $"\"score1\":{_liMyScores[0]},\"score2\":{_liMyScores[1]}," +
                      $"\"score3\":{_liMyScores[2]},\"score4\":{_liMyScores[3]},\"score5\":{_liMyScores[4]}}}";

        yield return SupabaseClient.Upsert(url, json,
            onSuccess: _ => isDone = true,
            onError: err => { Debug.LogError("Failed set scores: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    #endregion

    #region UserData

    public void zSetUserIDReference(string id)
    {
        // Supabase는 별도 reference 불필요
    }

    public IEnumerator zGetMyData(string uid)
    {
        bool isDone = false;
        string url  = SupabaseClient.RestUrl("users", $"id=eq.{uid}");

        yield return SupabaseClient.Get(url,
            onSuccess: json =>
            {
                var list = ParseArray<SupabaseUser>(json);
                _myDatas = list.Count > 0 ? list[0].ToTrUserData() : null;
                isDone = true;
            },
            onError: err => { Debug.LogError("Failed get user data: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zSetMyData()
    {
        bool isDone = false;

        int platformType = PlayerPrefs.GetInt(TrProjectSettings.AUTOLOGINPLATFORM, 0);

        string url  = SupabaseClient.RestUrl("users");
        string json = $"{{\"id\":\"{AuthManager._userId}\"," +
                      $"\"email\":\"{AuthManager._userEmail ?? ""}\"," +
                      $"\"nickname\":\"\"," +
                      $"\"stamina\":{StaminaManager._maxStamina}," +
                      $"\"max_score\":0," +
                      $"\"platform_type\":{platformType}}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: _ =>
            {
                _myDatas = new TrUserData
                {
                    userId      = AuthManager._userId,
                    email       = AuthManager._userEmail ?? "",
                    nickName    = "",
                    stamina     = StaminaManager._maxStamina,
                    staminaDate = "",
                    maxScore    = 0
                };
                isDone = true;
            },
            onError: err => { Debug.LogError("Failed set user data: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zPutNickname(string nick)
    {
        _isSuccess = false;
        bool isDone = false;

        // 닉네임 중복 체크
        string checkUrl = SupabaseClient.RestUrl("users", $"nickname=eq.{nick}");

        yield return SupabaseClient.Get(checkUrl,
            onSuccess: json =>
            {
                var list = ParseArray<SupabaseUser>(json);
                _isSuccess = list.Count == 0;
                isDone = true;
            },
            onError: err => { Debug.LogError("Failed check nickname: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
        if (!_isSuccess) yield break;

        // 닉네임 업데이트
        isDone = false;
        string url  = SupabaseClient.RestUrl("users", $"id=eq.{_uid}");
        string json = $"{{\"nickname\":\"{nick}\"}}";

        yield return SupabaseClient.Patch(url, json,
            onSuccess: _ => { _myDatas.nickName = nick; isDone = true; },
            onError: err => { Debug.LogError("Failed set nickname: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zDeleteUserData()
    {
        bool isDone = false;

        // user_scores 먼저 삭제 (FK 제약)
        yield return SupabaseClient.Delete(
            SupabaseClient.RestUrl("user_scores", $"user_id=eq.{_uid}"),
            onSuccess: () => isDone = true,
            onError: err => { Debug.LogError("Failed delete scores: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);

        isDone = false;
        yield return SupabaseClient.Delete(
            SupabaseClient.RestUrl("users", $"id=eq.{_uid}"),
            onSuccess: () => isDone = true,
            onError: err => { Debug.LogError("Failed delete user: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    public IEnumerator zConnectPlatform(string preUserId, string newUserId)
    {
        // 1. 게스트 데이터 조회
        bool isDone = false;
        TrUserData guestData = null;
        string getUrl = SupabaseClient.RestUrl("users", $"id=eq.{preUserId}");

        yield return SupabaseClient.Get(getUrl,
            onSuccess: json =>
            {
                var list = ParseArray<SupabaseUser>(json);
                if (list.Count > 0) guestData = list[0].ToTrUserData();
                isDone = true;
            },
            onError: err => { Debug.LogError("(Connect) failed get guest data: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
        if (guestData == null) yield break;

        // 2. 새 유저 레코드에 게스트 데이터 반영
        isDone = false;
        string patchUrl  = SupabaseClient.RestUrl("users", $"id=eq.{newUserId}");
        string patchJson = $"{{\"nickname\":\"{guestData.nickName}\"," +
                           $"\"stamina\":{guestData.stamina}," +
                           $"\"max_score\":{guestData.maxScore}}}";

        yield return SupabaseClient.Patch(patchUrl, patchJson,
            onSuccess: _ => isDone = true,
            onError: err => { Debug.LogError("(Connect) failed patch new user: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);

        // 3. user_scores user_id 업데이트 (preUserId → newUserId)
        isDone = false;
        string scoresUrl  = SupabaseClient.RestUrl("user_scores", $"user_id=eq.{preUserId}");
        string scoresJson = $"{{\"user_id\":\"{newUserId}\"}}";

        yield return SupabaseClient.Patch(scoresUrl, scoresJson,
            onSuccess: _ => isDone = true,
            onError: err => { Debug.LogError("(Connect) failed update scores: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);

        // 4. 게스트 유저 삭제
        isDone = false;
        yield return SupabaseClient.Delete(
            SupabaseClient.RestUrl("users", $"id=eq.{preUserId}"),
            onSuccess: () => isDone = true,
            onError: err => { Debug.LogError("(Connect) failed delete guest: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    #endregion

    #region Stamina

    public IEnumerator zSetStamina()
    {
        bool isDone = false;
        string url  = SupabaseClient.RestUrl("users", $"id=eq.{_uid}");

        // staminaDate가 비어있으면 null로 전송
        string staminaDateJson = string.IsNullOrEmpty(_myDatas.staminaDate)
            ? "null"
            : $"\"{_myDatas.staminaDate}\"";

        string json = $"{{\"stamina\":{_myDatas.stamina},\"stamina_updated_at\":{staminaDateJson}}}";

        yield return SupabaseClient.Patch(url, json,
            onSuccess: _ => isDone = true,
            onError: err => { Debug.LogError("Failed set stamina: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }

    #endregion

    public IEnumerator zCheckVersion()
    {
        bool isDone = false;
        _isSuccess  = false;

        string platform = TrProjectSettings.GOOGLE;
#if PLATFORM_IOS
        platform = TrProjectSettings.APPLE;
#endif

        string thisVersion = Application.version;
        string url = SupabaseClient.RestUrl("app_versions",
            $"character=eq.{TrProjectSettings._character}&platform=eq.{platform}");

        yield return SupabaseClient.Get(url,
            onSuccess: json =>
            {
                var list = ParseArray<SupabaseVersion>(json);
                if (list.Count > 0)
                    _isSuccess = list[0].version == thisVersion;
                isDone = true;
            },
            onError: err => { Debug.LogError("Failed get version: " + err); isDone = true; });

        yield return new WaitUntil(() => isDone);
    }
}

#region Supabase DTOs

[Serializable]
class JsonWrapper<T>
{
    public List<T> items;
}

[Serializable]
class SupabaseUser
{
    public string id;
    public string email;
    public string nickname;
    public int    stamina;
    public string stamina_updated_at;
    public int    max_score;
    public int    platform_type;

    public TrUserData ToTrUserData() => new TrUserData
    {
        userId      = id,
        email       = email,
        nickName    = nickname,
        stamina     = stamina,
        staminaDate = stamina_updated_at ?? "",
        maxScore    = max_score
    };
}

[Serializable]
class SupabaseUserScores
{
    public string user_id;
    public int    score1;
    public int    score2;
    public int    score3;
    public int    score4;
    public int    score5;
}

[Serializable]
class SupabaseTotalScore
{
    public string nickname;
    public int    max_score;
}

[Serializable]
class SupabaseVersion
{
    public string version;
}

#endregion

#region TeraDB DTOs

[Serializable]
public class TrUserData
{
    public string userId;
    public string email;
    public string nickName;
    public int    stamina;
    public string staminaDate;
    public int    maxScore;
}

[Serializable]
public class TrMySocres
{
    public int score1;
    public int score2;
    public int score3;
    public int score4;
    public int score5;
}

[Serializable]
public class TrTotalScore
{
    public string nickname;
    public int    maxScore;
}

#endregion
