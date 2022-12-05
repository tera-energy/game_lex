using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TrAudio_ : MonoBehaviour {
	protected AudioSource _audioSource;
	Tweener _tweenPitch, _tweenVolume;
	Camera _cam;
	Transform _camTransform;
	int _currCamSize = 8;
	float _camOffset = 0f;
	float _wait = .333f;
	protected float _volume = 1f;

	/*//////////////////////////////////////////////////////////////////////////////////////////////////////////////*/


	//===========================================================================================

	void yPlaySFX(AudioClip clip, Vector3 sourcePos=default(Vector3)){
		if(_camTransform != null && sourcePos != default(Vector3)){
			if(Vector2.Distance(_camTransform.position, sourcePos) < 22f+(_camOffset*1.75f)){
				_audioSource.PlayOneShot(clip);
			}
		}else{
			_audioSource.PlayOneShot(clip);
		}
	}

	public void zPlaySFX(AudioClip clip, float delay=-1f, Vector3 sourcePos=default(Vector3)){
		if(delay < 0f) yPlaySFX(clip,sourcePos);
		else {
			if(_camTransform != null && sourcePos != default(Vector3)) {
				if(Vector2.Distance(_camTransform.position,sourcePos) < 22f+(_camOffset*1.75f)) {
					_audioSource.clip = clip;
					_audioSource.loop = false;
					TT.UtilDelayedFunc.zCreate(()=>_audioSource.PlayOneShot(clip), delay);
				}
			} else {
				_audioSource.clip = clip;
				_audioSource.loop = false;
				TT.UtilDelayedFunc.zCreate(() => _audioSource.PlayOneShot(clip),delay);
			}
		}
	}

	public void zPlayMusic(AudioClip clip, bool isLoop, float delay=0f){
		_audioSource.clip = clip;
		_audioSource.loop = isLoop;
		_audioSource.PlayDelayed(delay);
	}

	public void zStopMusic(){
		_audioSource.Stop();
	}

	public void zChangePitch(float value){
		_audioSource.pitch = value;
	}

	//컷씬 이벤트나 카메라 거리 등 게임 상에서 볼륨을 직접 조절할 때 쓰이는 함수.
	public void zChangeVolume(float value){
		_audioSource.volume = value;
	}

	//게임 옵션 등에서 유저가 설정하는 볼륨 조절.
	protected void zSetFlatVolume(string configType, float? newVolume){
		if(newVolume != null) {
			_volume = (float)newVolume;
			PlayerPrefs.SetFloat(configType,_volume);
		} else {
			_volume = PlayerPrefs.GetFloat(configType, 1);
		}
		zChangeVolume(_volume);
	}

	public void zChangePitchOverTime(float from, float to, float time){
		if(_tweenPitch != null)	_tweenPitch.Kill();
		_tweenPitch = DOTween.To(()=>from, x=>_audioSource.pitch=x, to, time).Play();
	}

	public void zChangeVolumeOverTime(float from, float to, float time){
		if(_tweenVolume != null)	_tweenVolume.Kill();
		_tweenVolume = DOTween.To(()=>from, x=>_audioSource.volume=x, to, time).Play();
	}

	public bool zIsPlaying(){
		return _audioSource.isPlaying;
	}

	public AudioClip zGetCurrentAudioClip(){
		return _audioSource.clip;
	}

	//===========================================================================================

	IEnumerator yProcVolumeAdjustToCamSize(){
		if(Camera.main == null)		yield break;

		if(_cam == null){
			_cam = Camera.main;
			_camTransform = _cam.transform;
		}

		while(true){
			_camOffset = (_cam.orthographicSize - 8f);

			if(_currCamSize != (int)_cam.orthographicSize){
				_currCamSize = (int)_cam.orthographicSize;
				float newVolume = 1.0f - _camOffset*.08f;
				if(newVolume > 1f)			newVolume = 1f;
				else if(newVolume < .4f)	newVolume = .4f;
				zChangeVolume(newVolume*_volume);
			}

			yield return _wait;
		}
	}

	void yOnChangingPitch(float value){
		_audioSource.pitch = value;
	}

	void yOnChangingVolume(float value){
		_audioSource.volume = value;
	}

	void yDelayedStart(){
		StartCoroutine(yProcVolumeAdjustToCamSize());
	}

	//===========================================================================================

	protected void Awake(){
		_audioSource = GetComponent<AudioSource>();
		_volume = _audioSource.volume;
		//Invoke(nameof(yDelayedStart),.5f);
	}
}
