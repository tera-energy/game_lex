using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrUI_DifficultyMenu : MonoBehaviour
{
    static TrUI_DifficultyMenu _instance;
	public event TT.CallbackInt _onDifficultySelected;
	TMP_Dropdown _dropDown;

	static public TrUI_DifficultyMenu xInstance { get { return _instance; } }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	private void Awake() {
		if (_instance == null) {
			_instance = this;
			_dropDown = GetComponent<TMP_Dropdown>();
		} else
			Destroy(gameObject);
	}

	private void OnDestroy() {
		if(_onDifficultySelected != null) {
			System.Delegate[] events = _onDifficultySelected.GetInvocationList();
			foreach(System.Delegate e in events) {
				_onDifficultySelected -= (TT.CallbackInt)e;
			}
			_onDifficultySelected = null;
		}
	}

	//==========================================================================================================================

	public void zOnDifficultySelected(int nanido) {
		_onDifficultySelected?.Invoke(nanido + 1);	//TT.enumDifficultyLevel은 첫항목이 1이므로 1을 더해쥼.
	}

	public void zSetMenuIndex(int index) {
		_dropDown.value = index;
	}
}
