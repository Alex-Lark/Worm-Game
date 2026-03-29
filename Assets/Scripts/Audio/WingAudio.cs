using CreatureParts;
using UnityEngine;

namespace Audio
{
    public class WingAudio : MonoBehaviour
    {
        void Start()
        {
            GetComponent<WingPart>().OnWingFlap += OnWingFlap;
        }

        void OnDestroy()
        {
            GetComponent<WingPart>().OnWingFlap -= OnWingFlap;
        }
        
        private void OnWingFlap()
        {
            //wing flap audio
        }
    }
}
