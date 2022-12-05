using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrUI_PuzzleScore : MonoBehaviour
{
    static TrUI_PuzzleScore _instance;
    public static TrUI_PuzzleScore xInstance { get { return _instance; } }

    [SerializeField] TextMeshProUGUI _txtScore;

    public void zSetScore(int score)
    {
        _txtScore.text = string.Format("{0}$", score.ToString());
    }

    void Awake(){
        if (_instance == null){
            _instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    void Start()
    {
        zSetScore(0);
    }
}
