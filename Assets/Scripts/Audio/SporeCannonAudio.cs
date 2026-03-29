using CreatureParts;
using UnityEngine;

namespace Audio
{
    public class SporeCannonAudio : MonoBehaviour
    {
        void Start()
        {
            GetComponent<ProjectilePart>().OnCannonShoot += OnCannonShoot;
        }

        void OnDestroy()
        {
            GetComponent<ProjectilePart>().OnCannonShoot -= OnCannonShoot;
        }

        private void OnCannonShoot()
        {
            //play shoot audio
        }
    }
}
