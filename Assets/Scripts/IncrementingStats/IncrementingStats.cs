using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;

public class IncrementingStats : MonoBehaviour
{
    public TextMeshProUGUI statText;
    private string statName = "Kills";
    public Button incrementButton; // Assign in Inspector

    private void Start()
    {
        // Add a listener to call the function when the button is clicked
        incrementButton.onClick.AddListener(() => IncrementStat(1));

        // Load initial stat value
        GetCurrentStat();
    }

    public void IncrementStat(int incrementBy = 1)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "IncrementPlayerStat",
            FunctionParameter = new { statName, incrementBy },
            GeneratePlayStreamEvent = true
        };

        Debug.Log($"Updating {statName} by {incrementBy}...");

        PlayFabClientAPI.ExecuteCloudScript(request, OnSuccess, OnError);
    }

    private void OnSuccess(ExecuteCloudScriptResult result)
    {
        if (result.FunctionResult is Dictionary<string, object> dict && dict.TryGetValue("newTotalValue", out object newTotalValueObj))
        {
            int newTotalValue = int.Parse(newTotalValueObj.ToString());
            statText.text = $"{statName}: {newTotalValue}"; // Cập nhật UI với tổng giá trị mới
            Debug.Log($"Updated {statName}: {newTotalValue}");
        }
        else
        {
            Debug.LogWarning("CloudScript response does not contain expected data.");
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error updating stat: " + error.GenerateErrorReport());
    }

    private void GetCurrentStat()
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, OnGetStatsSuccess, OnError);
    }

    private void OnGetStatsSuccess(GetPlayerStatisticsResult result)
    {
        var stat = result.Statistics.Find(s => s.StatisticName == statName);
        int currentStatValue = stat != null ? stat.Value : 0;
        statText.text = $"{statName}: {currentStatValue}";
        Debug.Log($"Initial {statName}: {currentStatValue}");
    }
}
