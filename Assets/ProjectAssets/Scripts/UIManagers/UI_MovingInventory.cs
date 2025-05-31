using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_MovingInventory : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button readyButton;
    [SerializeField] private RectTransform inventoryWindow;
    [SerializeField] private RectTransform startPosition;
    [SerializeField] private RectTransform hidePosition;
    [SerializeField] private Image protector;
    [SerializeField] private InventoryItem[] inventoryItems;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float appearDelay = 0.5f;

    private bool isAnimating = false;


    void Start()
    {
        readyButton.interactable = false;
        StartCoroutine(AnimateWindow(hidePosition.position, startPosition.position, moveDuration, false, true));
    }

    public void CheckReadyButtonState()
    {
        bool allItemsDepleted = true;

        foreach (var item in inventoryItems)
        {
            if (item.currentQuantity > 0)
            {
                allItemsDepleted = false;
                break;
            }
        }

        readyButton.interactable = allItemsDepleted;
    }

    public void OnReadyButtonClicked()
    {
        if (!isAnimating && readyButton.interactable)
        {
            StartCoroutine(AnimateWindow(inventoryWindow.position, hidePosition.position, moveDuration, true, false));
        }
    }

    IEnumerator AnimateWindow(Vector3 startPos, Vector3 endPos, float duration, bool deactivateAfter, bool isAppearing)
    {
        if (isAnimating) yield break;

        isAnimating = true;
        protector.raycastTarget = true;

        if (isAppearing)
        {
            inventoryWindow.gameObject.SetActive(true);
            yield return new WaitForSeconds(appearDelay);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            inventoryWindow.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        inventoryWindow.position = endPos;
        protector.raycastTarget = false;
        isAnimating = false;

        if (deactivateAfter)
        {
            inventoryWindow.gameObject.SetActive(false);
        }
    }
}