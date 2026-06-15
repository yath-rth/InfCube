package org.yatharth.infcube.game;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketSession;
import org.yatharth.infcube.model.*;
import org.yatharth.infcube.model.payloads.*;
import org.yatharth.infcube.service.MatchService;
import org.yatharth.infcube.util.MatchUtil;
import org.yatharth.infcube.util.WebSocketUtil;
import tools.jackson.databind.ObjectMapper;

import java.util.List;

@Component
@RequiredArgsConstructor
public class GameController {

    private final ObjectMapper objectMapper;
    private final MatchService matchService;
    private final PathGenerator pathGenerator;
    private final SessionRegistry sessionRegistry;
    private final MatchStore matchStore;

    public void handleJoin(WebSocketSession session, ClientMessage message) {
        Match match = matchService.queuePlayer(session);

        if (match != null) {
            sendWelcome(match);
        }
    }

    private void sendWelcome(Match match) {
        Player p1 = match.getPlayer1();
        Player p2 = match.getPlayer2();

        WebSocketUtil.safeSend(
                sessionRegistry.get(p1.getSessionId()),
                objectMapper.writeValueAsString(BuildPayload.welcome(match, p1, p2))
        );
        WebSocketUtil.safeSend(
                sessionRegistry.get(p2.getSessionId()),
                objectMapper.writeValueAsString(BuildPayload.welcome(match, p2, p2))
        );
    }

    public void handleNewMap(WebSocketSession session) {
        String matchId = sessionRegistry.getMatchId(session.getId());
        if (matchId == null) return;
        Match match = matchStore.getMatch(matchId);
        if (match == null) return;
        List<WebSocketSession> sessions = sessionRegistry.getSessions(MatchUtil.getAllIds(match));

        List<PathInfo> map = pathGenerator.generatePath(match.getSeed());
        match.path.addAll(map);
        WebSocketUtil.sendAll(
                sessions,
                objectMapper.writeValueAsString(new ServerMessage(
                        "map_update",
                        GameConstants.TESTING_ROOM_ID,
                        System.currentTimeMillis(),
                        new NewMapPayload(map)
                ))
        );
    }

    public void handlePosition(WebSocketSession session, ClientMessage decodedMsg) {
        String matchId = sessionRegistry.getMatchId(session.getId());
        if (matchId == null) return;
        Match match = matchStore.getMatch(matchId);
        if (match == null) return;

        PositionPayload payload = objectMapper.convertValue(
                decodedMsg.payload,
                PositionPayload.class
        );
        Vector3 position = new Vector3(payload.x, payload.y, payload.z);

        if (position.y < -1f) {
            System.out.println("Player " + session.getId() + " fell out of the map.");
            List<WebSocketSession> sessions = sessionRegistry.getSessions(MatchUtil.getAllIds(match));
            WebSocketUtil.sendAll(
                    sessions,
                    objectMapper.writeValueAsString(
                            new ServerMessage(
                                    "game_over",
                                    GameConstants.TESTING_ROOM_ID,
                                    System.currentTimeMillis(),
                                    new GameOverPayload(session.getId())
                            )
                    )
            );

            matchService.stopMatch(match.getMatchId());
        }
    }

    public void handleInput(WebSocketSession session, ClientMessage message) {
        String matchId = sessionRegistry.getMatchId(session.getId());
        if (matchId == null) return;
        Match match = matchStore.getMatch(matchId);
        if (match == null) return;

        InputPayload payload = objectMapper.convertValue(message.payload, InputPayload.class);

        Player player = MatchUtil.getPlayerFromMatch(match, session.getId());
        if (player == null) return;
        player.direction = new Vector3(payload.side, 0f, 1f);
        player.position = new Vector3(payload.posX, 0f, payload.posZ);

        WebSocketSession opponentSession =
                sessionRegistry.get(MatchUtil.getOpponentFromMatch(match, session.getId())
                        .getSessionId());
        WebSocketUtil.safeSend(
                opponentSession,
                objectMapper.writeValueAsString(new ServerMessage(
                        "player_move",
                        GameConstants.TESTING_ROOM_ID,
                        System.currentTimeMillis(),
                        message.payload
                ))
        );
    }

    public void updateMatch(String matchId) {
        if (matchId == null) return;
        Match match = matchStore.getMatch(matchId);
        if (match == null) return;

        match.setSpeed(match.getSpeed() + GameConstants.ACCELERATION);
        if (match.getSpeed() > GameConstants.MAX_SPEED) match.setSpeed(GameConstants.MAX_SPEED);
        List<Player> players = MatchUtil.getAllPlayers(match);

        ServerMessage message = new ServerMessage(
                "update",
                GameConstants.TESTING_ROOM_ID,
                System.currentTimeMillis(),
                new UpdatePayload(players, match.getSpeed())
        );
        String json = objectMapper.writeValueAsString(message);
        if (players.isEmpty()) return;
        List<WebSocketSession> sessions = sessionRegistry.getSessions(MatchUtil.getAllIds(match));
        if (sessions.isEmpty()) return;

        WebSocketUtil.sendAll(sessions, json);
    }

    public void handleDisconnect(WebSocketSession session) {
        String matchId = sessionRegistry.getMatchId(session.getId());
        if (matchId == null) {
            matchService.removePlayer(session);
            return;
        }

        Match match = matchStore.getMatch(matchId);
        if (match == null) return;

        Player opponent = MatchUtil.getOpponentFromMatch(match, session.getId());
        if (opponent != null) {
            WebSocketUtil.safeSend(
                    sessionRegistry.get(opponent.getSessionId()),
                    objectMapper.writeValueAsString(new ServerMessage(
                            "opponent_disconnected", matchId,
                            System.currentTimeMillis(),
                            null
                    ))
            );
        }

        matchService.stopMatch(matchId);
    }


}
