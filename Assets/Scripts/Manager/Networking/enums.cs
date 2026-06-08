public enum InputDir
{
    NONE,
    FRONT,
    BACK,
    RIGHT,
    LEFT,
    FRIGHT,
    FLEFT,
    BRIGHT,
    BLEFT
}

public enum InputType
{
    NONE, MOVE, SHOOT
}

public enum ServerMessageType
{
    WELCOME,
    JOINED,
    UPDATE,
    PLAYER_JOIN,
    PLAYER_EXIT,
    EXIT,
    MAP_LAYOUT_CHANGE,
}

public enum ClientMessageType
{
    JOIN,
    UPDATE,
    EXIT
}

public enum EnemyState
{
    NONE,
    CHASING,
    ATTACK,
    DEAD
}

public enum ObstacleType
{
    WALL,
    BARREL,
    CRATE,
}

public enum PickupType
{
    AMMO,
    HEALTH,
    POWERUP_DAMAGE,
    POWERUP_SPEED,
    POWERUP_INFINITE_AMMO,
}

public enum PowerUpEffect
{
    NONE,
    DAMAGE_BOOST,
    SPEED_BOOST,
    INFINITE_AMMO,
}
