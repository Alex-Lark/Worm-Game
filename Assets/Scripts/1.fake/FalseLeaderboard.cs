using System.Collections;
using UnityEngine;

public class FalseLeaderboard : MonoBehaviour
{
    public Transform target;
    public float duration = 1.5f;
    public float delay = 1f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = target.position;
        
        StartCoroutine(SlideToTarget());
    }

    private IEnumerator SlideToTarget()
    {
        
        float time = 0f;
        while (time < delay)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        time = 0f;
        Vector3 initialPosition = transform.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            transform.position = Vector3.Lerp(initialPosition, startPosition, t);

            yield return null;
        }
        transform.position = startPosition;
    }
}
