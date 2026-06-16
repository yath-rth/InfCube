package org.yatharth.infcube.websocket;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;
import org.yatharth.infcube.game.GameController;
import org.yatharth.infcube.model.game.ClientMessage;
import tools.jackson.databind.ObjectMapper;

@Component
@RequiredArgsConstructor
public class GameWebSocketHandler extends TextWebSocketHandler {

    private final GameController gameController;
    private final ObjectMapper objectMapper;

    @Override
    public void afterConnectionEstablished(WebSocketSession session) throws Exception {

    }

    @Override
    public void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
        ClientMessage decodedMsg = objectMapper.readValue(
                message.getPayload(),
                ClientMessage.class
        );
//        System.out.println("Received message type: " + decodedMsg.getType() + " message: " + decodedMsg.toString());

        if (decodedMsg.type == null) return;
        switch (decodedMsg.type) {
            case "input" -> gameController.handleInput(session, decodedMsg);
            case "join" -> gameController.handleJoin(session, decodedMsg);
            case "map_over" -> gameController.handleNewMap(session);
            case "position" -> gameController.handlePosition(session, decodedMsg);
        }
    }

    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus closeStatus) throws Exception {
        gameController.handleDisconnect(session);
    }
}
