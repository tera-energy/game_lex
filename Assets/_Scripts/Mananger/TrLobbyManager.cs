using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
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
    [SerializeField] Color _colorHighlight;
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
    [SerializeField] GameObject _goNoScoreMyText;
    [SerializeField] GameObject _goNoScoreTotalText;

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
    bool _isPendingRematch;
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

    public void zOnClickBattleStart()
    {
        TrUI_MatchingPopup.xInstance.zShow();
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
        _goNoScoreMyText?.SetActive(false);

        if (DatabaseManager._liMyScores == null)
            yield return StartCoroutine(DatabaseManager.xInstance.zGetDataMyScores());

        TrPlayRankListBox[] rankUIs = _trPersonalRanksParent.GetComponentsInChildren<TrPlayRankListBox>();

        List<int> rankList = DatabaseManager._liMyScores;
        int listCount = rankList == null ? 0 : rankList.Count;
        string nickname = DatabaseManager._myDatas.nickName;

        // -1이 아닌 항목만 실제 플레이 점수
        int validCount = 0;
        if (rankList != null)
            for (int i = 0; i < listCount; i++)
                if (rankList[i] != -1) validCount++;
                else break;

        bool hasPlayed = validCount > 0;
        _goNoScoreMyText?.SetActive(!hasPlayed);
        _trPersonalRanksParent.gameObject.SetActive(hasPlayed);

        if (hasPlayed)
        {
            // 실제 플레이 횟수만큼만 표시, 나머지 슬롯은 비활성화
            for (int i = 0; i < 5; i++)
            {
                TrPlayRankListBox uis = rankUIs[i];
                if (i < validCount)
                {
                    uis.gameObject.SetActive(true);
                    uis._txtScore.text = string.Format("{0}P", rankList[i].ToString());
                    uis._txtName.text = nickname;
                }
                else
                {
                    uis.gameObject.SetActive(false);
                }
            }
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
        _goNoScoreTotalText?.SetActive(false);

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
            while (index < _numRanks && index < listCount)
            {
                var user = li[index];
                string name = user.nickname;
                string score = user.maxScore.ToString();

                Transform box = _rankBoxsTotal[index].transform;
                box.gameObject.SetActive(true);
                box.SetParent(_trTotalRanksParent);
                box.localScale = new Vector3(1f, 1f, 1f);
                TrPlayRankListBox listBox = box.GetComponent<TrPlayRankListBox>();
                listBox._txtName.text = name;
                listBox._txtScore.text = string.Format("{0}P", score);
                listBox._imgValueRanking.gameObject.SetActive(true);
                listBox._txtValueRanking.text = "";
                if (rank == 0) listBox._imgValueRanking.sprite = _spRanks[rank];
                else if (rank == 1) listBox._imgValueRanking.sprite = _spRanks[rank];
                else if (rank == 2) listBox._imgValueRanking.sprite = _spRanks[rank];
                else
                {
                    listBox._imgValueRanking.gameObject.SetActive(false);
                    listBox._txtValueRanking.text = (rank + 1).ToString();
                }

                // 내 항목 하이라이트
                if (name == ownName)
                {
                    yourRanking = rank;
                    // if (listBox._imgBg != null)
                    //     listBox._imgBg.color = _colorHighlight;
                }

                rank++;
                index++;
            }

            // 하단 고정 박스 — 항상 표시
            _rankBoxOwn.gameObject.SetActive(true);
            _rankBoxOwn._txtName.text = ownName;
            _rankBoxOwn._txtScore.text = string.Format("{0}P", DatabaseManager._myDatas.maxScore.ToString());

            Image imgRank = _rankBoxOwn._imgValueRanking;
            TextMeshProUGUI txtRank = _rankBoxOwn._txtValueRanking;

            if (yourRanking >= 0)
            {
                // 리스트 안에 있음 → 정확한 순위 표시
                if (yourRanking < 3)
                {
                    imgRank.gameObject.SetActive(true);
                    imgRank.sprite = _spRanks[yourRanking];
                    txtRank.text = "";
                }
                else
                {
                    imgRank.gameObject.SetActive(false);
                    txtRank.gameObject.SetActive(true);
                    txtRank.text = (yourRanking + 1).ToString();
                    txtRank.fontSize = 35;
                }
            }
            else
            {
                // 리스트 밖
                imgRank.gameObject.SetActive(false);
                txtRank.gameObject.SetActive(true);
                txtRank.text = "-";
                txtRank.fontSize = 35;
            }
        }
        else
        {
            _rankBoxOwn.gameObject.SetActive(false);
            _goNoScoreTotalText?.SetActive(true);
        }

        _goWaiting.SetActive(false);
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

            AuthManager.xInstance.zInitialize();
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
        _isPendingRematch = TrMatchingSession.PendingRematch;
        if (_isPendingRematch) TrMatchingSession.PendingRematch = false;
        _cokeCarbonic.Play();
        _btnAgree.zInteractDisable();
        StartCoroutine(yPosText());
        StartCoroutine(yWaitCheckVersion());
        if (_isPendingRematch)
            StartCoroutine(yAutoRematch());
    }

    IEnumerator yAutoRematch()
    {
        if (_imgFade == null)
        {
            Debug.LogError("[Lobby] _imgFade가 null — yAutoRematch 실행 불가");
            yield break;
        }

        // 로비 페이드 완료 대기 (최대 5초, 백그라운드 전환 대비 realtimeSinceStartup 사용)
        float deadline = Time.realtimeSinceStartup + 5f;
        while (_imgFade.alpha > 0.01f && Time.realtimeSinceStartup < deadline)
            yield return null;

        // 타임아웃으로 페이드 미완료인 경우 로비 명시적 복구 (팝업 표시 성공 여부와 무관하게)
        if (_imgFade.alpha > 0.01f)
            _imgFade.DOFade(0, 0.5f);

        TrUI_MatchingPopup popup = TrUI_MatchingPopup.xInstance
                                   ?? FindObjectOfType<TrUI_MatchingPopup>();

        if (popup != null)
        {
            popup.zShow();
        }
        else
        {
            Debug.LogError("[Lobby] TrUI_MatchingPopup 인스턴스를 찾을 수 없음 — 재매칭 팝업 표시 실패");
        }
    }

    /// <summary>
    /// 재매칭 취소 시 숨겨뒀던 로비를 페이드 인. TrUI_MatchingPopup에서 호출.
    /// </summary>
    public void zShowLobby()
    {
        _isPendingRematch = false;
        _imgFade.DOFade(0, 0.8f);
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
