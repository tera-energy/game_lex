using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_PuzzleCall : TrUI_PuzzleManager
{

    public static TrUI_PuzzleCall xInstance { get { return _instance; } }
    static TrUI_PuzzleCall _instance;



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

