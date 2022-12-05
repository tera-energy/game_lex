using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TrClickType
{
    None,
    Small,
    Normal,
    Big,
}

public class TrUI_HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] bool _isNotClickAfterClick;
    Animator _anim;
    [SerializeField] ParticleSystem _particleTouch;
    [SerializeField] ParticleSystem _particleOnDown;
    GameObject _goCurrentDown;
    public UnityEvent<PointerEventData> _buttonPressedDown;
    public UnityEvent<PointerEventData> _buttonPressedUp;
    Image _img;
    [SerializeField] Sprite _spChange;
    [SerializeField] Sprite _spOrigin;

    [SerializeField] TrClickType _clickDownType;
    [SerializeField] TrClickType _clickUpType;

    [SerializeField] Color _colorActivate = new Color(1, 1, 1);
    [SerializeField] Color _colorDisable = new Color(0.5f, 0.5f, 0.5f);

    bool isStay = false;
    bool _isInteract = false;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if ((GameManager._state == TT.enumGameState.Play && !GameManager.xInstance._isGameStarted) || !GameManager._canBtnClick || !_isInteract) return;
        //Debug.Log($">>> DOWN <<<: pressed mouse button is {eventData.button} and currently pointed gameobject is {eventData.pointerCurrentRaycast.gameObject}!", gameObject);
        _goCurrentDown = eventData.pointerCurrentRaycast.gameObject;
        _buttonPressedDown?.Invoke(eventData);
        zEffect();
    }

    public void zEffect()
    {
        if (_anim)
            _anim.SetTrigger("IsClickDown");

        if (_particleOnDown)
            _particleOnDown.Play();
        if (_particleTouch)
            _particleTouch.Play();

        if (_goCurrentDown != null)
            isStay = true;

        zChangeImg(true);


        if (_clickDownType != TrClickType.None)
            switch (_clickDownType)
            {
                case TrClickType.Small:
                    TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();
                    break;
                case TrClickType.Normal:
                    TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
                    break;
                case TrClickType.Big:
                    TrAudio_UI.xInstance.zzPlay_ClickButtonBig();
                    break;
            }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if ((GameManager._state == TT.enumGameState.Play && !GameManager.xInstance._isGameStarted) || !GameManager._canBtnClick || !_isInteract) return;
        //Debug.Log(">>> UP <<<: pressed mouse button is " + eventData.button + " and currently pointed gameobject is " + eventData.pointerCurrentRaycast.gameObject + "!", gameObject);

        if (isStay)
        {
            _buttonPressedUp?.Invoke(eventData);

            if (_isNotClickAfterClick)
                GameManager._canBtnClick = false;

            if (_clickUpType != TrClickType.None)
                switch (_clickUpType)
                {
                    case TrClickType.Small:
                        TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();
                        break;
                    case TrClickType.Normal:
                        TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
                        break;
                    case TrClickType.Big:
                        TrAudio_UI.xInstance.zzPlay_ClickButtonBig();
                        break;
                }
        }

        zChangeImg(false);

        _goCurrentDown = null;
        isStay = false;

        if (_particleOnDown)
            _particleOnDown.Stop();

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager._state == TT.enumGameState.Play && !GameManager.xInstance._isGameStarted)
        {
            if (_goCurrentDown)
            {
                if (_particleOnDown)
                    _particleOnDown.Stop();
                _goCurrentDown = null;
            }
        }

        if (_goCurrentDown)
        {
            if (eventData.pointerCurrentRaycast.gameObject != _goCurrentDown)
            {
                isStay = false;

                if (_particleOnDown)
                    _particleOnDown.Stop();

                zChangeImg(false);

            }
            else
            {
                if (!isStay)
                {
                    if (_clickDownType != TrClickType.None)
                        switch (_clickDownType)
                        {
                            case TrClickType.Small:
                                TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();
                                break;
                            case TrClickType.Normal:
                                TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
                                break;
                            case TrClickType.Big:
                                TrAudio_UI.xInstance.zzPlay_ClickButtonBig();
                                break;
                        }
                }
                isStay = true;

                if (_particleOnDown)
                    _particleOnDown.Play();

                zChangeImg(true);

            }
        }
    }

    void zChangeImg(bool isChange)
    {
        if (isChange)
        {
            if (_spChange)
            {
                _img.sprite = _spChange;
                _img.SetNativeSize();
            }
        }
        else
        {
            if (_spOrigin)
            {
                _img.sprite = _spOrigin;
                _img.SetNativeSize();
            }
        }
    }

    public void zInteractDisable(bool isOnlyColor = false)
    {
        _img.color = _colorDisable;

        if (!isOnlyColor)
            _isInteract = false;
    }

    public void zInteractActivate(bool isOnlyColor = false)
    {
        _img.color = _colorActivate;

        if (!isOnlyColor)
            _isInteract = true;
    }

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _img = GetComponent<Image>();
        _isInteract = true;
    }
}
