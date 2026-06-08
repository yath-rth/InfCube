package org.yatharth.infcube.game.logic;

import org.springframework.stereotype.Component;
import org.yatharth.infcube.game.GameConstants;
import org.yatharth.infcube.model.Player;
import org.yatharth.infcube.model.Tile;
import org.yatharth.infcube.model.Vector3;

import java.util.LinkedList;
import java.util.List;

@Component
public class PathGenerator {

    private final List<Tile> tiles = new LinkedList<>();

    public void generatePath(Player player) {
        Tile lastTile = (tiles.size() > 1) ? tiles.get(tiles.size() - 1) : null;
        float distance = player.getPosition().subtract((lastTile != null) ? lastTile.getPosition() : Vector3.zero()).magnitude();

        if (distance > GameConstants.TILE_SIZE / 2f) {
            Tile newTile = new Tile(player.getPosition().add(new Vector3(0f, 0f, GameConstants.TILE_SIZE)));
            tiles.add(newTile);
        }
    }

    public List<Tile> getCurrentTiles() {
        return tiles;
    }
}
