using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrSingleton : MonoBehaviour
{
    static TrSingleton _instance;

	public static TrSingleton xInstance { get { return _instance; } }

	//===================================================================================================

	// Start is called before the first frame update
	void Awake()
    {
        if( _instance == null)
		{
            _instance = this;
		}
		else
		{
            Destroy(gameObject);
		}
    }
}
