using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalDataManager : MonoBehaviour
{
    static AnimalDataManager _instance;
    public TrSO_DialogueAnimalData[] _animalDatas;
    public static AnimalDataManager xInstance { get { return _instance; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes()
    {
        _instance = null;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
