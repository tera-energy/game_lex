using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrFish : MonoBehaviour
{
    public TT.enumTrRainbowColor _color;
    //[SerializeField] Sprite[] _spritesFish; // 물고기의 Sprite들
    [SerializeField] GameObject[] _goFishs;
    [SerializeField] SpriteRenderer _srBounceFish;
    [SerializeField] SpriteRenderer _srHideFish;
    Rigidbody2D _rb;
    public Vector2[] _limitMovePos = new Vector2[2]; // 경로의 최소, 최대 위치
    Vector2 _targetPos;
    Vector2 _currDir;
    float _moveSpeed; // 물고기들의 이동 속도
    float _bounceSpeed; // 물고기의 튀어오르는 속도
    [SerializeField] float _bouncePosY; // 물고기가 어디까지 올라가는지
    [SerializeField] float _flopSpeed;
    [SerializeField] float _valueWaitMaxTime;
    bool _isBounce = false; // 튀어오르는 중인지
    bool _isMove = false;
    bool _isCheck = false;
    [SerializeField] ParticleSystem _psBounce;
    [SerializeField] ParticleSystem _psHideMove;

    public void zStopMove(){
        _isMove = false;
        _isBounce = false;
        StopAllCoroutines();
    }

    public void zCheckColors(){
        _isMove = false;
        _isBounce = true;
        _isCheck = true;
        StartCoroutine(yFlopCoroutine());
    }

    // 생성하면서 물고기의 정보 Setting
    public void zSetFishInfo(int numColor, Vector2 moveSpeed, float bounceSpeed, Vector2[] limitMovePos){
        _isBounce = false;
        _color = (TT.enumTrRainbowColor)numColor;
        _srBounceFish.color = TT.zSetColor(_color);
        _moveSpeed = Random.Range(moveSpeed.x, moveSpeed.y);
        _bounceSpeed = bounceSpeed;
        _limitMovePos = limitMovePos;
        yModeChange(_isBounce);
        StartCoroutine(yWait());
    }

    // 물고기가 터치에 의해 튀어오르는 로직
    public bool zBounceFish(){
        if (_isBounce) return false;
        _isBounce = true;
        _psBounce.Play();
        StartCoroutine(yBounceCoroutine());
        return true;
    }
    
    // 튀어오를 때 파닥거리는 코루틴
    IEnumerator yFlopCoroutine(){
        _rb.velocity = Vector2.zero;
        yModeChange(_isBounce);
        int right = 1;
        while (_isBounce)
        {
            Vector3 scale = _goFishs[1].transform.localScale;
            scale.x = _goFishs[1].transform.localScale.x * right;
            _goFishs[1].transform.localScale = scale;
            right *= -1;
            yield return new WaitForSeconds(_flopSpeed);
        }

        //ySetFlip();
    }

    // 튀어오르는 코루틴
    IEnumerator yBounceCoroutine(){
        float originPosY = transform.position.y;
        float posY = transform.position.y + _bouncePosY;

        StartCoroutine(yFlopCoroutine());

        while(transform.position.y <= posY){
            transform.position += Vector3.up * _bounceSpeed * Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector2(transform.position.x, posY);


        while (transform.position.y >= originPosY){
            transform.position += Vector3.down * _bounceSpeed * Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector2(transform.position.x, originPosY);

        if (!_isCheck){
            _isBounce = false;
            yModeChange(_isBounce);
        }
    }

    IEnumerator ySwimEffect(){
        bool isTrue = true;
        float swimSpeed = Random.Range(0.1f, 0.2f);
        while (_isMove){
            _srHideFish.flipY = isTrue;
            isTrue = !isTrue;
            _psHideMove.Play();
            yield return new WaitForSeconds(swimSpeed);
        }
    }

    // 랜덤으로 다음 경로 Setting
    void ySetPath(){
        float randX = Random.Range(_limitMovePos[0].x, _limitMovePos[1].x);
        float randY = Random.Range(_limitMovePos[0].y, _limitMovePos[1].y);
        _targetPos = new Vector2(randX, randY);
        //ySetFlip();
        _currDir = (Vector3)_targetPos - transform.position;
        
        _isMove = true;
        StartCoroutine(ySwimEffect());
    }

    /// <summary>
    /// bounce : true
    /// shadow : false,
    /// </summary>
    /// <param name="isBounce"></param>
    void yModeChange(bool isBounce){
        if (!isBounce){
            if (!_isCheck){
                _goFishs[0].SetActive(true);
                _goFishs[1].SetActive(false);
            }
        }
        else{
            _goFishs[0].SetActive(false);
            _goFishs[1].SetActive(true);
        }
    }

    // 목적지에 도착 후 잠시 기다리는(휴식) 코루틴
    IEnumerator yWait(){
        _isMove = false;
        _rb.velocity = Vector2.zero;
        float randTime = Random.Range(0, _valueWaitMaxTime);
        yield return new WaitForSeconds(randTime);
        ySetPath();
    }

    // 물고기의 움직임
    void yMove(){
        if (Vector2.Distance(_targetPos, transform.position) <= 0.1f){
            StartCoroutine(yWait());
            return;
        }

        _rb.velocity = _currDir.normalized * _moveSpeed;

        //float value = Vector2.Lerp(transform.position, _targetPos, 5f);
        float angle = Mathf.Atan2(_currDir.y, _currDir.x) * Mathf.Rad2Deg;
        _goFishs[0].transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Awake(){
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if(_isMove && !_isBounce)
            yMove();

        if (transform.position.x < _limitMovePos[0].x || transform.position.x > _limitMovePos[1].x
            || transform.position.y < _limitMovePos[0].y || transform.position.y > _limitMovePos[1].y)
        {
            ySetPath();
        }
    }

}
