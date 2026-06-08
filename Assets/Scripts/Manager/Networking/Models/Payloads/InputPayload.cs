using System;

[Serializable]
public class InputPayload
{
    public int side;

    public InputPayload() { }

    public InputPayload(int side)
    {
        this.side = side;
    }
}
