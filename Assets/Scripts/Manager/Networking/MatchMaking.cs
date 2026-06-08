using System.Collections;
using UnityEngine;

public class MatchMaking : MonoBehaviour
{
    public static MatchMaking instance;
    ConnectionManager connectionManager;

    [SerializeField] private GameObject matchmakingUI;  // "Searching..." screen
    [SerializeField] private GameObject foundUI;        // "Opponent found!" screen

    private bool _opponentFound = false;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;

        connectionManager = ConnectionManager.instance;
    }

    // ── called by your "Play" button ──
    public void StartSearch()
    {
        StartCoroutine(MatchmakingLoop());
    }

    private IEnumerator MatchmakingLoop()
    {
        _opponentFound = false;

        // show searching UI
        // matchmakingUI.SetActive(true);
        // foundUI.SetActive(false);

        // connect and send JOIN
        yield return StartCoroutine(connectionManager.Connect());
        Debug.Log("Starting search for opponent");

        connectionManager.SendMessage(
            new ClientMessage(
                MessageTypes.JOIN,
                connectionManager.roomId,
                connectionManager.playerId
            )
        );

        // loop every 0.5s until opponent found or timeout
        float elapsed = 0f;
        float timeout = 60f;  // give up after 60 seconds

        while (!_opponentFound && elapsed < timeout)
        {
            elapsed += 0.5f;
            yield return new WaitForSeconds(0.5f);
            // NetworkManager will call OpponentFound() when PLAYER_JOINED arrives
        }

        if (!_opponentFound)
        {
            // timed out — show error
            OnMatchmakingFailed();
            yield break;  // ← exits the coroutine
        }

        // opponent found — brief delay so player can read the UI
        // matchmakingUI.SetActive(false);
        // foundUI.SetActive(true);
        Debug.Log("Opponent found");
        // yield return new WaitForSeconds(1.5f);

        // load game scene
        sceneManager.instance.Game();
    }

    // ── called by NetworkManager when PLAYER_JOINED arrives ──
    public void OpponentFound()
    {
        _opponentFound = true;
    }

    private void OnMatchmakingFailed()
    {
        matchmakingUI.SetActive(false);
        Debug.Log("Matchmaking timed out");
        // show retry button etc
    }

    // ── called by your "Cancel" button ──
    public void CancelSearch()
    {
        StopAllCoroutines();
        connectionManager.Disconnect();
        matchmakingUI.SetActive(false);
    }
}