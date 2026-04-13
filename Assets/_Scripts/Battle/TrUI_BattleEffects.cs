using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// 대결 체력바 이펙트 컴포넌트.
/// 정답 맞출 때 충돌 포인트에서 $ 기호가 방사형으로 튀어나오며 위로 떠오르는 연출.
/// (스파크 타격감 + 달러 보상감 통합)
/// TrUI_BattleHPBar와 분리된 별도 컴포넌트 — 같은 GameObject에 추가.
/// </summary>
public class TrUI_BattleEffects : MonoBehaviour
{
    [Header("달러 팝업 설정")]
    [SerializeField] TMP_Text    _dollarPrefab;      // "$" 텍스트 프리팹
    [SerializeField] RectTransform _tfEffectParent;  // 이펙트 생성 부모 (바 컨테이너와 동일 좌표계)
    [SerializeField] int         _poolSize    = 12;
    [SerializeField] float       _riseHeight  = 90f;
    [SerializeField] float       _spreadRadius = 50f;
    [SerializeField] float       _duration    = 0.6f;
    [SerializeField] int         _countPerBurst = 4;  // 한 번 호출 시 달러 개수

    readonly Queue<TMP_Text> _pool = new Queue<TMP_Text>();

    void Awake()
    {
        yBuildPool();
    }

    void yBuildPool()
    {
        if (_dollarPrefab == null) return;

        for (int i = 0; i < _poolSize; i++)
        {
            var obj = Instantiate(_dollarPrefab, _tfEffectParent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    // ── 외부 호출 ──────────────────────────────────────────────

    /// <summary>정답 시 달러 이펙트 (위 반원 방사)</summary>
    public void zPlayAt(Vector2 anchoredPos)
    {
        if (_dollarPrefab == null || _tfEffectParent == null) return;
        for (int i = 0; i < _countPerBurst; i++)
            yPopDollar(anchoredPos, i, halfCircle: true);
    }

    /// <summary>게임 시작 충돌 대폭발 (전방향 360도 버스트)</summary>
    public void zPlayCollisionBurst(Vector2 anchoredPos)
    {
        if (_dollarPrefab == null || _tfEffectParent == null) return;
        int burstCount = Mathf.Min(_pool.Count, _countPerBurst * 3);
        for (int i = 0; i < burstCount; i++)
            yPopDollar(anchoredPos, i, halfCircle: false);
    }

    // ── 내부 ───────────────────────────────────────────────────

    void yPopDollar(Vector2 origin, int index, bool halfCircle)
    {
        if (_pool.Count == 0) return;

        TMP_Text txt = _pool.Dequeue();
        txt.gameObject.SetActive(true);
        txt.rectTransform.anchoredPosition = origin;
        txt.rectTransform.localScale = Vector3.one * 0.6f;

        // halfCircle=true: 위 반원(정답 이펙트), false: 360도(충돌 버스트)
        int total = halfCircle ? _countPerBurst : _countPerBurst * 3;
        float minAngle = halfCircle ? 30f : 0f;
        float maxAngle = halfCircle ? 150f : 360f;
        float angle = Mathf.Lerp(minAngle, maxAngle, (float)index / Mathf.Max(total - 1, 1));
        float rad   = angle * Mathf.Deg2Rad;
        Vector2 spread = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _spreadRadius;
        Vector2 target = origin + spread + new Vector2(0f, _riseHeight);

        // 초기 색상 (노란 달러색)
        txt.color = new Color(1f, 0.9f, 0.1f, 1f);

        Sequence seq = DOTween.Sequence();
        seq.Join(txt.rectTransform.DOAnchorPos(target, _duration).SetEase(Ease.OutCubic));
        seq.Join(txt.rectTransform.DOScale(1.2f, _duration * 0.25f)
                    .SetLoops(2, LoopType.Yoyo));
        seq.Join(txt.DOFade(0f, _duration).SetEase(Ease.InQuad).SetDelay(_duration * 0.3f));
        seq.SetDelay(index * 0.04f); // 약간 순차 발사
        seq.OnComplete(() =>
        {
            if (txt != null)
            {
                txt.gameObject.SetActive(false);
                _pool.Enqueue(txt);
            }
        });
    }
}
