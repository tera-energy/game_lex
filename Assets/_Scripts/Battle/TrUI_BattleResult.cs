using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 대결 종료 후 Result 씬으로 전환.
/// GameManager에 점수/결과를 세팅하고 트레이닝/챌린지와 동일한 TrUI_ResultManager를 사용.
/// </summary>
public class TrUI_BattleResult : MonoBehaviour
{
    bool _isShown = false;

    void Start()
    {
        var bm = TrBattleManager.xInstance;
        if (bm != null)
            bm.OnBattleEnded += OnBattleEnded;
        else
            Debug.LogError("[BattleResult] Start 시점에 TrBattleManager 없음 — 구독 실패");
    }

    void OnDestroy()
    {
        var bm = TrBattleManager.xInstance;
        if (bm != null) bm.OnBattleEnded -= OnBattleEnded;
    }

    void OnBattleEnded(bool didIWin, int myCorrect, int oppCorrect)
    {
        Debug.Log($"[BattleResult] OnBattleEnded — didIWin:{didIWin} myCorrect:{myCorrect}");
        if (_isShown) return;
        _isShown = true;

        if (GameManager.xInstance != null)
            GameManager.xInstance._isGameStarted = false;

        // Result 씬에 전달할 데이터 세팅 (_type은 TrUI_MatchingPopup에서 이미 Battle로 설정됨)
        // _correctNum은 TrPuzzleHamburger가 버거 완성마다 로컬에서 직접 증가시키므로
        // 서버 응답 지연이 있는 myCorrect(서버 카운트)로 덮어쓰지 않음
        int localCorrect = GameManager.xInstance != null
            ? GameManager.xInstance._correctNum
            : myCorrect;
        GameManager._score            = localCorrect * 100;
        GameManager._battleDidIWin    = didIWin;
        GameManager._battleOppCorrect = oppCorrect;

        StartCoroutine(yLoadResult());
    }

    IEnumerator yLoadResult()
    {
        TrBattleManager.xInstance?.Disconnect();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(TrProjectSettings.strRESULT);
    }
}
