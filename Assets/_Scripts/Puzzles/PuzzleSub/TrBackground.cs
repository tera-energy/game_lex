using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrBackground : MonoBehaviour
{
    static TrBackground _instance;
    bool _isScrollable = true;
    Material _material;
    float _backgroundScroll = 0f;
    Vector2 _origOffset;

	//=========================================================================================================
	public static TrBackground xInstance { get { return _instance; } }
	public bool xIsScrollable { get { return _isScrollable; } set { _isScrollable = value; } }

	private void Awake() {
		if( _instance == null) {
            _instance = this;
		} else {
            Destroy(gameObject);
		}
	}

	void Start()
    {
        _material = GetComponent<Renderer>().sharedMaterial;
        _origOffset = _material.mainTextureOffset;
    }

    void Update()
    {
        if (_isScrollable) {
            _backgroundScroll += 0.02f * Time.deltaTime;
            _material.mainTextureOffset = new Vector2(_backgroundScroll, _backgroundScroll);
        }
    }

	private void OnDestroy() {
        _material.mainTextureOffset = _origOffset;
	}
}
