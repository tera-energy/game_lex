using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TestScript : MonoBehaviour
{
    [Header("¸Þ´º ÅÇ")]
    [SerializeField] Image _imgSetting;
    [SerializeField] float _posYDistanceMenuTabs;
    Tween _tweenSetting;

    [Header("·¹º§ ÅÇ")]
    [SerializeField] Image[] _buttonLevels;
    [SerializeField] float _posYDistanceButtons;
    [SerializeField] RectTransform _rectSelectorLevel;
    [SerializeField] RectTransform _rectLevelButton;
    Tween[] _tweenButtons = new Tween[3];
    bool _isLevelTabActive;


    public void zOnOffMenu(bool isOpen)
    {

        if (_tweenSetting != null)
            _tweenSetting.Kill();

        if (isOpen)
        {
            TrAudio_UI.xInstance.zzPlay_Popup();
            _tweenSetting = _imgSetting.transform.DOLocalMoveY(_posYDistanceMenuTabs, 3f).SetSpeedBased();
        }
        else
        {
            TrAudio_UI.xInstance.zzPlay_PopupClose();
            _tweenSetting = _imgSetting.transform.DOLocalMoveY(0, 3f).SetSpeedBased();
        }
    }

    // ¸Ê Á¤º¸Ã¢ÀÇ On, Off
    public void zOnOffLevel()
    {
        _isLevelTabActive = !_isLevelTabActive;
        if (_tweenButtons[2] != null)
            for (int i = 0; i < 3; i++)
            {
                _tweenButtons[i].Kill();
            }

        float root = _rectLevelButton.localPosition.y;

        if (_isLevelTabActive)
        {
            root += 10;
          //  TrAudio_UI.xInstance.zzPlay_Popup();
            for (int i = 0; i < 3; i++)
            {
                _tweenButtons[i] = _buttonLevels[i].transform.DOLocalMoveY(root + (i + 1) * _posYDistanceButtons, 0.5f);
            }
            //_goStageInfo.SetActive(true);
        }
        else
        {
       //     TrAudio_UI.xInstance.zzPlay_PopupClose();
            for (int i = 0; i < 3; i++)
            {
                _tweenButtons[i] = _buttonLevels[i].transform.DOLocalMoveY(root, 0.5f);
            }
            //_goStageInfo.SetActive(false);
        }
    }
}
