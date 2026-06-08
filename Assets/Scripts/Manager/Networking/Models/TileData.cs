using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class TileData
{
    public float x, y, z;

    public TileData() { }

    [JsonProperty("position")]
    private JObject _position
    {
        set { x = (float)value["x"]; y = (float)value["y"]; z = (float)value["z"]; }
    }
}
