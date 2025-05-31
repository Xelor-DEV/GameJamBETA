using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BasePlayerController : MonoBehaviour
{
    protected Rigidbody rb;
    protected PlayerInput playerInput;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    protected virtual void EnableInput(bool enable)
    {
        if (playerInput != null)
            playerInput.enabled = enable;
    }
}
