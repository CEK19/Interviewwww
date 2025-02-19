using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class AnonymousLogin : MonoBehaviour
{
    [SerializeField] private TMP_InputField playFabIDTextInput;   // UI Text for showing PlayFab ID
    [SerializeField] private TMP_InputField nameInput; // UI InputField for entering display name & Enter
    [SerializeField] private TMP_Text displayNameText; // UI Text for showing Display Name
    [SerializeField] private Button loginButton; // UI Button for login
    [SerializeField] private IncrementingStats incrementingStats; // Reference to IncrementingStats script
    [SerializeField] private TopLeaderBoard topLeaderBoard; // Reference to TopLeaderBoard script

    private string customID; // Unique Custom ID for login

    private void Start()
    {
        Initialize();
        LoginWithCustomID();
    }


    private void Initialize()
    {
        // Enable login button & add listener
        loginButton.enabled = true;
        loginButton.onClick.AddListener(LoginWithCustomID);

        // Disable input field before login because we need PlayFab ID to update display name
        nameInput.interactable = false;
        nameInput.placeholder.GetComponent<TMP_Text>().text = "Enter Display Name & Press Enter";
        nameInput.onSubmit.AddListener(delegate { SetDisplayName(); });
    }

    private void LoginWithCustomID()
    {
        customID = playFabIDTextInput.text;
        Debug.Log("Logging in with Custom ID: " + customID);
        var request = new LoginWithCustomIDRequest
        {
            CustomId = customID,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {

        nameInput.interactable = true;
        playFabIDTextInput.interactable = false;
        loginButton.enabled = false;

        // Get the player profile (including Display Name)
        GetPlayerProfile();
        incrementingStats.GetCurrentStat();
        topLeaderBoard.FetchLeaderboard();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login Failed: " + error.GenerateErrorReport());
    }

    public void SetDisplayName()
    {

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameInput.text
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdated, OnDisplayNameUpdateFailed);
    }

    private void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        displayNameText.text = "Display Name: " + result.DisplayName;
    }

    private void OnDisplayNameUpdateFailed(PlayFabError error)
    {
        Debug.LogError("Failed to update display name: " + error.GenerateErrorReport());
    }

    void GetPlayerProfile()
    {
        var request = new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true }
        };

        PlayFabClientAPI.GetPlayerProfile(request, result =>
        {
            if (result.PlayerProfile != null)
            {
                displayNameText.text = "Display Name: " + (result.PlayerProfile.DisplayName ?? "None");
            }
        }, error =>
        {
            Debug.LogError("Failed to retrieve profile: " + error.GenerateErrorReport());
        });
    }
}
