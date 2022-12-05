using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrDialogue{
    [Header("대사 치는 캐릭터 이름")]
    public string _name;

    [Header("대사 내용")]
    public string[] _contexts;

    [Header("이미지 번호")]
    public int[] _numImage;

    [Header("이벤트 유무")]
    public char[] _event;
}


[System.Serializable]
public class TrDialogueEvent{
    public string _name;

    public TrDialogueClickEvent[] _clickEvent;

    public Vector2 _line;
    public TrDialogue[] _dialogues;
}

[System.Serializable]
public class TrDialogueClickEvent
{
    public bool _isUI;
    public bool _needClick;

    public Transform _trTarget;
    public Vector3 _addMaskPos;

    public RectTransform _rectParentTarget;
    public RectTransform _rectTarget;

    public Vector3 _sizeMask;

    public int _eventClickNumber;
}