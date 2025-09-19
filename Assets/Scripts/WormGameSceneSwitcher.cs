using UnityEngine;
using UnityEngine.SceneManagement;

public class WormGameSceneSwitcher : MonoBehaviour
{
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
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
}
