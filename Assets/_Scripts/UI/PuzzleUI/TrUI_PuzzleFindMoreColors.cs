using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TrUI_PuzzleFindMoreColors : MonoBehaviour
{
    public static TrUI_PuzzleFindMoreColors xInstance { get { return _instance; } }

    static TrUI_PuzzleFindMoreColors _instance;

    [SerializeField] TextMeshProUGUI _txtNotice, _txtScore;

    Coroutine _TextCoroutine;

    public void zSetScore(int score)
    {
        _txtScore.text = score.ToString();
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
        if (_TextCoroutine != null)
            StopCoroutine(_TextCoroutine);
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

    // Start is called before the first frame update
    void Start()
    {
        _txtScore.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
