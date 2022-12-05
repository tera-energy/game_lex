using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class TrCheckBoxEvent : UnityEvent<bool>{}

public class TrUI_CheckBox : TrUI_HoldButton
{
    [Header("CheckBox")]
    [SerializeField] public TrCheckBoxEvent _event;

    [SerializeField] GameObject _goCheck;
    [SerializeField] bool _isInitActivate;

    public bool _isActivated;

    public override void OnPointerUp(PointerEventData eventData)
    {
        _isActivated = !_isActivated;
        _goCheck.SetActive(_isActivated);
        _event.Invoke(_isActivated);
        //_btnAction.Invoke(_isActivated);
        //_buttonPressedUp?.Invoke(eventData);
    }

    void Start()
    { 
        _goCheck.SetActive(_isInitActivate);

        _isActivated = _isInitActivate;
        _goCheck.SetActive(_isActivated);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }
}
