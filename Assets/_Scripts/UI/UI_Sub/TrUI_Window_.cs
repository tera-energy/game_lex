using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[SelectionBase]

public class TrUI_Window_ : MonoBehaviour
{
	public GameObject _cancelField;
	[Space(10f)]
	[SerializeField] Vector2 _showPos;
	[SerializeField] float _showTweenTime = 0.75f;
	[SerializeField] Ease _showTweenEase = Ease.OutElastic;
	public AudioClip _showSFX;
	public float _showSoundDelay;
	[Space(10f)]
	[SerializeField] Vector2 _hidePos;
	[SerializeField] float _hideTweenTime = 0.5f;
	[SerializeField] Ease _hideTweenEase = Ease.InBack;
	[SerializeField] bool _hideAtStart = true;
	public AudioClip _hideSFX;
	public float _hideSoundDelay;

	Tweener _tween;
	Button _cancelFieldButton;
	Vector2 _clickStartPos;

	public bool xIsShowing { get { return gameObject.activeSelf; } }

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	protected void Start() {
		transform.DOLocalMove(_hidePos, 0);
		if(_cancelField != null)
			_cancelFieldButton = _cancelField.GetComponent<Button>();

		if(_hideAtStart) {
			transform.localPosition = _hidePos;
			gameObject.SetActive(false);
			if(_cancelField!=null)	_cancelField.SetActive(false);
		}
	}

	protected void Update() {
		if(Input.GetMouseButtonDown(0)) {
			_clickStartPos = Input.mousePosition;
		}

		if(Input.GetMouseButton(0)) {
			//드래그중엔 취소영역 버튼 비활성.
			if(Vector2.Distance(_clickStartPos,Input.mousePosition) > 25f)
				zSetCancelFieldButtonInteraction(false);
		}

		if(Application.platform == RuntimePlatform.Android && Input.touchCount>0) {
			if(Input.touches[0].phase == TouchPhase.Ended) {
				zSetCancelFieldButtonInteraction(true);
				_clickStartPos = Vector2.negativeInfinity;
			}
		} else {
			if(Input.GetMouseButtonUp(0)) {
				zSetCancelFieldButtonInteraction(true);
				_clickStartPos = Vector2.negativeInfinity;
			}
		}
	}

	//================================================================================================================

	public virtual void zShow(bool ignoreTimeScale=true) {
		if(xIsShowing) return;
		
		gameObject.SetActive(true);
		if(_cancelField!=null) _cancelField.SetActive(true);

		if(_showSFX)	TrAudio_UI.xInstance.zPlaySFX(_showSFX, _showSoundDelay);

		if(_tween != null) _tween.Kill();
		_tween = transform.DOLocalMove(_showPos,_showTweenTime).SetEase(_showTweenEase).SetUpdate(ignoreTimeScale);
	}

	public virtual void zHide(bool ignoreTimeScale = true) {
		if(!xIsShowing) return;

		if(_cancelField!=null)	_cancelField.SetActive(false);

		if(_hideSFX) TrAudio_UI.xInstance.zPlaySFX(_hideSFX, _hideSoundDelay);
		_tween?.Kill();
		_tween = transform.DOLocalMove(_hidePos,_hideTweenTime).SetEase(_hideTweenEase).SetUpdate(ignoreTimeScale).OnComplete(()=>gameObject.SetActive(false));
	}

	/// <summary>
	/// Pass a negative value for tweenTime if it needs to be shown immediately.
	/// </summary>
	public virtual void zManuallyShow(Vector2? showPos, float? tweenTime, Ease? ease, bool playSFX=false, bool ignoreTimeScale= true) {
		if(xIsShowing) return;

		gameObject.SetActive(true);
		if(_cancelField!=null) _cancelField.SetActive(true);

		if(playSFX && _showSFX) TrAudio_UI.xInstance.zPlaySFX(_showSFX,_showSoundDelay);
		if(showPos == null)	showPos = _showPos;
		if(tweenTime == null) tweenTime = _showTweenTime;
		else if(tweenTime < 0) {
			transform.localPosition = _showPos;
			return;
		}
		if(ease == null) ease = _showTweenEase;

		_tween?.Kill();
		_tween = transform.DOLocalMove((Vector2)showPos, (float)tweenTime).SetEase((Ease)ease).SetUpdate(ignoreTimeScale);
	}

	/// <summary>
	/// Pass a negative value for tweenTime if it needs to be hidden immediately.
	/// </summary>
	public virtual void zManuallyHide(Vector2? hidePos, float? tweenTime, Ease? ease, bool playSFX=false, bool ignoreTimeScale= true) {
		if(!xIsShowing) return;

		if(_cancelField!=null) _cancelField.SetActive(false);

		if(playSFX && _hideSFX) TrAudio_UI.xInstance.zPlaySFX(_hideSFX,_hideSoundDelay);
		if(hidePos == null) hidePos = _hidePos;
		if(tweenTime == null) tweenTime = _hideTweenTime;
		else if(tweenTime < 0) {
			transform.localPosition = _hidePos;
			gameObject.SetActive(false);
			return;
		}
		if(ease == null) ease = _hideTweenEase;

		_tween?.Kill();
		_tween = transform.DOLocalMove((Vector2)hidePos,(float)tweenTime).SetEase((Ease)ease).SetUpdate(ignoreTimeScale).OnComplete(() => gameObject.SetActive(false));
	}

	public void zSetCancelFieldButtonInteraction(bool isInteractable) {
		if(_cancelFieldButton != null)
			_cancelFieldButton.interactable = isInteractable;
	}
}
