using System;
using System.Collections.Generic;

[Serializable]
public class UpdatePayload
{
    public List<PlayerState> players;
    public List<TileData> tiles;

    public UpdatePayload() { }

    public UpdatePayload(List<PlayerState> players, List<TileData> tiles)
    {
        this.players = players;
        this.tiles = tiles;
    }
}
