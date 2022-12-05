using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    [SerializeField] UI_StatsRadarChart uistatsRadarChart;
    void Start()
    {
        zSetStats(60, 50, 70, 50, 90);
    }

    /// <summary>
    /// 기억력, 집중력, 사고력, 순발력, 문제해결능력
    /// </summary>
    public void zSetStats(int mem, int con, int tho, int qui, int pro){
        Stats stats = new Stats(mem, con, tho, qui, pro);

        uistatsRadarChart.zSetStats(stats);
    }
}
