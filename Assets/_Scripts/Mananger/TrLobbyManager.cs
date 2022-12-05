using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Database;
using TMPro;

public class TrLobbyManager : MonoBehaviour
{
    static TrLobbyManager _instance;
    public static TrLobbyManager xInstance { get { return _instance; } }
    public CanvasGroup _imgFade;
    [SerializeField] Animator _anim;
    [SerializeField] Transform _trUIPuzzle;
    [SerializeField] GameObject _goQuitButton;

    // ��ũ ����
    [Space]
    [SerializeField] TrPlayRankListBox _rankBoxOwn;
    [SerializeField] GameObject _goPreRankInfo;
    [SerializeField] Transform _trSelector;
    [SerializeField] TextMeshProUGUI _txtPersonalRank;
    [SerializeField] TextMeshProUGUI _txtTotalRank;
    [SerializeField] Color _colorPurple;
    [SerializeField] Sprite[] _spRanks;

    // ��ũ ����(��ü ��ŷ)
    [Space]
    [SerializeField] GameObject _goTotalRank;
    [SerializeField] Transform _trTotalRanksParent;
    [SerializeField] GameObject _goWaiting;
    [SerializeField] GameObject _goRanks;
    TrPlayRankListBox[] _rankBoxsTotal;
    [SerializeField] int _numRanks;
    [SerializeField] Transform _trTotalBtnTab;
    [SerializeField] int _maxReloadCoolTimeRank;
    float _currCoolTimeRank;


    // ��ũ ����(���� ��ŷ)
    [Space]
    [SerializeField] GameObject _goPersonalRank;
    [SerializeField] Transform _trPersonalRanksParent;
    [SerializeField] Transform _trPersonalBtnTab;

    // �޴� ��
    [Space]
    bool _isActivated = false;
    [SerializeField] TrUI_HoldButton _btnMenu;
    [SerializeField] Transform[] _trMenuComponents;
    int _numMenuComponenets;
    [SerializeField] float _distanceYMenuButton;
    [SerializeField] float _distanceYMenuComponents;
    [SerializeField] float _speedMCMove;
    Tween[] _tweenMoveMenuComponents;

    // ���� â
    [Space]
    [SerializeField] TrUI_Window_ _winSetting;
    [SerializeField] Slider _sliderBGM;
    [SerializeField] Slider _sliderSFX;
    [SerializeField] GameObject _goVibCheck;
    [SerializeField] TextMeshProUGUI _txtVersion;

    [Space]
    public static bool _isFirstLobby = true;
    [SerializeField] GameObject _goChecks;
    [SerializeField] Image _imgCheckFill;
    [SerializeField] TextMeshProUGUI _txtCheck;
    [SerializeField] TextMeshProUGUI _txtAnnounce;
    float _currValue = 0;
    float _targetValue = 0;

    [SerializeField] TextMeshProUGUI _posText;
    [SerializeField] ParticleSystem _cokeCarbonic;

    [Header("Only need android")]
    [SerializeField] TrUI_Window_ _windowQuit;
    [SerializeField] GameObject[] _goBtnAppleSignIn;

    bool _isAlreadyAgreePolicy;
    const string _urlPrivacyPolicy = "http://teraenergy.co.kr/teraenergy_2021/privacy.html";
    [SerializeField] TrUI_Window_ _windowPrivacy;
    [SerializeField] TrUI_HoldButton _btnAgree;

    public void zActivateAndDisableMenu()
    {
        for (int i = 0; i < _numMenuComponenets; i++)
        {
            if (_tweenMoveMenuComponents[i] != null)
                _tweenMoveMenuComponents[i].Kill();
        }

        _isActivated = !_isActivated;
        if (_isActivated)
        {
            _btnMenu.zInteractDisable(true);

            float targetY = _btnMenu.transform.localPosition.y - _distanceYMenuButton;
            float speed = _speedMCMove;
            for (int i = 0; i < _numMenuComponenets; i++)
            {
                Transform trCom = _trMenuComponents[i];
                trCom.gameObject.SetActive(true);
                _tweenMoveMenuComponents[i] = trCom.DOLocalMoveY(targetY, speed).SetSpeedBased();
                targetY -= _distanceYMenuComponents;
                speed *= 2;
            }
        }
        else
        {
            _btnMenu.zInteractActivate(true);

            float targetY = _btnMenu.transform.localPosition.y;
            float speed = _speedMCMove;
            for (int i = 0; i < _numMenuComponenets; i++)
            {
                Transform trCom = _trMenuComponents[i];
                _tweenMoveMenuComponents[i] = trCom.DOLocalMoveY(targetY, speed).SetSpeedBased().OnComplete(() => trCom.gameObject.SetActive(false));
                speed *= 2;
            }
        }
    }
    public void zClickTotalTankURL()
    {
        Application.OpenURL(TrProjectSettings._urlRanking);
    }

    #region OnClickEvent
    public void zClickPrivacyCheckBox(bool isActivate)
    {
        if (isActivate)
            _btnAgree.zInteractActivate();
        else
            _btnAgree.zInteractDisable();
    }

    public void zClickPrivacyURL() => Application.OpenURL(_urlPrivacyPolicy);
    public void zClickAgreePrivacy()
    {
        PlayerPrefs.SetInt(TT.strConfigAgreePrivacy, 1);
        _isAlreadyAgreePolicy = true;
        _windowPrivacy.zHide();
    }
    public void zQuitApplication()
    {
        Application.Quit();
    }
    IEnumerator yPosText()
    {
        Transform postext = _posText.transform;
        float startPosX = -120f;
        float targetPosX = 120f;
        Tween moveTween = null;
        while (true)
        {
            if (moveTween != null)
                moveTween.Kill();
            moveTween = postext.DOLocalMoveX(targetPosX, 2.5f).SetEase(Ease.Linear);
            yield return TT.WaitForSeconds(3f);
            postext.localPosition = new Vector2(startPosX, postext.localPosition.y);
        }

    }

    public void zOnClickActiveVibrate()
    {
        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
        int isActive = PlayerPrefs.GetInt(TT.strConfigVibrate);

        if (isActive == 1)
        {
            _goVibCheck.SetActive(false);
            PlayerPrefs.SetInt(TT.strConfigVibrate, 0);
        }
        else
        {
            _goVibCheck.SetActive(true);
            PlayerPrefs.SetInt(TT.strConfigVibrate, 1);
        }
    }


    void yGameStart()
    {
        //_anim.SetTrigger("Start");

        _trUIPuzzle.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _imgFade.DOFade(1, 2f).OnComplete(() => GameManager.xInstance.zSetPuzzleGame());
    }

    /// <summary>
    /// 0: Train
    /// 1: Challenge
    /// </summary>
    /// <param name="type"></param>
    public void zOnClickGameStart(int type)
    {
        if (type == 0)
        {
            GameManager._type = TT.enumGameType.Train;
            yGameStart();
        }
        else if (type == 1)
        {
            StartCoroutine(StaminaManager.xInstance.zCheckStamina(yGameStart));
        }
    }
    #endregion

    #region RankBoard
    /// <summary>
    /// 0:Personal
    /// 1:Total
    /// </summary>
    /// <param name="num"></param>
    public void xClickRank(int num)
    {
        TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();
        yChangeRank(num);
    }

    void yChangeRank(int num)
    {
        if (num == 0)
        {
            _goPersonalRank.SetActive(true);
            _goTotalRank.SetActive(false);
            _txtPersonalRank.color = Color.white;
            _txtTotalRank.color = _colorPurple;
            _trSelector.position = _trPersonalBtnTab.position;
        }
        else if (num == 1)
        {
            _goPersonalRank.SetActive(false);
            _goTotalRank.SetActive(true);
            if (_currCoolTimeRank >= _maxReloadCoolTimeRank)
            {
                _goWaiting.SetActive(true);
                _goRanks.SetActive(false);
                _currCoolTimeRank = 0;
                StartCoroutine(ySetTotalRankBoard());
            }
            _trSelector.position = _trTotalBtnTab.position;
            _txtPersonalRank.color = _colorPurple;
            _txtTotalRank.color = Color.white;
        }
    }

    IEnumerator ySetPersonalRankBaord(){
        if (DatabaseManager._liMyScores == null)
            yield return StartCoroutine(DatabaseManager.xInstance.zGetDataMyScores());

        TrPlayRankListBox[] rankUIs = _trPersonalRanksParent.GetComponentsInChildren<TrPlayRankListBox>();

        List<int> rankList = DatabaseManager._liMyScores;
        int listCount;
        if (rankList == null)
            listCount = 0;
        else
            listCount = rankList.Count;

        string nickname = DatabaseManager._myDatas.nickName;

        for (int i = 0; i < listCount; i++)
        {
            TrPlayRankListBox uis = rankUIs[i];
            uis._txtScore.text = string.Format("{0}P", rankList[i].ToString());
            uis._txtName.text = nickname;
        }

        for (int i = listCount; i < 5; i++)
        {
            TrPlayRankListBox uis = rankUIs[i];
            uis._txtScore.text = "0";
            uis._txtName.text = nickname;
        }
    }

    // ��ü ��ŷ ��Ȱ��ȭ(��ŷ��)
    void yDisableTotalRankInfos()
    {
        for (int i = 0; i < _numRanks; i++)
        {
            GameObject go = _rankBoxsTotal[i].gameObject;
            if (!go.activeSelf) return;
            go.SetActive(false);
        }
    }

    // ��ü ��ŷ ����
    IEnumerator ySetTotalRankBoard()
    {
        yield return StartCoroutine(DatabaseManager.xInstance.zGetDataTotalScores());

        if (DatabaseManager.xInstance._liTotalScores.Count != 0)
        {
            _txtAnnounce.text = "";
            List<TrTotalScore> li = DatabaseManager.xInstance._liTotalScores;
            int listCount = li.Count;
            int yourRanking = -1;
            int rank = 0;
            int index = 0;
            string ownName = DatabaseManager._myDatas.nickName;
            _rankBoxOwn._txtName.text = ownName;
            _rankBoxOwn._txtScore.text = string.Format("{0}P", DatabaseManager._myDatas.maxScore.ToString());
            while (index < _numRanks && index < listCount)
            {
                var user = li[index];
                string name = user.nickname;
                string score = user.maxScore.ToString();

                Transform box = _rankBoxsTotal[index].transform;
                box.gameObject.SetActive(true);
                box.SetParent(_trTotalRanksParent);
                box.localScale = new Vector3(1f, 1f, 1f);
                TrPlayRankListBox list = box.GetComponent<TrPlayRankListBox>();
                list._txtName.text = name;
                list._txtScore.text = string.Format("{0}P", score);
                list._imgValueRanking.gameObject.SetActive(true);
                list._txtValueRanking.text = "";
                if (rank == 0) list._imgValueRanking.sprite = _spRanks[rank];
                else if (rank == 1) list._imgValueRanking.sprite = _spRanks[rank];
                else if (rank == 2) list._imgValueRanking.sprite = _spRanks[rank];
                else
                {
                    list._imgValueRanking.gameObject.SetActive(false);
                    list._txtValueRanking.text = (rank + 1).ToString();
                }
                if (name == ownName) yourRanking = rank;
                rank++;
                index++;
            }

            Image imgRank = _rankBoxOwn._imgValueRanking;
            TextMeshProUGUI txtRank = _rankBoxOwn._txtValueRanking;

            if (yourRanking <= 2 && yourRanking >= 0)
            {
                txtRank.text = "";
                imgRank.gameObject.SetActive(true);
                imgRank.sprite = _spRanks[yourRanking];
            }
            else if (yourRanking == -1)
            {
                imgRank.gameObject.SetActive(false);
                txtRank.gameObject.SetActive(true);
                txtRank.text = "-";
                txtRank.fontSize = 35;
            }
            else
            {
                imgRank.gameObject.SetActive(false);
                txtRank.gameObject.SetActive(true);
                txtRank.text = (yourRanking + 1).ToString();
                txtRank.fontSize = 70;
            }
        }
        else
        {
            string ownName = DatabaseManager._myDatas.nickName;
            _rankBoxOwn._txtName.text = ownName;
            _rankBoxOwn._txtScore.text = "-";
            _rankBoxOwn._txtValueRanking.text = "-";
            _rankBoxOwn._imgValueRanking.gameObject.SetActive(false);
            _txtAnnounce.text = "No Ranks";

        }

        _goRanks.SetActive(true);
    }

    // ��ü ��ŷ �ڽ��� ����
    void yCreateRankBoxes()
    {
        _rankBoxsTotal = new TrPlayRankListBox[_numRanks];
        for (int i = 0; i < _numRanks; i++)
        {
            TrPlayRankListBox rankBox = Instantiate(_goPreRankInfo).GetComponent<TrPlayRankListBox>();
            rankBox.transform.SetParent(_trTotalRanksParent);
            _rankBoxsTotal[i] = rankBox;
            rankBox.gameObject.SetActive(false);
        }
    }
    #endregion

    #region setting
    // ����â �ʱ� ����
    void yInitSetting()
    {
        _txtVersion.text = string.Format("version {0}", Application.version);
        _sliderBGM.value = PlayerPrefs.GetFloat(TT.strConfigMusic, 1);
        _sliderSFX.value = PlayerPrefs.GetFloat(TT.strConfigSFX, 1);
        _goVibCheck.SetActive(PlayerPrefs.GetInt(TT.strConfigVibrate, 1) == 1);
    }
    public void xActiveSettings(bool isActive)
    {
        if (isActive) _winSetting.zShow();
        else _winSetting.zHide();
    }
    #endregion

    // ���� üũ ��ٸ���
    IEnumerator yWaitCheckVersion()
    {
        _goChecks.SetActive(false);
        yCreateRankBoxes();
        yChangeRank(0);
        yInitSetting();
        TrAudio_Music.xInstance.zzPlayMain(0.25f);
        _currCoolTimeRank = _maxReloadCoolTimeRank;
        AuthManager.xInstance.zSetBtnsConnect();
#if PLATFORM_ANDROID
        for (int i = 0; i < _goBtnAppleSignIn.Length; i++)
        {
            _goBtnAppleSignIn[i].SetActive(false);
        }
#endif
        _imgFade.DOFade(0, 2f);
        if (_isFirstLobby)
        {
            AuthManager.xInstance.zIsSignIn(true, true);

            yield return new WaitUntil(() => _imgFade.alpha <= 0.01f);

            _isAlreadyAgreePolicy = PlayerPrefs.GetInt(TT.strConfigAgreePrivacy, 0) == 1 ? true : false;
            if (!_isAlreadyAgreePolicy)
            {
                _windowPrivacy.zShow();
            }
            yield return new WaitUntil(() => _isAlreadyAgreePolicy);

            _goChecks.SetActive(true);

            if (!TrDeviceManager._isAlreadyCheckVersion)
            {
                TrDeviceManager.xInstance.zCheckVersion();
                yield return new WaitUntil(() => !TrDeviceManager._isCheckUpdate);
            }

            _targetValue = 0.3f;

            AuthManager.xInstance.zSetFirebase();
            yield return new WaitUntil(() => AuthManager.xInstance._isCheckAutoSignIn);
            _targetValue = 0.6f;

            if (AuthManager.xInstance._isAutoSignIn)
            {
                yield return new WaitUntil(() => AuthManager._isCompleteSignIn && StaminaManager.xInstance._isSetStamina);
                _targetValue = 1.1f;
                yield return new WaitUntil(() => _currValue >= 1);
                _isFirstLobby = false;
                _goChecks.SetActive(false);
            }
            else
            {
                _targetValue = 1.1f;
                yield return new WaitUntil(() => _currValue >= 1);
                _isFirstLobby = false;
                _goChecks.SetActive(false);
                AuthManager.xInstance.zIsSignIn(false);
                yield return new WaitUntil(() => AuthManager._isCompleteSignIn && StaminaManager.xInstance._isSetStamina);
            }
            GameManager._canBtnClick = true;
            yield return new WaitUntil(() => DatabaseManager._myDatas.nickName != "");
            AuthManager.xInstance.zIsSignIn(true);

        }
        else
        {
            _goChecks.SetActive(false);
            GameManager._canBtnClick = true;
            AuthManager.xInstance.zIsSignIn(true);
        }

        StartCoroutine(ySetPersonalRankBaord());
    }

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameManager.xInstance.zSetCamera();
        GameManager._canBtnClick = true;
        _numMenuComponenets = _trMenuComponents.Length;
        _tweenMoveMenuComponents = new Tween[_numMenuComponenets];
        for (int i = 0; i < _numMenuComponenets; i++)
        {
            _trMenuComponents[i].localPosition = _btnMenu.transform.localPosition;
        }
        _imgFade.alpha = 1;
        _cokeCarbonic.Play();
        _btnAgree.zInteractDisable();
        StartCoroutine(yPosText());
        StartCoroutine(yWaitCheckVersion());
    }

    void FixedUpdate()
    {
        if (_maxReloadCoolTimeRank > _currCoolTimeRank)
        {
            _currCoolTimeRank += Time.fixedDeltaTime;
        }

        if (_winSetting.transform.gameObject.activeSelf)
        {
            TrAudio_Music.xInstance.zzSetFlatVolume(_sliderBGM.value);
            TrAudio_SFX.xInstance.zzSetFlatVolume(_sliderSFX.value);
            TrAudio_UI.xInstance.zzSetFlatVolume(_sliderSFX.value);
        }

        if (_isFirstLobby)
        {
            if (_currValue <= _targetValue)
            {
                _currValue += Time.fixedDeltaTime;
                _imgCheckFill.fillAmount = _currValue;
            }
        }
    }


#if PLATFORM_ANDROID
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _windowQuit.zShow();
    }
#endif
}
