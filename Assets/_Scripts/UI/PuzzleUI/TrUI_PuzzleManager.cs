using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrUI_PuzzleManager : MonoBehaviour
{
    protected Text[] _txtUserNames = new Text[4], _txtUserScores = new Text[4];
    GameObject[] _goUserInfoUIs = new GameObject[4];


    public void zSetUsersScore(int num, int score){
        _txtUserScores[num].text = score.ToString();
    }

    public void zSetUsersInfo(int num, string name){
        _txtUserNames[num].text = name;
        _txtUserScores[num].text = "0";
    }

    public void zSetCompetition(int numPlayers){
        _txtUserNames = TrUI_PuzzleUserInfo.xInstance._textUserNames;
        _txtUserScores = TrUI_PuzzleUserInfo.xInstance._textUserScores;
        _goUserInfoUIs = TrUI_PuzzleUserInfo.xInstance._goUserUIs;

        TrUI_PuzzleUserInfo.xInstance.zActivateUIs(false);
        TrUI_PuzzleUserInfo.xInstance.zActivateUIs(true, numPlayers);

        TrUI_PuzzleChallenges.xInstance.zNotCampaign();
    }

    public void zSetCampaign(){
        TrUI_PuzzleUserInfo.xInstance.zActivateUIs(false);

        for (int i = 0; i < 3; i++){
            TrUI_PuzzleChallenges.xInstance.zSetDisableStar(i);
        }
    }

/*    public void zSetExercise(){
        TrUI_PuzzleUserInfo.xInstance.zActivateUIs(false);
        TrUI_PuzzleChallenges.xInstance.zNotCampaign();
    }*/
}
