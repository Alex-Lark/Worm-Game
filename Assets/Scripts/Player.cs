using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    public GameObject thirdPersonCamera;
    public CharacterController controller;

    public Transform wormHead;
    public List<Transform> wormParts;
    
    private float moveSpeed = 5f;
    private float rotationSpeed = 10f;
    private float maxPartDistance = 0.5f;
    
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

    void Start()
    {
        ConstructWorm();
    }

    private void ConstructWorm()
    {
        // Start from the head
        Vector3 currentPos = wormHead.position;
        Vector3 backDir = -wormHead.forward; // opposite of head's facing direction

        for (int i = 0; i < wormParts.Count; i++)
        {
            // Position each part maxPartDistance behind the previous one
            currentPos += backDir * maxPartDistance;

            Transform part = wormParts[i];
            part.position = currentPos;

            // Optional: align rotation with head
            part.rotation = wormHead.rotation;
        }
    }

    public void MoveForward()
    {
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        
        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            wormHead.rotation = Quaternion.Slerp(wormHead.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
        
        controller.Move(camForward * moveSpeed * Time.deltaTime);

        //MoveWormBody();
    }

    private void MoveWormBody() 
    {
        Vector3 previousPosition = wormHead.transform.position;
        float maxMovePerFrame = moveSpeed * Time.deltaTime; // Limit body movement speed
    
        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform part = wormParts[i];
            Vector3 toPrev = previousPosition - part.position;
            float distance = toPrev.magnitude;
        
            if (distance > maxPartDistance)
            {
                float moveDistance = distance - maxPartDistance;
                // Clamp the movement to prevent excessive speed
                moveDistance = Mathf.Min(moveDistance, maxMovePerFrame);
            
                part.position += toPrev.normalized * moveDistance;
            
                if (toPrev.sqrMagnitude > 0.001f)
                    part.rotation = Quaternion.Slerp(part.rotation,
                        Quaternion.LookRotation(toPrev),
                        rotationSpeed * Time.deltaTime);
            }
        
            previousPosition = part.position;
        }
    }
}
