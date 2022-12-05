using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrLobbyStarEffect : MonoBehaviour
{
    [SerializeField] GameObject _goStarTwinkleEffect;

    IEnumerator yTwinkle(){
        while (true){
            float targetValue = _goStarTwinkleEffect.transform.localScale.x > 0.9f ? 0.7f : 1.5f;

            float randTime = Random.Range(1, 5f);

            _goStarTwinkleEffect.transform.DOScale(targetValue, randTime);

            yield return new WaitForSeconds(randTime);
        }
    }

    void Start(){
        StartCoroutine(yTwinkle());
    }
}
