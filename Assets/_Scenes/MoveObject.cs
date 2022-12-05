using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveObject : MonoBehaviour
{
    [SerializeField] Text txt;
    [SerializeField] int maxScore;

    IEnumerator yEffectIncreaseScore(Text text, int score, bool isScore)
    {
        float currScore = 0;
        float maxScore = score;
        float speed = maxScore - currScore;
        int soundScore = 1;
        while (currScore < maxScore)
        {
            currScore += Time.deltaTime * speed;
            if(currScore >= soundScore){
                soundScore++;
                //Debug.Log(soundScore);
            }
            text.text = ((int)currScore).ToString();
            speed = (maxScore - currScore);
            if (speed < 1)
                speed = 1;
            yield return null;
        }
        text.text = score.ToString();
    }

    void Start()
    {
        StartCoroutine(yEffectIncreaseScore(txt, maxScore, true));
    }
}

