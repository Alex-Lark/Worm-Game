using Unity.VisualScripting;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //hi
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered collision with " + collision.gameObject.name);
        collision.rigidbody.AddForce(GameParameters.JumpPadForce * (gameObject.transform.up));
    }       
}
