package org.yatharth.infcube.util;

import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketMessage;
import org.springframework.web.socket.WebSocketSession;

public final class WebSocketSessionUtil {

    private WebSocketSessionUtil() {}

    public static void safeSend(WebSocketSession session, String message) {
        try {
            synchronized (session) {
                session.sendMessage(new TextMessage(message));
            }
        } catch (Exception e) {
            System.err.println("Failed to send message: " + e.getMessage());
        }
    }

}
