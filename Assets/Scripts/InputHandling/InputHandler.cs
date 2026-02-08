using UnityEngine;
using UnityEngine.SceneManagement;

namespace InputHandling
{
    public class InputHandler : MonoBehaviour
    {
        //TODO: switch away from using player instance with multiplayer
    
        #region Public Variables
    
        public static InputHandler Instance;
    
        #endregion
    
        #region Private Variables
    
        private IInputController currentController;
    
        #endregion
    
        #region Built-In Methods
    
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetControllerForScene(scene.name);
        }
    
        #endregion
    
        #region Private Methods
    
        private void SetControllerForScene(string sceneName)
        {
            if (GameSceneList.IsSceneAGameScene(sceneName))
            {
                currentController = new GameSceneInputController();
            }
            else if (sceneName == "CreatureBuilderScene")
            {
                currentController = new CreatureBuilderInputController();
            }
            else
            {
                currentController = new MenuInputController();
            }
        }
    
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OptionsMenu.Instance?.TryToToggleOptionsMenu();
            }
            
            currentController?.HandleUpdate();
        }
    
        private void FixedUpdate()
        {
            currentController?.HandleFixedUpdate();
        }
    
        #endregion
    }
}