using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class WelcomePayload
{
    public string playerId;
    public string otherId;
    public float spawnX, spawnY, spawnZ;
    public Queue<PathInfo> path;
    public float startSpeed;

    public WelcomePayload() { }

    [JsonProperty("spawnPosition")]
    private JObject _spawnPosition
    {
        set { spawnX = (float)value["x"]; spawnY = (float)value["y"]; spawnZ = (float)value["z"]; }
    }
}

[Serializable]
public class PathInfo
{
    public int side;
    public int count;
}
