using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;

public class TopLeaderBoard : MonoBehaviour
{
    public int maxResults = 5; // Number of top players to fetch
    public TextMeshProUGUI leaderboardText; // UI Text for showing leaderboard

    public void FetchLeaderboard()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTopScores",
            FunctionParameter = new { statName = "Kills", maxResultsCount = maxResults },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, OnCloudScriptSuccess, OnError);
    }

    private void OnCloudScriptSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Leaderboard fetched successfully!");

        try
        {
            var jsonString = JsonConvert.SerializeObject(result.FunctionResult);
            JObject jsonResult = JObject.Parse(jsonString);
            JArray leaderboardArray = (JArray)jsonResult["leaderboard"];

            string rankInfo = "";
            foreach (JObject player in leaderboardArray)
            {
                string name = player["DisplayName"].ToString();
                int rank = player["Rank"].Value<int>();
                int score = player["Score"].Value<int>();

                rankInfo += $"{rank}. {name} - {score}\n";
            }

            leaderboardText.text = rankInfo;
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON Parsing Error: {ex.Message}");
        }

    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab API Error: {error.GenerateErrorReport()}");
    }
}
