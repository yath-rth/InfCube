package org.yatharth.infcube.game;

import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;

@Service
public class MatchMakingQueue {

    //Stores the session ids of all players currently searching for a match
    private final Queue<String> matchMakingQueue = new ConcurrentLinkedQueue<String>();

    public void addPlayer(String sessionId) {
        matchMakingQueue.add(sessionId);
    }

    public void removePlayer(String sessionId) {
        matchMakingQueue.remove(sessionId);
    }

    /**
     * Returns a list of the first two players in the queue, removing them from the queue.
     * If there are fewer than two players, returns null.
     * Used to match two people for a match
     */
    public synchronized List<String> getMatchedPlayers() {
        if (matchMakingQueue.size() >= 2) {
            return List.of(matchMakingQueue.poll(), matchMakingQueue.poll());
        }
        return null;
    }

}
