package org.yatharth.infcube.model.game;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.yatharth.infcube.game.GameConstants;

import java.util.List;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class Match {
    public String matchId;
    public int seed;
    public float speed = GameConstants.MIN_SPEED;
    public List<PathInfo> path;

    public Player player1;
    public Player player2;
}
