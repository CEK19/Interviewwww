using System;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
public class API : MonoBehaviour
{
    public static API Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps this object persistent across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    public void LoginWithCustomID(string customID, Action<LoginResponse> onSuccess, Action<ErrorReport> onFailure)
    {
        if (string.IsNullOrEmpty(customID))
        {
            onFailure?.Invoke(new ErrorReport("Custom ID cannot be empty!"));
            return;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customID,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                var response = new LoginResponse { PlayFabID = result.PlayFabId };
                onSuccess?.Invoke(response);
            },
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }

    public void GetPlayerStats(Action<StatsResponse> onSuccess, Action<ErrorReport> onFailure)
    {

        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
            result =>
            {
                var response = new StatsResponse { Statistics = result.Statistics };
                onSuccess?.Invoke(response);
            },
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }

    public void CreateDefaultStats(Action<UpdatePlayerStatisticsResult> onSuccess, Action<ErrorReport> onFailure)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "Kills", Value = 0 },
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => onSuccess?.Invoke(result),
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }

    public void GetPlayerProfile(Action<UserProfileResponse> onSuccess, Action<ErrorReport> onFailure)
    {
        var request = new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true }
        };

        PlayFabClientAPI.GetPlayerProfile(request,
            result =>
            {
                var response = new UserProfileResponse { DisplayName = result.PlayerProfile.DisplayName ?? "None" };
                onSuccess?.Invoke(response);
            },
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }

    public void UpdateUserDisplayName(string displayName, Action<string> onSuccess, Action<ErrorReport> onFailure)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => onSuccess?.Invoke(displayName),
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }


    public void GetLeaderBoard(int maxResult, Action<KillsLeaderBoardResponse> onSuccess, Action<ErrorReport> onFailure)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTopScores",
            FunctionParameter = new { statName = "Kills", maxResultsCount = maxResult },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            try
            {
                var response = new KillsLeaderBoardResponse { TopKillPlayerInfos = new List<KillsLeaderBoardEntry>() };
                var jsonString = JsonConvert.SerializeObject(result.FunctionResult);
                JObject jsonResult = JObject.Parse(jsonString);
                JArray leaderboardArray = (JArray)jsonResult["leaderboard"];

                foreach (JObject player in leaderboardArray)
                {
                    string name = player["DisplayName"].ToString();
                    int rank = player["Rank"].Value<int>();
                    int score = player["Score"].Value<int>(); ;

                    response.TopKillPlayerInfos.Add(new KillsLeaderBoardEntry
                    {
                        DisplayName = name,
                        TopPosition = rank,
                        Kills = score
                    });
                }

                onSuccess?.Invoke(response);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON Parsing Error: {ex.Message}");
            }
        },
        error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }
}
