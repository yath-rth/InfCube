package org.yatharth.infcube.game;

import org.yatharth.infcube.model.Vector3;

public class GameConstants {

    private GameConstants() {}

    public static final float MIN_SPEED = 4f;
    public static final float MAX_SPEED = 12f;
    public static final float ACCELERATION = 0.0005f;
    public static Vector3 SPAWN_POSITION = new Vector3(0, 0, -10);

    public static final float TILE_SIZE = 1.25f;
    public static final int MIN_LENGTH = 1;
    public static final int MAX_LENGTH = 3;

    public static final int TICK_RATE_HZ        = 20;
    public static final long TICK_INTERVAL_MS   = 1000L / TICK_RATE_HZ;
    public static final String TESTING_ROOM_ID = "testing";

}
