using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Stats
{
    public event EventHandler OnStatsChanged;

    public static int STAT_MIN = 0;
    public static int STAT_MAX = 100;

    public enum Type{
        Memory,
        Concentration,
        Thought,
        Quickness,
        ProblemSolving
    }

    SingleStat _memoryStat;
    SingleStat _concentrationStat;
    SingleStat _thoughtStat;
    SingleStat _quicknessStat;
    SingleStat _problemSolvingStat;
    public Stats(int memoryStatAmount, int concentrationStatAmount, int thoughtStatAmount, int quicknessStatAmount, int problemSolvingStatAmount){
        _memoryStat = new SingleStat(memoryStatAmount);
        _concentrationStat = new SingleStat(concentrationStatAmount);
        _thoughtStat = new SingleStat(thoughtStatAmount);
        _quicknessStat = new SingleStat(quicknessStatAmount);
        _problemSolvingStat = new SingleStat(problemSolvingStatAmount);
    }

    SingleStat zGetSingleStat(Type statType){
        switch (statType){
            default:
            case Type.Memory: return _memoryStat;
            case Type.Concentration: return _concentrationStat;
            case Type.Thought: return _thoughtStat;
            case Type.Quickness: return _quicknessStat;
            case Type.ProblemSolving: return _problemSolvingStat;
        }
    }

    public void zSetStatAmount(Type statType, int statAmonut)
    {
        zGetSingleStat(statType).zSetStatAmount(statAmonut);
        if (OnStatsChanged != null) OnStatsChanged(this, EventArgs.Empty);
    }

    public void zIncreaseStatAmount(Type statType, int amount = 1)
    {
        zSetStatAmount(statType, zGetStatAmount(statType) + amount);
    }

    public void zDecreaseStatAmount(Type statType, int amount = -1)
    {
        zSetStatAmount(statType, zGetStatAmount(statType) + amount);
    }

    public int zGetStatAmount(Type statType)
    {
        return zGetSingleStat(statType).zGetStatAmount();
    }

    public float zGetStatAmountNormalized(Type statType)
    {
        return zGetSingleStat(statType).zGetStatAmountNormalized();
    }


    class SingleStat
    {
        private int _stat;

        public SingleStat(int statAmount){
            zSetStatAmount(statAmount);
        }

        public void zSetStatAmount(int statAmonut)
        {
            _stat = Mathf.Clamp(statAmonut, STAT_MIN, STAT_MAX);
        }

        public int zGetStatAmount()
        {
            return _stat;
        }

        public float zGetStatAmountNormalized()
        {
            return (float)_stat / STAT_MAX;
        }
    }
}

