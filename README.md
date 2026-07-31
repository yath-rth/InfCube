# InfCube Server Architecture

A real-time multiplayer game server built with Spring Boot, handling user authentication and live WebSocket-based gameplay for a 2-player competitive arena.

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Architecture Overview](#architecture-overview)
- [Feature Breakdown](#feature-breakdown)
- [How to Run](#how-to-run)
- [API Reference](#api-reference)
- [WebSocket Protocol](#websocket-protocol)
- [Learnings & Architecture Decisions](#learnings--architecture-decisions)

---

## Tech Stack

| Category | Technology |
|---|---|
| **Runtime** | Java 17, Spring Boot 4.0.6 |
| **Build** | Gradle |
| **Auth** | JWT (jjwt), BCrypt password hashing |
| **Real-time** | Spring WebSocket (`spring-boot-starter-websocket`) |
| **Security** | Spring Security, `SessionCreationPolicy.STATELESS` |
| **Data Persistence** | In-memory `ConcurrentHashMap` (no external DB) |
| **Game Loop** | `ScheduledExecutorService` — 20 Hz fixed-tick loop |
| **Helpers** | Lombok (boilerplate reduction), Jackson JSON |

---

## Architecture Overview

```
┌──────────────┐          ┌─────────────────────────────────────────┐
│   Unity      │◄────────►│         Spring Boot Server              │
│   Clients    │  WS/JSON │                                         │
└──────────────┘          └─────────────────────────────────────────┘
                            │
       ┌────────────────────┼────────────────────┐
       ▼                    ▼                     ▼
┌───────────────┐  ┌───────────────┐   ┌──────────────────────┐
│ Auth Layer    │  │ WebSocket     │   │ Game Loop            │
│ (HTTP REST)   │  │ Handler       │   │ (ScheduledExecutor)  │
├───────────────┤  ├───────────────┤   ├──────────────────────┤
│ /auth/register│  │ /game WS      │   │ GameLoop.tick() @20Hz│
│ /auth/login   │  │               │   ├──────────────────────┤
│ /auth/refresh │  │  GameController │   │ For each active match: │
│ /auth/logout  │  │               │   │   updateMatch()        │
└───────────────┘  └───────────────┘   └──────────────────────┘
                         │                       │
                    ┌────┴────┐          ┌───────┴────────┐
                    │SessionRegistry│      │MatchStore      │
                    │PlayerRegistry │      │PathGenerator    │
                    │              │      │MatchMakingQueue │
                    └─────────────┘      └────────────────┘
```

### Core Components

| Layer | Component | Responsibility |
|---|---|---|
| **Auth** | `AuthHTTPController` | REST endpoints for register/login/refresh/logout |
| | `JwtService` | JWT token generation, validation, and username extraction |
| | `AuthService` | Session management (active login tracking via refresh tokens) |
| | `SecurityConfig` | Spring Security filter chain — stateless JWT auth + CSRF disabled |
| | `JwtAuthFilter` | `OncePerRequestFilter` that intercepts requests, validates JWT, and populates `SecurityContext` |
| **WebSocket** | `GameWebSocketHandler` | Dispatches incoming JSON messages by type (`join`, `input`, `position`, `map_over`) |
| | `CustomHandshakeInterceptor` | (Currently disabled) Intended to validate tokens at WS connection time |
| **Game Engine** | `GameController` | Handles join, input, position, map generation, and disconnect events; orchestrates game state |
| | `GameLoop` | `@PostConstruct`-triggered fixed-rate executor that ticks every 50 ms (20 Hz) — calls `updateMatch()` per active match |
| **State Management** | `MatchStore` | Thread-safe map of `matchId → Match` objects |
| | `SessionRegistry` | Tracks WebSocket sessions, maps sessions to matches and players |
| | `PlayerRegistry` | Maps session IDs to `Player` entities (position, direction) |
| | `MatchMakingQueue` | FIFO queue — when 2 players are waiting, a match is created |
| | `PathGenerator` | Procedurally generates tile-path segments by seed (100 segments of alternating sides, variable length) |

### Data Flow (Single Match Lifecycle)

```
Client connects WS → Join game via /auth token
     │
     ▼
Client sends "join" message → GameController.handleJoin()
     │
     ▼
MatchMakingQueue.addPlayer(sessionId)
     │
     ▼
If 2nd player arrives → startMatch() → MatchStore.addMatch(match), SessionRegistry.registerMatch()
     │
     ▼
Both clients receive "welcome" payload (spawn position, path, opponent info)
     │
     ▼
Server game loop ticks every 50ms:
  - Speed increases by ACCELERATION each tick
  - Sends "update" message with both players' positions and current speed
     │
     ▼
Clients send "input" (movement intent) → GameController.handleInput()
  - Server updates local player direction & position
  - Broadcasts to opponent only ("player_move") — asymmetric push model
     │
     ▼
Client sends "position" → server validates y < -1f → if so, "game_over" broadcast → stopMatch()
```

---

## Feature Breakdown

### Authentication (REST)

| Endpoint | Method | Auth Required | Description |
|---|---|---|---|
| `/auth/register` | POST | No | Creates user with BCrypt-hashed password; returns JWT + refresh token |
| `/auth/login` | POST | No | Validates credentials; checks for active session (prevents concurrent logins); returns tokens |
| `/auth/refresh` | POST | No | Validates the stored refresh token against the session map; issues a new access token |
| `/auth/logout` | POST | Yes (`Bearer`) | Strips the user's active session from memory |

**Token model:** The access JWT is short-lived (configurable via `jwt.expiry-ms`). Refresh tokens are server-side — stored in a `ConcurrentHashMap<userId, refreshToken>` and validated at refresh time. This means only one active session per user at any given moment.

### WebSocket Game Protocol

| Message Type (client→server) | Description |
|---|---|
| `"join"` | Signals the player is ready to enter matchmaking |
| `"input"` | Movement intent: tile side, tile count, and grid position (posX, posZ) |
| `"position"` | Player's current world position; server checks for out-of-bounds (`y < -1f`) which triggers game over |
| `"map_over"` | Client signals it has rendered the full path; server generates additional segments |

| Message Type (server→client) | Description |
|---|---|
| `"welcome"` | Match assigned: player ID, opponent info, spawn position, starting path, initial speed |
| `"update"` | Periodic heartbeat from game loop: both players' positions + current game speed |
| `"player_move"` | Broadcasts the opponent's input data to this client (asymmetric push) |
| `"map_update"` | New `PathInfo` segments pushed when the map is extended |
| `"game_over"` | Triggered by out-of-bounds; includes `loserId` |
| `"opponent_disconnected"` | Sent on disconnect if a match was active |

---

## How to Run

### Prerequisites

- **Java 17+** (JDK)
- **Gradle** (wrapper included via `gradlew`)
- A **JWT secret key** and **expiry duration** — configure in the `.env` file or system properties

### Configuration

The server reads two values from environment variables / Spring property sources:

```properties
jwt.secret=your-super-secret-key-at-least-32-bytes-long
jwt.expiry-ms=86400000   # 24 hours in milliseconds (default example)
```

Create or edit the `.env` file at `Backend/.env`:

```bash
JWT_SECRET=your-random-alphanumeric-secret-key
JWT_EXPIRE_MS=86400000
```

### Running Locally

```bash
cd Backend
./gradlew bootRun
```

The server starts on **port 8080** (default Spring Boot) with Actuator endpoints at `/actuator/**`.

### Building a Production JAR

```bash
cd Backend
./gradlew clean build
java -jar build/libs/InfCube-0.0.1-SNAPSHOT.jar \
  --jwt.secret=your-secret-key \
  --jwt.expiry-ms=86400000
```

---

## API Reference

### POST `/auth/register`

**Request:**

```json
{ "username": "player1", "password": "secret" }
```

**Response (200 OK):**

```json
{
  "token": "<JWT access token>",
  "refreshToken": "<UUID refresh token>"
}
```

### POST `/auth/login`

**Request:** Same as register.

**Responses:** `200 OK` on success, `401 Unauthorized` for bad credentials, `409 Conflict` if user is already logged in elsewhere.

### POST `/auth/refresh`

**Request:**

```json
{ "token": "<refresh token>" }
```

### POST `/auth/logout`

**Header:** `Authorization: Bearer <access-token>`

---

## WebSocket Protocol Details

All messages are JSON-encoded strings sent over the single `/game` WebSocket endpoint. Authentication is **not** performed at the handshake layer (the `CustomHandshakeInterceptor` is currently disabled — access tokens are expected to be passed as query parameters or in a header by the Unity client).

### Message Schema

```json
// Client → Server
{
  "type": "<message_type>",
  "roomId": "<optional>",
  "playerId": "<optional>",
  "payload": { /* type-specific data */ }
}

// Server → Client
{
  "type": "<message_type>",
  "roomId": "testing",
  "timestamp": <unix-ms>,
  "payload": { /* type-specific data */ }
}
```

### Payload Types (Server → Client)

| Type | Payload Shape |
|---|---|
| `"welcome"` | `{ playerId, otherId, spawnPosition: {x,y,z}, path: [{side,count},...], startSpeed }` |
| `"update"` | `{ players: [{sessionId, playerId, position:{x,y,z}, direction:{x,y,z}}, ...], speed }` |
| `"player_move"` | Mirrors the client's `input` payload from the opponent |
| `"map_update"` | `{ extension: [{side,count},...] }` — new path segments |
| `"game_over"` | `{ loserId: "<session-id>" }` |

---

## Learnings & Architecture Decisions

### What Worked Well

**1. Separation of concerns across layers.** The auth flow (HTTP REST + JWT) is cleanly decoupled from the game loop (WebSocket + in-memory state). Each has its own entry point — `AuthHTTPController` for credential management and `GameWebSocketHandler` for real-time play — which keeps the codebase organized.

**2. Fixed-tick game loop with a single shared arena.** Instead of per-match loops, one `ScheduledExecutorService` runs at 20 Hz and iterates over all active matches in `MatchStore`. This is simple and works for a single-room setup. The speed accelerates uniformly across all matches (`ACCELERATION` added each tick), creating escalating tension.

**3. Asymmetric push model.** When Player A sends an input, the server updates Player A's local state *and* broadcasts only to the opponent. This avoids sending redundant data (a player doesn't need to receive their own movement). The `handlePosition` check for out-of-bounds is also server-authoritative — clients can't fake a fall.

**4. Match-making queue with atomic pairing.** `ConcurrentLinkedQueue` provides lock-free thread safety for adding players, and the synchronized `getMatchedPlayers()` method atomically polls two sessions to form a match. If only one player is waiting, they sit in the queue until another arrives — a natural FIFO matchmaking system.

**5. In-memory state via `ConcurrentHashMap`.** All registries (`SessionRegistry`, `PlayerRegistry`, `MatchStore`) use concurrent maps with no external dependencies. This eliminates setup overhead and makes prototyping fast. The trade-off (loss of data on restart) is acceptable for a prototype.

### Lessons Learned / Known Limitations

| Area | Issue | Potential Fix |
|---|---|---|
| **Scalability** | Everything lives in JVM memory — one server instance, no horizontal scaling. Redis-backed sessions and a message bus (e.g., RabbitMQ) would enable multi-instance support. |
| **Database** | `UserRepository` is an in-memory map with zero persistence. Switching to PostgreSQL/Postgres or MongoDB would add durability, password rotation, and user history. |
| **WebSocket Auth** | The `CustomHandshakeInterceptor` validates tokens but is currently disabled. Enabling it would secure the WebSocket endpoint at connection time rather than relying on client-side trust. |
| **Game Loop Rigidity** | A single fixed-rate loop for all rooms means every match shares the same tick interval and acceleration curve. Per-match loops or a configurable tick rate per room would allow different game modes. |
| **Path Generation** | The `PathGenerator` creates 100 segments of alternating sides (left/right) with random lengths. There's no collision detection, connectivity validation, or path-finding guarantee — the client is responsible for rendering and physics. |
| **Error Handling** | The WebSocket handler silently drops messages with null types but doesn't send error responses to clients for malformed payloads. A dedicated error message type would improve debugging. |
| **Refresh Token Rotation** | The current refresh endpoint returns a *new* access token but keeps the same refresh token (no rotation). This is vulnerable to replay attacks if a refresh token leaks — rotating it on each use would be safer. |

### Key Design Principles Observed

1. **Server-authoritative game state.** Positions, speed, and game-over conditions are all determined server-side; clients only send input intents.
2. **Stateless HTTP auth + stateful WebSocket games.** JWTs secure the REST endpoints while WebSocket sessions carry per-player state through registries — a practical split for real-time multiplayer.
3. **Minimal external dependencies.** Zero database, zero message queue, zero caching layer. The server is entirely self-contained behind Spring Boot's embedded Tomcat.
