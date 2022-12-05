using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirFishLab.ScrollingList;
using UnityEngine.Events;

[System.Serializable]
public class trScrollContentPractice {
	[SerializeField] TrSO_PuzzleData _puzzleData;

	public TrSO_PuzzleData xPuzzleData { get { return _puzzleData; } }
}



public class TrListBankPractice : BaseListBank
{
    // class 보단 ScriptableObject를 활용하는것이 좋을 듯. --> SO에선 이벤트 등록이 안되므로 그냥 클래스 사용.
    [SerializeField] trScrollContentPractice[] _contents;
	trScrollContentPractice[] _origContents;

	private void Awake() {
		_origContents = new trScrollContentPractice[_contents.Length];
		_contents.CopyTo(_origContents, 0);
	}


	public void zSetListContents(trScrollContentPractice[] newContents) {
		_contents = newContents;
	}

	//이걸 호출 한 후 해당 CircularScrollingList의 오브젝트.Refresh()를 해야 실제로 효과가 반영됨
	public void zResetToOriginalContents() {
		_contents = new trScrollContentPractice[_origContents.Length];
		_origContents.CopyTo(_contents, 0);
	}

	public trScrollContentPractice[] zGetAllListContents() {
		return _contents;
	}

    public override object GetListContent(int index) {
		return _contents[index];
    }

    public override int GetListLength() {
        return _contents.Length;
    }
}
