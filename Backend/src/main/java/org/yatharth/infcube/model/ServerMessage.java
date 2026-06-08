package org.yatharth.infcube.model;

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
