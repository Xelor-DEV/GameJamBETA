// PlayerCarryController.cs
using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Carry Settings")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float positionLerpSpeed = 15f;
    
    private PlayerMovement playerMovement;
    private PlatformMovement nearbyPlatform;
    private bool isCarrying = false;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!isCarrying) 
        {
            DetectNearbyPlatforms();
        }
        else
        {
            // Movimiento suave hacia el punto de acarreo
            transform.position = carryPoint.position;
        }
    }

    private void DetectNearbyPlatforms()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position, 
            detectionRadius, 
            platformLayer
        );
        
        nearbyPlatform = null;
        
        foreach (var collider in hitColliders)
        {
            PlatformMovement platform = collider.GetComponent<PlatformMovement>();
            if (platform != null)
            {
                nearbyPlatform = platform;
                break;
            }
        }
    }

public void TryAttach()
{
    if (isCarrying || nearbyPlatform == null) return;
    
    isCarrying = true;
    
    // Añade esto: Sincronizar rotación inicial con la plataforma
    transform.rotation = nearbyPlatform.transform.rotation;
    
    // Configurar posición inicial
    transform.position = carryPoint.position;
    
    // Configurar controles
    playerMovement.SetMovementEnabled(false);
    playerMovement.SetRotationEnabled(true);
    nearbyPlatform.SetActive(true);
    ChangeControlManager.Instance.SetCurrentPlatform(nearbyPlatform);
}

    public void Detach()
    {
        if (!isCarrying) return;
        
        isCarrying = false;
        
        // Restaurar controles
        playerMovement.SetMovementEnabled(true);
        nearbyPlatform.SetActive(false);
        ChangeControlManager.Instance.ClearCurrentPlatform();
        
        nearbyPlatform = null;
    }

    public bool IsNearPlatform() => nearbyPlatform != null;
    public bool IsCarrying => isCarrying;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}