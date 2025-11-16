using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class WormGameSceneSwitcher : MonoBehaviour
{
    public event Action OnSceneLoaded;
    
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
        Player.Instance.DeactivatePlayer();
    }

    public void LoadSettingsScene()
    {
        SceneManager.LoadSceneAsync("SettingsScene");
    }
    
    public void LoadJoinGameScene()
    {
        SceneManager.LoadSceneAsync("JoinGameScene");
    }

    public void LoadCreateGameScene()
    {
        SceneManager.LoadSceneAsync("CreateGameScene");
    }

    public void LoadGameLobbyScene()
    {
        SceneManager.LoadSceneAsync("GameLobbyScene");
    }
    
    public void LoadPartSelectionScene()
    {
        StartCoroutine(LoadSceneCoroutine("PartSelectionScene"));
    }
    
    public void LoadCreatureBuilderScene()
    {
        SceneManager.LoadSceneAsync("CreatureBuilderScene");
    }
    
    public void LoadGameScene()
    {
        if (CreatureBuilder.CreatureBuilder.Instance != null)
        {
            Debug.Log("attaching creature parts");
            CreatureBuilder.CreatureBuilder.Instance.AttachCreatureParts();
        }
        
        SceneManager.LoadScene("GameScene");
        Player.Instance.SetWormInGameScene();
    }
    
    public void LoadLeaderboardScene()
    {
        SceneManager.LoadSceneAsync("LeaderboardScene");
    }
    
    public void LoadGameEndScene()
    {
        SceneManager.LoadSceneAsync("GameEndScene");
        Player.Instance.DeactivatePlayer();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        OnSceneLoaded?.Invoke();
    }
}
