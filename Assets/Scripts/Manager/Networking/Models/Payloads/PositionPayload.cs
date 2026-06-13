using System;
using UnityEngine;

[Serializable]
public class PositionPayload
{
    public float x;
    public float y;
    public float z;

    public PositionPayload(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }
}