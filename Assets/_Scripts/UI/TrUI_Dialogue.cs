using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_Dialogue : MonoBehaviour
{
    static TrUI_Dialogue _instance;

    public GameObject _goContext;
    public Text _textContext;
    public Image _imgNameBackground;
    public Text _textName;
    public Image _imgAnimal;

    public static TrUI_Dialogue xInstance { get { return _instance; } }


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
