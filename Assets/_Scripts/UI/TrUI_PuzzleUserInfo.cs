using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_PuzzleUserInfo : MonoBehaviour
{
    static TrUI_PuzzleUserInfo _instance;

    public GameObject[] _goUserUIs;
    public Text[] _textUserNames, _textUserScores;

    static public TrUI_PuzzleUserInfo xInstance
    {
        get
        {
            if (_instance == null || _instance.gameObject == null)
                return null;
            else
                return _instance;
        }
    }

    public void zActivateUIs(bool isActivate, int num=4){
        for(int i=0; i<num; i++){
            _goUserUIs[i].SetActive(isActivate);
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

}
