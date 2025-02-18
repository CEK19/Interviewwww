using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class AnonymousLogin : MonoBehaviour
{
    public TMP_Text playFabIDText;   // UI Text for showing PlayFab ID
    public TMP_InputField nameInput; // UI InputField for entering display name & Enter
    public TMP_Text displayNameText; // UI Text for showing Display Name
    public IncrementingStats incrementingStats; // Reference to IncrementingStats script

    private string customID; // Unique Custom ID for login

    void Start()
    {
        // Disable input field before login
        nameInput.interactable = false;
        nameInput.placeholder.GetComponent<TMP_Text>().text = "Enter Display Name & Press Enter";
        nameInput.onSubmit.AddListener(delegate { SetDisplayName(); });
        LoginWithCustomID();
    }

    void LoginWithCustomID()
    {
        customID = SystemInfo.deviceUniqueIdentifier; // Use device ID as custom ID
        var request = new LoginWithCustomIDRequest
        {
            CustomId = customID,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success! PlayFab ID: " + result.PlayFabId);
        playFabIDText.text = "PlayFab ID: " + result.PlayFabId;

        // Enable input field after login
        nameInput.interactable = true;

        // Get the player profile (including Display Name)
        GetPlayerProfile();
        incrementingStats.GetCurrentStat();
    }

    void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login Failed: " + error.GenerateErrorReport());
    }

    public void SetDisplayName()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogError("Display name cannot be empty!");
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameInput.text
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdated, OnDisplayNameUpdateFailed);
    }

    void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Display Name Updated: " + result.DisplayName);
        displayNameText.text = "Display Name: " + result.DisplayName;
    }

    void OnDisplayNameUpdateFailed(PlayFabError error)
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
                Debug.Log("Retrieved Display Name: " + result.PlayerProfile.DisplayName);
                displayNameText.text = "Display Name: " + (result.PlayerProfile.DisplayName ?? "None");
            }
        }, error =>
        {
            Debug.LogError("Failed to retrieve profile: " + error.GenerateErrorReport());
        });
    }
}
