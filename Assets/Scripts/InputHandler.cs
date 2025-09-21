using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OptionsMenu.Instance.TryToToggleOptionsMenu();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                Player.Instance.MoveForward();
            }
        }
    }
}
