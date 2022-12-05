using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Video;

public class TrTitleScript : MonoBehaviour
{
    VideoPlayer _vidPlayer;
    public GameObject _rainbowSparkParticle;
    AsyncOperation async;

    private void Awake()
	{
        _vidPlayer = GetComponent<VideoPlayer>();
	}

	void Start()
    {
        async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
        async.allowSceneActivation = false;
        _vidPlayer.loopPointReached += yLoadNextScene;
        _vidPlayer.Play(); 
    }

    void Update() {
        //비디오가 플레이되고 2.3초가 지나면
        //(좀더 정확한 타이밍을 위해선 _vidPlayer.time 대신 _vidPlayer.frame을 사용)
        if(!_rainbowSparkParticle.activeSelf && _vidPlayer.time > 2.3f) {
            //만약 파티클이 비디오에 가려져 보이지 않는다면 비디오플레이어 컴포넌트의 render mode 값을 Camera Far Plane으로 변경하시오!
            _rainbowSparkParticle.SetActive(true);
		}
	}

    void yLoadNextScene(VideoPlayer vp) {
        TT.UtilDelayedFunc.zCreate(() => async.allowSceneActivation = true,1f);
	}

	private void OnDestroy() {
        _vidPlayer.loopPointReached -= yLoadNextScene;
        _vidPlayer = null;
	}
}
