using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnMove;
    public event Action OnChangeControl;
    public event Action OnInteract;
    public event Action<bool> OnDrag;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
            OnMove?.Invoke(context.ReadValue<Vector2>());
    }

    public void ChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnChangeControl?.Invoke();
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInteract?.Invoke();
    }

    public void Drag(InputAction.CallbackContext context)
    {
        if (context.started)
            OnDrag?.Invoke(true);
        else if (context.canceled)
            OnDrag?.Invoke(false);
    }
}
