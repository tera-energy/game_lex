using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_PuzzleArbeit : MonoBehaviour
{

    static TrUI_PuzzleArbeit _instance;
    
    [SerializeField] TextMeshProUGUI _txtNotice, _txtscore;
    



    Coroutine _TextCoroutine;

    public void zOnButtonOClick()
    {
        TrPuzzleArbiet.xInstance.zButtonInput(TrPuzzleArbiet.enumButtonInputOX.O);
    }
    public void zOnButtonXClick()
    {
        TrPuzzleArbiet.xInstance.zButtonInput(TrPuzzleArbiet.enumButtonInputOX.X);
    }


    public void zSetScore(int score)
    {
        _txtscore.text = score.ToString();
    }

    IEnumerator ySetStartText(string text)
    {
        _txtNotice.text = text;
        _txtNotice.transform.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if (_txtNotice.text == text)
        {
            _txtNotice.transform.gameObject.SetActive(false);

        }
    }

    public void zSetNotice(int count)
    {
        if (count > 0)
        {
            _txtNotice.text = count.ToString();
            return;
        }
        string txt = "";
        if (count == 0)
        {
            _txtNotice.transform.gameObject.SetActive(false);
            return;
        }
        else if (count == -1)
        {
            txt = "Start!";
        }
        else if (count == -2)
        {
            txt = "Wrong!";
        }
        else if (count == -3)
        {
            txt = "Good!";
        }
        if (_TextCoroutine != null)
        {
            StopCoroutine(_TextCoroutine);
            _TextCoroutine = StartCoroutine(ySetStartText(txt));
            return;
        }
        _TextCoroutine = StartCoroutine(ySetStartText(txt));
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

    public static TrUI_PuzzleArbeit xInstance { get { return _instance; } }
    // Start is called before the first frame update
    void Start()
    {
        _txtscore.text = "0";
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
