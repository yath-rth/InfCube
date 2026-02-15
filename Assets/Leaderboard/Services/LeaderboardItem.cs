using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] TMP_Text nameTxt;
    [SerializeField] TMP_Text rankTxt;
    [SerializeField] TMP_Text scoreTxt;

    public void Initialize(Unity.Services.Leaderboards.Models.LeaderboardEntry score)
    {
        nameTxt.text = score.PlayerName;
        rankTxt.text = score.Rank.ToString();
        scoreTxt.text = score.Score.ToString();
    }
}
