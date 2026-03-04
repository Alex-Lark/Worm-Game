using TMPro;
using UnityEngine;

namespace GameLoop
{
    public class Timertext : MonoBehaviour
    {
        void Update()
        {
            if (GameLoop.Instance||GameLoopTimeSyncer.Instance)
            {
                gameObject.GetComponent<TextMeshProUGUI>().text = ((int)GameLoop.TimeLeftInScene).ToString();
            }
        }
    }
}
