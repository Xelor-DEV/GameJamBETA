// ChangeControlManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class ChangeControlManager : MonoBehaviour
{
    public static ChangeControlManager Instance { get; private set; }
    
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCarryController carryController;
    [SerializeField] private PlatformMovement currentPlatform;
    
    public PlatformMovement CurrentPlatform
    {
        get
        {
            return currentPlatform;
        }
    }

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
        Vector2 input = context.ReadValue<Vector2>();
        
        // Actualizar siempre el input del jugador para la rotación
        playerMovement.OnMove(context);
        
        if (carryController.IsCarrying && currentPlatform != null)
        {
            // Obtener dirección relativa a la cámara
            Vector3 moveDirection = playerMovement.GetCameraRelativeDirection();
            
            // Mover plataforma en esa dirección
            currentPlatform.Move(moveDirection);
        }
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
    }

    public void ClearCurrentPlatform()
    {
        currentPlatform = null;
    }
}