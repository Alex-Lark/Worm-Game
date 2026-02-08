using UnityEngine;

namespace InputHandling
{
    public class CreatureBuilderInputController : IInputController
    {
        public void HandleUpdate()
        {
            // Add creature builder specific inputs here
            // Example:
            // if (Input.GetKeyDown(KeyCode.R))
            //     CreatureBuilder.Instance?.RotatePart();
            //
            // if (Input.GetMouseButtonDown(0))
            //     CreatureBuilder.Instance?.SelectPart();
            //
            // if (Input.GetMouseButtonDown(1))
            //     CreatureBuilder.Instance?.DeselectPart();
        }
        
        public void HandleFixedUpdate()
        {
            // Creature builder typically doesn't need FixedUpdate input handling
        }
    }
}