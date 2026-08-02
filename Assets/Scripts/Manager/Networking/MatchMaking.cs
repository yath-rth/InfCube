using System.Collections;
using UnityEngine;

public class MatchMaking : MonoBehaviour
{
    public static MatchMaking instance;
    ConnectionManager connectionManager;

    [SerializeField] private GameObject matchmakingUI;
    [SerializeField] private GameObject foundUI;

    private bool _opponentFound = false;
    private bool _isSearching = false;   // NEW: guards against duplicate searches
    private Coroutine _matchmakingCoroutine; // NEW: track so Cancel can stop just this one

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;

        connectionManager = ConnectionManager.instance;
    }

    public void StartSearch()
    {
        if (_isSearching)
        {
            Debug.LogWarning("Already searching for a match — ignoring duplicate StartSearch call.");
            return;
        }

        _isSearching = true;
        _matchmakingCoroutine = StartCoroutine(MatchmakingLoop());
    }

    private IEnumerator MatchmakingLoop()
    {
        _opponentFound = false;

        if (!connectionManager.isConnected)
            yield return StartCoroutine(connectionManager.Connect());

        Debug.Log("Starting search for opponent");

        connectionManager.SendMessage(
            new ClientMessage(
                MessageTypes.JOIN,
                connectionManager.roomId,
                connectionManager.playerId
            )
        );

        float elapsed = 0f;
        float timeout = 60f;

        while (!_opponentFound && elapsed < timeout)
        {
            elapsed += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        if (!_opponentFound)
        {
            OnMatchmakingFailed();
            _isSearching = false;   // NEW: release the guard on timeout
            yield break;
        }

        Debug.Log("Opponent found");

        _isSearching = false;       // NEW: release the guard on success
        sceneManager.instance.Game();
    }

    public void OpponentFound()
    {
        _opponentFound = true;
    }

    private void OnMatchmakingFailed()
    {
        matchmakingUI.SetActive(false);
        Debug.Log("Matchmaking timed out");
    }

    public void CancelSearch()
    {
        if (_matchmakingCoroutine != null)
            StopCoroutine(_matchmakingCoroutine);

        _isSearching = false;       // NEW: release the guard on cancel
        connectionManager.Disconnect();
        matchmakingUI.SetActive(false);
    }
}