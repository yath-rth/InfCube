using UnityEngine;
using NativeWebSocket;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager instance;
    WebSocket ws;
    public string url, playerId;
    public string roomId;
    [SerializeField] GameObject playerPrefab;
    List<NetworkPlayer> remotePlayers = new List<NetworkPlayer>();
    bool hasPlayerId = false, hasJoined = false;

    [SerializeField] TMP_Text roomIdText;

    private void Awake()
    {
        Application.targetFrameRate = 165;
        if (instance != null) Destroy(this);
        instance = this;
    }

    public async void Disconnect()
    {
        if (ws.State == WebSocketState.Open)
        {
            await ws.Close();
            hasJoined = false;
            ws = null;
        }
    }

    public IEnumerator Connect()
    {
        if (ws == null)
        {
            bool connected = false;

            if (Auth.instance.isAuthenticated)
            {
                ws = new WebSocket("ws://localhost:8080/game", new Dictionary<string, string>
                {
                    { "Authorization", "Bearer " + Auth.instance.token }
                });

                ws.OnOpen += () =>
                {
                    connected = true;
                    Debug.Log("WebSocket connected");
                };

                ws.OnMessage += (raw) =>
                {
                    var message = System.Text.Encoding.UTF8.GetString(raw);
                    // Debug.Log("Message received: " + message);
                    var jsonMsg = JsonConvert.DeserializeObject<ServerMessage>(message);
                    onMessageRecieve(jsonMsg);
                };
                ws.OnError += (e) => Debug.LogError("WS Error: " + e);
                ws.OnClose += (e) => Debug.Log("WS Closed");

                ws.Connect();  // non-blocking, fires OnOpen when ready

                yield return new WaitUntil(() => connected);
            }
        }
    }

    public void SendMessage(ClientMessage msg)
    {
        if (ws == null)
        {
            Debug.LogError("SendMessage called but ws is null — not connected yet");
            return;
        }

        if (ws.State != WebSocketState.Open)
        {
            Debug.LogError($"SendMessage called but socket state is {ws.State}");
            return;
        }

        // Debug.Log("Sending message: " + msg);
        var json = JsonConvert.SerializeObject(msg, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        });
        ws.SendText(json);
    }

    void Update()
    {
        if (ws != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            ws.DispatchMessageQueue();
#endif
        }

        if (roomIdText != null)
            roomIdText.text = roomId;
    }

    public async void SendMoveInput(int side)
    {
        if (!hasPlayerId) return;
        if (ws.State == WebSocketState.Open)
        {
            ClientMessage msg = new ClientMessage(MessageTypes.INPUT, roomId, playerId, new InputPayload(side, pathSpawner.instance.getCurrentTile(), Player.instance.transform.position));
            SendMessage(msg);
        }
    }

    public async void SendMapOver()
    {
        if (!hasPlayerId) return;
        if (ws.State == WebSocketState.Open)
        {
            ClientMessage msg = new ClientMessage(MessageTypes.MAP_OVER, roomId, playerId, null);
            SendMessage(msg);
        }
    }

    public async void SendPosition(PositionPayload payload)
    {
        if (!hasPlayerId) return;
        if (ws.State == WebSocketState.Open)
        {
            ClientMessage msg = new ClientMessage(MessageTypes.POSITION, roomId, playerId, payload);
            SendMessage(msg);
        }
    }

    private async void OnApplicationQuit()
    {
        await ws.Close();
    }

    void HandleWelcome(WelcomePayload payload)
    {
        playerId = payload.playerId;
        hasPlayerId = true;
        Player.instance.SetSpeed(payload.startSpeed);
        GameObject go = null;

        if (!hasJoined && playerPrefab != null)
        {
            go = Instantiate(playerPrefab, Player.instance.transform.position + Vector3.down + Vector3.forward, Quaternion.identity);
            remotePlayers.Add(go.GetComponent<NetworkPlayer>());
            go.GetComponent<NetworkPlayer>().SetSpeed(payload.startSpeed);
            Debug.Log("Made player: " + go.name);
        }

        pathSpawner.instance.Initialize(payload.path, go.transform);
        MatchMaking.instance.OpponentFound();
    }

    void HandleUpdate(UpdatePayload payload)
    {
        Player.instance.SetSpeed(payload.speed);
        // Debug.Log("Update payload received: " + payload.ToString());

        for (int i = 0; i < payload.players.Count - 1; i++)
        {
            var p = payload.players[i];
            var pos = Vec3(p.posX, p.posY, p.posZ);

            if (i < remotePlayers.Count)
            {
                remotePlayers[i].SetSpeed(payload.speed);
            }
            else if (playerPrefab != null)
            {
                GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
                remotePlayers.Add(go.GetComponent<NetworkPlayer>());
                Debug.Log("Couldn't find player so created a new one");
                Debug.Log("Made player: " + go.name);
            }
        }

        while (remotePlayers.Count > payload.players.Count)
        {
            var extra = remotePlayers[remotePlayers.Count - 1];
            remotePlayers.RemoveAt(remotePlayers.Count - 1);
            Destroy(extra);
        }
    }

    public Queue<InputPayload> pendingTurns = new Queue<InputPayload>();

    void HandlePlayerMove(InputPayload move)
    {
        pendingTurns.Enqueue(move);
    }

    void HandleGameOver(GameOverPayload payload)
    {
        GameManager.instance.OnPlayerDied();
    }

    public void ProcessTurn(NetworkPlayer networkPlayer)
    {
        if (pendingTurns.Count == 0) return;

        InputPayload turn = pendingTurns.Dequeue();
        networkPlayer.HandleTurn(turn);
    }

    void onMessageRecieve(ServerMessage msg)
    {
        switch (msg.type)
        {
            case MessageTypes.WELCOME:
                var welcome = ((JObject)msg.payload).ToObject<WelcomePayload>();
                HandleWelcome(welcome);
                break;

            case MessageTypes.UPDATE:
                var update = ((JObject)msg.payload).ToObject<UpdatePayload>();
                HandleUpdate(update);
                break;

            case MessageTypes.PLAYER_MOVE:
                var move = ((JObject)msg.payload).ToObject<InputPayload>();
                HandlePlayerMove(move);
                break;

            case MessageTypes.NEW_MAP:
                var newMap = ((JObject)msg.payload).ToObject<NewMapPayload>();
                pathSpawner.instance.UpdatePath(newMap.extension);
                break;

            case MessageTypes.GAME_OVER:
                var gameOver = ((JObject)msg.payload).ToObject<GameOverPayload>();
                HandleGameOver(gameOver);
                break;
        }
    }

    static Vector3 Vec3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

}
