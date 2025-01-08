using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage images;
    [SerializeField] private float _x, _y, _z;
    private float timer = 0f;
    private bool reverse = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 5f)
        {
            reverse = !reverse;
            timer = 0f;
        }

        float direction = reverse ? -1f : 1f;
        images.uvRect = new Rect(images.uvRect.position + new Vector2(_x, _y) * direction * Time.deltaTime, images.uvRect.size);
    }
}