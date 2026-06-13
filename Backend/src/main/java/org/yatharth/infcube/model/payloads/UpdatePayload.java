package org.yatharth.infcube.model.payloads;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.yatharth.infcube.model.Player;

import java.util.List;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UpdatePayload {
    public List<Player> players;
    public float speed;
}
