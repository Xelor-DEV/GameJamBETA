using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button itemButton;

    [Header("Settings")]
    [SerializeField] private int startingQuantity = 2;
    [SerializeField] public int currentQuantity;
    [SerializeField] private UI_MovingInventory inventoryManager;

    void Start()
    {
        currentQuantity = startingQuantity;
        UpdateUI();
    }

    public void ModifyQuantity(int amount)
    {
        currentQuantity += amount;
        UpdateUI();

        if (inventoryManager != null)
        {
            inventoryManager.CheckReadyButtonState();
        }
    }

    private void UpdateUI()
    {
        quantityText.text = "x" + currentQuantity.ToString();
        itemButton.interactable = currentQuantity > 0;
    }
}