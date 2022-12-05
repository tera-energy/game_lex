using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrCheckInputNickname : MonoBehaviour
{
    [SerializeField] TMP_InputField _txtInput;
    [SerializeField] TrUI_HoldButton _btnSubmit;

    void FixedUpdate()
    {
        int len = _txtInput.text.Length;
        if (len >= 2 && len <= 8) {
            _btnSubmit.zInteractActivate();
        } else {
            _btnSubmit.zInteractDisable();
        }
    }
}
