using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MenuButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private EventTrigger eventTrigger;

    [Header("Events")]
    public UnityEvent OnHighlightedClick;

    private bool isHighlighted = false;
    private RectTransform rectTransform;

    public RectTransform RectTransform => rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (button == null) button = GetComponent<Button>();
        if (eventTrigger == null) eventTrigger = GetComponent<EventTrigger>();

        SetupEventTriggers();
        button.onClick.AddListener(HandleClick);
    }

    private void SetupEventTriggers()
    {
        // Configurar evento PointerEnter
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((data) => { SetHighlighted(true); });

        // Configurar evento PointerExit
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { SetHighlighted(false); });

        eventTrigger.triggers.Add(pointerEnterEntry);
        eventTrigger.triggers.Add(pointerExitEntry);
    }

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
    }

    public void SetSelectedByKeyboard(bool selected)
    {
        isHighlighted = selected;
    }

    private void HandleClick()
    {
        if (isHighlighted)
        {
            OnHighlightedClick.Invoke();
        }
    }

    public Vector2 GetCenterPosition()
    {
        // Devuelve la posición del centro del botón en espacio local
        return rectTransform.anchoredPosition;
    }
}