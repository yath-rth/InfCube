package org.yatharth.infcube.game;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.web.socket.WebSocketSession;
import org.yatharth.infcube.model.Match;
import org.yatharth.infcube.model.PathInfo;
import org.yatharth.infcube.model.Player;

import java.util.List;
import java.util.Random;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class MatchService {

    private final MatchStore matchStore;
    private final MatchMakingQueue matchMakingQueue;
    private final PlayerRegistry playerRegistry;
    private final SessionRegistry sessionRegistry;
    private final PathGenerator pathGenerator;

    public Match queuePlayer(WebSocketSession session) {
        String sessionId = session.getId();
        matchMakingQueue.addPlayer(sessionId);
        playerRegistry.addPlayer(sessionId);
        sessionRegistry.add(session);

        List<String> matchedPlayers = matchMakingQueue.getMatchedPlayers();
        if (matchedPlayers != null) {
            return startMatch(playerRegistry.getPlayers(matchedPlayers));
        }
        return null;
    }

    public void removePlayer(WebSocketSession session) {
        String sessionId = session.getId();
        matchMakingQueue.removePlayer(sessionId);
        playerRegistry.removePlayer(sessionId);
        sessionRegistry.remove(session.getId());
    }

    public Match startMatch(List<Player> players) {
        if (players.size() != 2) {
            throw new IllegalArgumentException("Exactly two players are required for a match");
        }
        Player player1 = players.get(0);
        Player player2 = players.get(1);

        if (player1 == null || player2 == null) {
            throw new IllegalArgumentException("Both players must be provided");
        }

        String matchId = "match-" + UUID.randomUUID().toString().substring(0, 5);
        int seed = new Random().nextInt(Integer.MAX_VALUE);
        List<PathInfo> path = pathGenerator.generatePath(seed);
        Match match = new Match(matchId, seed, GameConstants.MIN_SPEED, path, player1, player2);
        matchStore.addMatch(match);
        sessionRegistry.registerMatch(player1.getSessionId(), matchId);
        sessionRegistry.registerMatch(player2.getSessionId(), matchId);

        System.out.println("Match started between " + player1.getPlayerId() + " and " + player2.getPlayerId());
        return match;
    }

    public void stopMatch(String matchId) {
        Match match = matchStore.deleteMatch(matchId);
        if (match == null) return;

        String p1Id = match.getPlayer1().getSessionId();
        String p2Id = match.getPlayer2().getSessionId();

        playerRegistry.removePlayer(p1Id);
        playerRegistry.removePlayer(p2Id);

        sessionRegistry.remove(p1Id);
        sessionRegistry.remove(p2Id);

        // matchLoopManager.stopLoop(matchId); ← add this when game loop is built

        System.out.println("Match stopped: " + matchId);
    }
}
