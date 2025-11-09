using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class WormGameSceneSwitcher : MonoBehaviour
{
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
        SceneManager.LoadSceneAsync("PartSelectionScene");
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
        
        SceneManager.LoadSceneAsync("GameScene");
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
}
