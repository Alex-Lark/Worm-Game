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

        if (Input.GetKey(KeyCode.W))
        {
            print("w pressed");
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                Player.Instance.MoveForward();
            }
        }

        // if (SceneManager.GetActiveScene().name == "GameScene")
        // {
        //     float h = Input.GetAxisRaw("Horizontal");
        //     float v = Input.GetAxisRaw("Vertical");
        //
        //     Player.Instance.Move(h, v);
        // }
    }
}
