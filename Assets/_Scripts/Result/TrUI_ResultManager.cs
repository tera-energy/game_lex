using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class TrUI_ResultManager : MonoBehaviour
{
    static TrUI_ResultManager _instance;
    [SerializeField] TextMeshProUGUI _scoreTxt;
    [SerializeField] TextMeshProUGUI _burgerTxt;
    [SerializeField] TextMeshProUGUI _ribText;
    [SerializeField] TextMeshProUGUI _txtRetryButton; // Battle 모드에서 "재매칭"으로 변경
    [SerializeField] CanvasGroup _imgFade;
    [SerializeField] RectTransform[] _rtCookies;
    [SerializeField] Sprite _srBurnCookie;
    [SerializeField] Sprite _srTwinkleCookie;
    [SerializeField] ParticleSystem[] _burnCookie;
    //[SerializeField] ParticleSystem[] _twinkleCookie;
    [SerializeField] ParticleSystem[] _particle;
    [SerializeField] ParticleSystem _dust;
    [SerializeField] ParticleSystem[] _fireCracker;
    [SerializeField] RectTransform _newRecord;
    [SerializeField] RectTransform _bestScore;
    [SerializeField] Animator[] _bestScoreAnim;
    [SerializeField] ParticleSystem _firstPar;
    [SerializeField] AudioClip _acGood;
    [SerializeField] AudioClip _acBad;
    [SerializeField] AudioClip _acGreat;
    [SerializeField] GameObject _goFireCracker;
    [SerializeField] Vector2 _fireInitPos;
    [SerializeField] Vector2 _fireMinRange;
    [SerializeField] Vector2 _fireMaxRange;
    bool _isSetScoreCom = false;
    bool _isSkipScoreEffect = false;
    bool _isAction = false;

    TrUI_ResultDollar[] _arrDollar;
    [SerializeField] int _maxIndex;
    [SerializeField] TrUI_ResultDollar _infoDollar;
    [SerializeField] GameObject _parentDollar;
    float _dollarOriPosY = 900f;
    float _dollarPosY = -750f;
    [SerializeField] AudioClip _acBurger;

    [SerializeField] RectTransform[] _rtHamburger;

    [Header("경쟁 모드 전용")]
    [SerializeField] GameObject          _goBattleScore;  // 경쟁 전용 패널 (기본 비활성)
    [SerializeField] TextMeshProUGUI     _txtMyCount;     // 내 햄버거 수
    [SerializeField] TextMeshProUGUI     _txtOppCount;    // 상대 햄버거 수
    [SerializeField] GameObject          _goRexWin;       // WIN 시 활성화 (Rex)
    [SerializeField] GameObject          _goRexLose;      // LOSE 시 활성화 (Rex_Sad)

    public static TrUI_ResultManager xInstance { get { return _instance; } }

    public void xBtnExit()
    {
        if (_isAction) return;
        _isAction = true;
        TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();
        _imgFade.DOFade(1, 0.5f).OnComplete(() => SceneManager.LoadScene(TrProjectSettings.strLOBBY));

    }
    public void xBtnRetry()
    {
        if (_isAction) return;
        _isAction = true;
        if (GameManager._type == TT.enumGameType.Train)
            _imgFade.DOFade(1, 0.5f).OnComplete(() => GameManager.xInstance.zSetPuzzleGame());
        else if (GameManager._type == TT.enumGameType.Challenge)
            StartCoroutine(StaminaManager.xInstance.zCheckStamina(() =>
            _imgFade.DOFade(1, 0.5f).OnComplete(() =>
            GameManager.xInstance.zSetPuzzleGame())));
        else if (GameManager._type == TT.enumGameType.Battle)
        {
            TrMatchingSession.PendingRematch = true;
            _imgFade.DOFade(1, 0.5f).OnComplete(() =>
                SceneManager.LoadScene(TrProjectSettings.strLOBBY));
        }
    }
    void ySetInfoDollar(int num)
    {
        float randX = Random.Range(-900f, 900f);
        int roate = Random.Range(0, 3);
        TrUI_ResultDollar dollar = _arrDollar[num];

        dollar.transform.localPosition = new Vector3(randX, _dollarOriPosY, 0f);
        
        
        dollar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (roate == 1)
            dollar.transform.rotation = Quaternion.Euler(0f, 0f, 20f);
        else if (roate == 2)
            dollar.transform.rotation = Quaternion.Euler(0f, 0f, -20f);
        dollar.transform.DOLocalMove(new Vector3(randX, _dollarPosY, 0f), 4f).SetEase(Ease.Linear).OnComplete(() => dollar.gameObject.SetActive(false));
        
        dollar.gameObject.SetActive(true);
        _arrDollar[num] = dollar;
    }
    void yDollarInstantiate()
    {
        _arrDollar = new TrUI_ResultDollar[_maxIndex];
        for (int i = 0; i < _maxIndex; i++)
        {
            TrUI_ResultDollar setDollar = Instantiate(_infoDollar);
            setDollar.transform.SetParent(_parentDollar.transform);
            _arrDollar[i] = setDollar;
            _arrDollar[i].gameObject.SetActive(false);                       
        }
        _infoDollar.gameObject.SetActive(false);
    }
    IEnumerator ySetInitDollar()
    {

        for (int i = 0; i < _maxIndex; i++)
        {
            ySetInfoDollar(i);
            yield return TT.WaitForSeconds(0.6f);          
        }
    }

        IEnumerator yEffectIncreaseScore(TextMeshProUGUI text, float score, bool isScore){
        float maxScore = score;
        float currScore = 0f;
        float speed = maxScore > 0 ? maxScore : 1f;
        int soundScore = 1;
        while (currScore < maxScore){
            if (_isSkipScoreEffect)
            {
                text.text = ((int)maxScore).ToString();
                break;
            }
            currScore += Time.deltaTime * speed;
            if (isScore && currScore >= soundScore){
                while (currScore > soundScore)
                    soundScore++;

                if (speed > 20){
                    if ((int)speed % 3 == 0)
                        TrAudio_SFX.xInstance.zzPlay_AnimalsBomb();
                }
                else
                    TrAudio_SFX.xInstance.zzPlay_AnimalsBomb();
            }
            text.text = ((int)currScore).ToString();
            speed = (maxScore - currScore);
            if (speed < 1)
                speed = 1;
            yield return null;
        }
        text.text = ((int)maxScore).ToString();

        if (isScore){
            yield return TT.WaitForSeconds(0.5f);
            _isSetScoreCom = true;
        }
    }

 
    IEnumerator yStarsEffect(int num){
        yield return TT.WaitForSeconds(1f);
        
        Vector3 target = new Vector3(1.3f, 1.3f, 1.3f);
        Vector3 origin = new Vector3(1f, 1f, 1f);
        while (true){
            for(int i=2; i >= num; i--)
            {
                int captured = i;
                _rtHamburger[captured].DOScale(target, 0.25f).OnComplete(()=> _rtHamburger[captured].DOScale(origin, 0.25f));
                yield return TT.WaitForSeconds(0.75f);
            }

            int randWaitTime = Random.Range(1, 5);
            yield return TT.WaitForSeconds(randWaitTime);
        }
    }

    IEnumerator ySetGameDatas()
    {
        yield return new WaitUntil(() => TrNetworkManager.zGetIsConnectNetwork());
        int correctNum = GameManager.xInstance._correctNum;
        
        int score = GameManager._score;
        int rank = -1;
        if (GameManager._type == TT.enumGameType.Challenge)
        {
            if (DatabaseManager._myDatas != null)
            {
                Debug.Log($"[Ranking] _myDatas OK. score={score}, currentMaxScore={DatabaseManager._myDatas.maxScore}");
                if (score >= DatabaseManager._myDatas.maxScore)
                {
                    DatabaseManager._myDatas.maxScore = score;
                    yield return StartCoroutine(DatabaseManager.xInstance.zSetMaxScore());
                    Debug.Log($"[Ranking] zSetMaxScore called. score={score}");
                }
                else
                {
                    Debug.Log($"[Ranking] score({score}) < maxScore({DatabaseManager._myDatas.maxScore}), skip update.");
                }
            }
            else
            {
                Debug.LogWarning("[Ranking] _myDatas is NULL — max_score not updated!");
            }

            bool isChangeMyScore = false;
            if (DatabaseManager._liMyScores == null || DatabaseManager._liMyScores.Count == 0)
            {
                isChangeMyScore = true;
                DatabaseManager._liMyScores = new List<int>();
                DatabaseManager._liMyScores.Add(score);
            }
            else
            {
                for (int i = 0; i < DatabaseManager._liMyScores.Count; i++)
                {
                    if (i >= 5) break;

                    // -1은 빈 슬롯 → 이 자리에 새 점수 삽입
                    if (DatabaseManager._liMyScores[i] == -1)
                    {
                        isChangeMyScore = true;
                        DatabaseManager._liMyScores[i] = score;
                        break;
                    }

                    if (DatabaseManager._liMyScores[i] == score) break;

                    if (DatabaseManager._liMyScores[i] < score)
                    {
                        isChangeMyScore = true;
                        DatabaseManager._liMyScores.Insert(i, score);
                        break;
                    }
                }

                if (DatabaseManager._liMyScores.Count >= 5)
                    DatabaseManager._liMyScores.RemoveAt(5);
            }

            for (int i = DatabaseManager._liMyScores.Count; i < 5; i++)
            {
                if (!isChangeMyScore) isChangeMyScore = true;
                DatabaseManager._liMyScores.Add(-1);
            }

            if (isChangeMyScore)
            {
                yield return StartCoroutine(DatabaseManager.xInstance.zSetMyScores());

            }
        }

        GameManager._canBtnClick = true;

        _imgFade.DOFade(0, 1f);
        yield return new WaitUntil(() => _imgFade.alpha <= 0.01f);

        _scoreTxt.text = score.ToString();
        _burgerTxt.text = correctNum.ToString();
        _isSetScoreCom = true;
        Color color = Color.white;
        int burnCookie = 1;

        bool isNewRecord = rank == -1 ? false : true;
        bool isBestScore = rank == 0 ? true : false;
        if (isNewRecord)
        {
            if (isBestScore)
            {
                _bestScore.gameObject.SetActive(true);
            }
            else
                _newRecord.gameObject.SetActive(true);
            TrAudio_SFX.xInstance.zzPlayNewScore(0.1f);
            _dust.Play();
            yield return TT.WaitForSeconds(1f);
        }

        // Battle 모드: 카운트업 완료 후 WIN/LOSE + 햄버거 동시 등장
        if (GameManager._type == TT.enumGameType.Battle)
        {
            bool win = GameManager._battleDidIWin;
            Color battleColor;
            ColorUtility.TryParseHtmlString(win ? "#FFD700" : "#AAAAAA", out battleColor);
            _ribText.text  = win ? "WIN!" : "LOSE...";
            _ribText.color = battleColor;
            _ribText.gameObject.SetActive(true);
            _ribText.transform.DOPunchScale(Vector3.one * 0.35f, 0.5f, 7, 0.4f);

            // 경쟁 전용 패널: 내 햄버거 수 / 상대 햄버거 수 표시
            if (_txtMyCount  != null)
                _txtMyCount.text  = correctNum.ToString();
            else
                Debug.LogWarning("[ResultManager] _txtMyCount가 Inspector에 연결되지 않음");

            if (_txtOppCount != null)
                _txtOppCount.text = GameManager._battleOppCorrect.ToString();
            else
                Debug.LogWarning("[ResultManager] _txtOppCount가 Inspector에 연결되지 않음");

            TrAudio_SFX.xInstance.zPlaySFX(_acBurger);
            _rtHamburger[0].transform.localPosition = new Vector3(4f, 322f, 0f);
            _rtHamburger[0].gameObject.SetActive(true);
            _rtHamburger[0].transform.localScale = new Vector3(0f, 0f, 0f);
            _rtHamburger[0].transform.DOScale(new Vector3(1f, 1f, 1f), 1f);
            _burnCookie[0].Play();

            StartCoroutine(yStarsEffect(1));
            yield return TT.WaitForSeconds(10f);
            yield break;
        }

        TrAudio_SFX.xInstance.zPlaySFX(_acBurger);
        if (score <= 100)
        {
            burnCookie = 2;
            _rtHamburger[0].transform.localPosition = new Vector3(4f, 322f, 0f);
            _rtHamburger[0].gameObject.SetActive(true);
            _rtHamburger[0].transform.localScale = new Vector3(0f, 0f, 0f);
            _rtHamburger[0].transform.DOScale(new Vector3(1f, 1f, 1f), 1f);
            _burnCookie[0].Play();
            _ribText.text = "BAD";
            ColorUtility.TryParseHtmlString("#FF6666", out color);
            TrAudio_UI.xInstance.zzPlay_WangWaWang(0f);
            TrAudio_Music.xInstance.zzPlayMain(1.7f, _acBad);
        }
        else if (score > 100 && score <= 300)
        {
            burnCookie = 2;

            for (int i = 0; i < burnCookie; i++)
            {
                _rtHamburger[0].transform.localPosition = new Vector3(60f, 315f, 0f);
                _rtHamburger[1].transform.localPosition = new Vector3(-55f, 315f, 0f);
                _rtHamburger[i].gameObject.SetActive(true);
                _rtHamburger[i].transform.localScale = new Vector3(0f, 0f, 0f);
                _rtHamburger[i].transform.DOScale(new Vector3(1f, 1f, 1f), 1f);
                _burnCookie[i].Play();
            }

            _ribText.text = "GOOD!";
            ColorUtility.TryParseHtmlString("#ffffff", out color);
            TrAudio_UI.xInstance.zzPlay_PangPang(1.2f);
            TrAudio_Music.xInstance.zzPlayMain(3f, _acGood);
            _firstPar.Play();

            yield return TT.WaitForSeconds(1f);
            for (int i = 0; i < _particle.Length; i++)
            {
                _particle[i].Play();
            }
        }
        else if (score > 300)
        {
            burnCookie = 3;
            for (int i = 0; i < burnCookie; i++)
            {
                _rtHamburger[i].gameObject.SetActive(true);
                _rtHamburger[i].transform.localScale = new Vector3(0f, 0f, 0f);
                _rtHamburger[i].transform.DOScale(new Vector3(1f, 1f, 1f), 1f);
                _burnCookie[i].Play();
            }
            _ribText.text = "GREAT!!";
            ColorUtility.TryParseHtmlString("#fff04e", out color);
            _ribText.color = TT.zSetColor(TT.enumTrRainbowColor.PURPLE);
            TrAudio_UI.xInstance.zzPlay_GreatSound(0.5f);

            TrAudio_Music.xInstance.zzPlayMain(3.8f, _acGreat);
        }

        _ribText.color = color;
        _ribText.gameObject.SetActive(true);

        StartCoroutine(yStarsEffect(burnCookie));
        yield return TT.WaitForSeconds(10f);
        
    }
    void Awake(){
        if (_instance == null){
            _instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start(){
        GameManager._canBtnClick = false;
        GameManager.xInstance.zSetCamera();
        _scoreTxt.text = "0";
        _burgerTxt.text = "0";
        _ribText.gameObject.SetActive(false);
        _imgFade.alpha = 1;

        if (GameManager._type == TT.enumGameType.Battle && _txtRetryButton != null)
            _txtRetryButton.text = "재매칭";

        // 경쟁 전용 패널: Battle 모드일 때만 표시
        if (_goBattleScore != null)
            _goBattleScore.SetActive(GameManager._type == TT.enumGameType.Battle);

        // Rex 오브젝트 ON/OFF: Battle 모드 WIN/LOSE 에 따라
        if (GameManager._type == TT.enumGameType.Battle)
        {
            bool win = GameManager._battleDidIWin;
            if (_goRexWin  != null) _goRexWin.SetActive(win);
            if (_goRexLose != null) _goRexLose.SetActive(!win);

            // 카운트 텍스트: 데이터 로딩 완료 전까지 "-" 표시 (빈 텍스트 노출 방지)
            if (_txtMyCount  != null) _txtMyCount.text  = "-";
            if (_txtOppCount != null) _txtOppCount.text = "-";
        }

        yDollarInstantiate();
        StartCoroutine(ySetInitDollar());
        StartCoroutine(ySetGameDatas());
    }

#if PLATFORM_ANDROID
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isSkipScoreEffect = true;
        }
    }
#endif
}
