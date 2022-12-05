using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrUI_InputTimer : MonoBehaviour
{
    static TrUI_InputTimer _instance;
    public static TrUI_InputTimer xInstance { get { return _instance; } }

    [SerializeField] bool _isUseAnimator;
    [SerializeField] Animator _animTicTok;

    public void yTicTok()
    {
        if (GameManager.xInstance._isGameStarted == false)
        _animTicTok.SetTrigger("IsTicTok");
        else if (GameManager.xInstance._isGameStarted == true)
            _animTicTok.ResetTrigger("IsTicTok");
    }

    // Start is called before the first frame update
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
        if (_isUseAnimator)
            _animTicTok.GetComponent<Animator>();
    }
    void Update()
    {
        yTicTok();
    }
}
