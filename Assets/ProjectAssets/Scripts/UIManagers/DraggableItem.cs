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
    [SerializeField]private float currentHeight;
    [SerializeField]private float heightAdjustmentSpeed = 5f;
    [SerializeField]private float minHeight = 0.5f;
    [SerializeField]private float maxHeight = 22; // Altura máxima aumentada a 13
    [SerializeField]private Vector3 offset;
    [SerializeField]private int originalLayer;
    [SerializeField]private int ignoreRaycastLayer = 2; // Layer "Ignore Raycast"
    [SerializeField] private bool isOnGround = false;
    private int groundCollisionCount = 0;

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

        // Guardar la capa original
        originalLayer = gameObject.layer;

        // Configurar inicialmente
        if (objCollider != null && rb != null && !isBeingDragged)
        {
            objCollider.isTrigger = true;
            rb.isKinematic = true;
        }

        carryController = Object.FindFirstObjectByType<PlayerCarryController>();
    }


    void OnMouseDown()
    {
        // Comportamiento antes del Ready (arrastre libre)
        if (!gameStarted)
        {
            StartDragging();
            return;
        }

        // Comportamiento después del Ready (arrastre restringido)
        if (gameObject.tag != "Suelo") return;
        if (carryController != null && carryController.IsCarrying) return;
        if (!canBeDragged) return;

        StartDragging();
    }

    private void StartDragging()
    {
        if (rb == null || objCollider == null) return;

        // Cambiar a capa que ignora raycast
        gameObject.layer = ignoreRaycastLayer;

        isBeingDragged = true;
        rb.isKinematic = true;
        objCollider.isTrigger = true;
        currentHeight = transform.position.y;

        // Calcular offset
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;
        offset.y = 0;

        // Ocultar inventario
        if (inventoryManager != null)
        {
            inventoryManager.HideWindow();
        }
    }



    void OnMouseDrag()
    {
        if (!isBeingDragged) return;

        // Obtener posición del mouse
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 newPosition = mouseWorldPos + offset;
        newPosition.y = currentHeight;

        // Mover objeto
        transform.position = newPosition;

        // Ajustar altura con la rueda del mouse
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

        // Restaurar propiedades físicas
        if (objCollider != null) objCollider.isTrigger = false;
        if (rb != null) rb.isKinematic = false;

        // Restaurar capa original
        gameObject.layer = originalLayer;

        // Mostrar inventario solo en fase de preparación
        if (!gameStarted && inventoryManager != null)
        {
            inventoryManager.ShowWindow();
        }
    }


    private Vector3 GetMouseWorldPosition()
    {
        // Usar Raycast para obtener posición precisa
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, currentHeight, 0));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Fallback
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            gameObject.tag = "Suelo";
            groundCollisionCount++;
            UpdateGroundStatus();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            groundCollisionCount = Mathf.Max(0, groundCollisionCount - 1);
            UpdateGroundStatus();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Suelo"))
        {
            groundCollisionCount++;
            UpdateGroundStatus();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Suelo"))
        {
            groundCollisionCount = Mathf.Max(0, groundCollisionCount - 1);
            UpdateGroundStatus();
        }

        // Código existente para jugador
        if (gameStarted && other.CompareTag("Player") && isBeingDragged)
        {
            SetCanBeDragged(false);
        }
    }
    private void UpdateGroundStatus()
    {
        isOnGround = groundCollisionCount > 0;
    }
}