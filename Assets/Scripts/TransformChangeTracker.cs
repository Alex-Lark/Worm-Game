using UnityEngine;

public class TransformChangeTracker : MonoBehaviour
{
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    private Vector3 _lastScale;

    void Awake()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
        _lastScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (transform.position != _lastPosition)
        {
            Debug.LogWarning($"[TransformTracker] Position changed on '{gameObject.name}'" +
                             $"\n  From: {_lastPosition}" +
                             $"\n  To:   {transform.position}" +
                             $"\n  Stack Trace:\n{StackTraceUtility.ExtractStackTrace()}",
                gameObject);
            _lastPosition = transform.position;
        }

        if (transform.rotation != _lastRotation)
        {
            Debug.LogWarning($"[TransformTracker] Rotation changed on '{gameObject.name}'" +
                             $"\n  From: {_lastRotation.eulerAngles}" +
                             $"\n  To:   {transform.rotation.eulerAngles}",
                gameObject);
            _lastRotation = transform.rotation;
        }

        if (transform.localScale != _lastScale)
        {
            Debug.LogWarning($"[TransformTracker] Scale changed on '{gameObject.name}'" +
                             $"\n  From: {_lastScale}" +
                             $"\n  To:   {transform.localScale}",
                gameObject);
            _lastScale = transform.localScale;
        }
    }
}