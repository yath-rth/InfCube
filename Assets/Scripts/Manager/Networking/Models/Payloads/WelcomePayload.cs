using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class WelcomePayload
{
    public string playerId;
    public float spawnX, spawnY, spawnZ;
    public List<PlayerState> players;
    public int seed;

    public WelcomePayload() { }

    [JsonProperty("spawnPosition")]
    private JObject _spawnPosition
    {
        set { spawnX = (float)value["x"]; spawnY = (float)value["y"]; spawnZ = (float)value["z"]; }
    }
}
