using UnityEngine;
using UnityEditor.Recorder;

namespace Graphics
{
    public class AutoStartRecorder : MonoBehaviour
    {
        [SerializeField] private RecorderControllerSettings recorderSettings;

        private RecorderController _recorderController;

        void Start()
        {
            if (ParrelSync.ClonesManager.IsClone()) return;

            _recorderController = new RecorderController(recorderSettings);
            _recorderController.PrepareRecording();
            _recorderController.StartRecording();
        }

        void OnDisable()
        {
            if (_recorderController != null && _recorderController.IsRecording())
                _recorderController.StopRecording();
        }
    }
}