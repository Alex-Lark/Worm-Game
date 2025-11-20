using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance;
    public bool isJumping = false;
    public bool isAttacking = false;
    
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
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                Player.Player.Instance.StartWormMoving();
            }
        }
        
        if (Input.GetKeyUp(KeyCode.W))
        {
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                Player.Player.Instance.StopWormMoving();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                isJumping = true;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                isAttacking = true;
            }
        }
    }

    private void FixedUpdate()
    {
        
        if (Input.GetKey(KeyCode.W))
        {
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                Player.Player.Instance.MoveForward();
            }
        }

        if (isJumping == true)
        {
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                Player.Player.Instance.Jump();
            }

            isJumping = false;
        }
        
        if (isAttacking == true)
        {
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                Player.Player.Instance.Attack();
            }
            isAttacking = false;
        }
    }
}
