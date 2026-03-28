using GameLoop;
using UnityEngine;

public class PartSelectionAudio : MonoBehaviour
{
    void Start()
    {
        GetComponent<PartSelection>().OnCardSelected += OnCardSelected;
    }

    void OnDisable()
    {
        GetComponent<PartSelection>().OnCardSelected -= OnCardSelected;
    }

    private void OnCardSelected()
    {
        //play sound
    }
}
