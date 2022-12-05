using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrResultManager : MonoBehaviour
{
/*    [SerializeField] GameObject[] _twinkleStar;
    //[SerializeField] GameObject _canvas;
    [SerializeField] int _maxStar;
    [SerializeField] float _minXPos, _maxXPos, _minYPos, _maxYPos;
    bool _isResult;
    static TrResultManager _instance;
    public static TrResultManager xInstance { get { return _instance; } }

    Queue<GameObject> _starQueue = new Queue<GameObject>();


    *//*void yInitStar(){
        for(int i=0; i<_maxStar; i++){
            int randStar = Random.Range(0, _twinkleStar.Length);
            GameObject star = Instantiate(_twinkleStar[randStar]);
            yResetStar(ref star);
        }
    }

    void yResetStar(ref GameObject star){
        star.transform.localScale = Vector2.zero;
        _starQueue.Enqueue(star);
    }

    IEnumerator yTwinkleStar(){
        while (true){
            if (_starQueue.Count == 0)
                yield return new WaitUntil(() => _starQueue.Count != 0);
            GameObject star = _starQueue.Dequeue();
            float randXPos = Random.Range(_minXPos, _maxXPos);
            float randYPos = Random.Range(_minYPos, _maxYPos);
            star.transform.localPosition = new Vector2(randXPos, randYPos);

            float randScale = Random.Range(0.5f, 1.5f);
            star.transform.localScale = new Vector2(randScale, randScale);

            Vector3 targetScale = Vector3.zero;
            float randScaleTime = Random.Range(0.5f, 1.5f);
            star.transform.DOScale(targetScale, randScaleTime).OnComplete(() => yResetStar(ref star));

            float randRot = Random.Range(0, 360);
            star.transform.localEulerAngles = new Vector3(0, 0, randRot);

            float randTime = Random.Range(0.1f, 0.3f);
            yield return new WaitForSeconds(randTime);
        }
    }*//*

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }*/

}
