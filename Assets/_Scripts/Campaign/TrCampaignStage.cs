using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrCampaignStage : MonoBehaviour
{
    [HideInInspector] public int _stageNum;
    public TrSO_PuzzleData _stageData;

    public int _star;
    public bool _isOpen = false;

    public int[] _challengesScoreEasy;
    public int[] _challengesScoreNormal;
    public int[] _challengesScoreHard;
}
