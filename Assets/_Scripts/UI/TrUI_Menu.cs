using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrUI_Menu : MonoBehaviour
{
    [SerializeField] RectTransform[] _rectMenuButtons;
    [SerializeField] float _posYDistanceMenuTabs;
    [SerializeField] RectTransform _rectMenuIcon;
    Tween[] _tweenMenus;
    public bool _isMenuTabActive = false;

    public void zOnOffMenu()
    {
        _isMenuTabActive = !_isMenuTabActive;
        int numButton = _rectMenuButtons.Length;
        if (_tweenMenus[numButton - 1] != null)
            for (int i = 0; i < numButton; i++)
            {
                _tweenMenus[i].Kill();
            }

        float root = _rectMenuIcon.localPosition.y - 80;

        if (_isMenuTabActive)
        {
            TrAudio_UI.xInstance.zzPlay_Popup();
            float posY = root - 60;
            for (int i = 0; i < numButton; i++)
            {
                _rectMenuButtons[i].transform.gameObject.SetActive(true);
                _tweenMenus[i] = _rectMenuButtons[i].transform.DOLocalMoveY(posY, 0.25f);
                posY += _posYDistanceMenuTabs;
            }
        }
        else
        {
            TrAudio_UI.xInstance.zzPlay_PopupClose();
            for (int i = 0; i < numButton; i++)
            {
                int index = i;
                _tweenMenus[i] = _rectMenuButtons[i].transform.DOLocalMoveY(root, 0.25f).OnComplete(() => _rectMenuButtons[index].transform.gameObject.SetActive(false));
            }
        }
    }
    
    void Start()
    {
        _tweenMenus = new Tween[_rectMenuButtons.Length];
    }
}
