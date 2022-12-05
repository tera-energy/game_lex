using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class TrAnimalsSmoke : MonoBehaviour
{
    float _initX;
    float _targetX;
    bool _isTarget = false;

    IEnumerator yMoveSmoke(){
        while (true){
            float targetX;
            if (_isTarget)
                targetX = _initX;
            else
                targetX = _targetX;
            Tween move;
            float randSpeed = Random.Range(0.1f, 0.7f);
            move = transform.DOMoveX(targetX, randSpeed).SetSpeedBased();

            float randTime = Random.Range(3, 5);
            yield return TT.WaitForSeconds(randTime);
            move.Kill();
            _isTarget = !_isTarget;
        }
    }

    void Awake()
    {
        _initX = transform.position.x;
        _targetX = -1 * _initX;
    }

    void OnEnable()
    {
        transform.position = new Vector3(_initX, transform.position.y, transform.position.z);
        StartCoroutine(yMoveSmoke());
    }
}
