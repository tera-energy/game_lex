using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]

public class TrLeaf : MonoBehaviour
{
    [SerializeField] GameObject _leafLandable, _leafLanded, _rock;
    bool _isLanded = false;
    bool _isRock = false;
    TrLeaf _previousLeaf = null;    // 백트랙킹을 위한 이전 발판 저장.
    int _id = -1;

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public int xID { get { return _id; } set { _id = value; } }
	public bool xIsLanded { get { return _isLanded; } }
	public bool xIsRock { 
        get { return _isRock; } 
        set {
            _isRock = value;
            _leafLandable.SetActive(!value);
            _leafLanded.SetActive(false);
            _rock.SetActive(value);
        }
    }
	public TrLeaf xPreviousLeaf { get { return _previousLeaf; } set { _previousLeaf = value; } }

	public void zChangeLeafState(bool isLanded) {
        _isLanded = isLanded;
        _leafLandable.SetActive(!isLanded);
        _leafLanded.SetActive(isLanded);
	}

    //====================================================================================================
}
