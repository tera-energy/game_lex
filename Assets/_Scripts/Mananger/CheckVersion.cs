using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

namespace UpgradeSystem
{
    struct GameData
    {
        public string _version;
    }

    public class CheckVersion : MonoBehaviour
    {
        [SerializeField] GameObject _goPannel;
        [SerializeField] string _googleUrl;

        static bool _isAlreadyCheckedForUpdates = false;

        IEnumerator yLoadVersionDatas()
        {
            UnityWebRequest req = UnityWebRequest.Get(_googleUrl);
            req.disposeDownloadHandlerOnDispose = true;
            req.timeout = 60;
            //Debug.Log("request");
            yield return req.SendWebRequest();

            if (req.isDone)
            {
                //Debug.Log("done");
                _isAlreadyCheckedForUpdates = true;
                if (req.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log("success");
                    string pattern = @"<span class=""htlgb"">[0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}<";
                    Regex _Regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    Match mat = _Regex.Match(req.downloadHandler.text);
                    //Debug.Log(mat);
                    
                    //if(string.IsNullOrEmpty(newVersion) && Application.version.Equals()

                    // TODO : 링크이동
                }

            }
            else
            {
                // 무한 확인창 팝업 및 업데이트 요구
            }
        }

        void Start()
        {
            if (!_isAlreadyCheckedForUpdates)
                StartCoroutine(yLoadVersionDatas());

        }
        
    }
}
