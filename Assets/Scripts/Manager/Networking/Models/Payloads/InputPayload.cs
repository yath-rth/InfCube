using System;
using UnityEngine;

[Serializable]
public class InputPayload
{
    public int side;
    public int tileCount;
    public float posX;   // ← add
    public float posZ;   // ← add

    public InputPayload(int side, int tileCount, Vector3 pos)
    {
        this.side = side;
        this.tileCount = tileCount;
        this.posX = pos.x;
        this.posZ = pos.z;
    }

    public override string ToString()
    {
        return $"side: {side}, tileCount: {tileCount}";
    }
}
