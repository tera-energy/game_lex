using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrNetworkManager : MonoBehaviour
{
    static TrNetworkManager _instance;
    public static TrNetworkManager xInstance { get { return _instance; } }
    public GameObject _goWinDisConnection;

    public static bool zGetIsConnectNetwork(){
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public void zActiveConnectWindow(bool isOpen){
        _goWinDisConnection.SetActive(isOpen);
    }

    void Awake(){
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate(){
        if (!zGetIsConnectNetwork()){
            if(GameManager._state != TT.enumGameState.Play)
                zActiveConnectWindow(true);
        }
        else
            zActiveConnectWindow(false);
    }
}
