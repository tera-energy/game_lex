using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrRankLeaderBoardListBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _txtNumRank;
    [SerializeField] TextMeshProUGUI _txtUserName;
    [SerializeField] TextMeshProUGUI _txtUserScore;

    public TextMeshProUGUI xTextNumRank { get { return _txtNumRank; } }
    public TextMeshProUGUI xTextUserName { get { return _txtUserName; } }
    public TextMeshProUGUI xTextUserScore { get { return _txtUserScore; } }
}
