// ChangeControlManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class ChangeControlManager : MonoBehaviour
{
    public static ChangeControlManager Instance { get; private set; }

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCarryController carryController;
    [SerializeField] private PlatformMovement currentPlatform;

    private Vector2 currentInput;
    private bool isCarryingLastFrame;
    [SerializeField] private BlockDragController blockDragController;

    public PlatformMovement CurrentPlatform => currentPlatform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
        playerMovement.OnMove(context);

        if (carryController.IsCarrying && currentPlatform != null)
        {
            Vector3 moveDirection = playerMovement.GetCameraRelativeDirection();
            currentPlatform.Move(moveDirection);
        }
    }

    private void Update()
    {
        // Aplicar rotación solo cuando cambia el input
        if (carryController.IsCarrying && currentPlatform != null && currentInput != Vector2.zero)
        {
            playerMovement.ApplyFixedRotation(currentInput);
        }

        // Forzar rotación al iniciar acople
        if (carryController.IsCarrying && !isCarryingLastFrame)
        {
            playerMovement.ApplyFixedRotation(currentInput);
        }

        isCarryingLastFrame = carryController.IsCarrying;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (carryController.IsCarrying)
        {
            carryController.Detach();
        }
        else if (carryController.IsNearPlatform())
        {
            carryController.TryAttach();
        }
    }

    public void SetCurrentPlatform(PlatformMovement platform)
    {
        currentPlatform = platform;
        blockDragController.SetActive(false);
    }

    public void ClearCurrentPlatform()
    {
        currentPlatform = null;
        blockDragController.SetActive(true);
    }
}