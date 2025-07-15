using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static bool isPlayConnected = false;
    player Player;
    sceneManager sceneManager;

    public bool isGameOver = false;

    [SerializeField] TMP_Text scoreText, endScreenScoreText, pauseScreenScoreText, highScoreText, coinsText;
    [SerializeField] GameObject scoreText_obj, endScreen_Obj, deathParticles, otherUI_obj, pauseScreen_obj, mainMenu_obj, shopMenu_obj;

    int gameState = 1;

    [SerializeField] RectTransform transition;
    Vector2 originalOffset;

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Start()
    {
        Application.targetFrameRate = -1;
        LoginToGooglePlay();

        Player = player.instance;
        sceneManager = GetComponent<sceneManager>();

        if (coinsText != null) coinsText.text = 0000.ToString("D2");
        if (scoreText != null) scoreText.text = 000.ToString("D3");
        if (endScreenScoreText != null) endScreenScoreText.text = PointsManager.instance.lastScore.ToString("D4");
        if (highScoreText != null) highScoreText.text = PointsManager.instance.highScore.ToString("D4");

        if (transition != null)
        {
            transition.gameObject.SetActive(true);

            float startY = 620f;
            float endY = -1620f;

            // Set initial bottom value
            Vector2 offset = transition.offsetMax;
            originalOffset = offset;
            offset.y = startY;
            transition.offsetMax = offset;
            transition.offsetMin = new Vector2(-722, -620);

            DOTween.To(() => transition.offsetMax.y,
                       y =>
                       {
                           Vector2 o = transition.offsetMax;
                           o.y = y;
                           transition.offsetMax = o;
                       },
                       endY,
                       1f); // Duration in seconds
        }

        PointsManager.instance.coinAddedEvent += updateScoreUI;
        PointsManager.instance.scoreAddedEvent += updateScore;
        player.instance.playerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        PointsManager.instance.coinAddedEvent -= updateScoreUI;
        PointsManager.instance.scoreAddedEvent -= updateScore;
        if (Player != null) Player.playerDied -= OnPlayerDied;
    }

    void LoginToGooglePlay()
    {
        //PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    // internal void ProcessAuthentication(SignInStatus status)
    // {
    //     if (status == SignInStatus.Success)
    //     {
    //         isPlayConnected = true;
    //     }
    //     else isPlayConnected = false;
    // }

    void Awake()
    {
        if (instance != null) Destroy(instance.gameObject);
        instance = this;
    }

    public sceneManager GetSceneManager()
    {
        return sceneManager;
    }

    public int GetPlayerScore()
    {
        int val = 0;

        // PlayGamesPlatform.Instance.LoadScores(
        //     GPGSIds.leaderboard_high_scores,
        //     LeaderboardStart.PlayerCentered,
        //     1, // Load only the player's score
        //     LeaderboardCollection.Public, // Specify the leaderboard collection
        //     LeaderboardTimeSpan.AllTime,
        //     (data) =>
        //     {
        //         if (data != null && data.Valid && data.Scores.Length > 0)
        //         {
        //             foreach (UnityEngine.SocialPlatforms.IScore score in data.Scores)
        //             {
        //                 // Match against local player
        //                 if (score.userID == Social.localUser.id)
        //                 {
        //                     Debug.Log($"Player Score: {score.value}");
        //                     Debug.Log($"Player Rank: {score.rank}");
        //                     val = (int)score.value;
        //                     break;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             Debug.LogError("Failed to get player score or score not valid. Using Player prefs now");
        //             val = PointsManager.instance.highScore;
        //         }
        //     }
        // );

        return val;
    }

    public void updateScoreUI(int coins)
    {
        if (coinsText != null) coinsText.text = coins.ToString("D2");
    }

    public void updateScore(int score)
    {
        scoreText_obj.transform.localScale = Vector3.one;
        if (scoreText != null) scoreText.text = score.ToString("D3");
    }

    public IEnumerator GameOver()
    {
        gameState = 0;
        isGameOver = true;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveData();
        }

        if (transition != null)
        {
            float startY = 1620f;
            float endY = -620f;

            // Set initial bottom value
            Vector2 offset = transition.offsetMin;
            offset.y = startY;
            transition.offsetMin = offset;
            transition.offsetMax = originalOffset;

            DOTween.To(() => transition.offsetMin.y,
                       y =>
                       {
                           Vector2 o = transition.offsetMin;
                           o.y = y;
                           transition.offsetMin = o;
                       },
                       endY,
                       1f); // Duration in seconds
        }

        // if (isPlayConnected == true)
        // {
        //     PlayGamesPlatform.Instance.ReportScore(highScore, GPGSIds.leaderboard_high_scores, (bool success) =>
        //     {
        //         if (success) Debug.Log("Score: " + highScore + " reported successfully.");
        //         else Debug.Log("Failed to report score.");
        //     });
        // }

        yield return new WaitForSeconds(1);

        restart();
    }

    private void OnPlayerDied()
    {
        StartCoroutine(GameOver());
    }

    public void gameStart()
    {
        if (mainMenu_obj != null) mainMenu_obj.SetActive(false);
        if (otherUI_obj != null) otherUI_obj.SetActive(true);
    }

    public void showLeaderboard()
    {
        if (sceneManager.GameState == 0)
        {
            // if (isPlayConnected == false) LoginToGooglePlay();
            // PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_high_scores);
        }
    }

    public void mainMenu()
    {
        if (gameState == 0)
        {
            sceneManager.MainMenu();
        }
    }

    public void shop()
    {
        if (sceneManager.GameState == 0)
        {
            sceneManager.instance.ShopView();
            if (shopMenu_obj != null) shopMenu_obj.SetActive(true);
            if (mainMenu_obj != null) mainMenu_obj.SetActive(false);
            sceneManager.GameState = 2;
        }
    }

    public void close()
    {
        if (sceneManager.GameState == 2)
        {
            sceneManager.instance.GameView();
            if (mainMenu_obj != null) mainMenu_obj.SetActive(true);
            if (shopMenu_obj != null) shopMenu_obj.SetActive(false);
            sceneManager.GameState = 0;
        }
        else if (sceneManager.GameState == 0)
        {
            Application.Quit();
        }
    }

    public void restart()
    {
        if (gameState == 0 && sceneManager.GameState == 1) sceneManager.MainMenu();
    }

    public void pause()
    {
        if (gameState == 1)
        {
            pauseScreenScoreText.text = PointsManager.instance.score.ToString("D5");
            otherUI_obj.SetActive(false);
            pauseScreen_obj.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void resume()
    {
        if (gameState == 0)
        {
            otherUI_obj.SetActive(true);
            pauseScreen_obj.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void pauseResume()
    {
        if (isGameOver == false)
        {
            if (gameState == 1)
            {
                pause();
                gameState = 0;
            }
            else
            {
                resume();
                gameState = 1;
            }
        }
    }
}
