using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrAudio_Music : TrAudio_ {
	public AudioClip  _puzzleSolved, _gameover;
	[Space]
	[Tooltip("루프되지 않는 음악의 첫 도입부")] public AudioClip _headClip;
	[Tooltip("루프되는 메인음악")] public AudioClip _bodyClip;
	AudioSource _head, _body;
	static TrAudio_Music _instance = null;
	Coroutine _procPlayHeadBody;

	/*///////////////////////////////////////////////////////////////////////////////////////////////////////////*/
	public static TrAudio_Music xInstance{get{return _instance;}}

	//==========================================================================================================
	public void zzPlayMain(float delay){
		if(_headClip != null) {
			_head.volume = _audioSource.volume;
			_head.pitch = _audioSource.pitch;
			_head.Stop();
			_body.Stop();
			if(_procPlayHeadBody != null) StopCoroutine(_procPlayHeadBody);

			double startTime = AudioSettings.dspTime+delay+0.5; //add extra 0.5 second to compensate audio preparation time.
			double duration = (double)_headClip.samples/_headClip.frequency;
			_head.clip = _headClip;
			_head.loop = false;
			_head.PlayScheduled(startTime);
			_audioSource = _head;

			_body.clip = _bodyClip;
			_body.loop = true;
			_body.PlayScheduled(startTime+duration);
			_procPlayHeadBody = StartCoroutine(yProcPlayHeadBody((float)duration+delay-0.5f));
		} else {
			zPlayMusic(_bodyClip,true,delay);
		}
	}

	public void zzPlayMain(float delay, AudioClip clip)
	{
		_bodyClip = clip;
		zzPlayMain(delay);
    }

	public void zzPlayPuzzleSolved(float delay)			{zPlayMusic(_puzzleSolved, true, delay);}
	public void zzPlayGameOver(float delay)				{zPlayMusic(_gameover, false, delay);}
	
	public void zzSetFlatVolume(float? newVolume=null) {
		zSetFlatVolume(TT.strConfigMusic,newVolume);
		if(newVolume < 0) zStopMusic();
	}

	//==========================================================================================================
	IEnumerator yProcPlayHeadBody(float scheduledTime){
		yield return new WaitForSecondsRealtime(scheduledTime);
		if(!_audioSource.isPlaying || _audioSource.clip != _headClip){
			_body.Stop();
		}else{
			_body.volume = _audioSource.volume;
			_body.pitch = _audioSource.pitch;
			_audioSource = _body;
			
		}
	}

	//==========================================================================================================
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	new void Awake () {
		if(_instance == null){
			base.Awake();
			_head = transform.GetChild(0).GetComponent<AudioSource>();
			_body = transform.GetChild(1).GetComponent<AudioSource>();
			_instance = this;
			zzSetFlatVolume();
		}else
			Destroy(gameObject);
	}
}
