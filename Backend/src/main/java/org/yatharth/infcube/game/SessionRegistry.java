// util/SessionRegistry.java
package org.yatharth.infcube.game;

import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketSession;

import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.stream.Collectors;

@Component
public class SessionRegistry {

    private final Map<String, WebSocketSession> sessions = new ConcurrentHashMap<>();
    private final Map<String, String> sessionToMatch = new ConcurrentHashMap<>();
    private final Map<String, String> userToSession = new ConcurrentHashMap<>();

    public void add(WebSocketSession session) {
        sessions.put(session.getId(), session);
    }

    public void remove(String sessionId) {
        sessions.remove(sessionId);
        sessionToMatch.remove(sessionId);
    }

    public List<WebSocketSession> getSessions(List<String> ids) {
        return ids.stream()
                .filter(sessions::containsKey)
                .map(sessions::get)
                .collect(Collectors.toList());
    }

    public WebSocketSession get(String sessionId) {
        return sessions.get(sessionId);
    }

    public List<WebSocketSession> getAll() {
        return sessions.values().stream().toList();
    }

    public void registerMatch(String sessionId, String matchId) {
        sessionToMatch.put(sessionId, matchId);
    }

    public String getMatchId(String sessionId) {
        return sessionToMatch.get(sessionId);
    }
}