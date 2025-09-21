using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
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
        gameObject.GetComponent<Transform>().position = new Vector3(gameObject.GetComponent<Transform>().position.x + 1, gameObject.GetComponent<Transform>().position.y, gameObject.GetComponent<Transform>().position.z);
    }
}
