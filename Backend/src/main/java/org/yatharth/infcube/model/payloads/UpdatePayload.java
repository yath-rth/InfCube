package org.yatharth.infcube.model.payloads;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.RequiredArgsConstructor;
import org.yatharth.infcube.model.Player;
import org.yatharth.infcube.model.Tile;

import java.util.List;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UpdatePayload {
    public List<Player> players;
    public List<Tile> tiles;
}
