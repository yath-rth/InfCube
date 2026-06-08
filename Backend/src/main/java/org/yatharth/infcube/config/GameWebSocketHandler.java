package org.yatharth.infcube.config;

import lombok.RequiredArgsConstructor;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;
import org.yatharth.infcube.game.GameConstants;
import org.yatharth.infcube.game.GameController;
import org.yatharth.infcube.game.SessionRegistry;
import org.yatharth.infcube.model.ClientMessage;
import org.yatharth.infcube.model.ServerMessage;
import org.yatharth.infcube.model.Vector3;
import org.yatharth.infcube.model.payloads.WelcomePayload;
import org.yatharth.infcube.util.WebSocketSessionUtil;
import tools.jackson.databind.ObjectMapper;

import java.util.UUID;

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
        ClientMessage decodedMsg = objectMapper.readValue(message.getPayload(), ClientMessage.class);
        System.out.println("Received message type: " + decodedMsg.getType());

        switch (decodedMsg.type) {
            case "input" -> gameController.handleInput(session, decodedMsg);
            case "join" -> gameController.handleJoin(session, decodedMsg);
        }
    }

    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus closeStatus) throws Exception {
        gameController.handleDisconnect(session);
    }
}
