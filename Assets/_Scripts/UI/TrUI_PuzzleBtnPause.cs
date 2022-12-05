using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_PuzzleBtnPause : MonoBehaviour
{
    static TrUI_PuzzleBtnPause _instance;
    [SerializeField] GameObject[] _goPauseButtons;
    [SerializeField] GameObject _goBlackImage;
    public static TrUI_PuzzleBtnPause xInstance { get { return _instance; } }
    // Start is called before the first frame update
    void Awake(){
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        _goBlackImage.SetActive(false);
        _goPauseButtons[0].SetActive(true);
        _goPauseButtons[1].SetActive(false);
    }

    public void zActivePause(){
        _goBlackImage.SetActive(true);
        _goPauseButtons[0].SetActive(false);
        _goPauseButtons[1].SetActive(true);
    }

    public void zDisablePause(){
        _goBlackImage.SetActive(false);
        _goPauseButtons[0].SetActive(true);
        _goPauseButtons[1].SetActive(false);
    }
}
