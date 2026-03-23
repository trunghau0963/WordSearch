using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage images;
    [SerializeField] private float _x, _y, _z;
    [SerializeField] private float cycleDuration = 5f;
    private float timer = 0f;
    private bool reverse = false;
    private float _currentDirection = 1f;
    private float _targetDirection = 1f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= cycleDuration)
        {
            reverse = !reverse;
            _targetDirection = reverse ? -1f : 1f;
            timer = 0f;
        }

        // Smooth direction transition instead of abrupt flip
        _currentDirection = Mathf.Lerp(_currentDirection, _targetDirection, Time.deltaTime * 3f);

        images.uvRect = new Rect(
            images.uvRect.position + new Vector2(_x, _y) * (_currentDirection * Time.deltaTime),
            images.uvRect.size);
    }
}