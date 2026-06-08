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
    List<GameObject> remotePlayers = new List<GameObject>();
    bool hasPlayerId = false, hasJoined = false;

    [SerializeField] TMP_Text roomIdText;

    private void Awake()
    {
        Application.targetFrameRate = 165;
        if (instance != null) Destroy(this);
        instance = this;
    }

    public void UpdateRoomId(string _roomId)
    {
        roomId = _roomId;
    }

    public async void JoinRoom()
    {
        if (roomId.Length != 6) return;
        if (ws.State == WebSocketState.Open)
        {
            var msg = new ClientMessage(MessageTypes.JOIN, roomId, playerId, new PlayerJoinPayload());
            SendMessage(msg);
            hasJoined = true;
        }
    }

    public async void CreateRoom()
    {
        if (ws.State == WebSocketState.Open)
        {
            var msg = new ClientMessage(MessageTypes.JOIN, "CreateRoom", playerId, new PlayerJoinPayload());
            SendMessage(msg);
            hasJoined = true;
        }
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

            ws = new WebSocket("ws://localhost:8080/game");

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

        Debug.Log("Sending message: " + msg);
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
        if (!hasJoined) return;
        if (ws.State == WebSocketState.Open)
        {
            var msg = new ClientMessage(MessageTypes.INPUT, roomId, playerId, new InputPayload(side));
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

        if (!hasJoined && playerPrefab != null)
        {
            var go = Instantiate(playerPrefab, Vec3(payload.spawnX, payload.spawnY, payload.spawnZ), Quaternion.identity);
            remotePlayers.Add(go);
        }

        pathSpawner.instance.Initialize(payload.seed);
        MatchMaking.instance.OpponentFound();
    }

    void HandleUpdate(UpdatePayload payload)
    {
        for (int i = 0; i < payload.players.Count; i++)
        {
            var p = payload.players[i];
            var pos = Vec3(p.posX, p.posY, p.posZ);

            if (i < remotePlayers.Count)
            {
                remotePlayers[i].GetComponent<NetworkPlayer>().movePlayer(pos);
            }
            else if (playerPrefab != null)
            {
                GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
                remotePlayers.Add(go);
                Debug.Log("Couldn't find player so created a new one");
            }
        }

        while (remotePlayers.Count > payload.players.Count)
        {
            var extra = remotePlayers[remotePlayers.Count - 1];
            remotePlayers.RemoveAt(remotePlayers.Count - 1);
            Destroy(extra);
        }
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
        }
    }

    static Vector3 Vec3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

}
