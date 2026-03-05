using System.Collections.Generic;
using GameLoop;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance;

    public GameObject optionsPanel;
    
    private bool isOpen = false;
    
    private HashSet<string> allowedScenes = new HashSet<string>
    {
        "GameLobbyScene",
        "PartSelectionScene",
        "CreatureBuilderScene",
        "GameScene",
        "Worm League"
    };
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
    
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"Scene changed from {oldScene.name} to {newScene.name}");
        
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (!allowedScenes.Contains(currentScene))
        {
            CloseOptionsMenu();
        }
    }

    public void TryToToggleOptionsMenu()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (allowedScenes.Contains(currentScene))
        {
            if (!isOpen)
            {
                OpenOptionsMenu();
            }
            else
            {
                CloseOptionsMenu();
            }
        }
    }

    private void OpenOptionsMenu()
    {
        optionsPanel.SetActive(true);
        isOpen = true;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);
        isOpen = false;

        if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
