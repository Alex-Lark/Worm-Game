using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    public GameObject thirdPersonCamera;
    public CharacterController controller;
    
    private float moveSpeed = 5f;
    private float rotationSpeed = 10f;
    
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
        
        print("moving forward: " + (camForward * moveSpeed * Time.deltaTime));
        controller.Move(camForward * moveSpeed * Time.deltaTime);
    }

    public void Move(float horizontalInput, float verticalInput)
    {
        
    }
}
