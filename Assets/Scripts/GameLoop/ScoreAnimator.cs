using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreAnimator : MonoBehaviour
{
    public float duration = 1.5f;

    public void AnimateScore(TextMeshProUGUI text, string playerName, int startValue, int endValue)
    {
        StartCoroutine(CountUp(text, playerName, startValue, endValue));
    }

    private IEnumerator CountUp(TextMeshProUGUI text, string playerName, int start, int end)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            int value = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            text.text = playerName + ": " + value;

            yield return null;
        }

        text.text = playerName + ": " + end;
    }
}