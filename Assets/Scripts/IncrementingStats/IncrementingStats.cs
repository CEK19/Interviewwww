using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class IncrementingStats : MonoBehaviour
{
    public TextMeshProUGUI statText;
    private string statName = "Kills";
    public Button incrementButton; // Assign in Inspector

    private void Start()
    {
        // Add a listener to call the function when the button is clicked
        incrementButton.onClick.AddListener(() => IncrementStat(1));
    }

    public void SetStat(int numKills)
    {
        statText.text = $"{statName}: {numKills}";
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
        try
        {
            var jsonString = JsonConvert.SerializeObject(result.FunctionResult);
            JObject jsonResult = JObject.Parse(jsonString);

            if (jsonResult.TryGetValue("newTotalValue", out JToken newTotalValueToken))
            {
                int newTotalValue = newTotalValueToken.Value<int>();
                SetStat(newTotalValue);
            }
            else
            {
                Debug.LogWarning("Key 'newTotalValue' not found in FunctionResult.");
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON Parsing Error: {ex.Message}");
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error updating stat: " + error.GenerateErrorReport());
    }

}
