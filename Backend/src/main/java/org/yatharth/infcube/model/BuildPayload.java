package org.yatharth.infcube.model;

import org.yatharth.infcube.game.GameConstants;
import org.yatharth.infcube.model.game.Match;
import org.yatharth.infcube.model.game.Player;
import org.yatharth.infcube.model.game.ServerMessage;
import org.yatharth.infcube.model.payloads.WelcomePayload;

public class BuildPayload {

    public static ServerMessage welcome(Match match, Player self, Player other) {
        return new ServerMessage(
                "welcome",
                match.getMatchId(),
                System.currentTimeMillis(),
                new WelcomePayload(
                        self.getPlayerId(),
                        other.getPlayerId(),
                        GameConstants.SPAWN_POSITION,
                        match.getPath(),
                        match.getSpeed()
                )
        );
    }

}
