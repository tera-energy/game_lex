using System.Collections;
using UnityEngine;

public class TrDeviceManager : MonoBehaviour
{
    static TrDeviceManager _instance;
    public static TrDeviceManager xInstance { get { return _instance; } }

    public static bool _isCheckUpdate = true;
    public static bool _isAlreadyCheckVersion = false;

#if PLATFORM_ANDROID
    public bool _isGooglePlay = true;
#endif

    [SerializeField] TrUI_Window_ _winWinUpdate;

    public void xOnClickUpdate()
    {
        Application.OpenURL(TrProjectSettings._urlStore);
    }

    public void xOnClickCancel()
    {
        _isCheckUpdate = false;
    }

    public void zCheckVersion()
    {
        StartCoroutine(yCheckNetwork());
    }

    IEnumerator CheckForUpdate()
    {
        yield return StartCoroutine(DatabaseManager.xInstance.zCheckVersion());

        bool isLatest = DatabaseManager.xInstance._isSuccess;
        if (isLatest)
        {
            _isCheckUpdate = false;
        }
        else
        {
            _winWinUpdate.zShow();
        }
        _isAlreadyCheckVersion = true;
    }

    IEnumerator yCheckNetwork()
    {
        if (!TrNetworkManager.zGetIsConnectNetwork())
            TrNetworkManager.xInstance.zActiveConnectWindow(true);
        yield return new WaitUntil(() => TrNetworkManager.zGetIsConnectNetwork());
        TrNetworkManager.xInstance.zActiveConnectWindow(false);

        StartCoroutine(CheckForUpdate());


#if PLATFORM_ANDROID
        if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
        }
#endif
    }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
#if PLATFORM_ANDROID
        if (_isGooglePlay)
            TrProjectSettings._urlStore = TrProjectSettings._googleUrl;
        else
            TrProjectSettings._urlStore = TrProjectSettings._oneUrl;
#endif
        _isCheckUpdate = true;
    }
}