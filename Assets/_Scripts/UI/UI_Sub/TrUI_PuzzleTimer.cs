using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrUI_PuzzleTimer : MonoBehaviour
{
	static TrUI_PuzzleTimer _instance;
    public Image _timerBar, _imgMode, _imgClock;
	float _origFontSize;

	[SerializeField] Sprite _spChallenege;
	[SerializeField] Sprite _spTrain;

	float _tictokTime;
	float _waitTime = 1f;

	Color _colRed = new Color(1, 0.38f, 0.28f, 1);
	Color _colBlue = new Color(0.6f, 0.68f, 0.96f, 1);


	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public static TrUI_PuzzleTimer xInstance { get { return _instance; } }

	void ySetTimerColor(float r, float g, float b, float a=1){
		Color color = new Color(r, g, b, 1);
		_timerBar.color = color;
		//_txtTimer.color = color;
	}

	void ySetTimerColor(Color col)
	{
		Color color = col;
		_timerBar.color = color;
		//_txtTimer.color = color;
	}

	public void zUpdateTimerBar(int timeMax, float currRemainTime) {
		if (currRemainTime < 0) currRemainTime = 0;
		float fillRate = currRemainTime / timeMax;
		_timerBar.fillAmount = fillRate;

		//남은 시간에 따라 타임바의 색깔을 바꿈.
		/*if (fillRate > .6f) {
			ySetTimerColor(0.5f, 0.8f, 0, 1);
		} else if (fillRate > 0.3f) {
			ySetTimerColor(TT.zSetColor(TT.enumTrRainbowColor.YELLOW));
		} else {
			ySetTimerColor(1, 0, 0, 1);
		}*/

	}

	//================================================================================================================

	// 유니티 메뉴에서 Edit>Project Settings>Editor 항목의 하단에 enter play mode 가 체크되되고 reload domain이 체크해제되게
	// 설정되어 있다면 유니티 자체를 포함한 모든 static변수의 값은 직접 리셋해주어야 함.
	// 특히 static delegate/event의 리스너 등록이나 static 데이터구조를 사용할 시 매우 주의해야 함.
	// 단, 이것은 에디터상의 편의를 위한것이므로 실제빌드 결과물엔 영향없음.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	void Awake() {
		if(_instance == null) {
			_instance = this;
			//_origFontSize = _txtTimer.fontSize;
		} else {
			Destroy(gameObject);
		}
		_timerBar.fillAmount = 1;
	}

	void Start(){
		if (GameManager._type == TT.enumGameType.Train){
			_imgMode.sprite = _spTrain;
			_imgClock.color = _colBlue;
			_timerBar.color = _colBlue;
		}
		else if (GameManager._type == TT.enumGameType.Challenge){
			_imgMode.sprite = _spChallenege;
			_imgClock.color = _colRed;
			_timerBar.color = _colRed;
		}
		_imgMode.SetNativeSize();
	}
}
