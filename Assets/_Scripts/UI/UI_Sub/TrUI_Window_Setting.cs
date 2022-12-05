using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_Window_Setting : TrUI_Window_
{
    static TrUI_Window_Setting _instance = null;

	[SerializeField] Button _btnMusic, _btnSFX;

	static public TrUI_Window_Setting xInstance { get { return _instance; } }

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

	private new void Start() {
		base.Start();
		
		if(PlayerPrefs.GetFloat(TT.strConfigMusic,1) > 0){
			_btnMusic.image.color = Color.white;
		} else {
			_btnMusic.image.color = Color.grey;
		}

		if(PlayerPrefs.GetFloat(TT.strConfigSFX, 1) > 0) {
			_btnSFX.image.color = Color.white;
		} else {
			_btnSFX.image.color = Color.grey;
		}
	}

	//============================================================================================================
	public void zOnMusicToggle() {
		if(PlayerPrefs.GetFloat(TT.strConfigMusic,1) > 0) {
			_btnMusic.image.color = Color.grey;
			TrAudio_Music.xInstance.zzSetFlatVolume(-1);
		} else {
			_btnMusic.image.color = Color.white;
			TrAudio_Music.xInstance.zzSetFlatVolume(1);
			TrAudio_Music.xInstance.zzPlayMain(0f);
		}
	}

	public void zOnSFXToggle() {
		if(PlayerPrefs.GetFloat(TT.strConfigSFX,1) > 0) {
			_btnSFX.image.color = Color.grey;
			TrAudio_SFX.xInstance.zzSetFlatVolume(-1);
			TrAudio_UI.xInstance.zzSetFlatVolume(-1);
		} else {
			_btnSFX.image.color = Color.white;
			TrAudio_SFX.xInstance.zzSetFlatVolume(1);
			TrAudio_UI.xInstance.zzSetFlatVolume(1);
		}
	}

	public override void zHide(bool ignoreTimeScale=false) {
		base.zHide(ignoreTimeScale);
		TrAudio_UI.xInstance.zzPlay_ClickNo();
	}

	public void whatever() {
	}
}

