package org.yatharth.infcube.model.game;

import lombok.AllArgsConstructor;
import lombok.Data;

@Data
@AllArgsConstructor
public class ServerMessage {
    public String type;
    public String roomId;
    public Long timestamp;
    public Object payload;
}
