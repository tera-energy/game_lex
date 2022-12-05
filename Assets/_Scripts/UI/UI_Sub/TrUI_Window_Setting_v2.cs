using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrUI_Window_Setting_v2 : TrUI_Window_
{
    static TrUI_Window_Setting_v2 _instance = null;

	public GameObject _chkMusicOn, _chkMusicOff, _chkSFXOn, _chkSFXOff;
	[SerializeField] TextMeshProUGUI _version;

	static public TrUI_Window_Setting_v2 xInstance { get { return _instance; } }

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
			_chkMusicOn.SetActive(true);
			_chkMusicOff.SetActive(false);
		} else {
			_chkMusicOn.SetActive(false);
			_chkMusicOff.SetActive(true);
		}

		if(PlayerPrefs.GetFloat(TT.strConfigSFX, 1) > 0) {
			_chkSFXOn.SetActive(true);
			_chkSFXOff.SetActive(false);
		} else {
			_chkSFXOn.SetActive(false);
			_chkSFXOff.SetActive(true);
		}

		_version.text = "¹öÀü " + Application.version;
	}

	//============================================================================================================
	public void zzOnMusicToggle() {
		if(PlayerPrefs.GetFloat(TT.strConfigMusic,1) > 0) {
			_chkMusicOn.SetActive(false);
			_chkMusicOff.SetActive(true);
			TrAudio_Music.xInstance.zzSetFlatVolume(-1);
		} else {
			_chkMusicOn.SetActive(true);
			_chkMusicOff.SetActive(false);
			TrAudio_Music.xInstance.zzSetFlatVolume(1);
			TrAudio_Music.xInstance.zzPlayMain(0f);
		}
	}

	public void zzOnSFXToggle() {
		if(PlayerPrefs.GetFloat(TT.strConfigSFX,1) > 0) {
			_chkSFXOn.SetActive(false);
			_chkSFXOff.SetActive(true);
			TrAudio_SFX.xInstance.zzSetFlatVolume(-1);
			TrAudio_UI.xInstance.zzSetFlatVolume(-1);
		} else {
			_chkSFXOn.SetActive(true);
			_chkSFXOff.SetActive(false);
			TrAudio_SFX.xInstance.zzSetFlatVolume(1);
			TrAudio_UI.xInstance.zzSetFlatVolume(1);
		}
	}
}

