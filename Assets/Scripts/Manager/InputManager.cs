using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1)]
public class InputManager : MonoBehaviour
{
    Controls controls;
    GameManager gameManager;
    ConnectionManager connectionManager;
    MatchMaking matchMaking;
    Player player;

    void Awake()
    {
        gameManager = GameManager.instance;
        player = Player.instance;
        connectionManager = ConnectionManager.instance;
        matchMaking = MatchMaking.instance;

        controls = new Controls();

        controls.movement.turn_singlePlayer.performed += ctx => player.turn();
        controls.movement.escape.performed += ctx => gameManager.close();
        controls.movement.mainMenu.performed += ctx => gameManager.mainMenu();
        controls.movement.space.performed += ctx => gameManager.restart();
        controls.movement.space.performed += ctx =>
        {
            if (sceneManager.GameState != 1) matchMaking.StartSearch();
        };
        controls.movement.leaderboard.performed += ctx => gameManager.showLeaderboard();
        controls.movement.shop.performed += ctx => gameManager.shop();

        // StartCoroutine(startGameTest());
    }

    IEnumerator startGameTest()
    {
        yield return new WaitForSeconds(4f);
        if (sceneManager.GameState != 1) matchMaking.StartSearch();
    }

    void OnEnable()
    {
        controls.Enable();
    }
    void OnDisable()
    {
        controls.Disable();
    }
}