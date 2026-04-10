using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    public float speed = 0.03f;
    private RawImage img;

    void Start()
    {
        img = GetComponent<RawImage>();
    }

    void Update()
    {
        Rect r = img.uvRect;
        r.x += speed * Time.deltaTime;
        img.uvRect = r;
    }
}