using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// 체력 줄다리기 바 UI.
/// fillAmount 기반 슬라이더 방식. _tfCollisionPoint가 두 바의 경계 마커 역할.
/// 상대 햄버거 완성 시 화면 테두리 빨간 플래시 효과.
/// </summary>
public class TrUI_BattleHPBar : MonoBehaviour
{
    static TrUI_BattleHPBar _instance;
    public static TrUI_BattleHPBar xInstance => _instance;

    [Header("색상 설정")]
    [SerializeField] Color _myColor         = new Color(0.92f, 0.18f, 0.18f, 1f);
    [SerializeField] Color _oppColor        = new Color(0.18f, 0.48f, 0.92f, 1f);
    [SerializeField] Color _backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

    [Header("바 (배경 + 내 fill + 상대 fill)")]
    [SerializeField] Image _imgBarBackground;
    [SerializeField] Image _imgMyBar;
    [SerializeField] Image _imgOppBar;  // Inspector: Image Type=Filled, Fill Origin=Right

    [Header("경계 마커")]
    [SerializeField] RectTransform _tfCollisionPoint;
    [SerializeField] float         _barWidth = 600f;

    [Header("플레이어 이름")]
    [SerializeField] TextMeshProUGUI _txtMyName;
    [SerializeField] TextMeshProUGUI _txtOppName;

    [Header("방향성 각인 플래시")]
    [SerializeField] GameObject _goMySideFlash;

    [Header("상대 완성 효과 (화면 테두리 플래시)")]
    [Tooltip("화면 가장자리 빨간 CanvasGroup — Image Type=Sliced or Simple, 반투명 빨간색")]
    [SerializeField] CanvasGroup _cgOppFlash;

    [Header("이펙트")]
    [SerializeField] TrUI_BattleEffects _battleEffects;

    const float AnimDuration = 0.4f;

    float _prevFill    = 0.5f;
    bool  _isSubscribed = false;

    // ── 생명주기 ───────────────────────────────────────────────

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        yApplyColors();

        // Awake()는 모든 Start()보다 먼저 실행됨
        // TrMatchingSession.Clear()는 TrBattleManager.Start()에서 호출되므로 여기서는 아직 유효
        // OnMatchFound 이벤트 방식은 실행 순서 레이스로 인해 미수신 → 직접 읽기
        if (_txtMyName  != null) _txtMyName.text  = TrMatchingSession.MyNickname  ?? "나";
        if (_txtOppName != null) _txtOppName.text = TrMatchingSession.OppNickname ?? "상대";
    }

    void OnEnable()  => yTrySubscribe();
    void OnDisable() => yUnsubscribe();

    void yTrySubscribe()
    {
        if (_isSubscribed) return;
        var bm = TrBattleManager.xInstance;
        if (bm == null) return;

        bm.OnGaugeChanged       += OnGaugeChanged;
        bm.OnProgressChanged    += OnProgressChanged;
        bm.OnMatchFound         += OnMatchFound;
        bm.OnOppBurgerCompleted += zPlayOppFlash;
        _isSubscribed = true;
    }

    void yUnsubscribe()
    {
        if (!_isSubscribed) return;
        var bm = TrBattleManager.xInstance;
        if (bm == null) { _isSubscribed = false; return; }

        bm.OnGaugeChanged       -= OnGaugeChanged;
        bm.OnProgressChanged    -= OnProgressChanged;
        bm.OnMatchFound         -= OnMatchFound;
        bm.OnOppBurgerCompleted -= zPlayOppFlash;
        _isSubscribed = false;
    }

    void Update()
    {
        if (!_isSubscribed) yTrySubscribe();
    }

    // ── 색상 초기 적용 ─────────────────────────────────────────

    void yApplyColors()
    {
        if (_imgMyBar         != null) _imgMyBar.color         = _myColor;
        if (_imgOppBar        != null) _imgOppBar.color        = _oppColor;
        if (_imgBarBackground != null) _imgBarBackground.color = _backgroundColor;
    }

    // ── 초기화 ────────────────────────────────────────────────

    public void zInit()
    {
        _prevFill = 0.5f;
        zUpdateGauge(0.5f, instant: true);
        zFlashMySide();
    }

    // ── 게이지 갱신 ────────────────────────────────────────────

    void OnGaugeChanged(float myRatio) => zUpdateGauge(myRatio);

    void OnMatchFound()
    {
        var bm = TrBattleManager.xInstance;
        if (bm != null)
        {
            if (_txtMyName  != null) _txtMyName.text  = bm.MyNickname;
            if (_txtOppName != null) _txtOppName.text = bm.OppNickname;
        }
        zInit();
    }

    public void zUpdateGauge(float myRatio, bool instant = false)
    {
        float fill    = Mathf.Clamp01(myRatio);
        bool  iGained = fill > _prevFill;
        float oppFill = 1f - fill;

        if (instant)
        {
            if (_imgMyBar  != null) _imgMyBar.fillAmount  = fill;
            if (_imgOppBar != null) _imgOppBar.fillAmount = oppFill;
            yMoveMarker(fill);
        }
        else
        {
            _imgMyBar?.DOFillAmount(fill,     AnimDuration).SetEase(Ease.OutCubic);
            _imgOppBar?.DOFillAmount(oppFill, AnimDuration).SetEase(Ease.OutCubic);
            DOVirtual.Float(_imgMyBar != null ? _imgMyBar.fillAmount : 0.5f,
                            fill, AnimDuration,
                            t => yMoveMarker(t))
                     .SetEase(Ease.OutCubic);

            if (iGained && _battleEffects != null && _tfCollisionPoint != null)
                _battleEffects.zPlayAt(_tfCollisionPoint.anchoredPosition);
        }

        _prevFill = fill;
    }

    // 경계 마커 위치 갱신 (두 fillAmount 바의 경계선)
    void yMoveMarker(float ratio)
    {
        if (_tfCollisionPoint == null) return;
        float x = (ratio - 0.5f) * _barWidth;
        _tfCollisionPoint.anchoredPosition = new Vector2(x, _tfCollisionPoint.anchoredPosition.y);
    }

    // ── 게임 시작 인트로 ───────────────────────────────────────

    /// <summary>
    /// 게임 시작 시 바 등장 연출. TrPuzzleHamburger에서 호출.
    /// </summary>
    public void zPlayIntroAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOPunchScale(
                new Vector3(0.08f, 0.15f, 0f), 0.45f, 5, 0.4f));
    }

    // ── 상대 완성 효과 ─────────────────────────────────────────

    void zPlayOppFlash()
    {
        if (_cgOppFlash == null) return;
        _cgOppFlash.DOKill();
        _cgOppFlash.gameObject.SetActive(true);
        _cgOppFlash.alpha = 0.4f;
        _cgOppFlash.DOFade(0f, 0.35f)
                   .OnComplete(() => _cgOppFlash.gameObject.SetActive(false));
    }

    // ── 진행 상황 수신 ─────────────────────────────────────────

    void OnProgressChanged(int myCorrect, int oppCorrect) { }

    // ── 게임 시작 플래시 ───────────────────────────────────────

    public void zFlashMySide()
    {
        if (_goMySideFlash == null) return;
        _goMySideFlash.SetActive(true);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (_goMySideFlash != null)
                _goMySideFlash.SetActive(false);
        });
    }
}
