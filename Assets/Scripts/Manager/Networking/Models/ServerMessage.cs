using System;

[Serializable]
public class ServerMessage
{
    public string type;
    public string roomId;
    public long timestamp;
    public object payload;

    public ServerMessage() { }

    public ServerMessage(string type, string roomId, long timestamp, object payload = null)
    {
        this.type = type;
        this.roomId = roomId;
        this.timestamp = timestamp;
        this.payload = payload;
    }
}
