package org.yatharth.infcube.model;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class ClientMessage {
    public String type;
    public String roomId;
    public String playerId;
    public Object payload;
}
