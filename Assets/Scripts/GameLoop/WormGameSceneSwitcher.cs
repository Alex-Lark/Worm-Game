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
        Player.Player.Instance.DeactivatePlayer();
    }

    public void LoadSettingsScene()
    {
        SceneManager.LoadScene("SettingsScene");
    }
    
    public void LoadJoinGameScene()
    {
        SceneManager.LoadScene("JoinGameScene");
    }

    public void LoadCreateGameScene()
    {
        SceneManager.LoadScene("CreateGameScene");
    }

    public void LoadGameLobbyScene()
    {
        SceneManager.LoadScene("GameLobbyScene");
    }
    
    public void LoadPartSelectionScene()
    {
        SceneManager.LoadScene("PartSelectionScene");
    }
    
    public void LoadCreatureBuilderScene()
    {
        SceneManager.LoadScene("CreatureBuilderScene");
    }
    
    public void LoadGameScene()
    {
        SceneManager.LoadScene(GameSceneList.GetRandomGameScene());
        Player.Player.Instance.SetWormInGameScene();
    }
    
    public void LoadLeaderboardScene()
    {
        SceneManager.LoadScene("LeaderboardScene");
        Player.Player.Instance.DeactivatePlayer();
    }
    
    public void LoadGameEndScene()
    {
        SceneManager.LoadScene("GameEndScene");
        Player.Player.Instance.DeactivatePlayer();
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
