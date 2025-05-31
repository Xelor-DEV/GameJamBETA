using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Rigidbody rb;
    private Collider objCollider;
    private UI_MovingInventory inventoryManager;
    private bool isBeingDragged = false;
    private float currentHeight;
    private float heightAdjustmentSpeed = 5f;
    private const float minHeight = 0.5f;
    private const float maxHeight = 22; // Altura máxima aumentada a 13
    private Vector3 offset;
    private int originalLayer;
    private const int ignoreRaycastLayer = 2; // Layer "Ignore Raycast"

    private ControladorPersonajeIndependiente playerController;

    public void SetInventoryManager(UI_MovingInventory manager)
    {
        inventoryManager = manager;
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

        playerController = Object.FindFirstObjectByType<ControladorPersonajeIndependiente>();
    }


    void OnMouseDown()
    {
        if (playerController != null && playerController.JuegoIniciado)
        {
            // Solo permitir arrastrar si está en el suelo
            if (gameObject.tag != "Suelo")
            {
                return;
            }

            // Verificar proximidad
            if (!playerController.CanDragBlock(gameObject))
            {
                return;
            }
        }

        if (rb != null && objCollider != null)
        {
            // Cambiar a capa que ignora raycast para evitar problemas de selección
            gameObject.layer = ignoreRaycastLayer;

            isBeingDragged = true;
            rb.isKinematic = true;
            objCollider.isTrigger = true;
            currentHeight = transform.position.y;

            // Calcular offset para mantener posición relativa
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            offset = transform.position - mouseWorldPos;
            offset.y = 0; // Solo mantener offset en XZ

            // Ocultar inventario si aún está visible
            if (inventoryManager != null)
            {
                inventoryManager.HideWindow();
            }
        }
    }

    void OnMouseDrag()
    {
        if (isBeingDragged)
        {
            // Obtener posición del mouse en el mundo
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // Calcular nueva posición con offset
            Vector3 newPosition = mouseWorldPos + offset;
            newPosition.y = currentHeight;

            // Mover objeto usando transform (no física)
            transform.position = newPosition;

            // Ajustar altura con la rueda del mouse
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentHeight = Mathf.Clamp(currentHeight + scroll * heightAdjustmentSpeed, minHeight, maxHeight);
                newPosition.y = currentHeight;
                transform.position = newPosition;
            }
        }
    }

    void OnMouseUp()
    {
        if (isBeingDragged)
        {
            isBeingDragged = false;

            // Restaurar propiedades físicas
            if (objCollider != null)
            {
                objCollider.isTrigger = false;
            }

            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Restaurar capa original
            gameObject.layer = originalLayer;

            // Mostrar inventario cuando se suelta el objeto
            if (inventoryManager != null)
            {
                inventoryManager.ShowWindow();
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Usar un Raycast para obtener la posición precisa en el plano de juego
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Crear un plano en la altura actual del objeto
        Plane plane = new Plane(Vector3.up, new Vector3(0, currentHeight, 0));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Fallback: usar el método anterior si el raycast falla
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Cambiar a tag "Suelo" cuando toca el suelo
        if (collision.gameObject.CompareTag("Suelo"))
        {
            gameObject.tag = "Suelo";
        }
        // Cambiar a tag "Plataforma" cuando toca la plataforma
        else if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
        }
    }
}