package org.yatharth.infcube.model.payloads;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.yatharth.infcube.model.game.PathInfo;
import org.yatharth.infcube.model.game.Vector3;

import java.util.List;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class WelcomePayload {
    public String playerId;
    public String otherId;
    public Vector3 spawnPosition;
    public List<PathInfo> path;
    public float startSpeed;
}
