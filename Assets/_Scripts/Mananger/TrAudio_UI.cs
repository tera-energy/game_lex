using UnityEngine;

public class TrAudio_UI : TrAudio_ {
	public AudioClip	_clickButtonNormal, _clickButtonSmall, _clickButtonBig, _clickTab, _clickYes, _clickNo,  _slideCenterChangePractice, _timertictok, _invalid,   _level_select, 
						 _pause, _correct, 
							_popup, _popdown, 	
							 _gameover1, _gameover2,
							_pangPang, _wangWaWang, _greatSound;
	public AudioClip[] _animalsBtnClick, _ready, _btnIngredients;

	static TrAudio_UI _instance = null;

	/*///////////////////////////////////////////////////////////////////////////////////////////////////////////*/
	public static TrAudio_UI xInstance{get{return _instance;}}

	//==========================================================================================================
	public void zzPlay_Correct(float delay = -1) { zPlaySFX(_correct, delay); }
	public void zzPlay_BtnIngredient1(float delay =-1) { zPlaySFX(_btnIngredients[0], delay); }
	public void zzPlay_BtnIngredient2(float delay =-1) { zPlaySFX(_btnIngredients[1], delay); }
	public void zzPlay_BtnIngredient3(float delay =-1) { zPlaySFX(_btnIngredients[2], delay); }
	public void zzPlay_BtnIngredient4(float delay =-1) { zPlaySFX(_btnIngredients[3], delay); }
	public void zzPlay_GreatSound(float delay =-1) { zPlaySFX(_greatSound, delay); }
	public void zzPlay_WangWaWang(float delay =-1) { zPlaySFX(_wangWaWang, delay); }
	public void zzPlay_PangPang(float delay = -1) { zPlaySFX(_pangPang, delay); }
	public void zzPlay_AnimalsRBtnClick(float delay = -1) { zPlaySFX(_animalsBtnClick[1], delay); }
	public void zzPlay_AnimalsLBtnClick(float delay = -1) { zPlaySFX(_animalsBtnClick[0], delay); }
	public void zzPlay_ClickButtonSmall(float delay=-1)					{ zPlaySFX(_clickButtonSmall,delay); }
	public void zzPlay_ClickButtonNormal(float delay = -1)					{zPlaySFX(_clickButtonNormal,delay);}
	public void zzPlay_ClickButtonBig(float delay = -1) { zPlaySFX(_clickButtonBig, delay); }
	public void zzPlay_ClickYes(float delay = -1)						{zPlaySFX(_clickYes,delay);}
	public void zzPlay_ClickNo(float delay = -1)							{zPlaySFX(_clickNo,delay);}
	public void zzPlay_SlideCenterChangePractice(float delay = -1)		{ zPlaySFX(_slideCenterChangePractice,delay); }
	public void zzPlay_TimerTicTok(float delay = -1)					{ zPlaySFX(_timertictok, delay); }
	public void zzPlay_ClickTab(float delay = -1)							{zPlaySFX(_clickTab,delay);}
	public void zzPlay_Pause(float delay = -1)							{zPlaySFX(_pause,delay);}
	public void zzPlay_Popup(float delay = -1)							{zPlaySFX(_popup,delay);}
	public void zzPlay_PopupClose(float delay = -1)							{ zPlaySFX(_popdown,delay); }
	public void zzPlay_GameOver1(float delay = -1)						{ zPlaySFX(_gameover1,delay);}
	public void zzPlay_GameOver2(float delay = -1)						{ zPlaySFX(_gameover2,delay); }
	public void zzPlay_Ready(int index, float delay = -1)						{ zPlaySFX(_ready[index], delay); }
		

	public void zzSetFlatVolume(float? newVolume = null) {
		zSetFlatVolume(TT.strConfigSFX,newVolume);
	}

	//==========================================================================================================
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	new void Awake() {
		if(_instance == null) {
			base.Awake();
			_instance = this;
			zzSetFlatVolume();
		} else
			Destroy(gameObject);
	}

	void Start() {
		//몇몇 UI 요소(슬라이더 등)가 시작할 때 소리를 내므로 초반 잠깐 볼륨을 0으로 함.
		zChangeVolume(0f);
		TT.UtilDelayedFunc.zCreate(() => zzSetFlatVolume(), .75f); 
	}
}
