using UnityEngine;

namespace InputHandling
{
    public class CreatureBuilderInputController : IInputController
    {
        private CreatureBuilder.CreatureBuilderWindow window = Object.FindFirstObjectByType<CreatureBuilder.CreatureBuilderWindow>();
        
        public void HandleUpdate()
        {
            // mouse controls handled in CreatureBuilder window, any keybaord controls can go here
        }
        
        public void HandleFixedUpdate()
        {
            
        }
    }
}