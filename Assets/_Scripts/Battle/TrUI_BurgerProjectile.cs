using UnityEngine;
using DG.Tweening;

/// <summary>
/// 햄버거 완성 시 아이콘이 체력 바 방향으로 날아가는 포물선 애니메이션.
///
/// Fix #5: DOJump는 월드 좌표 기준으로 동작하므로 UI에서 사용할 때
///         DOLocalJump + 로컬 좌표 기준으로 통일해야 Canvas 모드에 관계없이 안전.
///         Instantiate 후 부모 공간에서 localPosition을 기준으로 이동.
/// </summary>
public class TrUI_BurgerProjectile : MonoBehaviour
{
    static TrUI_BurgerProjectile _instance;
    public static TrUI_BurgerProjectile xInstance => _instance;

    [SerializeField] GameObject _goProjectilePrefab;  // 날아갈 버거 아이콘 프리팹
    [SerializeField] RectTransform _tfLaunchPoint;    // 발사 위치 (조립 트레이 위) — RectTransform으로 변경
    [SerializeField] RectTransform _tfTargetPoint;    // 목표 위치 (상대 HP 바) — RectTransform으로 변경

    [SerializeField] float _flightDuration = 0.6f;
    [SerializeField] float _arcHeight      = 120f;   // 포물선 높이 (UI 로컬 좌표 단위)

    // Fix #6: 동일한 구독 재시도 패턴 적용
    bool _isSubscribed = false;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        yTrySubscribe();
    }

    void OnDisable()
    {
        yUnsubscribe();
    }

    void yTrySubscribe()
    {
        if (_isSubscribed) return;
        var bm = TrBattleManager.xInstance;
        if (bm == null) return;
        bm.OnGaugeChanged += yOnGaugeChanged;
        bm.OnMatchFound   += yOnMatchFound;
        _isSubscribed = true;
    }

    void yUnsubscribe()
    {
        if (!_isSubscribed) return;
        var bm = TrBattleManager.xInstance;
        if (bm != null)
        {
            bm.OnGaugeChanged -= yOnGaugeChanged;
            bm.OnMatchFound   -= yOnMatchFound;
        }
        _isSubscribed = false;
    }

    void Update()
    {
        // Fix #6: TrBattleManager 지연 생성 대비 재시도
        if (!_isSubscribed)
            yTrySubscribe();
    }

    float _prevMyRatio = 0.5f;

    // OnMatchFound: 새 매치 시작 시 비율 초기값으로 리셋 (이전 게임 상태 오염 방지)
    void yOnMatchFound() => _prevMyRatio = 0.5f;

    void yOnGaugeChanged(float myRatio)
    {
        // 내가 게이지를 밀었을 때(myRatio 증가) 발사
        if (myRatio > _prevMyRatio)
            zLaunch();
        _prevMyRatio = myRatio;
    }

    public void zLaunch()
    {
        if (_goProjectilePrefab == null || _tfLaunchPoint == null || _tfTargetPoint == null)
            return;

        // Fix #5: 부모를 LaunchPoint의 부모로 지정해 로컬 좌표계 통일
        Transform parent = _tfLaunchPoint.parent != null ? _tfLaunchPoint.parent : transform;
        GameObject proj  = Instantiate(_goProjectilePrefab, parent);
        var projRect     = proj.GetComponent<RectTransform>();

        if (projRect == null)
        {
            // RectTransform이 없는 비-UI 프리팹이라면 월드 좌표 폴백
            proj.transform.position = _tfLaunchPoint.position;
            proj.SetActive(true);
            proj.transform.DOJump(_tfTargetPoint.position, _arcHeight, 1, _flightDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => Destroy(proj));
            return;
        }

        // Fix #5: RectTransform 기준 — 부모 로컬 좌표로 통일
        //         anchoredPosition을 사용해 Canvas 모드(Overlay/Camera/World)에 무관하게 동작
        projRect.anchoredPosition = _tfLaunchPoint.anchoredPosition;
        proj.SetActive(true);

        Vector2 targetLocal = _tfTargetPoint.anchoredPosition;

        // Fix #5: DOLocalJump 대신 anchoredPosition 기반 커스텀 포물선 사용
        //         DOJump/DOLocalJump는 내부적으로 transform.localPosition을 조작하므로
        //         RectTransform의 anchoredPosition과 동기화되지 않을 수 있음.
        //         DOAnchorPos + DOVirtual로 Y 오프셋을 합산해 포물선 구현.
        yAnimateProjectile(projRect, targetLocal, proj);
    }

    void yAnimateProjectile(RectTransform projRect, Vector2 targetPos, GameObject proj)
    {
        Vector2 startPos = projRect.anchoredPosition;
        float elapsed    = 0f;

        // DOTween의 DOVirtual.Float으로 t(0→1)를 구동하고
        // 포물선 수식으로 anchoredPosition을 직접 제어
        DOVirtual.Float(0f, 1f, _flightDuration, t =>
        {
            if (projRect == null) return;

            // 선형 보간 위치
            Vector2 linear = Vector2.LerpUnclamped(startPos, targetPos, t);
            // 포물선 Y 오프셋: sin(π*t) 곡선
            float arcOffset = Mathf.Sin(Mathf.PI * t) * _arcHeight;

            projRect.anchoredPosition = new Vector2(linear.x, linear.y + arcOffset);
        })
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (proj != null) Destroy(proj);
        });
    }
}
