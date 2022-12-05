using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
public class TrUI_PuzzleHamburger : TrUI_PuzzleManager
{
    //[SerializeField] ParticleSystem[] _btnParticles;

    bool _isPlay = true;
    //TrUI_HamburgerFLEX[] _flex;
    TrUI_HamburgerFLEX _flex;
    
    TrUI_HamburgerHurry[] _arrHuryyUP;
    [SerializeField] TrUI_HamburgerHurry _infoHurry;
    [SerializeField] int _maxIndex;
    int _currIndex=0;
    [SerializeField] GameObject _parentHurry;
    static TrUI_PuzzleHamburger _instance;
    public static TrUI_PuzzleHamburger xInstance {  get { return _instance; } }
    
    [SerializeField] Vector2 _minLeft, _maxLeft, _minRight, _maxRight;
    [SerializeField] Vector2 _minLeftH, _maxLeftH, _minRightH, _maxRightH;
    [SerializeField] CanvasGroup _imgTimeTense;
    [HideInInspector] public bool _btnON;
    //int _indexCurrFLEX;
    //int _indexMaxFLEX;
    /* void ySetParticle(int num)
     {
         ParticleSystem.EmissionModule _emission = _btnParticles[num].emission;
     }
 */
    IEnumerator yTimerTenseExec()
    {
        int reach = 1;
        while (true)
        {
            _imgTimeTense.DOFade(reach, 0.5f);
            yield return TT.WaitForSeconds(0.5f);
            reach = reach == 1 ? 0 : 1;

        }
        _imgTimeTense.alpha = 0;
    }
    public void zSetTimerTenseEffect()
    {
        StartCoroutine(yTimerTenseExec());        
    }
    public void zHurryUp()
    {
        StartCoroutine(ySetHurryUPInit());
    }

    IEnumerator yBtnDelay()
    {
        _btnON = false;
        yield return TT.WaitForSeconds(0.2f);
        _btnON = true;
    }

    public void BtnInputIngredients(int num)
    {
        
        if (!GameManager.xInstance._isGameStarted) return;
        
        if (_btnON == true)
        {
            TrPuzzleHamburger.xInstance.zInputIngredients(num);
            
            if (num == 2)
                TrAudio_UI.xInstance.zzPlay_BtnIngredient1();
            else if (num == 3)
                TrAudio_UI.xInstance.zzPlay_BtnIngredient2();
            else if (num == 4)
                TrAudio_UI.xInstance.zzPlay_BtnIngredient3();
            else if (num == 5)
                TrAudio_UI.xInstance.zzPlay_BtnIngredient4();

        }
        else
        {
            //Debug.Log("¹öÆ°¾Ó´ë");
        }
        
        /*_btnParticles[1].gameObject.SetActive(true);
        _btnParticles[1].Play();*/
        //TrAudio_SFX.xInstance.zzPlay_HamburgerInputBtn();
    }
    public void zResetHurry()
    {
/*        for (int i = 0; i < _maxIndex; i++)
        {
            _arrHuryyUP[i].gameObject.SetActive(false);
        }*/
    }
    void ySetEffectHurryUP(int num)
    {
        int randLeftOrRight = Random.Range(0, 2);
        int randX, randY;
        TrUI_HamburgerHurry hurry = _arrHuryyUP[num];
        if (randLeftOrRight == 0)
        {
            randX = Random.Range((int)_minLeftH.x, (int)_maxLeftH.x + 1);
            randY = Random.Range((int)_minLeftH.y, (int)_maxLeftH.y + 1);
        }
        else
        {
            randX = Random.Range((int)_minRightH.x, (int)_maxRightH.x + 1);
            randY = Random.Range((int)_minRightH.y, (int)_maxRightH.y + 1);
        }
        
        
        CanvasGroup canvasGroup = hurry.GetComponent<CanvasGroup>();

        hurry.transform.localPosition = new Vector3(randX, randY, 0f);
        hurry.transform.localScale = new Vector3(1f, 1f, 1f);
        if (GameManager.xInstance._isGameStarted == true)
        {
            //hurry.gameObject.SetActive(true);
            //GameObject imgHurry = hurry._imgHurryUP.gameObject;    
            //imgHurry.SetActive(true);
            canvasGroup.alpha = 1;
        }

        _arrHuryyUP[num] = hurry;
        canvasGroup.alpha = 1;

        hurry.transform.DOLocalMoveY(randY + 200, 1f);
        canvasGroup.DOFade(0, 1.8f);
        /*TrUI_HamburgerHurry hurry = _hurry;
            GameObject imgHurry = hurry._ingHurryUP.gameObject;
            CanvasGroup canvasGroup = hurry.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            if (GameManager.xInstance._isGameStarted == true)
            {
                imgHurry.SetActive(true);
            }
            hurry.transform.localPosition = new Vector2(randX, randY);
            hurry.transform.DOLocalMoveY(randY + 200, 1f);
            canvasGroup.DOFade(0, 1.8f);*/
    }
    public void zHurryUPInstantiate()
    {
        _arrHuryyUP = new TrUI_HamburgerHurry[_maxIndex];
        for (int i = 0; i < _maxIndex; i++)
        {
            TrUI_HamburgerHurry setHurry = Instantiate(_infoHurry);
            setHurry.transform.SetParent(_parentHurry.transform);
            _arrHuryyUP[i] = setHurry;
            CanvasGroup canvasGroup = _arrHuryyUP[i].GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }
        _infoHurry.gameObject.SetActive(false);
    }
    IEnumerator ySetHurryUPInit()
    {

        while (true){
            ySetEffectHurryUP(_currIndex++);
            if(_currIndex >= _maxIndex){
                _currIndex = 0;
            }
            yield return TT.WaitForSeconds(0.7f);
        }
        
    }
    public void zEffectFLEX(int num)
    {
        //if (_indexCurrFLEX++ >= _indexMaxFLEX) _indexCurrFLEX = 0;

        int randLeftOrRight = Random.Range(0, 2);
        int randX, randY;

        if (randLeftOrRight == 0)
        {
            randX = Random.Range((int)_minLeft.x, (int)_maxLeft.x + 1);
            randY = Random.Range((int)_minLeft.y, (int)_maxLeft.y + 1);
        }
        else
        {
            randX = Random.Range((int)_minRight.x, (int)_maxRight.x + 1);
            randY = Random.Range((int)_minRight.y, (int)_maxRight.y + 1);
        }

        TrUI_HamburgerFLEX flex = _flex;
        ParticleSystem ps = flex._psMoneyFLEX;
        

        
        CanvasGroup canvasGroup = flex.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        GameObject imgFLEX = flex._imgFLEX[0].gameObject;
        GameObject imgFAIL = flex._imgFLEX[1].gameObject;
        if (num == 0)
        {

            imgFLEX.SetActive(true);
            imgFAIL.SetActive(false);
            ps.gameObject.SetActive(true);
            ps.Play();
            canvasGroup.DOFade(0, 1.8f);
        }
        else if (num == 1)
        {
            imgFAIL.SetActive(true);
            imgFLEX.SetActive(false);
            canvasGroup.DOKill();
            canvasGroup.alpha = 1;
        }

        canvasGroup.DOFade(0, 1.8f);
        flex.transform.localPosition = new Vector2(randX, randY);
        flex.transform.DOLocalMoveY(randY + 200, 1f);
        
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
        //_flex = GetComponentsInChildren<TrUI_HamburgerFLEX>();
        _flex = GetComponentInChildren<TrUI_HamburgerFLEX>();
        _arrHuryyUP = GetComponentsInChildren<TrUI_HamburgerHurry>();
        
    }
}
