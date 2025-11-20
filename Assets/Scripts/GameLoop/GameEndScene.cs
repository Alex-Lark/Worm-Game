using TMPro;
using UnityEngine;

public class GameEndScene : MonoBehaviour
{
    public GameObject gameEndText;
    
    void Start()
    {
        ShowWinner();
    }

    private void ShowWinner()
    {
        Player.Player topPlayer = null;
        int highestScore = int.MinValue;

        foreach (Player.Player player in GameLoop.Instance.players)
        {
            if (player.PlayerScore > highestScore)
            {
                highestScore = player.PlayerScore;
                topPlayer = player;
            }
        }

        gameEndText.GetComponent<TextMeshProUGUI>().text = "Winner: " + topPlayer.PlayerName;
    }
}
