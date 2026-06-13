package org.yatharth.infcube.util;

import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;

import java.util.List;

public final class WebSocketUtil {

    private WebSocketUtil() {
    }

    public static void safeSend(WebSocketSession session, String message) {
        if (session == null || !session.isOpen()) return;
        try {
            synchronized (session) {
                session.sendMessage(new TextMessage(message));
            }
        } catch (Exception e) {
            System.err.println("Failed to send message to " + session.getId() + ": " + e.getMessage());
        }
    }

    public static void sendAll(List<WebSocketSession> sessions, String message) {
        for (WebSocketSession session : sessions) {
            if (session != null && session.isOpen()) {
                WebSocketUtil.safeSend(session, message);
            }
        }
    }

}
