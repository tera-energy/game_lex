using System.Collections;
using UnityEngine;

public class TrPuzzleManager : MonoBehaviour
{
    [SerializeField] protected TT.enumDifficultyLevel _levelSelector = TT.enumDifficultyLevel.Easy;
    protected TT.enumPlayGameType _type;

    float _currTimer;

    [SerializeField] protected int _maxGameTime = 30;
    protected float _currGameTime;

    protected int _currScore;
    TrRankUsersData[] _playerScores;
    int _playerCount = 0;
    bool _isRankGameStart;

    protected bool _isTutorial;

    protected int[] _challenges;

    protected float[] _numCurrChallenges = new float[3];
    protected bool _isThridChallengeSame;
    int _maxCombo = 0;
    protected int _currCombo;
    protected bool _isOnVibrate;

    [SerializeField] protected TrUI_PuzzleManager _ui;

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            TrUI_PuzzlePause.xInstance.xAppearPauseWindow();
    }

    // score���� �޾Ҵٸ� ������ �� ȹ��
    protected virtual void zSetChallengeByNum(int num, bool isClear = false, int score = 1)
    {
        if (_type != TT.enumPlayGameType.Campaign) return;

        if (isClear)
        {
            TrUI_PuzzleChallenges.xInstance.zSetActiveStar(num);
            return;
        }

        _numCurrChallenges[num] += score;
        if (_numCurrChallenges[num] <= 0) _numCurrChallenges[num] = score;

        if (_numCurrChallenges[num] >= _challenges[num])
            TrUI_PuzzleChallenges.xInstance.zSetActiveStar(num);
        else if (_numCurrChallenges[num] < _challenges[num])
            TrUI_PuzzleChallenges.xInstance.zSetDisableStar(num);
    }

    protected virtual void zWrong(bool isNotice = true, int score = -1)
    {
        if (!GameManager.xInstance._isGameStarted) return;
        if (isNotice)
            TrUI_PuzzleNotice.xInstance.zSetNotice("Wrong");
        _currScore += score;
        if (_currScore < 0) _currScore = 0;
        TrUI_PuzzleScore.xInstance.zSetScore(_currScore);

        // ���߿� �ּ��� ���ְ� �ٽ� ����� ����
        /*if (_type == TT.enumPlayGameType.Campaign){
            zSetChallengeByNum(0, false, score);

            if (_isThridChallengeSame)
                _numCurrSeqCorrect = 0;
        }
        else if (_type == TT.enumPlayGameType.Rank){
            photonView.RPC("yRPCSetPlayerScore", RpcTarget.All, GameManager.xInstance._numPlayerId, _currScore);
        }*/
    }
    protected virtual void zCorrect(bool isNotice = true, int score = 1)
    {
        if (!GameManager.xInstance._isGameStarted) return;
        if (isNotice)
            TrUI_PuzzleNotice.xInstance.zSetNotice("Correct!");
        _currScore += score;
        TrUI_PuzzleScore.xInstance.zSetScore(_currScore);

        if (_currCombo > _maxCombo)
            _maxCombo = _currCombo;

        // ���߿� �ּ��� ���ְ� �ٽ� ����� ����
        /*if (_type == TT.enumPlayGameType.Campaign) {
            zSetChallengeByNum(0, false, score);
            if (_isThridChallengeSame)
            {
                if (++_numCurrSeqCorrect >= _challenges[2])
                    zSetChallengeByNum(2, true);
            }
        }else if(_type == TT.enumPlayGameType.Rank){
            photonView.RPC("yRPCSetPlayerScore", RpcTarget.All, GameManager.xInstance._numPlayerId, _currScore);
        }*/
    }

    protected virtual void zSetResultGame(bool isCollide)
    {
    }

    protected virtual void zEndGame(bool isCollide = false)
    {
        TrUI_PuzzleNotice.xInstance._isGameOver = true;
        GameManager.xInstance._isGameStarted = false;
        TrUI_PuzzleNotice.xInstance.zSetNoticeWithRex("GAME OVER!!!", 60, 10f);
        zSetResultGame(isCollide);
        GameManager.xInstance._numMaxCombo = _maxCombo;
        GameManager._score = _currScore;
        
        GameManager.xInstance._valuePlayTime = (int)_currTimer;
        TrUI_PuzzlePause.xInstance.zGameEndAndSceneMove();
    }

    void yDecreaseFuel()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad9)) _currGameTime += 9;
#endif

        if (_currGameTime >= 0)
        {
            if (_type == TT.enumPlayGameType.Rank)
            {
                if (_isRankGameStart)
                {
                    _currGameTime -= Time.deltaTime;
                    TrUI_PuzzleTimer.xInstance.zUpdateTimerBar(_maxGameTime, _currGameTime);
                }
            }
            else
            {
                _currGameTime -= Time.deltaTime;
                TrUI_PuzzleTimer.xInstance.zUpdateTimerBar(_maxGameTime, _currGameTime);
            }
        }
        else
        {
            // ���� ��
            GameManager.xInstance._isGameStarted = false;
            TrUI_PuzzleTimer.xInstance.zUpdateTimerBar(_maxGameTime, 0);
            zEndGame();
        }


    }

    protected virtual void yBeforeReadyGame() { }

    // ���� ī��Ʈ
    protected virtual IEnumerator yProcReadyGame()
    {
        yBeforeReadyGame();
        GameManager.xInstance._isGameStopped = false;
        TrAudio_Music.xInstance.zzPlayMain(0.25f);
        _maxCombo = 0;
        _isRankGameStart = false;
        _isOnVibrate = PlayerPrefs.GetInt(TT.strConfigVibrate, 1) == 1 ? true : false;

        TT.UtilDelayedFunc.zCreateAtLate(() => TrAudio_UI.xInstance.zzSetFlatVolume());

        yield return new WaitUntil(() => TrUI_PuzzlePause.xInstance._fade.alpha == 0);

        TrUI_PuzzleNotice.xInstance.zSetNoticeWithRex("START ~!", 60, 1.4f);
        yield return TT.WaitForSeconds(1.6f);

        yAfterReadyGame();

    }

    protected virtual void yAfterReadyGame() { }

    void Start()
    {
        GameManager.xInstance.zSetCamera();
        GameManager._canBtnClick = true;
        GameManager.xInstance._isGameStarted = false;
        TrAudio_UI.xInstance.zzPlay_Ready(0, 2f);

        _currGameTime = _maxGameTime;
        TrUI_PuzzleTimer.xInstance.zUpdateTimerBar(_maxGameTime, _currGameTime);

        StartCoroutine(yProcReadyGame());
    }

    protected virtual void Update()
    {
#if PLATFORM_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
            TrUI_PuzzlePause.xInstance.xAppearPauseWindow();
#endif

        if (!GameManager.xInstance._isGameStarted) return;
        yDecreaseFuel();

        _currTimer += Time.deltaTime;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            zEndGame();
        }
#endif
    }
}
