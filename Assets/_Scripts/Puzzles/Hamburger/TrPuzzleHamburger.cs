using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
class TrIngredientsAnswer
{
    public int _numSp;
}

public class TrPuzzleHamburger : TrPuzzleManager
{

    static TrPuzzleHamburger _instance;
    public static TrPuzzleHamburger xInstance { get { return _instance; } }

        [Tooltip("happy=0, sad=1")]
    [SerializeField] GameObject[] _goCharacterFaces;
    //[SerializeField] ParticleSystem _happyPar;


    Ingredient[] _arrViewIngredients;  //예시 버거 배열
    Ingredient[] _arrInputIngredients; //인풋 버거 배열
    [SerializeField] Ingredient _ingredientsView; //재료 정보
    [SerializeField] GameObject _wrong;
    [SerializeField] int _maxIndex; //프리팹 생성 수
    
    int _currInputIndex; //인풋 버거 배열 넘버, 재료 저장
    int _currSort; //레이어 우선순위
    int _numUseIngredient = 6; //사용할 버거 재료 수
    int _numViewIngredients; //나타날 재료 수, 난이도 조절

    [SerializeField] float _viewSetPosY; //버거 떨어질 위치
    [SerializeField] float _viewDropPosY; //뷰 버거 떨어질 위치
    float _inputIngredientsPosY = 0.35f; //인풋 버거 재료 간격
    float _viewIngredientsPosY = 0.35f; //뷰 버거 재료 간격 +
    float _ingredientsDropPosY = 10f; //버거 떨어질 위치
    [SerializeField] float _inputDropPosY; //인풋 버거 초기 위치
    //[SerializeField] float _maxInputCount; //입력 제한시간 3
    float _ingredientsDropTerm; //재료가 떨어지는 속도
    /*[SerializeField] Image _inputSlider; //인풋 제한시간 타이머 슬라이더 이미지
    [SerializeField] GameObject _inputSliderParent;
    [SerializeField] TextMeshProUGUI _inputTimerText;*/
    [SerializeField] GameObject _viewParent;
    [SerializeField] GameObject _inputParent;
    bool _isCompleteSetView = false; //뷰 버거 생성이 완료 됐는지
    [HideInInspector] public bool isCorrect = false;
    bool _inputTimerON = true;
    Coroutine _coViewInit; //뷰 버거 재료 초기화 코루틴
    [SerializeField] GameObject _takeOut;
    bool _isTimeTenseEffectExec;
    List<Tween> _setViewDTList = new List<Tween>();
    Tween _setViewDT;
    List<Tween> _inputDTList = new List<Tween>();
    Tween _inputDT;
    [HideInInspector] public bool _goTimeTense = true;
    int _viewNum = 0;
    float _viewBurgerSpeed;
    
    //표정변화
    /*IEnumerator yChangesCharacterFace(bool _isCheck){
        if (_isCheck == true){
            _goCharacterFaces[0].SetActive(true);
            _happyPar.gameObject.SetActive(true);
            _happyPar.Play();
            //TrAudio_SFX.xInstance.zzPlay_BurgerCorrect(0.1f);
        }
        else if (_isCheck == false){
            _goCharacterFaces[1].SetActive(true);
            //TrAudio_SFX.xInstance.zzPlay_BurgerWrong(0.1f);
        }        

        yield return TT.WaitForSeconds(0.5f);
        _goCharacterFaces[0].SetActive(false);
        _goCharacterFaces[1].SetActive(false);

    }*/
    IEnumerator yPlusViewNum()
    {
        yield return TT.WaitForSeconds(0.5f);
        _viewNum++;
    }

    //답 체크
    public bool yAnswerCheck(int num) {
        
        
        if (num == _arrViewIngredients[_currInputIndex-1]._indexId) {
            isCorrect = true;
        }
        else
        {
            isCorrect = false;
        }
        
        return isCorrect;


    }

    //인풋 버거 생성
    void yStackIngredient(int num){
        //Debug.Log(num);
        Ingredient Inputingredient = _arrInputIngredients[_currInputIndex];
        /*int randZ = Random.Range(0, 3);
        if (randZ == 1)
            Inputingredient.transform.eulerAngles = new Vector3(0f, 0f, 20f);
        else if (randZ == 2)
            Inputingredient.transform.eulerAngles = new Vector3(0f, 0f, -20f);*/


        float posY = _inputDropPosY;
        //Inputingredient.transform.DORotate(new Vector3(0f, 0f, 0f), 0.8f);
        /*_inputDT = Inputingredient.transform.DOLocalMoveY(posY, 0.3f).SetEase(Ease.Linear).OnComplete(()
               => Inputingredient.transform.DOLocalJump(new Vector3(0f, posY, 0f), 0.2f, 1, 0.5f));*/
        _inputDT = Inputingredient.transform.DOLocalMoveY(posY, _viewBurgerSpeed).SetEase(Ease.OutCirc);
        _inputDTList.Add(_inputDT);

        Inputingredient.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        Inputingredient.transform.localPosition = new Vector3(0f, _ingredientsDropPosY, 0);
        Inputingredient.gameObject.SetActive(true);
        Inputingredient.zSetIngredient(num, ++_currSort);
        
        _arrInputIngredients[_currInputIndex] = Inputingredient;
        _inputDropPosY += _inputIngredientsPosY;
        _currInputIndex++;
        
    }

    //인풋 버거 버튼 및 답 체크
    public void zInputIngredients(int num)
    {

        if (_currInputIndex > _numViewIngredients - 1)
        {
            yWrong();
            return;
        }
        if (_viewNum < _currInputIndex)
        {
            //Debug.Log("막 누르지 마말라");
            yWrong();
            return;
        }
        
        

        yStackIngredient(num);
        int PlusS = 0;


        if (yAnswerCheck(num)){
            if (_currInputIndex == _numViewIngredients - 1){

                switch (_numViewIngredients)
                {
                    case 3:
                        PlusS = 5;
                        _currGameTime = _currGameTime + 2.5f;
                        break;
                    case 4:
                        PlusS = 10;
                        _currGameTime = _currGameTime + 3f;
                        break;
                    case 5:
                        PlusS = 15;
                        _currGameTime = _currGameTime + 3.5f;
                        break;
                    case 6:
                        PlusS = 20;
                        _currGameTime = _currGameTime + 4f;
                        break;
                    case 7:
                        PlusS = 25;
                        _currGameTime = _currGameTime + 4.5f;
                        break;
                    case 8:
                        PlusS = 30;
                        _currGameTime = _currGameTime + 5f;
                        break;
                }

                zCorrect(false, PlusS);
                GameManager.xInstance._correctNum++;
                //Debug.Log(GameManager.xInstance._correctNum);
                StartCoroutine(yLastInputBurger());
                StartCoroutine(yViewDisappear());
                StartCoroutine(yCompleteInputBurger());

            }
            
        }
        else if(isCorrect == false)
        {
            yWrong();
        }

        
    }
    void yWrong()
    {
        if(_isOnVibrate)
            Handheld.Vibrate();
        TrUI_PuzzleHamburger.xInstance._btnON = false;
        //yEndGame(); 대전모드
        zWrong(false, -_numViewIngredients);
        _currGameTime = _currGameTime - 3f;
        TrAudio_SFX.xInstance.zzPlayBurgerWrong(0f);
        StartCoroutine(yX());
        StartCoroutine(yFailInputBurger());
    }

    IEnumerator yLastInputBurger()
    {
        TrUI_PuzzleHamburger.xInstance._btnON = false;
        yield return TT.WaitForSeconds(0.3f);
        yStackIngredient(1);
    }

    IEnumerator yX()
    {
        _wrong.SetActive(true);
        yield return TT.WaitForSeconds(0.2f);
        _wrong.SetActive(false);
        yield return TT.WaitForSeconds(0.2f);
        _wrong.SetActive(true);
        TrUI_PuzzleHamburger.xInstance.zEffectFLEX(1);
        yield return TT.WaitForSeconds(0.2f);
        _wrong.SetActive(false);
        yield return TT.WaitForSeconds(0.2f);
        _wrong.SetActive(true);
        yield return TT.WaitForSeconds(0.2f);
        _wrong.SetActive(false);


        
    }
    IEnumerator yViewDisappear()
    {
        yield return TT.WaitForSeconds(0.8f);
        ySizeUpDown();
        StartCoroutine(yTakeOut());
        yield return TT.WaitForSeconds(0.3f);
    }
    IEnumerator yTakeOut()
    {
        yield return TT.WaitForSeconds(0.3f);
        _takeOut.SetActive(true);
        yield return TT.WaitForSeconds(0.3f);
        Vector3 ori = new Vector3(1f, 1f, 1f);
        TrAudio_UI.xInstance.zzPlay_Correct(0.3f);
        _takeOut.transform.DOScale(ori, 0.5f).OnComplete(()=> _takeOut.SetActive(false));
        yield return TT.WaitForSeconds(0.5f);
        TrUI_PuzzleHamburger.xInstance.zEffectFLEX(0);
        
    }
    //인풋 버거를 다 쌓은 후
    IEnumerator yCompleteInputBurger(){
        isCorrect = true;
        yield return TT.WaitForSeconds(2.2f);
        
        foreach (Tween setViewDT in _setViewDTList)
        {
            if(setViewDT != null)
                setViewDT.Kill();
        }
        _setViewDTList.Clear();

        
        

        if (_coViewInit != null){
            StopCoroutine(_coViewInit);
        }

        yResetBurger();
        ySetNumIngredients();

        
        _coViewInit = StartCoroutine(yInitViewIngredients());
        
        TrUI_PuzzleHamburger.xInstance.zResetHurry();
    }
    IEnumerator yFailInputBurger()
    {
        isCorrect = false;
        yield return TT.WaitForSeconds(1f);
        

        


        //_takeOut.SetActive(false);
        if (_coViewInit != null)
        {
            StopCoroutine(_coViewInit);
        }

        
        yResetBurger();
        ySetNumIngredients();

        _coViewInit = StartCoroutine(yInitViewIngredients());
        TrUI_PuzzleHamburger.xInstance.zResetHurry();
    }
    //인풋 타임아웃 코루틴
    /*IEnumerator yInputCount()
    {
        
        float inputCount = _maxInputCount;
        while(inputCount >= 0){
            //Debug.Log((int)inputCount);
            inputCount -= Time.deltaTime;
            
            if (GameManager.xInstance._isGameStarted){     
                _inputTimerText.text = ((int)inputCount + 1).ToString();
                //인풋 타임아웃 슬라이더
                _inputSlider.fillAmount = inputCount / _maxInputCount;
            }
            else
                yield break;
            
            if (_inputSlider.fillAmount == 0) {
                _inputTimerText.text = "0";
            }
               
            
            yield return null;
        }
        yEndGame();
    }*/



    //입력 제한시간 코루틴 (3초 대기시간)
    /*IEnumerator ySetCount()
    {
        StartCoroutine(yInitViewIngredients());
        
        //yield return new WaitUntil(() => _isCompleteSetView);
        yield return TT.WaitForSeconds(0f);
        //GameManager.xInstance._isGameStarted = true;
    }*/
    void ySetNumIngredients()
    {
        int randNum = Random.Range(3, 9);
        _numViewIngredients = randNum;
    }

    void yViewDropSpeed()
    {
        if(0 <= _currScore && _currScore < 40)
        {
            _viewBurgerSpeed = 0.3f;
            _ingredientsDropTerm = 0.6f;
        }
        else if (40 <= _currScore && _currScore < 90)
        {
            _viewBurgerSpeed = 0.2f;
            _ingredientsDropTerm = 0.5f;
        }
        else if(90 <= _currScore && _currScore < 140)
        {
            _viewBurgerSpeed = 0.15f;
            _ingredientsDropTerm = 0.4f;
        }
        else if(140 <= _currScore && _currScore < 180)
        {
            _viewBurgerSpeed = 0.1f;
            _ingredientsDropTerm = 0.3f;
        }
        else if (180 <= _currScore)
        {
            _viewBurgerSpeed = 0.1f;
            _ingredientsDropTerm = 0.2f;
        }

    }

    void yResetBurger()
    {
        _inputParent.transform.localScale = new Vector3(1f, 1f, 1f);
        for (int i = 0; i < _numViewIngredients; i++)
        {
            _arrInputIngredients[i].gameObject.SetActive(false);
            _arrViewIngredients[i].gameObject.SetActive(false);
        }

        yViewDropSpeed();
        //재료 정보 리셋
        _currSort = 0;
        _currInputIndex = 0;
        _inputDropPosY = -0.76f;

        //인풋 재료 초기 아래 빵 생성
        yFirstInputBurger();

        _viewDropPosY = -0.76f;
        _viewNum = 1;
    }
    void yFirstInputBurger()
    {
        yStackIngredient(0);
    }


    //버거 사이즈 DOTOWEEN
    void ySizeUpDown()
    {
        Vector3 big = new Vector3(1.1f, 1.1f, 1.1f);
        Vector3 small = new Vector3(0f, 0f, 0f);
        _viewParent.transform.DOScale(big, 0.2f).OnComplete(() => _viewParent.transform.DOScale(small, 0.3f));
        _inputParent.transform.DOScale(big, 0.2f).OnComplete(() => _inputParent.transform.DOScale(small, 0.3f));
        TrAudio_SFX.xInstance.zzPlaySizeUpDown(0f);


    }

    //인풋 재료 생성
    void yInputIngredientsInstantiate()
    {
        _arrInputIngredients = new Ingredient[_maxIndex];

        for (int i = 0; i < _maxIndex; i++)
        {
            Ingredient inputBurgerIgre = Instantiate(_ingredientsView);
            inputBurgerIgre.transform.SetParent(_inputParent.transform);
            _arrInputIngredients[i] = inputBurgerIgre;
            _arrInputIngredients[i].gameObject.SetActive(false);
        }
        _ingredientsView.gameObject.SetActive(false);
    }

    //뷰 버거 재료 생성
    void ySetViewBurger(int num)
    {
        int randZ = Random.Range(0, 3);
        Ingredient ViewIngredient = _arrViewIngredients[num];
        int rand = Random.Range(2, _numUseIngredient);
        ViewIngredient.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        ViewIngredient.transform.localPosition = new Vector3(0f, _viewSetPosY, 0f);
        ViewIngredient.gameObject.SetActive(true);




        //내려올 때 비스듬히
        /*if (randZ == 1)
            ViewIngredient.transform.eulerAngles = new Vector3(0f, 0f, 20f);
        else if(randZ == 2)
            ViewIngredient.transform.eulerAngles = new Vector3(0f, 0f, -20f);*/

        float posY = _viewDropPosY;

        //ViewIngredient.transform.DORotate(new Vector3(0f, 0f, 0f), 0.8f);
        /*_setViewDT = ViewIngredient.transform.DOLocalMoveY(posY, 0.3f).SetEase(Ease.Linear).OnComplete(()
               => ViewIngredient.transform.DOLocalJump(new Vector3(0f, posY, 0f), 0.2f, 1, 0.5f));*/
        _setViewDT = ViewIngredient.transform.DOLocalMoveY(posY, _viewBurgerSpeed).SetEase(Ease.OutCirc);

        _setViewDTList.Add(_setViewDT);
        //내려옴
        //ViewIngredient.transform.DOLocalMoveY(_viewOriIngredientsPosY, 0.6f).SetEase(Ease.Linear);

        StartCoroutine(yPlusViewNum());


        if (num == 0){
            ViewIngredient.zSetIngredient(0, 0);
            
        }
        else if (num == _numViewIngredients -1){
            ViewIngredient.zSetIngredient(1, _currSort + 10);
            /*if(_inputTimerON == true)
                _inputSliderParent.SetActive(true);      */
            TrUI_PuzzleHamburger.xInstance._btnON = true;
        }
        else{
            ViewIngredient.zSetIngredient(rand, ++_currSort);
        }
        _arrViewIngredients[num] = ViewIngredient;

        _viewDropPosY += _viewIngredientsPosY;

        
    }
    //뷰 버거 재료 초기화

    IEnumerator yInitViewIngredients()
    {
        
        _viewParent.transform.localScale = new Vector3(1f, 1f, 1f);
        for (int i = 0; i < _numViewIngredients; i++){
            ySetViewBurger(i);
            yield return TT.WaitForSeconds(_ingredientsDropTerm);
        }

        
    }

    //뷰 버거 재료 생성
    void ySetIngredientsInstantiate()
    {
        _arrViewIngredients = new Ingredient[_maxIndex];
        
        for (int i = 0; i < _maxIndex; i++)
        {
            Ingredient viewBurgerIgre = Instantiate(_ingredientsView);
            viewBurgerIgre.transform.SetParent(_viewParent.transform);
            _arrViewIngredients[i] = viewBurgerIgre;
            _arrViewIngredients[i].gameObject.SetActive(false);
        }
        _ingredientsView.gameObject.SetActive(false);        
    }
    IEnumerator yExecTicTok()
    {
        while (_currGameTime > 0 && _currGameTime < 20)
        {
            TrAudio_UI.xInstance.zzPlay_TimerTicTok(0.1f);
            //TrAudio_UI.xInstance.zzPlay_Klaxon(0.1f);
            yield return TT.WaitForSeconds(0.5f);
        }
    }
    public void zQuitApplication()
    {
#if PLATFORM_ANDROID
        Application.Quit();
#endif
    }
    void yStartSet()
    {
        _numViewIngredients = 6;
        _viewBurgerSpeed = 0.3f;
        _ingredientsDropTerm = 0.6f;
        ySetIngredientsInstantiate();
        TrUI_PuzzleHamburger.xInstance.zHurryUPInstantiate();
        yInputIngredientsInstantiate();
        zInputIngredients(0);

        _coViewInit = StartCoroutine(yInitViewIngredients());
    }
    protected override void yBeforeReadyGame(){
        base.yBeforeReadyGame();
        TrAudio_Music.xInstance.zzPlayMain(0.25f);
        _isThridChallengeSame = true;

    }

    protected override void yAfterReadyGame()
    {
        base.yAfterReadyGame();
        TrUI_PuzzleNotice.xInstance._goPause = true;
        yStartSet();
        TrUI_PuzzleHamburger.xInstance._btnON = false;
        


        GameManager.xInstance._isGameStarted = true;
    }
    protected override void Update()
    {
        base.Update();
        if (!GameManager.xInstance._isGameStarted) return;

        if (_currGameTime <= _maxGameTime * 0.25f && !_isTimeTenseEffectExec)
        {
            _isTimeTenseEffectExec = true;
            TrUI_PuzzleHamburger.xInstance.zHurryUp();
            TrUI_PuzzleHamburger.xInstance.zSetTimerTenseEffect();
            StartCoroutine(yExecTicTok());
        }
        
    }
    void Awake(){
        
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
        
    
}