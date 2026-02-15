using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System;
using System.Threading.Tasks;
using TMPro;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager instance;
    [SerializeField] string leaderboardId;
    [SerializeField] private int playersPerPage = 25;
    [SerializeField] private LeaderboardItem leaderboardItemPrefab = null;
    [SerializeField] private RectTransform leaderboardItemContainer = null;
    [SerializeField] public TMP_Text pageText = null;
    private int currentPage = 1;
    private int totalPages = 0;
    private List<LeaderboardItem> items = new List<LeaderboardItem>();
    bool servicesReady = false;

    private async void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        servicesReady = true;
    }

    private async void OnEnable()
    {
        await WaitForServices();

        LoadPlayers(1);
        Debug.Log("opened leaderboard");
    }

    public async void AddPlayerScore(int score)
    {
        try
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            LoadPlayers(1);
            currentPage = 1;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public async void LoadPlayers(int page)
    {
        try
        {
            GetScoresOptions options = new GetScoresOptions
            {
                Offset = (page - 1) * playersPerPage,
                Limit = playersPerPage
            };
            var scores = await LeaderboardsService.Instance.GetScoresAsync("InfCube_Leaderboard", options);
            ClearPlayersList();
            for (int i = 0; i < scores.Results.Count; i++)
            {
                LeaderboardItem item = Instantiate(leaderboardItemPrefab, leaderboardItemContainer);
                item.Initialize(scores.Results[i]);
                items.Add(item);
            }
            totalPages = Mathf.CeilToInt((float)scores.Total / (float)scores.Limit);
            currentPage = page;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        pageText.text = currentPage.ToString() + "/" + totalPages.ToString();
    }

    private void ClearPlayersList()
    {
        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Destroy(items[i].gameObject);
            }

            items.Clear();
        }
    }

    private async Task WaitForServices()
    {
        while (!servicesReady)
        {
            await Task.Yield();
        }
    }

}
