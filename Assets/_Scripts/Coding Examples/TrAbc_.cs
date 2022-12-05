using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrAbc_ : MonoBehaviour
{
    [SerializeField] int _efg;
    protected float _abc;
    
    public void zMethod1() {
        string meh = "meh";
		Debug.Log("debug: " + meh, gameObject);
	}

    virtual public void zzMethod2() {
        string meh = "virtual meh";
        Debug.Log("debug: " + meh, gameObject);
    }
}
