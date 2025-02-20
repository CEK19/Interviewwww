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

    public void GrantGold(int amount, Action<GrantCurrencyResponse> onSuccess, Action<ErrorReport> onFailure)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GrantGold",
            FunctionParameter = new { amount = amount },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            var jsonString = JsonConvert.SerializeObject(result.FunctionResult);
            JObject jsonResult = JObject.Parse(jsonString);
            var response = new GrantCurrencyResponse { Balance = jsonResult["newBalance"].Value<int>() };
            onSuccess?.Invoke(response);
        },
        error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }

    public void PurchaseItem(string itemId, Action<PurchaseItemResponse> onSuccess, Action<ErrorReport> OnFailure)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "PurchaseItem",
            FunctionParameter = new { itemId = itemId },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            var jsonString = JsonConvert.SerializeObject(result.FunctionResult);
            JObject jsonResult = JObject.Parse(jsonString);
            var response = new PurchaseItemResponse
            {
                Balance = jsonResult["newBalance"].Value<int>(),
                ItemId = jsonResult["itemId"].Value<string>()
            };
            onSuccess?.Invoke(response);
        },
        error => OnFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }

    public void GetItemPrice(string itemId, Action<GetItemPriceResponse> onSuccess, Action<ErrorReport> onFailure)
    {
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(),
        result =>
        {
            foreach (var item in result.Catalog)
            {
                if (item.ItemId == itemId)
                {
                    var response = new GetItemPriceResponse { ItemId = item.ItemId, Price = (int)item.VirtualCurrencyPrices["GC"] };
                    onSuccess?.Invoke(response);
                    return;
                }
            }
        },
        error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport()))
        );
    }

    public void GetInventory(Action<InventoryResponse> onSuccess, Action<ErrorReport> onFailure)
    {

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result =>
            {

                var response = new InventoryResponse { Inventory = new List<Item>() };

                foreach (var item in result.Inventory)
                {
                    response.Inventory.Add(new Item
                    {
                        ItemId = item.ItemId,
                        DisplayName = item.DisplayName,
                    });
                }
                onSuccess?.Invoke(response);
            },
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));

    }

    public void GetUserBalance(Action<GetUserBalanceResponse> onSuccess, Action<ErrorReport> onFailure)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result =>
            {
                var response = new GetUserBalanceResponse { Balance = result.VirtualCurrency["GC"] };
                onSuccess?.Invoke(response);
            },
            error => onFailure?.Invoke(new ErrorReport(error.GenerateErrorReport())));
    }
}
