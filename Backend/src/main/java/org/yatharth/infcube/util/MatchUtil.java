package org.yatharth.infcube.util;

import org.yatharth.infcube.model.game.Match;
import org.yatharth.infcube.model.game.Player;

import java.util.List;

public class MatchUtil {

    public static Player getPlayerFromMatch(Match match, String sessionId) {
        if (match.getPlayer1().getSessionId().equals(sessionId)) return match.getPlayer1();
        if (match.getPlayer2().getSessionId().equals(sessionId)) return match.getPlayer2();
        return null;
    }

    public static Player getOpponentFromMatch(Match match, String sessionId) {
        if (match.getPlayer1().getSessionId().equals(sessionId)) return match.getPlayer2();
        if (match.getPlayer2().getSessionId().equals(sessionId)) return match.getPlayer1();
        return null;
    }

    public static List<String> getAllIds(Match match) {
        return List.of(match.player1.getSessionId(), match.player2.getSessionId());
    }

    public static List<Player> getAllPlayers(Match match) {
        return List.of(match.getPlayer1(), match.getPlayer2());
    }

}
