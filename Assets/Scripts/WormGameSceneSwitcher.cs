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
        CreatureBuilder.CreatureBuilder.Instance.AttachCreatureParts();
        SceneManager.LoadScene("GameScene");
        Player.Instance.SetWormInGameScene();
    }
    
    public void LoadGameEndScene()
    {
        SceneManager.LoadScene("GameEndScene");
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
