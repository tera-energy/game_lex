using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrUI_Window_Notice : TrUI_Window_
{
    static TrUI_Window_Notice _instance = null;

	[SerializeField] TextMeshProUGUI _title, _content;

	static public TrUI_Window_Notice xInstance { get { return _instance; } }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	new void Awake() {
		if(_instance == null) {
			_instance = this;
		} else {
			Destroy(gameObject);
		}
	}


	//============================================================================================================
	public void zShow(string title,string content) {
		base.zShow();
		_title.text = title;
		_content.text = content;
	}

	public override void zHide(bool ignoreTimeScale = false) {
		base.zHide(ignoreTimeScale);
		TrAudio_UI.xInstance.zzPlay_ClickNo();
	}
}

