using Unity.Cinemachine;
using UnityEngine;

namespace Graphics
{
    public enum EndBehaviour { Stop, Loop, PingPong }

    [RequireComponent(typeof(CinemachineCamera))]
    public class SplineDollyController : MonoBehaviour
    {
        [HideInInspector] public bool isPlaying = false;
        [HideInInspector] public float playbackSpeed = 1f;
        [HideInInspector] public bool autoPlayOnStart = false;
        [HideInInspector] public EndBehaviour endBehaviour = EndBehaviour.Loop;

        private CinemachineSplineDolly _dolly;
        private float _startPosition = 0f;
        private float _direction = 1f;

        void Awake()
        {
            _dolly = GetComponent<CinemachineSplineDolly>();
        }

        void Start()
        {
            if (autoPlayOnStart)
                Play();
        }

        void Update()
        {
            if (!isPlaying || _dolly == null) return;

            _dolly.CameraPosition += playbackSpeed * _direction * Time.deltaTime;

            if (_dolly.CameraPosition >= 1f)
            {
                switch (endBehaviour)
                {
                    case EndBehaviour.Stop:
                        _dolly.CameraPosition = 1f;
                        Stop();
                        break;
                    case EndBehaviour.Loop:
                        _dolly.CameraPosition = 0f;
                        break;
                    case EndBehaviour.PingPong:
                        _dolly.CameraPosition = 1f;
                        _direction = -1f;
                        break;
                }
            }
            else if (_dolly.CameraPosition <= 0f && _direction < 0f)
            {
                // PingPong hit the start, go forward again
                _dolly.CameraPosition = 0f;
                _direction = 1f;
            }
        }

        public void Play()
        {
            _direction = 1f;
            isPlaying = true;
        }

        public void Stop() => isPlaying = false;

        public void Reset()
        {
            isPlaying = false;
            _direction = 1f;
            if (_dolly == null) _dolly = GetComponent<CinemachineSplineDolly>();
            _dolly.CameraPosition = _startPosition;
        }
    }
}