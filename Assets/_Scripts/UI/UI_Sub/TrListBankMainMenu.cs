using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirFishLab.ScrollingList;
using UnityEngine.Events;

/// The whole concept around ListBank, ListBox, and contents is like,
/// the Bank holds all its contents of a client in its deep secret vault, and the Boxes are placed in client's UI.
/// Now, the client asks the bank to store some(or all) of its contents into their boxes so that
/// they can display their contents in their UI.

[System.Serializable]
public class trScrollContentMainMenu {
	[SerializeField] Sprite _sprite;
	[SerializeField] string _btnTitle;
	[SerializeField] UnityEvent _onClick;

	public Sprite xSprite { get { return _sprite; } }
	public string xBtnTitle { get { return _btnTitle; } }

	public void zInvokeOnClickEvent() {
		_onClick?.Invoke();
	}
}


public class TrListBankMainMenu : BaseListBank
{
    // class 보단 ScriptableObject를 활용하는것이 좋을 듯. --> SO에선 이벤트 등록이 안되므로 그냥 클래스 사용.
    [SerializeField] trScrollContentMainMenu[] _contents;

    public override object GetListContent(int index) {
		return _contents[index];
    }

    public override int GetListLength() {
        return _contents.Length;
    }
}
