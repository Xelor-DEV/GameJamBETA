using UnityEngine;
using NaughtyAttributes;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Events;

[System.Serializable]
public class UI_Window
{
    public RectTransform windowTransform;
    public UnityEvent onWindowShown; 
    public UnityEvent onWindowHidden; 
}

public class UI_MainMenu : MonoBehaviour
{
    [Header("Buttons Container")]
    [Required][SerializeField] private GameObject buttonContainer;

    [Header("Buttons")]
    [SerializeField] private UI_MenuButton[] menuButtons;

    [Header("Selector")]
    [Required][SerializeField] private RectTransform selector;
    [SerializeField] private float selectorSpeed = 15f;

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [Required][SerializeField] private Image protector;

    [Header("Windows Settings")]
    [SerializeField] private UI_Window[] windows; 
    [Required][SerializeField] private RectTransform hidePosition;
    [Required][SerializeField] private RectTransform showPosition;

    [Header("Sound")]
    [SerializeField] private int changeSelectionSoundId = 0;

    private int currentSelectedIndex = 0;
    private float selectorXPosition;

    private int currentActiveWindowIndex = -1;
    private bool isTweening = false;
    private bool isWindowActive = false; 

    private void Start()
    {
        InitializeSelectorPosition();
        InitializeButtons();
        InitializeWindows();
        InitializeProtector();
        MoveSelector(0);
    }

    private void InitializeProtector()
    {
        protector.gameObject.SetActive(true);
        protector.raycastTarget = false;
    }

    private void InitializeWindows()
    {
        foreach (UI_Window window in windows)
        {
            window.windowTransform.position = hidePosition.position;
        }
    }

    private void InitializeSelectorPosition()
    {
        selectorXPosition = selector.anchoredPosition.x;
    }

    [Button("Auto Assign Buttons")]
    private void AutoAssignButtons()
    {
        if (buttonContainer == null)
        {
            Debug.LogWarning("Button container is not assigned!");
            return;
        }

        menuButtons = buttonContainer.GetComponentsInChildren<UI_MenuButton>();
        Debug.Log($"Assigned {menuButtons.Length} buttons automatically");
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i;

            EventTrigger trigger = menuButtons[i].GetComponent<EventTrigger>();
            if (trigger == null) trigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
            pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener((data) => { OnButtonPointerEnter(index); });

            trigger.triggers.Add(pointerEnterEntry);
        }
    }

    public void OnButtonPointerEnter(int index)
    {
        if (isWindowActive) return;

        MoveSelector(index);
        currentSelectedIndex = index;
    }


    public void MoveSelector(int index)
    {
        if (isWindowActive) return;

        if (selector == null || index < 0 || index >= menuButtons.Length) return;
        
        if (index == currentSelectedIndex) return;

        AudioManager.Instance.PlaySfx(changeSelectionSoundId);

        if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
        {
            menuButtons[currentSelectedIndex].SetSelectedByKeyboard(false);
        }

        menuButtons[index].SetSelectedByKeyboard(true);
        currentSelectedIndex = index;

        RectTransform buttonRect = menuButtons[index].RectTransform;
        Vector2 buttonCenter = GetButtonCenterInScreenSpace(buttonRect);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            buttonCenter,
            canvas.worldCamera,
            out Vector2 canvasPosition);

        Vector2 targetPosition = new Vector2(
            selectorXPosition,
            canvasPosition.y
        );

        StopAllCoroutines();
        StartCoroutine(SmoothMoveSelector(targetPosition));
    }

    private Vector2 GetButtonCenterInScreenSpace(RectTransform buttonRect)
    {
        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);

        Vector2 center = Vector2.zero;
        foreach (Vector3 corner in corners)
        {
            center += (Vector2)corner;
        }
        center /= 4;

        return center;
    }

    private IEnumerator SmoothMoveSelector(Vector2 targetPosition)
    {
        Vector2 startPos = selector.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * selectorSpeed;
            selector.anchoredPosition = Vector2.Lerp(startPos, targetPosition, elapsedTime);
            yield return null;
        }

        selector.anchoredPosition = targetPosition;
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (isWindowActive) return;
        if (context.performed && !isTweening)
        {
            ChangeSelection(-1);
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (isWindowActive) return;
        if (context.performed && !isTweening)
        {
            ChangeSelection(1);
        }
    }

    public void Accept(InputAction.CallbackContext context)
    {
        if (isWindowActive) return;
        if (context.performed && !isTweening)
        {
            ActivateSelectedButton();
        }
    }

    private void ChangeSelection(int direction)
    {
        int newIndex = currentSelectedIndex + direction;

        if (newIndex < 0) newIndex = menuButtons.Length - 1;
        else if (newIndex >= menuButtons.Length) newIndex = 0;

        MoveSelector(newIndex);
    }

    private void ActivateSelectedButton()
    {
        if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
        {
            menuButtons[currentSelectedIndex].GetComponent<Button>().onClick.Invoke();
        }
    }

    // =============== VENTANAS CON EVENTOS ESPECÍFICOS ===============

    public void ActivateWindow(int index)
    {
        if (isTweening) return;
        if (index < 0 || index >= windows.Length) return;

        if (currentActiveWindowIndex != -1)
        {
            DeactivateWindow(currentActiveWindowIndex);
        }

        StartCoroutine(ShowWindowRoutine(index));
    }

    public void DeactivateWindow(int index)
    {
        if (isTweening) return;
        if (index < 0 || index >= windows.Length) return;

        StartCoroutine(HideWindowRoutine(index));
    }

    public void HideActiveWindow()
    {
        if (currentActiveWindowIndex != -1)
        {
            DeactivateWindow(currentActiveWindowIndex);
        }
    }

    private IEnumerator ShowWindowRoutine(int index)
    {
        isTweening = true;
        isWindowActive = true; // Marcar que hay ventana activa

        // Activar protector inmediatamente
        protector.raycastTarget = true;

        UI_Window uiWindow = windows[index];
        RectTransform window = uiWindow.windowTransform;

        window.DOMove(showPosition.position, 0.5f)
              .SetEase(Ease.OutBack)
              .OnStart(() => window.gameObject.SetActive(true));

        yield return new WaitForSeconds(0.5f);


        currentActiveWindowIndex = index;
        isTweening = false;

        // Disparar eventos específicos de la ventana
        uiWindow.onWindowShown?.Invoke();
    }

    private IEnumerator HideWindowRoutine(int index)
    {
        isTweening = true;

        UI_Window uiWindow = windows[index];
        RectTransform window = uiWindow.windowTransform;

        window.DOMove(hidePosition.position, 0.5f)
              .SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.5f);

        window.gameObject.SetActive(false);

        if (currentActiveWindowIndex == index)
            currentActiveWindowIndex = -1;

        isTweening = false;

        // Desactivar protector solo después de que la ventana haya salido
        protector.raycastTarget = false;
        isWindowActive = false; // Marcar que no hay ventana activa

        // Disparar eventos específicos de la ventana
        uiWindow.onWindowHidden?.Invoke();
    }
}