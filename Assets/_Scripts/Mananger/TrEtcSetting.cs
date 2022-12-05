using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrEtcSetting : MonoBehaviour
{
    public TrMode _selectMode;
    static TrMode _mode;

    public enum TrMode
    {
        local,
        SemiLocal,
        Production,
    }

    private void Awake()
    {
        _mode = _selectMode;
    }

    public static string API_URL
    {
        get
        {
            if (_mode == TrMode.local)
            {
                return "http://118.67.143.233:8080";
            }
            else if (_mode == TrMode.SemiLocal)
            {
                return "http://192.168.0.171:3003";
            }
            else
            {
                return "http://118.67.143.233:8080";
            }
        }
    }
}
