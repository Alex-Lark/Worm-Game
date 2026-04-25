using System.Collections;
using UnityEngine;

namespace Audio
{
    public class LobbyMusic : MonoBehaviour
    {
        public AudioSource introSource;
        public AudioSource loopSource;
        public AudioClip introClip;
        public AudioClip loopClip;

        void OnEnable()
        {
            double introStartTime = AudioSettings.dspTime;
            introSource.clip = introClip;
            introSource.PlayScheduled(introStartTime);
            
            double loopStartTime = introStartTime + (double)introClip.length;
            loopSource.clip = loopClip;
            loopSource.loop = true;
            loopSource.PlayScheduled(loopStartTime);
        }
    }
}
