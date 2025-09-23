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
    private float maxPartDistance = 1f;
    
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
        
        controller.Move(camForward * moveSpeed * Time.deltaTime);
    }
}
