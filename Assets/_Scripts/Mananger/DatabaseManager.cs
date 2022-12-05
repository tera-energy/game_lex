using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Networking;

#if PLATFORM_IOS
using UnityEngine.iOS;
#endif

public class TrUserNormalInfo
{
    public string UserId;
    public TrUserNormalInfo(string userId)
    {
        UserId = userId;
    }
}

[Serializable]
public class TrPlayRank
{
    public string _name;
    public int _score;
    public TrPlayRank(string name, int score)
    {
        _name = name;
        _score = score;
    }
}

/*public class TrJsonAbleListWrapper
{
    public List<TrPlayRank> _list;
    public TrJsonAbleListWrapper(List<TrPlayRank> list) => _list = list;
}*/

public class DatabaseManager : MonoBehaviour
{
    static DatabaseManager _instance;

    public static int _maxWaitingTime = 30;

    DatabaseReference _versionReference;
    DatabaseReference _rootReference;
    DatabaseReference _uidReference;

    [HideInInspector] public static List<int> _liMyScores;
    [HideInInspector] public List<TrTotalScore> _liTotalScores;

    [HideInInspector] public static TrUserData _myDatas;

    [HideInInspector] public bool _isSuccess;
    public static DatabaseManager xInstance { get { return _instance; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes()
    {
        _instance = null;
    }

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


#if PLATFORM_ANDROID
        string platform = TrProjectSettings.GOOGLE;
#endif
#if PLATFORM_IOS
        string platform = TrProjectSettings.APPLE;
#endif

        _rootReference = FirebaseDatabase.DefaultInstance.RootReference.Child(TrProjectSettings._character + "User");
        _versionReference = FirebaseDatabase.DefaultInstance.RootReference.Child("Versions").Child(platform);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(zSetMyData());

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //StartCoroutine(zGetMyDataNew());
            //StartCoroutine(zGetMyData());
        }

    }

    #region Score

    public IEnumerator zGetDataMyScores()
    {
        bool isExec = true;
        _uidReference.Child(TrProjectSettings.SCORES).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed get scores");
            }
            else
            {
                DataSnapshot dataSnapshot = task.Result;
                _liMyScores = new List<int>();
                foreach (var score in dataSnapshot.Children)
                {
                    _liMyScores.Add(int.Parse(score.Value.ToString()));
                }
                isExec = false;
            }
        });

        yield return new WaitUntil(() => !isExec);
    }

    /*public IEnumerator zGetDataTotalScores()
    {
        _liScores = null;
        DateTime baseDate = DateTime.Now;
        var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
        var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
        string addr = "rank_unity_api";
        string options = $"stDate={thisWeekStart.ToString("yyyy-MM-dd")}&endDate={thisWeekEnd.ToString("yyyy-MM-dd")}";
        var www = UnityWebRequest.Get($"{TrEtcSetting.API_URL}/{TrProjectSettings._character}{TrProjectSettings._apiVersion}/{addr}?{options}");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ProtocolError || www.result != UnityWebRequest.Result.Success)
        {
            //Debug.LogError(www.error);
        }
        else
        {
            byte[] results = www.downloadHandler.data;
            var json = Encoding.UTF8.GetString(results);
            //Debug.Log(json);
            _liScores = TT.zFromJson<TrScoresDTO>(json).ToList();
        }
    }*/

    public IEnumerator zGetDataTotalScores()
    {
        bool isExec = true;
        Stack<TrTotalScore> tempStack = new Stack<TrTotalScore>();
        _rootReference.OrderByChild(TrProjectSettings.MAXSCORE).LimitToLast(50).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed get total scores");
            }
            else
            {
                DataSnapshot dataSnapshot = task.Result;
                foreach (DataSnapshot user in dataSnapshot.Children)
                {
                    TrUserData data = JsonUtility.FromJson<TrUserData>(user.GetRawJsonValue());
                    TrTotalScore add = new TrTotalScore();
                    add.nickname = data.nickName;
                    add.maxScore = data.maxScore;
                    tempStack.Push(add);
                }
                isExec = false;
            }
        });

        yield return new WaitUntil(() => !isExec);

        _liTotalScores = new List<TrTotalScore>();
        while (tempStack.Count() > 0)
        {
            _liTotalScores.Add(tempStack.Pop());
        }
    }

    public IEnumerator zSetMaxScore()
    {
        bool isExec = true;

        Dictionary<string, object> childUpdate = new Dictionary<string, object>();
        childUpdate[string.Format("{0}{1}", '/', TrProjectSettings.MAXSCORE)] = _myDatas.maxScore;
        _uidReference.UpdateChildrenAsync(childUpdate).ContinueWith(task => {
            if (task.IsFaulted)
                Debug.Log("Failed Set my scores");
            else
                isExec = false;
        });

        yield return new WaitUntil(() => !isExec);
    }

    public IEnumerator zSetMyScores()
    {
        bool isExec = true;
        TrMySocres scores = new TrMySocres();

        scores.score1 = _liMyScores[0];
        scores.score2 = _liMyScores[1];
        scores.score3 = _liMyScores[2];
        scores.score4 = _liMyScores[3];
        scores.score5 = _liMyScores[4];

        string json = JsonUtility.ToJson(scores);
        _uidReference.Child(TrProjectSettings.SCORES).SetRawJsonValueAsync(json).ContinueWith(task => {
            if (task.IsFaulted)
                Debug.Log("Failed Set my scores");
            else
                isExec = false;
        });

        yield return new WaitUntil(() => !isExec);
    }
    #endregion
    #region UserData
    public void zSetUserIDReference(string id)
    {
        _uidReference = _rootReference.Child(id);
    }

    public IEnumerator zGetMyData(string uid)
    {
        bool isRead = true;
        _rootReference.Child(uid).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Failed Get Data");
            }
            else
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value != null)
                {
                    var user = JsonUtility.FromJson<TrUserData>(snapshot.GetRawJsonValue());
                    _myDatas = user;
                    zSetUserIDReference(_myDatas.userId);
                }
                else
                {
                    _myDatas = null;
                }
                isRead = false;
            }
        });

        yield return new WaitUntil(() => !isRead);
    }
    public IEnumerator zSetMyData()
    {
        TrUserData user = new TrUserData
        {
            userId = AuthManager.User.UserId,
            email = AuthManager.User.Email,
            nickName = "",
            stamina = StaminaManager._maxStamina,
            staminaDate = "",
            maxScore = 0,
        };

        string json = JsonUtility.ToJson(user);

        bool isRead = true;
        _rootReference.Child(AuthManager.User.UserId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (!task.IsFaulted)
            {
                _myDatas = user;
                zSetUserIDReference(_myDatas.userId);
                AuthManager._userId = _myDatas.userId;
                isRead = false;
            }
            else
            {
                Debug.Log(task.Exception);
            }
        });

        yield return new WaitUntil(() => !isRead);
    }

    public IEnumerator zPutNickname(string nick)
    {
        _isSuccess = true;
        bool isFind = true;
        _rootReference.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed get users data");
            }
            else
            {
                DataSnapshot dataSnapshot = task.Result;
                foreach (DataSnapshot snap in dataSnapshot.Children)
                {
                    TrUserData user = JsonUtility.FromJson<TrUserData>(snap.GetRawJsonValue());

                    if (user.nickName == nick)
                    {
                        _isSuccess = false;
                        break;
                    }
                }
                isFind = false;
            }
        });

        yield return new WaitUntil(() => !isFind);

        if (_isSuccess)
        {
            _myDatas.nickName = nick;
            Dictionary<string, object> childUpdate = new Dictionary<string, object>();
            childUpdate[string.Format("{0}{1}", '/', TrProjectSettings.NICKNAME)] = nick;
            _uidReference.UpdateChildrenAsync(childUpdate);
        }
    }
    public IEnumerator zDeleteUserData()
    {
        bool isExec = true;
        _uidReference.RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("remove error");
            }
            else
            {
                isExec = false;
                //Debug.Log("succesed remove");
            }
        });

        yield return new WaitUntil(() => !isExec);
    }

    public IEnumerator zConnectPlatform(string preUserId, string newUserId)
    {
        bool isExec = true;
        TrUserData tempUserData = null;
        _rootReference.Child(preUserId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError("(Connect) failed get userdata");
            else
            {
                DataSnapshot dataSnapshot = task.Result;
                TrUserData user = JsonUtility.FromJson<TrUserData>(dataSnapshot.GetRawJsonValue());

                tempUserData = user;
                tempUserData.userId = newUserId;
                tempUserData.email = AuthManager.User.Email;
                isExec = false;
            }
        });
        yield return new WaitUntil(() => !isExec);


        isExec = true;
        string json = JsonUtility.ToJson(tempUserData);
        _rootReference.Child(newUserId).SetRawJsonValueAsync(json).ContinueWith(task => {
            if (task.IsFaulted)
                Debug.LogError("(Connect) failed set new userdata");
            else
            {
                //Debug.Log("(Connect) Succeed set new userdata");
                isExec = false;
            }
        });
        yield return new WaitUntil(() => !isExec);

        isExec = true;
        _rootReference.Child(preUserId).RemoveValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
                Debug.LogError("(Connect) Failed remove userdata");
            else
            {
                //Debug.Log("Succeed remove userdata");
                isExec = false;
            }
        });
        yield return new WaitUntil(() => !isExec);
    }

    #endregion
    #region Stamina
    public IEnumerator zSetStamina()
    {
        bool isExec = true;
        Dictionary<string, object> childUpdate = new Dictionary<string, object>();
        childUpdate[string.Format("{0}{1}", '/', TrProjectSettings.STAMINA)] = _myDatas.stamina;
        childUpdate[string.Format("{0}{1}", '/', TrProjectSettings.STAMINADATE)] = _myDatas.staminaDate;
        _uidReference.UpdateChildrenAsync(childUpdate).ContinueWith(task => {
            if (task.IsFaulted)
                Debug.Log("Failed set stamina");
            else
                isExec = false;
        });

        yield return new WaitUntil(() => !isExec);
    }
    #endregion

    public IEnumerator zCheckVersion()
    {
        bool isRead = true;
        _isSuccess = false;
        string thisVerison = Application.version;
        _versionReference.Child(TrProjectSettings._character).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed get version info");
            }
            else
            {
                //string getVersion = task.Result.Value.ToString();
                DataSnapshot snap = task.Result;
                string getVersion = snap.Value.ToString();
                if (getVersion.CompareTo(thisVerison) == 0)
                {
                    _isSuccess = true;
                }
                else
                {
                    _isSuccess = false;
                }
                isRead = false;
            }
        });

        yield return new WaitUntil(() => !isRead);
    }
}

#region TeraDB DTOs
[Serializable]
public class TrUserData
{
    public string userId;
    public string email;
    public string nickName;
    public int stamina;
    public string staminaDate;
    public int maxScore;
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
    public int maxScore;
}

#endregion
