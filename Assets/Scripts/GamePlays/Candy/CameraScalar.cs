using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{

    private Board board;
    public float cameraOffset;
    public float aspectRatio = 0.625f;
    public float padding = 2;
    public float yOffset = 1;
    // Start is called before the first frame update
    void Start()
    {
        board = FindFirstObjectByType<Board>();
        if(board != null)
        {
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

    void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition = new(x / 2, y / 2 + yOffset, cameraOffset);
        transform.position = tempPosition;
        print(board.width + " " + board.height);
        if(board.width >= board.height)
        {
            Camera.main.orthographicSize = board.height + padding;
        }
        else
        {
            Camera.main.orthographicSize = (board.width + padding) * aspectRatio;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
