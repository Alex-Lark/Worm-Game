using UnityEngine;

namespace InputHandling
{
    public class GameSceneInputController : IInputController
    {
        public void HandleUpdate()
        {
            if (Input.GetKeyDown(KeyCode.W))
                Player.Player.Instance?.StartWormMoving();
        
            if (Input.GetKeyUp(KeyCode.W))
                Player.Player.Instance?.StopWormMoving();
        
            if (Input.GetKeyDown(KeyCode.Space))
                Player.Player.Instance?.Jump();
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
                Player.Player.Instance?.Attack();
        }
        
        public void HandleFixedUpdate()
        {
            if (Input.GetKey(KeyCode.W))
                Player.Player.Instance?.MoveForward();
        }
    }
}