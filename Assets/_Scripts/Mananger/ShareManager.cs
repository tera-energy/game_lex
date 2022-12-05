using UnityEngine;

public class ShareManager : MonoBehaviour
{
	public void Share()
	{
#if !UNITY_EDITOR
		new NativeShare().SetSubject(TrProjectSettings._subjectForShare).SetText(TrProjectSettings._contentForShare).SetUrl(TrProjectSettings._urlStore)
			.SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
			.Share();
#endif
	}
}