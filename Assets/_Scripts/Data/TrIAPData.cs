using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrIAPType
{
    NONE,
    STAMINA,
}

public class TrIAPData : MonoBehaviour{
    public int _serialNumber;
    public string _id;
    public string _price;

    public TrIAPType _type;
    public int _number;

}
