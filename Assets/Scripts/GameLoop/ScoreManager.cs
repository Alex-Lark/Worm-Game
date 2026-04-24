using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; 

    private Dictionary<string, int> previousScores = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetPreviousScore(string playerName)
    {
        if (previousScores.TryGetValue(playerName, out int score))
            return score;

        return 0;
    }

    public void SetScore(string playerName, int score)
    {
        previousScores[playerName] = score;
    }
}