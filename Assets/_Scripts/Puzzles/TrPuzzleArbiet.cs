using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public struct TrArbeit
{
    public GameObject _currCustomer; //고객
    public int _customerMoney; //고객 돈
    public int _productPrice; //가격
    public GameObject _currProduct; //상품
    public int _rndPrice; //랜덤한 가격
    public int _realPrice; //올바른 가격
    public int _saveRndPrice; //랜덤한 가격 저장
}

public class TrPuzzleArbiet : MonoBehaviour
{

    [SerializeField] TT.enumDifficultyLevel _difficulty = TT.enumDifficultyLevel.Normal;
    public enum enumButtonInputOX { O, X }


    static TrPuzzleArbiet _instance;

    TrArbeit _info;
    [SerializeField] int[] currCusMoney;
    [SerializeField] Image Pos;
    [SerializeField] Image CustomerMoney;
    [SerializeField] Image[] ProductImage;

    [SerializeField] GameObject[] _cusPrefab;
    [SerializeField] GameObject[] _proPrefab;

    [SerializeField] TextMeshProUGUI _currCusMoney;
    [SerializeField] TextMeshProUGUI _currPosMoney;
    [SerializeField] TextMeshProUGUI[] ProductPriceText;

    [SerializeField] int[] _proPrice;

    [SerializeField] int _startTime = 3;
    [SerializeField] int _maxGameTime = 15;
    float _currTimer;
    float _currGameTime;
    int _currStartTime;
    int _currScore = 0;
    int productNum;
    int customerMoneyNum;

    bool _isGameStarted = false;

    public static TrPuzzleArbiet xInstance { get { return _instance; } }


    void yGameTimer()
    {
        if (_isGameStarted && _currGameTime >= 0)
        {

            _currGameTime -= Time.deltaTime;
            TrUI_PuzzleTimer.xInstance.zUpdateTimerBar(_maxGameTime, _currGameTime);
        }
    }

    IEnumerator yProcReadyGame()
    {
        TrUI_PuzzleArbeit.xInstance.zSetNotice(_currStartTime);

        while (_currStartTime > 0)
        {
            yield return new WaitForSeconds(1f);
            TrUI_PuzzleArbeit.xInstance.zSetNotice(--_currStartTime);
        }
        _isGameStarted = true;
        zSetGame();

    }
    //고객 생성
    void ySetCustomer()
    {
        
        int rndCustomer = Random.Range(0, _cusPrefab.Length);
        _info._currCustomer = Instantiate(_cusPrefab[rndCustomer]);
        _info._currCustomer.transform.localPosition = new Vector3(-5f, 0.5f, 0);
        _info._currCustomer.transform.localScale = new Vector3(0.4f, 0.4f, 0);
    }

    //상품 생성 및 상품 가격
    void ySetProduct()
    {

        //상품 생성
        int rndProduct = Random.Range(0, productNum);
        _info._currProduct = Instantiate(_proPrefab[rndProduct]);
        _info._currProduct.transform.localPosition = new Vector3(-5f, -1.5f, 0);
        _info._currProduct.transform.localScale = new Vector3(0.2f, 0.2f, 0);
        _info._productPrice = _proPrice[rndProduct];
        
        //고객의 돈 생성 및 상품의 가격 계산
        int rndCusMoney = Random.Range(0, customerMoneyNum);
        _info._customerMoney = currCusMoney[rndCusMoney];
        _currCusMoney.text = _info._customerMoney.ToString();

        //포스기의 값에 랜덤하게 뺄 가격
        int rndProPrice = Random.Range(0, _proPrice.Length);
        _info._rndPrice = _proPrice[rndProPrice];
        
        //랜덤하게 뺀 가격을 포스기에 입력
        _currPosMoney.text = (_info._customerMoney - _info._rndPrice).ToString();
        _info._saveRndPrice = _info._customerMoney - _info._rndPrice; //값 저장
        
        //올바르게 뺀 값
         _info._realPrice = _info._customerMoney - _info._productPrice;
    }

    public void zButtonInput(enumButtonInputOX input)
    {
        if(input == enumButtonInputOX.O)
        {
            
            if (_info._realPrice == _info._saveRndPrice)
            {

                _currScore += 5;
                if (_currScore <= 0) _currScore = 0;
                TrUI_PuzzleArbeit.xInstance.zSetNotice(-3);
                TrUI_PuzzleArbeit.xInstance.zSetScore(_currScore);
            }
            if (_info._realPrice != _info._saveRndPrice)
            {
                
                TrUI_PuzzleArbeit.xInstance.zSetNotice(-2);
                TrUI_PuzzleArbeit.xInstance.zSetScore(--_currScore);
            }
        }
        //점수 수정
        else if (input == enumButtonInputOX.X)
        {
            if (_info._realPrice == _info._saveRndPrice)
            {

                _currScore -= 5;
                if (_currScore <= 0) _currScore = 0;
                TrUI_PuzzleArbeit.xInstance.zSetNotice(-2);
                TrUI_PuzzleArbeit.xInstance.zSetScore(_currScore);

            }

            if(_info._realPrice != _info._saveRndPrice)
            {
                TrUI_PuzzleArbeit.xInstance.zSetNotice(-3);
                TrUI_PuzzleArbeit.xInstance.zSetScore(++_currScore);
            }
        }
        StartCoroutine(yResetGame());
    }
    
    IEnumerator yResetGame()
    {
        
        yield return new WaitForSeconds(0.1f);
        //포스기 텍스트 초기화
        _currPosMoney.text = "";
        //고객 프리팹과 텍스트 초기화
       _info._currCustomer.gameObject.SetActive(false);
        _currCusMoney.text = "";
        //상품 초기화
        _info._currProduct.gameObject.SetActive(false);

        zSetGame();
    }

    void zSetGame()
    {
        Pos.gameObject.SetActive(true);
        CustomerMoney.gameObject.SetActive(true);

        ySetCustomer();
        ySetProduct();        
                         
    }

    void yOnDifficultyChange(int nanido)
    {
        switch (nanido)
        {
            case 1:
                //상품 2개, 고객 보유 최대 돈 300
                ProductImage[2].gameObject.SetActive(false);
                ProductPriceText[2].gameObject.SetActive(false);
                ProductImage[3].gameObject.SetActive(false);
                ProductPriceText[3].gameObject.SetActive(false);
                
                productNum = 2;
                customerMoneyNum = 1;
                
                break;

            case 2:
                //상품 2개, 고객 보유 최대 돈 500
                ProductImage[2].gameObject.SetActive(false);
                ProductPriceText[2].gameObject.SetActive(false);
                ProductImage[3].gameObject.SetActive(false);
                ProductPriceText[3].gameObject.SetActive(false);

                productNum = 2;
                customerMoneyNum = 2;

                break;

            case 3:
                //상품 3개 고객 보유 최대 돈 700
                ProductImage[2].gameObject.SetActive(true);
                ProductPriceText[2].gameObject.SetActive(true);

                productNum = 3;
                customerMoneyNum = 3;

                break;
            case 4:
                //상품 4개, 고객 보유 최대 돈 700
                ProductImage[3].gameObject.SetActive(true);
                ProductPriceText[3].gameObject.SetActive(true);

                productNum = 4;
                customerMoneyNum = 3;

                break;

            case 5:
                //상품 4개, 고객 보유 최대 돈 1000
                ProductImage[3].gameObject.SetActive(true);
                ProductPriceText[3].gameObject.SetActive(true);

                productNum = 4;
                customerMoneyNum = 5;
                break;

        }
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
    // Start is called before the first frame update
    void Start()
    {

        productNum = 3;
        customerMoneyNum = 3;
        TrUI_DifficultyMenu.xInstance.zSetMenuIndex((int)_difficulty - 1);
        TrUI_DifficultyMenu.xInstance._onDifficultySelected += yOnDifficultyChange;
        StartCoroutine(yProcReadyGame());
    }

    // Update is called once per frame
    void Update()
    {
        yGameTimer();


    }
}

