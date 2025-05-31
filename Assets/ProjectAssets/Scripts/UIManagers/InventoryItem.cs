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
    [SerializeField] private GameObject itemPrefab;  // Nueva variable para el prefab a spawnear

    void Start()
    {
        currentQuantity = startingQuantity;
        UpdateUI();
    }

    public void OnItemButtonClicked()
    {
        if (currentQuantity > 0)
        {
            // Instanciar el prefab en la posición del ratón
            Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)
            );
            GameObject spawnedObject = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

            // Configurar Rigidbody
            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // Añadir componente para manejar el arrastre
            DraggableItem draggable = spawnedObject.GetComponent<DraggableItem>();
            draggable.SetRigidbody(rb);

            // Reducir cantidad
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