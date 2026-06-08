package org.yatharth.infcube.game.logic;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketSession;
import org.yatharth.infcube.game.GameConstants;
import org.yatharth.infcube.game.SessionRegistry;
import org.yatharth.infcube.model.Player;
import org.yatharth.infcube.model.ServerMessage;
import org.yatharth.infcube.model.Vector3;
import org.yatharth.infcube.model.payloads.PlayerJoinPayload;
import org.yatharth.infcube.model.payloads.WelcomePayload;
import org.yatharth.infcube.util.WebSocketSessionUtil;
import tools.jackson.databind.ObjectMapper;

import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Component
@RequiredArgsConstructor
public class PlayerManager {

    private final SessionRegistry sessionRegistry;
    private final PathGenerator pathGenerator;
    private final Map<String, Player> players = new ConcurrentHashMap<>();
    private final ObjectMapper objectMapper;

    private final Vector3 spawnPosition = new Vector3(0f, -1f, -11f);

    private final int seed = new Random().nextInt(Integer.MAX_VALUE);

    public void addPlayer(WebSocketSession session) {
        sessionRegistry.add(session);
        players.put(session.getId(), new Player(spawnPosition, Vector3.forward(), GameConstants.SPEED));
        System.out.println("Player added: " + session.getId() + " and there are " + players.size() + " players.");

        if (players.size() == 2) {
            for (WebSocketSession ids : getSessions()) {
                String id = "player-" + UUID.randomUUID().toString().substring(0, 4);
                ServerMessage message = new ServerMessage(
                        "welcome",
                        GameConstants.TESTING_ROOM_ID,
                        System.currentTimeMillis(),
                        new WelcomePayload(
                                id, Vector3.zero(), getPlayers(), seed
                        )
                );

                if (ids != null && session.isOpen())
                    WebSocketSessionUtil.safeSend(ids, objectMapper.writeValueAsString(message));
            }
        }
    }

    public void removePlayer(WebSocketSession session) {
        players.remove(session.getId());
        sessionRegistry.remove(session.getId());
    }

    public void movePlayers() {
        if (players.isEmpty()) return;

        for (Player player : players.values()) {
            if (player == null) continue;
            player.position = player.position.add(player.direction.scale(player.speed * 0.02f));
//            pathGenerator.generatePath(player);
        }
    }

    public Player getPlayer(String id) {
        return players.get(id);
    }

    public List<Player> getPlayers() {
        return players.values().stream().toList();
    }

    public List<WebSocketSession> getSessions() {
        return sessionRegistry.getAll();
    }

}
