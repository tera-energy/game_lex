using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public delegate void CallbackSkillFilter(List<TT.enumPlayerSkills> list);

public class TrUI_SkillFilterBar : MonoBehaviour
{
    static TrUI_SkillFilterBar _instance;
	public GameObject _imgMemoryActive, _imgMemoryInactive, _imgFocusActive, _imgFocusInactive, _imgReflexActive, _imgReflexInactive, _imgSolvingActive, _imgSolvingInactive, _imgThinkingActive, _imgThinkingInactive;
	public TextMeshProUGUI _txtMemory, _txtFocus, _txtReflex, _txtSolving, _txtThinking;
	List<TT.enumPlayerSkills> _filteringSkills = new List<TT.enumPlayerSkills>();


	static public TrUI_SkillFilterBar xInstance { get { return _instance; } }
	public List<TT.enumPlayerSkills> xFilteringSkills { get { return _filteringSkills; } }
	public event CallbackSkillFilter _onFilterClick;

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	void Awake() {
		if(_instance == null) {
			_instance = this;
			_txtMemory.color = Color.grey;
			_txtFocus.color = Color.grey;
			_txtReflex.color = Color.grey;
			_txtSolving.color = Color.grey;
			_txtThinking.color = Color.grey;
		} else {
			Destroy(gameObject);
		}
	}

	private void OnDestroy() {
		zUnsubscribeAllFilterEvents();
	}


	//============================================================================================================
	public void zUnsubscribeAllFilterEvents() {
		if(_onFilterClick != null) {
			System.Delegate[] events = _onFilterClick.GetInvocationList();
			foreach(System.Delegate e in events) {
				_onFilterClick -= (CallbackSkillFilter)e;
			}
			_onFilterClick = null;
		}
	}

	public void zOnAllClick() {
		TrAudio_UI.xInstance.zzPlay_ClickTab();

		_filteringSkills.Clear();

		//*/ 필터링 리스트 전부가 게임의 스킬 리스트에 포함되어야 해당되는 경우 <참조: zUpdateScrollListByFilter(...)>
		_imgMemoryActive.SetActive(false);
		_imgMemoryInactive.SetActive(true);
		_txtMemory.color = Color.grey;

		_imgFocusActive.SetActive(false);
		_imgFocusInactive.SetActive(true);
		_txtFocus.color = Color.grey;

		_imgReflexActive.SetActive(false);
		_imgReflexInactive.SetActive(true);
		_txtReflex.color = Color.grey;

		_imgSolvingActive.SetActive(false);
		_imgSolvingInactive.SetActive(true);
		_txtSolving.color = Color.grey;

		_imgThinkingActive.SetActive(false);
		_imgThinkingInactive.SetActive(true);
		_txtThinking.color = Color.grey;
		//*/

		/*/ 게임 스킬리스트 전부가 필터링 리스트에 포함되어야 해당되는 경우
		_imgMemoryActive.SetActive(true);
		_imgMemoryInactive.SetActive(false);
		_filteringSkills.Add(TT.enumPlayerSkills.Memory);
		_txtMemory.color = Color.white;

		_imgFocusActive.SetActive(true);
		_imgFocusInactive.SetActive(false);
		_filteringSkills.Add(TT.enumPlayerSkills.Focus);
		_txtFocus.color = Color.white;

		_imgReflexActive.SetActive(true);
		_imgReflexInactive.SetActive(false);
		_filteringSkills.Add(TT.enumPlayerSkills.Reflex);
		_txtReflex.color = Color.white;

		_imgSolvingActive.SetActive(true);
		_imgSolvingInactive.SetActive(false);
		_filteringSkills.Add(TT.enumPlayerSkills.Solving);
		_txtSolving.color = Color.white;

		_imgThinkingActive.SetActive(true);
		_imgThinkingInactive.SetActive(false);
		_filteringSkills.Add(TT.enumPlayerSkills.Thinking);
		_txtThinking.color = Color.white;
		//*/

		_onFilterClick?.Invoke(_filteringSkills);
	}

	public void zToggleMemory(bool force = false) {
		//현재 필터링 켜져있는게 기억력 하나뿐이면 끌 수 없음. 게임 스킬리스트 전부가 필터링 리스트에 포함되어야 해당되는 경우에만 이용.
		//if(_filteringSkills.Count == 1 && _imgMemoryActive.activeSelf) {    
		//	return;
		//}

		TrAudio_UI.xInstance.zzPlay_ClickTab();

		bool toggle = force || !_imgMemoryActive.activeSelf;

		_imgMemoryActive.SetActive(toggle);
		_imgMemoryInactive.SetActive(!toggle);

		if(toggle) {
			_filteringSkills.Remove(TT.enumPlayerSkills.Memory);
			_filteringSkills.Add(TT.enumPlayerSkills.Memory);
			_txtMemory.color = Color.white;
		} else {
			_filteringSkills.Remove(TT.enumPlayerSkills.Memory);
			_txtMemory.color = Color.grey;
		}

		_onFilterClick?.Invoke(_filteringSkills);
	}

	public void zToggleFocus(bool force = false) {
		//if(_filteringSkills.Count == 1 && _imgFocusActive.activeSelf) {    
		//	return;
		//}

		TrAudio_UI.xInstance.zzPlay_ClickTab();

		bool toggle = force || !_imgFocusActive.activeSelf;

		_imgFocusActive.SetActive(toggle);
		_imgFocusInactive.SetActive(!toggle);

		if(toggle) {
			_filteringSkills.Remove(TT.enumPlayerSkills.Focus);
			_filteringSkills.Add(TT.enumPlayerSkills.Focus);
			_txtFocus.color = Color.white;
		} else {
			_filteringSkills.Remove(TT.enumPlayerSkills.Focus);
			_txtFocus.color = Color.grey;
		}

		_onFilterClick?.Invoke(_filteringSkills);
	}

	public void zToggleReflex(bool force = false) {
		//if(_filteringSkills.Count == 1 && _imgReflexActive.activeSelf) {    
		//	return;
		//}

		TrAudio_UI.xInstance.zzPlay_ClickTab();

		bool toggle = force || !_imgReflexActive.activeSelf;

		_imgReflexActive.SetActive(toggle);
		_imgReflexInactive.SetActive(!toggle);

		if(toggle) {
			_filteringSkills.Remove(TT.enumPlayerSkills.Reflex);
			_filteringSkills.Add(TT.enumPlayerSkills.Reflex);
			_txtReflex.color = Color.white;
		} else {
			_filteringSkills.Remove(TT.enumPlayerSkills.Reflex);
			_txtReflex.color = Color.grey;
		}

		_onFilterClick?.Invoke(_filteringSkills);
	}

	public void zToggleSolving(bool force = false) {
		//if(_filteringSkills.Count == 1 && _imgSolvingActive.activeSelf) {    
		//	return;
		//}

		TrAudio_UI.xInstance.zzPlay_ClickTab();

		bool toggle = force || !_imgSolvingActive.activeSelf;

		_imgSolvingActive.SetActive(toggle);
		_imgSolvingInactive.SetActive(!toggle);

		if(toggle) {
			_filteringSkills.Remove(TT.enumPlayerSkills.Solving);
			_filteringSkills.Add(TT.enumPlayerSkills.Solving);
			_txtSolving.color = Color.white;
		} else {
			_filteringSkills.Remove(TT.enumPlayerSkills.Solving);
			_txtSolving.color = Color.grey;
		}

		_onFilterClick?.Invoke(_filteringSkills);
	}

	public void zToggleThinking(bool force = false) {
		//if(_filteringSkills.Count == 1 && _imgThinkingActive.activeSelf) {    
		//	return;
		//}

		TrAudio_UI.xInstance.zzPlay_ClickTab();

		bool toggle = force || !_imgThinkingActive.activeSelf;

		_imgThinkingActive.SetActive(toggle);
		_imgThinkingInactive.SetActive(!toggle);

		if(toggle) {
			_filteringSkills.Remove(TT.enumPlayerSkills.Thinking);
			_filteringSkills.Add(TT.enumPlayerSkills.Thinking);
			_txtThinking.color = Color.white;
		} else {
			_filteringSkills.Remove(TT.enumPlayerSkills.Thinking);
			_txtThinking.color = Color.grey;
		}
				
		_onFilterClick?.Invoke(_filteringSkills);
	}
}
