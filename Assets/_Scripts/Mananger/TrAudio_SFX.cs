using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrAudio_SFX : TrAudio_ {
	static TrAudio_SFX _instance = null;

	public AudioClip[] _resultFireCracker;
	public AudioClip _animalsWrong, _burgerWrong, _animalCombo, _animalsBombExec, _animalsBomb, _animalsSmoke, _animalsSmokeExec, _animalsBee, _animalsDog, _fire, _burnCookie, _newScore, _templeBomb, _sizeUpDown;
	public static TrAudio_SFX xInstance{get{return _instance;}}

	//==========================================================================================================
	public void zzPlayBurgerWrong(float delay = -1f) { zPlaySFX(_burgerWrong, delay); }
	public void zzPlaySizeUpDown(float delay =-1f) { zPlaySFX(_sizeUpDown, delay); }
	public void zzPlayNewScore(float delay = -1f) { zPlaySFX(_newScore, delay); }
	public void zzPlayBurnCookie(float delay =-1f) { zPlaySFX(_burnCookie, delay); }
	public void zzPlay_Fire(float delay =-1f) { zPlaySFX(_fire, delay); }
	public void zzPlay_FireCracker(float delay =-1f) { zPlaySFX(_resultFireCracker[Random.Range(0, _resultFireCracker.Length)], delay); }
	public void zzPlay_AnimalsBomb(float delay = -1f) { zPlaySFX(_animalsBomb, delay); }
	public void zzPlay_TempleBomb(float delay = -1f) { zPlaySFX(_templeBomb, delay); }
	public void zzPlay_AnimalsCombo(float delay = -1f) { zPlaySFX(_animalCombo, delay); }
	public void zzPlay_AnimalsDog(float delay = -1f) { zPlaySFX(_animalsDog, delay); }
	public void zzPlay_AnimalsSmoke(float delay = -1f) { zPlaySFX(_animalsSmoke, delay); }
	public void zzPlay_AnimalsSmokeExec(float delay = -1f) { zPlaySFX(_animalsSmokeExec, delay); }
	public void zzPlay_AnimalsWrong(float delay = -1f) { zPlaySFX(_animalsWrong, delay); }
	public void zzPlay_AnimalsBee(float delay = -1f) { zPlaySFX(_animalsBee, delay); }
	public void zzPlay_AnimalsBombExec(float delay = -1f) { zPlaySFX(_animalsBombExec, delay); }
	public void zzSetFlatVolume(float? newVolume = null) {
		zSetFlatVolume(TT.strConfigSFX,newVolume);
	}

	//==========================================================================================================
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	new void Awake () {
		if(_instance == null){
			base.Awake();
			_instance = this;
			zzSetFlatVolume();
		} else
			Destroy(gameObject);
	}
}
