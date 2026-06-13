using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class PlayerState
{
    public String sessionId;
    public String playerId;
    public float posX, posY, posZ;
    public float dirX, dirY, dirZ;

    public PlayerState() { }

    [JsonProperty("position")]
    private JObject _position
    {
        set { posX = (float)value["x"]; posY = (float)value["y"]; posZ = (float)value["z"]; }
    }

    [JsonProperty("direction")]
    private JObject _direction
    {
        set { dirX = (float)value["x"]; dirY = (float)value["y"]; dirZ = (float)value["z"]; }
    }
}
