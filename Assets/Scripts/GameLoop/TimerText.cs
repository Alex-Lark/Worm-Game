using TMPro;
using UnityEngine;

public class Timertext : MonoBehaviour
{
    void Update()
    {
        if (GameLoop.Instance)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = ((int)GameLoop.Instance.TimeLeftInScene).ToString();
        }
    }
}
