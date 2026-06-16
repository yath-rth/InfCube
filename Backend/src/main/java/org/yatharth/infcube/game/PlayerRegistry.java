package org.yatharth.infcube.game;

import org.springframework.stereotype.Component;
import org.yatharth.infcube.model.game.Player;
import org.yatharth.infcube.model.game.Vector3;

import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.stream.Collectors;

@Component
public class PlayerRegistry {
    private final Map<String, Player> sessions = new ConcurrentHashMap<>();

    public void addPlayer(String sessionId) {
        if (sessions.containsKey(sessionId)) {
            return;
        }
        String playerId = "player-" + UUID.randomUUID().toString().substring(0, 4);
        sessions.put(
                sessionId, new Player(
                        sessionId, playerId, GameConstants.SPAWN_POSITION,
                        Vector3.forward()
                )
        );
    }

    public List<Player> getPlayers(List<String> ids) {
        return ids.stream()
                .filter(sessions::containsKey)
                .map(sessions::get)
                .collect(Collectors.toList());
    }

    public void removePlayer(String sessionId) {
        sessions.remove(sessionId);
    }

    public Player getPlayer(String sessionId) {
        return sessions.get(sessionId);
    }

    public List<Player> getAllPlayers() {
        return sessions.values().stream().toList();
    }
}
