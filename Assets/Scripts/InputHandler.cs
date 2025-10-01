using System;
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

        // if (SceneManager.GetActiveScene().name == "GameScene")
        // {
        //     float h = Input.GetAxisRaw("Horizontal");
        //     float v = Input.GetAxisRaw("Vertical");
        //
        //     Player.Instance.Move(h, v);
        // }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                Player.Instance.StartWormMoving();
            }
        }
        
        if (Input.GetKeyUp(KeyCode.W))
        {
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                Player.Instance.StopWormMoving();
            }
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                Player.Instance.MoveForward();
            }
        }
    }
}
