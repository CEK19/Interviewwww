using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class AnonymousLogin : MonoBehaviour
{
    [SerializeField] private TMP_InputField customIDTextInput;   // UI Text for showing PlayFab ID
    [SerializeField] private TMP_Text playFabIDText; // UI Text for showing PlayFab ID
    [SerializeField] private TMP_InputField nameInput; // UI InputField for entering display name & Enter
    [SerializeField] private TMP_Text displayNameText; // UI Text for showing Display Name
    [SerializeField] private Button loginButton; // UI Button for login
    [SerializeField] private TopLeaderBoard topLeaderBoard; // Reference to TopLeaderBoard script
    [SerializeField] private IncrementingStats incrementingStats; // Reference to IncrementingStats script
    [SerializeField] private Purchase purchase; // Reference to Purchase script


    private string customID; // Unique Custom ID for login

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Enable login button & add listener
        loginButton.gameObject.SetActive(true);
        loginButton.onClick.AddListener(Login);

        // Disable input field before login because we need PlayFab ID to update display name
        nameInput.interactable = false;
        nameInput.placeholder.GetComponent<TMP_Text>().text = "Enter Display Name & Press Enter";
        nameInput.onSubmit.AddListener(delegate { UpdateName(); });
    }

    // Login with Custom ID
    private void Login()
    {
        customID = customIDTextInput.text;
        API.Instance.LoginWithCustomID(customID, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResponse result)
    {

        nameInput.interactable = true;
        customIDTextInput.interactable = false;
        loginButton.gameObject.SetActive(false);
        playFabIDText.text = "PlayFab ID: " + result.PlayFabID;


        InitUserStats();
        GetPlayerProfile();

        topLeaderBoard.FetchLeaderboard();
        purchase.GetUserBalanace();
        purchase.GetInventory();
    }

    private void OnLoginFailure(ErrorReport error)
    {
        Debug.LogError("Failed to login: " + error.Message);
    }

    // Initialize user stats
    private void InitUserStats()
    {
        API.Instance.GetPlayerStats(
            (response) =>
            {
                bool isStatsFound = response.Statistics.Count > 0;

                if (!isStatsFound)
                {
                    CreateDefaultStats();
                }
                else
                {
                    foreach (var stat in response.Statistics)
                    {
                        if (stat.StatisticName == "Kills")
                        {
                            incrementingStats.SetStat(stat.Value);
                        }
                    }
                }
            },
            (error) => Debug.LogError("Failed to get player stats: " + error)
        );
    }


    public void UpdateName()
    {

        API.Instance.UpdateUserDisplayName(nameInput.text,

        (name) =>
        {
            SetUIDisplayName(name);
        },
        (error) => Debug.LogError("Failed to update display name: " + error.Message)
        );
    }

    void GetPlayerProfile()
    {
        API.Instance.GetPlayerProfile(
            (response) =>
            {
                SetUIDisplayName(response.DisplayName);
            },
            (error) => Debug.LogError("Failed to get player profile: " + error.Message)
        );
    }


    private void CreateDefaultStats()
    {
        API.Instance.CreateDefaultStats(
            (result) =>
            {
                Debug.Log("Default stats created successfully.");
            },
            (error) => Debug.LogError("Failed to create default stats: " + error.Message)
        );
    }

    private void SetUIDisplayName(string name)
    {
        displayNameText.text = "Display Name: " + name;
    }
}
