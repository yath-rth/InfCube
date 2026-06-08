package org.yatharth.infcube.game;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.yatharth.infcube.game.logic.PathGenerator;
import org.yatharth.infcube.game.logic.PlayerManager;
import org.yatharth.infcube.model.ClientMessage;
import org.yatharth.infcube.model.Player;
import org.yatharth.infcube.model.ServerMessage;
import org.yatharth.infcube.model.Vector3;
import org.yatharth.infcube.model.payloads.InputPayload;
import org.yatharth.infcube.model.payloads.UpdatePayload;
import org.yatharth.infcube.util.WebSocketSessionUtil;
import tools.jackson.databind.ObjectMapper;

import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

@Component
@RequiredArgsConstructor
public class GameController {

    private final ObjectMapper objectMapper;
    private final PlayerManager playerManager;
    private final PathGenerator pathGenerator;

    public void handleJoin(WebSocketSession session, ClientMessage message) {
        playerManager.addPlayer(session);
        System.out.println("Player joined: " + session.getId());
    }

    public void handleInput(WebSocketSession session, ClientMessage message) {
        InputPayload payload = objectMapper.convertValue(message.payload, InputPayload.class);

        Player player = playerManager.getPlayer(session.getId());
        if (player == null) return;
        player.direction = new Vector3(payload.side, 0f, 1f);
    }

    public void updateAll() throws Exception {
        if(playerManager.getPlayers().size() < 2) return;

        playerManager.movePlayers();
        ServerMessage message = new ServerMessage(
                "update", GameConstants.TESTING_ROOM_ID, System.currentTimeMillis(),
                new UpdatePayload(
                        playerManager.getPlayers(), null
                )
        );
        String json = objectMapper.writeValueAsString(message);
        if(playerManager.getPlayers().isEmpty()) return;
        if(playerManager.getSessions().isEmpty()) return;

        for (WebSocketSession session : playerManager.getSessions()) {
            if (session != null && session.isOpen())
                WebSocketSessionUtil.safeSend(session, json);
        }
    }

    public void handleDisconnect(WebSocketSession session) throws Exception {
        playerManager.removePlayer(session);
        System.out.println("Player removed: " + session.getId());
    }

}
