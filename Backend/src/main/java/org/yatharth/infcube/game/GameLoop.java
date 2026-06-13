// game/GameLoop.java
package org.yatharth.infcube.game;

import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.TimeUnit;

@Component
@RequiredArgsConstructor
public class GameLoop {

    private static final String ROOM_ID = GameConstants.TESTING_ROOM_ID;
    private final GameController gameController;
    private final MatchStore matchStore;
    private final ScheduledExecutorService scheduler
            = Executors.newSingleThreadScheduledExecutor();
    private ScheduledFuture<?> loop;

    @PostConstruct
    public void start() {
        loop = scheduler.scheduleAtFixedRate(
                this::tick,
                0,
                GameConstants.TICK_INTERVAL_MS,
                TimeUnit.MILLISECONDS
        );
        System.out.println("Game loop started for room: " + ROOM_ID);
    }

    @PreDestroy
    public void stop() {
        if (loop != null) loop.cancel(false);
        scheduler.shutdown();
        System.out.println("Game loop stopped.");
    }

    private void tick() {
        try {
            for (String matchId : matchStore.getAllMatchIds()) {
                gameController.updateMatch(matchId);
            }
        } catch (Exception e) {
            System.err.println("Tick error: " + e.getMessage());
            e.printStackTrace();
        }
    }
}