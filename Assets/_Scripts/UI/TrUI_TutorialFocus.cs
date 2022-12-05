using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_TutorialFocus : MonoBehaviour
{
    static TrUI_TutorialFocus _instance;

    public GameObject _goTutorial;
    public RectTransform _rectEventMaskParent; // 마스크의 부모 객체(anchormin, anchormax 조정을 위한)
    public RectTransform _rectEventMask; // 마스크의 위치
    public RectTransform _rectEventFinger; // 손가락 위치
    public Button _btnUnmask;

    void OnDestroy()
    {
        _btnUnmask.GetComponent<Button>()
            .onClick
            .RemoveAllListeners();
    }

    public static TrUI_TutorialFocus xInstance { get { return _instance; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes(){
        _instance = null;
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
