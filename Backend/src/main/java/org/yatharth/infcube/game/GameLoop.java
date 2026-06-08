// game/GameLoop.java
package org.yatharth.infcube.game;

import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import tools.jackson.databind.ObjectMapper;

import java.util.List;
import java.util.Map;
import java.util.concurrent.*;

@Component
@RequiredArgsConstructor
public class GameLoop {

    private final GameController gameController;

    private final ScheduledExecutorService scheduler
            = Executors.newSingleThreadScheduledExecutor();

    private ScheduledFuture<?> loop;
    private static final String ROOM_ID = GameConstants.TESTING_ROOM_ID;

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
            gameController.updateAll();
        } catch (Exception e) {
            System.err.println("Tick error: " + e.getMessage());
            e.printStackTrace();
        }
    }
}