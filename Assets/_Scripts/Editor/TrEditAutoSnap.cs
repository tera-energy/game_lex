using UnityEngine;
using UnityEditor;

//Place this script in any "Enditor" folder. ex) [Project]/Assets/_Scripts/Editor.

public class TrEditAutoSnap : EditorWindow {
	private bool doSnap = false;
	private bool doSnapUI = false;
	private bool doSnapChildren = false;
	private float snapValue = .5f;

	[MenuItem("Edit/TeraUtil/Auto Snap %&L")]    //open menu with ctrl+alt+L. %(ctrl), &(alt), #(shift)

	static void Init() {
		var window = (TrEditAutoSnap)EditorWindow.GetWindow(typeof(TrEditAutoSnap));
		window.maxSize = new Vector2(200,100);
	}

	public void OnGUI() {
		doSnap = EditorGUILayout.Toggle("Auto Snap",doSnap);
		doSnapChildren = EditorGUILayout.Toggle("Auto Snap Children",doSnapChildren);
		doSnapUI = EditorGUILayout.Toggle("Auto SnapUI",doSnapUI);
		snapValue = EditorGUILayout.FloatField("Snap Value",snapValue);

	}

	public void Update() {
		if(!EditorApplication.isPlaying && Selection.transforms.Length > 0) {
			if(doSnap) Snap();
			if(doSnapChildren) SnapChildren();
			if(doSnapUI) SnapUI();
		}
	}

	private void Snap() {
		foreach(var t in Selection.transforms) {
			if(t.parent == null && t.GetComponent<TrEditNoAutoSnap>() == null) {
				var pos = t.transform.position;
				pos.x = Round(pos.x);
				pos.y = Round(pos.y);
				pos.z = Round(pos.z);
				t.transform.position = pos;
			}
		}
	}

	private void SnapChildren() {
		foreach(var t in Selection.transforms) {
			if(t.parent != null && t.GetComponent<TrEditNoAutoSnap>() == null) {
				var pos = t.transform.position;
				pos.x = Round(pos.x);
				pos.y = Round(pos.y);
				pos.z = Round(pos.z);
				t.transform.position = pos;
			}
		}
	}

	private void SnapUI() {
		foreach(var t in Selection.transforms) {
			if(t.parent != null && t.GetComponent<TrEditNoAutoSnap>() == null) {
				RectTransform rectTrans = t as RectTransform;
				if(rectTrans) {
					var pos = rectTrans.anchoredPosition;
					pos.x = Round(pos.x);
					pos.y = Round(pos.y);
					rectTrans.anchoredPosition = pos;
					var size = rectTrans.sizeDelta;
					size.x = Round(size.x);
					size.y = Round(size.y);
					rectTrans.sizeDelta = size;
				}
			}
		}
	}

	private float Round(float input) {
		return snapValue * Mathf.Round((input / snapValue));
	}
}