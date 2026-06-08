// util/SessionRegistry.java
package org.yatharth.infcube.game;

import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketSession;

import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

@Component
public class SessionRegistry {

    private final Map<String, WebSocketSession> sessions = new ConcurrentHashMap<>();

    public void add(WebSocketSession session) {
        sessions.put(session.getId(), session);
    }

    public void remove(String sessionId) {
        sessions.remove(sessionId);
    }

    public WebSocketSession get(String sessionId) {
        return sessions.get(sessionId);
    }

    public List<WebSocketSession> getAll(){
        return sessions.values().stream().toList();
    }
}