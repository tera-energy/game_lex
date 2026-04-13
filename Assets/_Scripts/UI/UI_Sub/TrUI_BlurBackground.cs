using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 배경 블러 싱글턴 컴포넌트.
/// UIBlur 셰이더(GrabPass)가 적용된 Image를 켜고 끄는 방식으로 블러 처리.
/// Inspector 연결 불필요 — xInstance로 어디서든 접근.
/// </summary>
public class TrUI_BlurBackground : MonoBehaviour
{
    static TrUI_BlurBackground _instance;
    public static TrUI_BlurBackground xInstance => _instance;

    [SerializeField] float _fadeDuration = 0.3f;

    Image _blurImage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomain() => _instance = null;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _blurImage = GetComponent<Image>();
            if (_blurImage == null)
                Debug.LogError("[Blur] Image 컴포넌트 없음 — BlurBG에 Image 추가 필요");
            else
                _blurImage.enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        if (_blurImage == null) yield break;
        // GrabPass 셰이더 사전 컴파일 — 모바일 JIT 지연(0.5~1s) 방지
        var c = _blurImage.color;
        c.a = 0f;
        _blurImage.color = c;
        _blurImage.enabled = true;
        yield return null;
        yield return null;
        _blurImage.enabled = false;
        c.a = 1f;
        _blurImage.color = c;
    }

    // ── 외부 호출 ──────────────────────────────────────────────

    public IEnumerator zBlurIn(float? duration = null)
    {
        if (_blurImage == null) yield break;

        _blurImage.DOKill();
        var c = _blurImage.color;
        c.a = 1f;
        _blurImage.color = c;
        _blurImage.enabled = true;
        yield return null;
    }

    public IEnumerator zBlurOut(float? duration = null)
    {
        if (_blurImage == null || !_blurImage.enabled) yield break;

        _blurImage.DOKill();
        yield return _blurImage.DOFade(0f, duration ?? _fadeDuration).WaitForCompletion();
        _blurImage.enabled = false;
    }

    public void zFireBlurIn(float? duration = null)  => StartCoroutine(zBlurIn(duration));
    public void zFireBlurOut(float? duration = null) => StartCoroutine(zBlurOut(duration));
}
