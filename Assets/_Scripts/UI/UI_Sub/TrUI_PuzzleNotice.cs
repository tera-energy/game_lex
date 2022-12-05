using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class TrUI_PuzzleNotice : MonoBehaviour
{
    static TrUI_PuzzleNotice _instance;
    public static TrUI_PuzzleNotice xInstance { get { return _instance; } }
    float _receiptPosY = 0f;
    [SerializeField] TextMeshProUGUI _txtNotice, _txtOrderRex, _txtGameOver;
    [SerializeField] GameObject _receipt, _failReceipt, _angryRexHead, _gameOverBlah;
    [SerializeField] public GameObject _orderRex;
    [HideInInspector] public bool _isGameOver = false;
    [HideInInspector] public bool _goPause = false;
    Coroutine _textCoroutine;
    int _count = 3;
    IEnumerator ySetText(string txt, float time)
    {
        _txtNotice.text = txt;

        yield return TT.WaitForSeconds(time);
        if (_txtNotice.text == txt)
        {
            _txtNotice.text = "";
            _orderRex.SetActive(false);
            _goPause = true;
        }
    }
    IEnumerator ySetCount(int fontSize, float time)
    {

        for (int count = 3; count >= 0; count--)
        {
            _count -= (int)Time.deltaTime;            
            _txtNotice.text = count.ToString();
            if (count == 0)
            {
                _txtNotice.text = "";
                _orderRex.SetActive(false);
                _goPause = true;
            }
            yield return TT.WaitForSeconds(1f);
        }
        
/*        if (_txtNotice.text == _count.ToString())
        {
            _txtNotice.text = "";
            _goMoomoo.SetActive(false);
        }*/

    }

    public void zSetWaitCount(int fontSize, float time)
    {

        _txtOrderRex.fontSize = fontSize;
        if (_textCoroutine != null)
        {
            StopCoroutine(_textCoroutine);
            _textCoroutine = StartCoroutine(ySetCount(fontSize, time));
        }
    }

    public void zSetNotice(string content, float time=0.5f)
    {
        _txtNotice.text = content;

        if (_textCoroutine != null)
            StopCoroutine(_textCoroutine);
        _textCoroutine = StartCoroutine(ySetText(content, time));
    }

    public void zSetNoticeWithRex(string content, int fontSize, float time)
    {


        
        if (_isGameOver == false)
        {
            _txtOrderRex.text = content;
            _txtOrderRex.fontSize = fontSize;
            _orderRex.SetActive(true);
            _receipt.transform.DOLocalMoveY(_receiptPosY, 1f);
        }else if (_isGameOver == true)
        {
            _receipt.SetActive(false);
            _failReceipt.SetActive(true);
            _txtGameOver.text = content;
            _txtGameOver.fontSize = fontSize;

            _angryRexHead.SetActive(true);
            _gameOverBlah.SetActive(true);
            _orderRex.SetActive(true);

            _failReceipt.transform.DOLocalMoveY(_receiptPosY, 1f);
            

        }
        
        TT.UtilDelayedFunc.zCreate(() => _orderRex.SetActive(false), time);
        
    }

    void Awake(){
        if (_instance == null){
            _instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }
}
