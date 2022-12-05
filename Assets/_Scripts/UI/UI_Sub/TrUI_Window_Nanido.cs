using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TrUI_Window_Nanido : TrUI_Window_
{
	static TrUI_Window_Nanido _instance;
	public GameObject _imgEasyActive, _imgEasyInactive, _imgNormalActive, _imgNormalInactive, _imgHardActive, _imgHardInactive;
	public TextMeshProUGUI _textEasy, _textNormal, _textHard;
	TT.enumDifficultyLevel _difficultySelected = TT.enumDifficultyLevel.Normal;

	static public TrUI_Window_Nanido xInstance { get { return _instance; } }
	public TT.enumDifficultyLevel xxSelectedDifficulty { get { return _difficultySelected; } }

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

	new void Start() {
		base.Start();

		zzOnNormalClick();
	}


	//============================================================================================================
	public void zzOnEasyClick() {
		TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();

		_imgEasyActive.SetActive(true);
		_imgEasyInactive.SetActive(false);
		_textEasy.color = Color.black;
		_imgNormalActive.SetActive(false);
		_imgNormalInactive.SetActive(true);
		_textNormal.color = Color.grey;
		_imgHardActive.SetActive(false);
		_imgHardInactive.SetActive(true);
		_textHard.color = Color.grey;

		_difficultySelected = TT.enumDifficultyLevel.Easy;
	}

    public void zzOnNormalClick() {
		TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();

		_imgEasyActive.SetActive(false);
		_imgEasyInactive.SetActive(true);
		_textEasy.color = Color.grey;
		_imgNormalActive.SetActive(true);
		_imgNormalInactive.SetActive(false);
		_textNormal.color = Color.white;
		_imgHardActive.SetActive(false);
		_imgHardInactive.SetActive(true);
		_textHard.color = Color.grey;

		_difficultySelected = TT.enumDifficultyLevel.Normal;
	}

    public void zzOnHardClick() {
		TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();

		_imgEasyActive.SetActive(false);
		_imgEasyInactive.SetActive(true);
		_textEasy.color = Color.grey;
		_imgNormalActive.SetActive(false);
		_imgNormalInactive.SetActive(true);
		_textNormal.color = Color.grey;
		_imgHardActive.SetActive(true);
		_imgHardInactive.SetActive(false);
		_textHard.color = Color.white;

		_difficultySelected = TT.enumDifficultyLevel.Hard;
	}
}
