using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Google;
#if PLATFORM_IOS
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class AuthManager : MonoBehaviour
{
    static AuthManager _instance;
    static public AuthManager xInstance { get { return _instance; } }

    public bool IsFirebaseReady { get; private set; }   // 이름 유지 (TrLobbyManager 호환)
    public bool IsSignInOnProgress { get; private set; }

    public static string _userId;
    public static string _userEmail;

    // 닉네임 입력창
    [SerializeField] TrUI_Window_ _goInputNickName;
    [SerializeField] TMP_InputField _textNickName;

    // 알림창
    [SerializeField] TrUI_Window_ _goNoticeWindow;
    [SerializeField] TextMeshProUGUI _txtNotice;
    Coroutine _coNotice;

    [SerializeField] GameObject _goBeforeSignIn;
    [SerializeField] GameObject _goAfterSignIn;

    [SerializeField] Button[] _btnSignIns;
    [SerializeField] Button[] _btnConnects;
    [SerializeField] TextMeshProUGUI _txtId;

    // 새 Google Cloud 프로젝트(GameLexOauth)의 Web Client ID
    string _webClientId = "168415577336-9705a2h50rf2cafmek2r918bbsuqlnnl.apps.googleusercontent.com";

    static GoogleSignInConfiguration _configuration;

#if PLATFORM_IOS
    IAppleAuthManager _appleAuthManager;
#endif

    public bool _isCheckAutoSignIn = false;
    public bool _isAutoSignIn;
    public static bool _isCompleteSignIn;
    bool _doSignOut = false;
    bool _doConnect = false;
    public static bool _isGuest;

    Coroutine _coCertify;

    const string KEY_REFRESH_TOKEN = "SupabaseRefreshToken";

    public enum TrPlatformType
    {
        NONE,
        GUEST,
        GOOGLE,
        APPLE,
    }

    // ──────────────────────────────────────────
    // 게스트 로그인
    // ──────────────────────────────────────────
    public void zGuestLogin()
    {
        if (!IsFirebaseReady || IsSignInOnProgress || _userId != null) return;

        TT.zSetInteractButtons(ref _btnSignIns, false);
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        IsSignInOnProgress = true;

        StartCoroutine(yGuestLogin());
    }

    IEnumerator yGuestLogin()
    {
        bool isDone = false;
        string url  = SupabaseClient.AuthUrl("token?grant_type=anonymous");

        yield return SupabaseClient.Post(url, "{}",
            onSuccess: json =>
            {
                var session = JsonUtility.FromJson<SupabaseSession>(json);
                if (session != null && session.access_token != "")
                {
                    SupabaseClient.AccessToken = session.access_token;
                    PlayerPrefs.SetString(KEY_REFRESH_TOKEN, session.refresh_token);
                    _userId    = session.user.id;
                    _userEmail = session.user.email ?? "";
                }
                isDone = true;
            },
            onError: err =>
            {
                Debug.LogError("Guest login failed: " + err);
                yCheckSignInResult(false);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);

        if (_userId != null)
            _coCertify = StartCoroutine(yCertify(TrPlatformType.GUEST));
    }

    // ──────────────────────────────────────────
    // Apple 로그인
    // ──────────────────────────────────────────
    public void zAppleSignIn()
    {
#if PLATFORM_IOS
        if (!IsFirebaseReady || IsSignInOnProgress || _userId != null) return;

        TT.zSetInteractButtons(ref _btnSignIns, false);
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        IsSignInOnProgress = true;

        StartCoroutine(yAppleSignIn());
#endif
    }

#if PLATFORM_IOS
    string yGenerateNonce(string rawNonce)
    {
        SHA256 sha = new SHA256Managed();
        var sb = new StringBuilder();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(rawNonce));
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    IEnumerator yAppleSignIn()
    {
        string rawNonce = Guid.NewGuid().ToString();
        string nonce    = yGenerateNonce(rawNonce);

        var quickLoginArgs = new AppleAuthQuickLoginArgs(nonce);
        bool isQuickLoginDone = false;
        bool isSuccessLogin   = false;
        string idToken = "";

        _appleAuthManager.QuickLogin(quickLoginArgs,
            credential =>
            {
                try
                {
                    var appleIdCredential = credential as IAppleIDCredential;
                    idToken        = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    isSuccessLogin = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    yCheckSignInResult(false);
                }
                isQuickLoginDone = true;
            },
            error =>
            {
                isQuickLoginDone = true;
                yCheckSignInResult(false);
            });

        yield return new WaitUntil(() => isQuickLoginDone);

        if (isSuccessLogin)
        {
            yield return StartCoroutine(ySignInWithAppleOnSupabase(idToken, rawNonce));
            yield break;
        }

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail, nonce);
        _appleAuthManager.LoginWithAppleId(loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    idToken = Encoding.UTF8.GetString(
                        appleIdCredential.IdentityToken, 0,
                        appleIdCredential.IdentityToken.Length);
                    isSuccessLogin = true;
                }
                else
                    yCheckSignInResult(false);
            },
            error => yCheckSignInResult(false));

        yield return new WaitUntil(() => isSuccessLogin);

        if (isSuccessLogin)
            yield return StartCoroutine(ySignInWithAppleOnSupabase(idToken, rawNonce));
    }

    IEnumerator ySignInWithAppleOnSupabase(string idToken, string rawNonce)
    {
        bool isDone = false;
        string url  = SupabaseClient.AuthUrl("token?grant_type=id_token");
        string json = $"{{\"provider\":\"apple\",\"id_token\":\"{idToken}\",\"nonce\":\"{rawNonce}\"}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: response =>
            {
                var session = JsonUtility.FromJson<SupabaseSession>(response);
                if (session != null && session.access_token != "")
                {
                    SupabaseClient.AccessToken = session.access_token;
                    PlayerPrefs.SetString(KEY_REFRESH_TOKEN, session.refresh_token);
                    _userId    = session.user.id;
                    _userEmail = session.user.email ?? "";
                }
                isDone = true;
            },
            onError: err =>
            {
                Debug.LogError("Apple Supabase login failed: " + err);
                yNotice("Failed login to Apple");
                yCheckSignInResult(false);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);

        if (_userId != null)
        {
            if (!_doConnect)
                _coCertify = StartCoroutine(yCertify(TrPlatformType.APPLE));
        }
    }
#endif

    // ──────────────────────────────────────────
    // Google 로그인
    // ──────────────────────────────────────────
    public void zGoogleSignIn()
    {
        if (!IsFirebaseReady || IsSignInOnProgress || _userId != null) return;

        TT.zSetInteractButtons(ref _btnSignIns, false);
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        IsSignInOnProgress = true;

        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn  = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        StartCoroutine(yGoogleSignInCoroutine());
    }

    IEnumerator yGoogleSignInCoroutine()
    {
        var task = GoogleSignIn.DefaultInstance.SignIn();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            using (var e = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    var error = (GoogleSignIn.SignInException)e.Current;
                    Debug.Log("Google error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log(task.Exception.ToString());
                }
            }
            yNotice("Failed login to Google");
            yCheckSignInResult(false);
            IsSignInOnProgress = false;
        }
        else if (task.IsCanceled)
        {
            yCheckSignInResult(false);
        }
        else
        {
            yield return StartCoroutine(ySignInWithGoogleOnSupabase(task.Result.IdToken));
        }
    }

    IEnumerator ySignInWithGoogleOnSupabase(string idToken)
    {
        bool isDone = false;
        string url  = SupabaseClient.AuthUrl("token?grant_type=id_token");
        string json = $"{{\"provider\":\"google\",\"id_token\":\"{idToken}\"}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: response =>
            {
                var session = JsonUtility.FromJson<SupabaseSession>(response);
                if (session != null && session.access_token != "")
                {
                    SupabaseClient.AccessToken = session.access_token;
                    PlayerPrefs.SetString(KEY_REFRESH_TOKEN, session.refresh_token);
                    _userId    = session.user.id;
                    _userEmail = session.user.email ?? "";
                }
                isDone = true;
            },
            onError: err =>
            {
                Debug.LogError("Google Supabase login failed: " + err);
                yNotice("Failed login to Google");
                yCheckSignInResult(false);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);

        if (_userId != null)
        {
            if (!_doConnect)
                _coCertify = StartCoroutine(yCertify(TrPlatformType.GOOGLE));
        }
    }

    // ──────────────────────────────────────────
    // 공통 인증 흐름
    // ──────────────────────────────────────────
    IEnumerator yCertify(TrPlatformType type, bool isAutoSignIn = false)
    {
        yield return StartCoroutine(DatabaseManager.xInstance.zGetMyData(_userId));

        if (DatabaseManager._myDatas == null)
            yield return StartCoroutine(DatabaseManager.xInstance.zSetMyData());

        if (!isAutoSignIn)
            ySetLocalDatas(_userId, (int)type);

        _txtId.text      = _userId;
        _isCompleteSignIn = true;

        if (type == TrPlatformType.GUEST)
        {
            _isGuest = true;
            TT.zSetInteractButtons(ref _btnConnects, true);
        }
        else if (type == TrPlatformType.GOOGLE || type == TrPlatformType.APPLE)
        {
            _isGuest = false;
            TT.zSetInteractButtons(ref _btnConnects, false);
        }

        if (DatabaseManager._myDatas.nickName == "" || DatabaseManager._myDatas.nickName == null)
            _goInputNickName.zShow();
        else if (_doSignOut)
            zIsSignIn(true);
    }

    void ySetLocalDatas(string id, int type)
    {
        PlayerPrefs.SetString(TrProjectSettings.AUTOLOGINID, id);
        PlayerPrefs.SetInt(TrProjectSettings.AUTOLOGINPLATFORM, type);
        PlayerPrefs.Save();
    }

    // ──────────────────────────────────────────
    // 닉네임
    // ──────────────────────────────────────────
    IEnumerator yCheckNickname()
    {
        string txtName = _textNickName.text;

        if (txtName.Length < 2 || txtName.Length > 10)
        {
            yNotice("Length of nickname doesn't match");
            yield break;
        }

        bool checkSL = Regex.IsMatch(txtName, @"[^a-zA-Z0-9가-힣]");
        if (checkSL)
        {
            yNotice("Special characters are not allowed");
            yield break;
        }

        yield return StartCoroutine(DatabaseManager.xInstance.zPutNickname(txtName));

        if (!DatabaseManager.xInstance._isSuccess)
        {
            yNotice("It's a nickname that already exists");
            yield break;
        }

        yNotice("The nickname is decided");
        _goInputNickName.zHide();
    }

    public void zSelectNickname()
    {
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        StartCoroutine(yCheckNickname());
    }

    // ──────────────────────────────────────────
    // 로그아웃 / 탈퇴
    // ──────────────────────────────────────────
    public void zSignoutAuth()
    {
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        yResetStatics();
        PlayerPrefs.SetInt(TrProjectSettings.AUTOLOGINPLATFORM, (int)TrPlatformType.NONE);
        PlayerPrefs.SetString(TrProjectSettings.AUTOLOGINID, "");
        PlayerPrefs.DeleteKey(KEY_REFRESH_TOKEN);
        PlayerPrefs.Save();
        TrLobbyManager._isFirstLobby = true;
        DatabaseManager._myDatas = null;

        SceneManager.LoadScene(TrProjectSettings.strLOBBY);
    }

    IEnumerator yDeleteAuth()
    {
        yield return StartCoroutine(DatabaseManager.xInstance.zDeleteUserData());
        zSignoutAuth();
    }

    public void zDeleteAuth()
    {
        StartCoroutine(yDeleteAuth());
    }

    // ──────────────────────────────────────────
    // 게스트 → 플랫폼 연동
    // ──────────────────────────────────────────
    public void zConnectOtherPlatformForGuest(int type)
    {
        if (!_doConnect)
        {
            _doConnect = true;
            StartCoroutine(yConnectPlatform((TrPlatformType)type));
        }
    }

    IEnumerator yConnectPlatform(TrPlatformType type)
    {
        yNotice("Connecting...");
        string tempUserId   = _userId;
        TrUserData tempData = DatabaseManager._myDatas;
        yResetStatics();

        switch (type)
        {
            case TrPlatformType.GOOGLE: zGoogleSignIn(); break;
            case TrPlatformType.APPLE:
#if PLATFORM_IOS
                zAppleSignIn();
#endif
                break;
        }

        yield return new WaitUntil(() => _userId != null);

        yield return StartCoroutine(DatabaseManager.xInstance.zGetMyData(_userId));
        if (DatabaseManager._myDatas == null)
        {
            _isGuest = false;
            zSetBtnsConnect();
            yield return StartCoroutine(DatabaseManager.xInstance.zConnectPlatform(tempUserId, _userId));
            ySetLocalDatas(_userId, (int)type);
            _txtId.text = _userId;
            yNotice("Connection successful!");
        }
        else
        {
            _userId = tempUserId;
            DatabaseManager._myDatas = tempData;
            yNotice("This account already exists!");
        }
    }

    void yResetStatics()
    {
        _userId           = null;
        _userEmail        = null;
        IsSignInOnProgress = false;
        _isCompleteSignIn  = false;
        SupabaseClient.AccessToken = "";
    }

    public void zDeleteSessions()
    {
        PlayerPrefs.DeleteAll();
        yNotice("sessions have been removed");
    }

    public void yCheckSignInResult(bool isSuccess)
    {
        if (!isSuccess)
        {
            yNotice("Failed signin, Please retry signIn");
            TT.zSetInteractButtons(ref _btnSignIns, true);
            IsSignInOnProgress = false;
            _userId = null;

            if (_coCertify != null)
                StopCoroutine(_coCertify);
        }
    }

    // ──────────────────────────────────────────
    // 알림
    // ──────────────────────────────────────────
    public void yNotice(string text)
    {
        if (_coNotice != null)
        {
            _goNoticeWindow.zHide();
            StopCoroutine(_coNotice);
        }
        _goNoticeWindow.zShow();
        _txtNotice.text = text;
        _coNotice = StartCoroutine(yCancelNoticeWindow());
    }

    IEnumerator yCancelNoticeWindow()
    {
        yield return TT.WaitForSeconds(2f);
        _goNoticeWindow.zHide();
    }

    public void zCancelInfoWIndow()
    {
        TrAudio_UI.xInstance.zzPlay_ClickNo();
        _goNoticeWindow.zHide(false);
    }

    public void zIsSignIn(bool isAfter, bool isInit = false)
    {
        if (isInit)
        {
            _goAfterSignIn.SetActive(false);
            _goBeforeSignIn.SetActive(false);
            return;
        }
        _goAfterSignIn.SetActive(isAfter);
        _goBeforeSignIn.SetActive(!isAfter);
    }

    // ──────────────────────────────────────────
    // 자동 로그인
    // ──────────────────────────────────────────
    IEnumerator yRefreshSession()
    {
        string refreshToken = PlayerPrefs.GetString(KEY_REFRESH_TOKEN, "");
        if (refreshToken == "") yield break;

        bool isDone = false;
        string url  = SupabaseClient.AuthUrl("token?grant_type=refresh_token");
        string json = $"{{\"refresh_token\":\"{refreshToken}\"}}";

        yield return SupabaseClient.Post(url, json,
            onSuccess: response =>
            {
                var session = JsonUtility.FromJson<SupabaseSession>(response);
                if (session != null && session.access_token != "")
                {
                    SupabaseClient.AccessToken = session.access_token;
                    PlayerPrefs.SetString(KEY_REFRESH_TOKEN, session.refresh_token);
                    _userEmail = session.user.email ?? "";
                }
                isDone = true;
            },
            onError: err =>
            {
                Debug.LogWarning("Session refresh failed: " + err);
                isDone = true;
            });

        yield return new WaitUntil(() => isDone);
    }

    IEnumerator yCheckAutoLogin()
    {
        _userId      = PlayerPrefs.GetString(TrProjectSettings.AUTOLOGINID, "");
        _isAutoSignIn = _userId != "";

        if (_isAutoSignIn)
        {
            yield return StartCoroutine(yRefreshSession());
            TrPlatformType type = (TrPlatformType)PlayerPrefs.GetInt(TrProjectSettings.AUTOLOGINPLATFORM);
            _coCertify = StartCoroutine(yCertify(type, true));
            _isCheckAutoSignIn = true;
            yield break;
        }

        _userId           = null;
        _isAutoSignIn     = false;
        _isCheckAutoSignIn = true;
    }

    // ──────────────────────────────────────────
    // 초기화 (TrLobbyManager에서 zSetFirebase() 호출)
    // ──────────────────────────────────────────
    public void zSetFirebase()
    {
#if PLATFORM_ANDROID || PLATFORM_IOS
        if (_configuration == null)
            _configuration = new GoogleSignInConfiguration
            {
                WebClientId   = _webClientId,
                RequestEmail  = true,
                RequestIdToken = true
            };
#endif

#if PLATFORM_IOS
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }
#endif

        IsSignInOnProgress = false;
        IsFirebaseReady    = true;   // Supabase는 별도 초기화 불필요

#if UNITY_EDITOR
        StartCoroutine(yCreateDummyUser());
        _isAutoSignIn     = true;
        _isCheckAutoSignIn = true;
#endif

#if !UNITY_EDITOR
        StartCoroutine(yCheckAutoLogin());
#endif
    }

    // 에디터 전용 더미 유저 (DB 호출 없이 로컬 데이터로 대체)
    IEnumerator yCreateDummyUser()
    {
        _userId    = "00000000-0000-0000-0000-000000000001";
        _userEmail = "dummy@editor.local";

        DatabaseManager._myDatas = new TrUserData
        {
            userId      = _userId,
            email       = _userEmail,
            nickName    = "Editor",
            stamina     = StaminaManager._maxStamina,
            staminaDate = "",
            maxScore    = 0
        };

        _txtId.text       = _userId;
        _isCompleteSignIn  = true;
        yield return null;
    }

    public void zSetBtnsConnect()
    {
        TT.zSetInteractButtons(ref _btnConnects, _isGuest);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes()
    {
        _userId           = null;
        _userEmail        = null;
        _configuration    = null;
        _instance         = null;
        _isCompleteSignIn  = false;
        _isGuest          = false;
    }

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

#if PLATFORM_IOS
    void Update()
    {
        _appleAuthManager?.Update();
    }
#endif
}

// ──────────────────────────────────────────
// Supabase Auth DTOs
// ──────────────────────────────────────────
[Serializable]
class SupabaseSession
{
    public string access_token;
    public string refresh_token;
    public SupabaseAuthUser user;
}

[Serializable]
class SupabaseAuthUser
{
    public string id;
    public string email;
}
