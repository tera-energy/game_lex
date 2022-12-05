using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
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
    public bool IsFirebaseReady { get; private set; }
    public bool IsSignInOnProgress { get; private set; }

    public static string _userId;

    // �г��� �Է�â
    [SerializeField] TrUI_Window_ _goInputNickName;
    [SerializeField] TMP_InputField _textNickName;

    // �˸�â
    [SerializeField] TrUI_Window_ _goNoticeWindow;
    [SerializeField] TextMeshProUGUI _txtNotice;
    Coroutine _coNotice;

    [SerializeField] GameObject _goBeforeSignIn;
    [SerializeField] GameObject _goAfterSignIn;

    [SerializeField] Button[] _btnSignIns;
    [SerializeField] Button[] _btnConnects;
    [SerializeField] TextMeshProUGUI _txtId;

    string webClientId = "614919104208-n1p1d97v4ob7k2foihlouug9f07l6bad.apps.googleusercontent.com";

    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;
    public static GoogleSignInConfiguration configuration;
#if PLATFORM_IOS
    IAppleAuthManager _appleAuthManager;
#endif
    public static FirebaseUser User = null;

    public bool _isCheckAutoSignIn = false;
    public bool _isAutoSignIn;
    public static bool _isCompleteSignIn;
    bool _doSignOut = false;
    bool _doConnect = false;
    public static bool _isGuest;

    Coroutine _coCertify;

    public enum TrPlatformType
    {
        NONE,
        GUEST,
        GOOGLE,
        APPLE,
    }

    public void zGuestLogin()
    {
        if (!IsFirebaseReady || IsSignInOnProgress || User != null) return;

        TT.zSetInteractButtons(ref _btnSignIns, false);
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        IsSignInOnProgress = true;

        firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                yCheckSignInResult(false);
            }
            else
            {
                IsSignInOnProgress = false;
                User = task.Result;
                _userId = User.UserId;

                _coCertify = StartCoroutine(yCertify(TrPlatformType.GUEST));
            }
        });
    }

    public void zAppleSignIn()
    {
#if PLATFORM_IOS
        if (!IsFirebaseReady || IsSignInOnProgress || User != null) return;

        TT.zSetInteractButtons(ref _btnSignIns, false);
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        IsSignInOnProgress = true;

        StartCoroutine(yAppleSignIn());
#endif
    }
#if PLATFORM_IOS
    string yGenerateNonce(string rawNonce){
        SHA256 sha = new SHA256Managed();
        var sb = new StringBuilder();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(rawNonce));
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }


    IEnumerator yAppleSignIn(){
        string rawNonce = Guid.NewGuid().ToString();
        string nonce = yGenerateNonce(rawNonce);

        var quickLoginArgs = new AppleAuthQuickLoginArgs(nonce);
        bool isQuickLoginDone = false;
        bool isSuccessLogin = false;

        string authCode = "";
        string idToken = "";

        _appleAuthManager.QuickLogin(
            quickLoginArgs, credential =>{
                try
                {
                    var appleIdCredential = credential as IAppleIDCredential;
                    authCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                    idToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    isSuccessLogin = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    yCheckSignInResult(false);
                    isSuccessLogin = false;
                }
                isQuickLoginDone = true;
            },
            error =>
            {
                isSuccessLogin = false;
                isQuickLoginDone = true;
                yCheckSignInResult(false);
            });

        yield return new WaitUntil(() => isQuickLoginDone);


        if (isSuccessLogin){
            ySignInWithAppleOnFirebase(idToken, rawNonce, authCode);
            yield break;
        }

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail, nonce);

        _appleAuthManager.LoginWithAppleId(loginArgs, credential => {
            // Obtained credential, cast it to IAppleIDCredential
            var appleIdCredential = credential as IAppleIDCredential;
            if (appleIdCredential != null)
            {
                var userId = appleIdCredential.User;
                var email = appleIdCredential.Email;

                idToken = Encoding.UTF8.GetString(
                            appleIdCredential.IdentityToken,
                            0,
                            appleIdCredential.IdentityToken.Length);
                authCode = Encoding.UTF8.GetString(
                            appleIdCredential.AuthorizationCode,
                            0,
                            appleIdCredential.AuthorizationCode.Length);
                ySignInWithAppleOnFirebase(idToken, rawNonce, authCode);
            }
            else
                yCheckSignInResult(false);
            
        },
            error => {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                yCheckSignInResult(false);
            });

    }

    void ySignInWithAppleOnFirebase(string idToken, string rawNonce, string authCode){
        Credential credential = OAuthProvider.GetCredential("apple.com", idToken, rawNonce, authCode);

        firebaseAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null){
                Debug.Log(task.Exception);
                yNotice("Failed login to Apple");
                yCheckSignInResult(false);
            }
            else{
                IsSignInOnProgress = false;
                User = task.Result;
                _userId = User.UserId;

                if(!_doConnect)
                    _coCertify = StartCoroutine(yCertify(TrPlatformType.APPLE));
                
            }
        });
    }
#endif

    public void zGoogleSignIn()
    {
        if (!IsFirebaseReady || IsSignInOnProgress || User != null) return;

        TT.zSetInteractButtons(ref _btnSignIns, false);
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        IsSignInOnProgress = true;
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        //yNotice("Logging in....");
        //Debug.Log("Google login");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(yOnAuthenticationFinished);
    }

    internal void yOnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                    yNotice("Failed login to Google");
                    yCheckSignInResult(false);
                }
                else
                {
                    Debug.Log(task.Exception.ToString());
                    yNotice("Failed login to Google");
                    yCheckSignInResult(false);
                }
            }
            IsSignInOnProgress = false;
        }
        else if (task.IsCanceled)
        {
            yCheckSignInResult(false);
        }
        else
        {
            ySignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }

    void ySignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        firebaseAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                yNotice("Failed login to Google");
                IsSignInOnProgress = false;
                Debug.Log(task.Exception);
                yCheckSignInResult(false);
            }
            else
            {
                IsSignInOnProgress = false;
                User = task.Result;
                _userId = User.UserId;

                if (!_doConnect)
                    _coCertify = StartCoroutine(yCertify(TrPlatformType.GOOGLE));

            }
        });
    }

    IEnumerator yCertify(TrPlatformType type, bool isAutoSignIn = false)
    {
        yield return StartCoroutine(DatabaseManager.xInstance.zGetMyData(_userId));

        if (DatabaseManager._myDatas == null)
        {
            yield return StartCoroutine(DatabaseManager.xInstance.zSetMyData());
        }

        if (!isAutoSignIn)
        {
            ySetLocalDatas(User.UserId, (int)type);
        }

        _txtId.text = _userId;
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
        {
            _goInputNickName.zShow();
        }
        else
        {
            if (_doSignOut)
                zIsSignIn(true);
            //yNotice("Succeed login to Google");
        }
    }

    void ySetLocalDatas(string id, int type)
    {
        PlayerPrefs.SetString(TrProjectSettings.AUTOLOGINID, id);
        PlayerPrefs.SetInt(TrProjectSettings.AUTOLOGINPLATFORM, type);
        PlayerPrefs.Save();
    }

    IEnumerator yCheckNickname()
    {
        string txtName = _textNickName.text;

        if (txtName.Length < 2 || txtName.Length > 10)
        {
            yNotice("Length of nickname doesn't match");
            yield break;
        }

        // Ư������ ���� true, �ƴϸ� false
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

    public void zSignoutAuth()
    {
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        yResetStatics();
        PlayerPrefs.SetInt(TrProjectSettings.AUTOLOGINPLATFORM, (int)TrPlatformType.NONE);
        PlayerPrefs.SetString(TrProjectSettings.AUTOLOGINID, "");
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
        string tempUserId = _userId;
        TrUserData tempUserData = DatabaseManager._myDatas;
        yResetStatics();

        switch (type)
        {
            case TrPlatformType.GOOGLE:
                zGoogleSignIn();
                break;
            case TrPlatformType.APPLE:
#if PLATFORM_IOS
                zAppleSignIn();
#endif
                break;
        }
        yield return new WaitUntil(() => User != null);

        yield return StartCoroutine(DatabaseManager.xInstance.zGetMyData(User.UserId));
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
            //Failed
            _userId = tempUserId;
            DatabaseManager.xInstance.zSetUserIDReference(_userId);
            DatabaseManager._myDatas = tempUserData;
            yNotice("This account already exists!");
        }
    }

    void yResetStatics()
    {
        User = null;
        IsSignInOnProgress = false;
        _isCompleteSignIn = false;
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
            User = null;

            if (_coCertify != null)
                StopCoroutine(_coCertify);
        }
    }

    public void yNotice(string text)
    {
        if (_coNotice != null)
        {
            _goNoticeWindow.zHide();
            StopCoroutine(_coNotice);
        }
        _goNoticeWindow.zShow();
        _txtNotice.text = text;
        _coNotice = StartCoroutine(yCancelNoticeWindow(text));
    }

    IEnumerator yCancelNoticeWindow(string text)
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

    IEnumerator yCreateDummyUser()
    {
        yield return new WaitUntil(() => IsFirebaseReady);
        bool isDummyCom = false;
        firebaseAuth.SignInWithEmailAndPasswordAsync("test@test.com", "123456").ContinueWithOnMainThread(task =>
        {
            User = task.Result;
            _userId = User.UserId;
            isDummyCom = true;
        });
        yield return new WaitUntil(() => isDummyCom);

        yield return StartCoroutine(DatabaseManager.xInstance.zGetMyData(User.UserId));
        Debug.Log("dummy");

        //yield return StartCoroutine(DatabaseManager.xInstance.zSetMyData());
        _txtId.text = _userId;

        _isCompleteSignIn = true;
    }

    IEnumerator yCheckAutoLogin()
    {
        _userId = PlayerPrefs.GetString(TrProjectSettings.AUTOLOGINID, "");
        _isAutoSignIn = _userId != "";

        if (_isAutoSignIn)
        {
            yield return new WaitUntil(() => IsFirebaseReady);
            TrPlatformType type = (TrPlatformType)PlayerPrefs.GetInt(TrProjectSettings.AUTOLOGINPLATFORM);
            _coCertify = StartCoroutine(yCertify(type, true));
            _isCheckAutoSignIn = true;
            yield break;
        }
        _isAutoSignIn = false;
        _isCheckAutoSignIn = true;
    }

    public void zSetFirebase()
    {
#if PLATFORM_ANDROID || PLATFORM_IOS
        if (configuration == null)
            configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };

        IsSignInOnProgress = false;
        IsFirebaseReady = false;
#endif
#if PLATFORM_IOS
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            _appleAuthManager = new AppleAuthManager(deserializer);
        }
#endif
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var result = task.Result;

            if (result != DependencyStatus.Available)
            {
                yNotice(result.ToString());
                IsFirebaseReady = false;
            }
            else
            {
                IsFirebaseReady = true;
                if (firebaseApp == null)
                    firebaseApp = FirebaseApp.DefaultInstance;
                if (firebaseAuth == null)
                    firebaseAuth = FirebaseAuth.DefaultInstance;
            }
        });

#if UNITY_EDITOR
        StartCoroutine(yCreateDummyUser());
        _isAutoSignIn = true;
        _isCheckAutoSignIn = true;
#endif

#if !UNITY_EDITOR
        StartCoroutine(yCheckAutoLogin());
#endif
    }

    public void zSetBtnsConnect()
    {
        TT.zSetInteractButtons(ref _btnConnects, _isGuest);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes()
    {
        firebaseApp = null;
        firebaseAuth = null;
        User = null;
        configuration = null;
        _instance = null;
    }

    void Awake()
    {
        if (_instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

#if PLATFORM_IOS
    void Update()
    {
        _appleAuthManager?.Update();
    }
#endif
}