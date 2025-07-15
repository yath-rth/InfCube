using UnityEngine;
using System;

public class PointsManager : MonoBehaviour, ISaveFuncs
{
    public static PointsManager instance;
    [SerializeField] string saveName => "PointsManager";

    public Action<int> coinAddedEvent, scoreAddedEvent;

    int coins = 0;
    public int score { get; private set; }
    public int lastScore { get; private set; }
    public int highScore { get; private set; }
    public int Allcoins { get; private set; }

    public string id => saveName;

    void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;
        score = 0;
    }

    public void addCoin()
    {
        coins++;
        Allcoins++;

        coinAddedEvent?.Invoke(coins);
    }

    public void removeCoins(int _coins)
    {
        Allcoins -= _coins;
    }

    public void addScore(int value)
    {
        score += value;
        scoreAddedEvent?.Invoke(score);

        if (score > highScore) highScore = score;
    }

    public void LoadData(object data)
    {
        if (data is PointsData d)
        {
            highScore = d.highScore;
            lastScore = d.score;
            Allcoins = d.AllCoins;
        }
    }

    public object SaveData()
    {
        return new PointsData
        {
            AllCoins = Allcoins,
            score = score,
            highScore = highScore
        };
    }
}


class PointsData
{
    public int AllCoins;
    public int score;
    public int highScore;
}