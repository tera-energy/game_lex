using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    static public GameManager xInstance { get { return _instance; } }

    public static TT.enumGameType _type;
    public static TT.enumGameState _state;
    public bool _isGameStarted;
    public bool _isGameStopped;

    public static int _score;
    public int _valuePlayTime;
    public int _correctNum;
    public int _numMaxCombo;
    public static bool _canBtnClick = true;
    public static bool _battleDidIWin;
    public static int  _battleOppCorrect;
    static string _sceneName;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes()
    {
        _instance = null;
    }

    void Awake(){
        ySetState();
        if (_instance == null){
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start(){
        Application.targetFrameRate = 60;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 시 AudioListener 중복 제거 (Camera.main 우선 유지)
        var listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length <= 1) return;

        // Camera.main에 붙은 것을 우선 살리고 나머지 비활성화
        var keep = listeners.FirstOrDefault(l => l.GetComponent<Camera>() != null)
                ?? listeners[0];
        foreach (var l in listeners)
            if (l != keep) l.enabled = false;
    }

    void ySetState(){
        string sceneName = SceneManager.GetActiveScene().name;

        if(sceneName == TrProjectSettings.strLOBBY){
            _state = TT.enumGameState.Lobby;
        }else if(sceneName == TrProjectSettings.strPUZZLE){
            _state = TT.enumGameState.Play;
        }else if(sceneName == TrProjectSettings.strRESULT){
            _state = TT.enumGameState.Result;
        }
    }

    public void zSetUIRect(ref RectTransform[] rts, RectTransform can = null)
    {

        int num = rts.Length;
        Rect rect = can.rect;
        float width = rect.width;
        float height = rect.height;
        for (int i = 0; i < num; i++)
        {
            rts[i].sizeDelta = new Vector2(width, height);
        }
    }

    public void zSetCamera()
    {
        Camera cam = Camera.main;
        Rect rect = cam.rect;
        float scaleHeight = ((float)Screen.width / Screen.height) / ((float)16 / 9);
        float scaleWidth = 1f / scaleHeight;

        if (scaleHeight < 1)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;
        }
        cam.rect = rect;
    }


    public void zSetPuzzleGame()
    {
        _score = 0;
        _correctNum = 0;

        SceneManager.LoadScene(TrProjectSettings.strPUZZLE);
    }
}
