package org.yatharth.infcube.game;

import org.springframework.stereotype.Repository;
import org.yatharth.infcube.model.Match;
import org.yatharth.infcube.model.PathInfo;
import org.yatharth.infcube.model.Player;

import java.util.*;
import java.util.concurrent.ConcurrentHashMap;

@Repository
public class MatchStore {

    private final Map<String, Match> matchMap = new ConcurrentHashMap<>();

    public Match getMatch(String matchId) {
        if(matchId == null) return null;
        return matchMap.get(matchId);
    }

    public void addMatch(Match match) {
        matchMap.put(match.getMatchId(), match);
    }

    public void updateMatch(Match match) {
        matchMap.put(match.getMatchId(), match);
    }

    public Match deleteMatch(String matchId) {
        Match deleteMatch = matchMap.get(matchId);
        matchMap.remove(matchId);
        return deleteMatch;
    }

    public List<String> getAllMatchIds() {
        return new ArrayList<>(matchMap.keySet());
    }

}
