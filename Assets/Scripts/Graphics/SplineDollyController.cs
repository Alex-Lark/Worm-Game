using Unity.Cinemachine;
using UnityEngine;

namespace Graphics
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class SplineDollyController : MonoBehaviour
    {
        [HideInInspector] public bool isPlaying = false;
        [HideInInspector] public float playbackSpeed = 1f;

        private CinemachineSplineDolly _dolly;
        private float _startPosition = 0f;

        void Awake()
        {
            _dolly = GetComponent<CinemachineSplineDolly>();
        }

        void Update()
        {
            if (!isPlaying || _dolly == null) return;

            _dolly.CameraPosition += playbackSpeed * Time.deltaTime;

            // Loop or clamp at end
            if (_dolly.CameraPosition >= 1f)
                _dolly.CameraPosition = 1f; // change to 0f to loop
        }

        public void Play()  => isPlaying = true;
        public void Stop()  => isPlaying = false;
        public void Reset() 
        {
            isPlaying = false;
            if (_dolly == null) _dolly = GetComponent<CinemachineSplineDolly>();
            _dolly.CameraPosition = _startPosition;
        }
    }
}