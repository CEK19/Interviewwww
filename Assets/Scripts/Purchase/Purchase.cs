using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Purchase : MonoBehaviour
{
    [SerializeField] private TMP_Text InventoryText;
    [SerializeField] private TMP_Text BalanceText;
    [SerializeField] private Button GrantGoldBtn;
    [SerializeField] private Button BuyItemBtn;

    private void Start()
    {
        GrantGoldBtn.onClick.AddListener(GrantGoldToUser);
        BuyItemBtn.onClick.AddListener(BuyItem);
    }

    private void GrantGoldToUser()
    {
        API.Instance.GrantGold(
            100,
            (result) =>
            {
                SetUIBalance(result.Balance);
            },
            (error) =>
            {
                Debug.LogError("Error: " + error.Message);
            }
            );
    }

    private void BuyItem()
    {
        API.Instance.PurchaseItem(
            "Sword",
            (result) =>
            {
                BalanceText.text = "Balance: " + result.Balance;
                GetInventory();
            },
            (error) =>
            {
                Debug.LogError("Error: " + error.Message);
            }
            );
    }

    public void GetInventory()
    {
        API.Instance.GetInventory(
            (result) =>
            {
                SetUIInventory(result.Inventory);
            },
            (error) =>
            {
                Debug.LogError("Error: " + error.Message);
            }
            );
    }

    public void GetUserBalanace()
    {
        API.Instance.GetUserBalance(
            (result) =>
            {
                SetUIBalance(result.Balance);
            },
            (error) =>
            {
                Debug.LogError("Error: " + error.Message);
            }
            );
    }

    private void SetUIInventory(List<Item> inventory)
    {
        string inventoryText = "Inventory: ";
        foreach (var item in inventory)
        {
            inventoryText += item.DisplayName + ", ";
        }
        InventoryText.text = inventoryText;
    }

    private void SetUIBalance(int balance)
    {
        BalanceText.text = "Balance: " + balance;
    }
}
