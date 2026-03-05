using Player;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    //attach to player object

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            LocalPlayer.Instance.OnPlayerDeath();
        }
    }
}
