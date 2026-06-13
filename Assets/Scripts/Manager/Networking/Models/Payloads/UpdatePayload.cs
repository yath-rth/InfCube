using System;
using System.Collections.Generic;

[Serializable]
public class UpdatePayload
{
    public List<PlayerState> players;
    public float speed;

    public UpdatePayload() { }

    public UpdatePayload(List<PlayerState> players, float speed)
    {
        this.players = players;
        this.speed = speed;
    }

    public override string ToString()
    {
        return $"UpdatePayload{{ players: {players}, speed: {speed} }}";
    }
}
