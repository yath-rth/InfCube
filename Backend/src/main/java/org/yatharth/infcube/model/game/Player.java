package org.yatharth.infcube.model.game;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class Player {
    public String sessionId;
    public String playerId;
    public Vector3 position;
    public Vector3 direction;
}
