using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrUI_PuzzleChallenges : MonoBehaviour
{
    static TrUI_PuzzleChallenges _instance;

    [SerializeField] Image[] _imgStars;
    [SerializeField] Sprite _spDisableStar;
    [SerializeField] Sprite _spActivateStar;

    static public TrUI_PuzzleChallenges xInstance {get { return _instance; } }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes(){
        _instance = null;
    }

    void Awake(){
        if (_instance == null){
            _instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    public void zSetActiveStar(int num){
        _imgStars[num].sprite = _spActivateStar;
    }

    public void zSetDisableStar(int num){
        _imgStars[num].sprite = _spDisableStar;
    }

    public void zNotCampaign(){
        for(int i=0; i<3; i++){
            _imgStars[i].transform.gameObject.SetActive(false);
        }
    }

    public string zGetResult(){
        string result = "";

        for(int i=0; i<3; i++){
            if (_imgStars[i].sprite == _spActivateStar)
                result += "1";
            else
                result += "0";
        }

        return result;
    }
}
