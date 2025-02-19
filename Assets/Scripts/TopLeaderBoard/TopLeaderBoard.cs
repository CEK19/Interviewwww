using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

public class TopLeaderBoard : MonoBehaviour
{
    public int maxResults = 5; // Number of top players to fetch
    public TextMeshProUGUI leaderboardText; // UI Text for showing leaderboard

    public void FetchLeaderboard()
    {
        API.Instance.GetLeaderBoard(maxResults,
        (result) =>
        {
            var topPlayers = result.TopKillPlayerInfos;
            string rankInfo = "";

            foreach (var player in topPlayers)
            {
                rankInfo += $"{player.TopPosition}. {player.DisplayName} - {player.Kills}\n";
            }

            leaderboardText.text = rankInfo;
        },
        (error) =>
        {
            Debug.LogError($"PlayFab API Error: {error.Message}");
        });
    }
}
