using System;

[Serializable]
public class ClientMessage
{
    public string type;
    public string roomId;
    public string playerId;
    public object payload;

    public ClientMessage() { }

    public ClientMessage(string type, string roomId, string playerId, object payload = null)
    {
        this.type = type;
        this.roomId = roomId;
        this.playerId = playerId;
        this.payload = payload;
    }
}
