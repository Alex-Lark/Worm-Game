using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FalseScoreAnimator : MonoBehaviour
{
    public Text text;
    public int targetScore = 100;
    public float duration = 1.5f;

    private void Start()
    {
        StartCoroutine(CountUp(text, 0, targetScore));
    }

    private IEnumerator CountUp(Text text, int start, int end)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            int value = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            text.text = value.ToString();

            yield return null;
        }

        text.text = end.ToString();
    }
}

