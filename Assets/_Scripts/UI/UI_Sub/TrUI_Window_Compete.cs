using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrUI_Window_Compete : TrUI_Window_
{
    static TrUI_Window_Compete _instance;
	public TrUI_Window_ _underConstructWindow;

	static public TrUI_Window_Compete xInstance { get { return _instance; } }

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
	public void zOnRandomMatchClick() {
		_underConstructWindow.zShow();
		TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
	}

	public void zOnTierMatchClick() {
		_underConstructWindow.zShow();
		TrAudio_UI.xInstance.zzPlay_ClickButtonNormal();
	}

	public override void zHide(bool ignoreTimeScale=false) {
		base.zHide(ignoreTimeScale);
		TrAudio_UI.xInstance.zzPlay_ClickNo();
	}
}
