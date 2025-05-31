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

    [Header("Item Prefab")]
    [SerializeField] private GameObject itemPrefab;

    void Start()
    {
        currentQuantity = startingQuantity;
        UpdateUI();
        itemButton.onClick.AddListener(OnItemButtonClicked);
    }

    public void OnItemButtonClicked()
    {
        if (currentQuantity > 0 && inventoryManager != null)
        {
            // Instanciar en el punto de spawn del inventario
            GameObject spawnedObject = Instantiate(
                itemPrefab,
                inventoryManager.GetSpawnPosition(),
                Quaternion.identity
            );

            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            DraggableItem draggable = spawnedObject.GetComponent<DraggableItem>();

            draggable.SetInventoryManager(inventoryManager);

            // Ocultar la ventana del inventario
            inventoryManager.HideWindow();

            ModifyQuantity(-1);
        }
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