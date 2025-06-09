// PlayerCarryController.cs
using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Carry Settings")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private Transform carryPoint;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    private PlayerMovement playerMovement;
    private PlatformMovement nearbyPlatform;
    private bool isCarrying = false;
    private Rigidbody playerRigidbody;
    private bool wasCarrying = false;
    private BlockDragController blockDragController;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody>();
        blockDragController = GetComponent<BlockDragController>();
    }

    private void Update()
    {
        UpdateCarryAnimation();

        if (!isCarrying)
        {
            DetectNearbyPlatforms();
        }
        else
        {
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
        animator.SetTrigger("Pickup");

        // Configurar física
        playerRigidbody.isKinematic = true;

        // Configurar controles
        playerMovement.SetMovementEnabled(false);
        playerMovement.SetRotationEnabled(false); // Desactivar rotación automática
        nearbyPlatform.SetActive(true);
        ChangeControlManager.Instance.SetCurrentPlatform(nearbyPlatform);
        blockDragController.SetActive(false);
    }

    private void UpdateCarryAnimation()
    {
        if (isCarrying != wasCarrying)
        {
            animator.SetBool("isCarrying", isCarrying);
            wasCarrying = isCarrying;
        }
    }

    public void Detach()
    {
        if (!isCarrying) return;

        isCarrying = false;
        animator.SetTrigger("Drop");

        // Restaurar física
        playerRigidbody.isKinematic = false;

        // Restaurar controles
        playerMovement.SetMovementEnabled(true);
        playerMovement.SetRotationEnabled(true);
        nearbyPlatform.SetActive(false);
        ChangeControlManager.Instance.ClearCurrentPlatform();

        nearbyPlatform = null;
        blockDragController.SetActive(true);

    }

    public bool IsNearPlatform() => nearbyPlatform != null;
    public bool IsCarrying => isCarrying;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}