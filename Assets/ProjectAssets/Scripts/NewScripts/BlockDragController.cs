using UnityEngine;

public class BlockDragController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.red;

    private PlayerCarryController carryController;
    private bool isActive = true;
    private GameObject currentBlock;

    private void Awake()
    {
        carryController = GetComponent<PlayerCarryController>();
    }

    private void Update()
    {
        if (!isActive || carryController.IsCarrying) return;

        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            blockLayer
        );

        foreach (var collider in hitColliders)
        {
            // Cambiado a nuevo tag para bloques arrastrables
            if (collider.CompareTag("DraggableBlock"))
            {
                DraggableItem draggable = collider.GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.SetCanBeDragged(true);
                }
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? activeColor : inactiveColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}