using UnityEngine;

namespace InputHandling
{
    public class GameSceneInputController : IInputController
    {
        public void HandleUpdate()
        {
            if (Input.GetKeyDown(KeyCode.W))
                Player.LocalPlayer.Instance?.StartWormMoving();
        
            if (Input.GetKeyUp(KeyCode.W))
                Player.LocalPlayer.Instance?.StopWormMoving();
        
            if (Input.GetKeyDown(KeyCode.Space))
                Player.LocalPlayer.Instance?.Jump();
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
                Player.LocalPlayer.Instance?.Attack();
        }
        
        public void HandleFixedUpdate()
        {
            if (Input.GetKey(KeyCode.W))
                Player.LocalPlayer.Instance?.MoveForward();
        }
    }
}