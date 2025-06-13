using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("Drag Restrictions")]
    [SerializeField] private bool canBeDragged = true;
    [SerializeField] private bool gameStarted = false;
    private PlayerCarryController carryController;

    private Rigidbody rb;
    private Collider objCollider;
    [SerializeField] private UI_MovingInventory inventoryManager;
    [SerializeField] private bool isBeingDragged = false;
    [SerializeField] private float currentHeight;
    [SerializeField] private float heightAdjustmentSpeed = 5f;
    [SerializeField] private float minHeight = 0.5f;
    [SerializeField] private float maxHeight = 22;
    [SerializeField] private Vector3 offset;
    [SerializeField] private int originalLayer;
    [SerializeField] private int ignoreRaycastLayer = 2;
    [SerializeField] private bool isOnGround = false;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);
    [SerializeField] private Vector3 boxOffset = Vector3.zero;
    [SerializeField] private Color gizmoColorGround = Color.green;
    [SerializeField] private Color gizmoColorAir = Color.red;

    public void SetInventoryManager(UI_MovingInventory manager)
    {
        inventoryManager = manager;
    }

    public bool IsOnGround()
    {
        return isOnGround;
    }

    public void SetCanBeDragged(bool canDrag)
    {
        canBeDragged = canDrag;
    }

    public void SetGameStarted(bool started)
    {
        gameStarted = started;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objCollider = GetComponent<Collider>();
        currentHeight = transform.position.y;
        originalLayer = gameObject.layer;

        // Asegurar que el objeto tenga el tag correcto para arrastre
        gameObject.tag = "DraggableBlock";

        if (objCollider != null && rb != null && !isBeingDragged)
        {
            objCollider.isTrigger = true;
            rb.isKinematic = true;
        }

        carryController = Object.FindFirstObjectByType<PlayerCarryController>();
    }

    void Update()
    {
        // Solo verificar suelo cuando no se está arrastrando
        if (!isBeingDragged)
        {
            CheckGroundStatus();
        }
    }

    private void CheckGroundStatus()
    {
        // Calcular posición de la caja
        Vector3 boxCenter = transform.position + boxOffset;

        // Realizar OverlapBox para detectar suelo
        Collider[] hitColliders = Physics.OverlapBox(
            boxCenter,
            boxSize / 2,
            transform.rotation,
            groundLayer
        );

        // Resetear estado inicial
        isOnGround = false;

        // Verificar cada collider detectado
        foreach (var collider in hitColliders)
        {
            // Ignorarse a sí mismo
            if (collider.gameObject == gameObject) continue;

            // Si detecta suelo, actualizar estado y salir
            if (collider.CompareTag("Ground"))
            {
                isOnGround = true;
                break;
            }
        }
    }

    void OnMouseDown()
    {
        if (!gameStarted)
        {
            StartDragging();
            return;
        }

        // Cambiado a nuevo tag para bloques arrastrables
        if (gameObject.tag != "DraggableBlock") return;
        if (carryController != null && carryController.IsCarrying) return;
        if (!canBeDragged) return;

        StartDragging();
    }

    private void StartDragging()
    {
        if (rb == null || objCollider == null) return;

        gameObject.layer = ignoreRaycastLayer;
        isBeingDragged = true;
        rb.isKinematic = true;
        objCollider.isTrigger = true;
        currentHeight = transform.position.y;
        isOnGround = false; // Al arrastrar, no está en suelo

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;
        offset.y = 0;

        if (inventoryManager != null)
        {
            inventoryManager.HideWindow();
        }
    }

    void OnMouseDrag()
    {
        if (!isBeingDragged) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 newPosition = mouseWorldPos + offset;
        newPosition.y = currentHeight;
        transform.position = newPosition;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentHeight = Mathf.Clamp(
                currentHeight + scroll * heightAdjustmentSpeed,
                minHeight,
                maxHeight
            );
            newPosition.y = currentHeight;
            transform.position = newPosition;
        }
    }

    void OnMouseUp()
    {
        if (!isBeingDragged) return;

        isBeingDragged = false;
        gameObject.layer = originalLayer;

        if (objCollider != null) objCollider.isTrigger = false;
        if (rb != null) rb.isKinematic = false;

        if (!gameStarted && inventoryManager != null)
        {
            inventoryManager.ShowWindow();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, currentHeight, 0));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnTriggerExit(Collider other)
    {
        if (gameStarted && other.CompareTag("Player") && isBeingDragged)
        {
            SetCanBeDragged(false);
        }
    }

    // Debug: Visualizar el área de detección
    void OnDrawGizmos()
    {
        Gizmos.color = isOnGround ? gizmoColorGround : gizmoColorAir;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + boxOffset, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}