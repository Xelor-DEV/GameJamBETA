using UnityEngine;
using UnityEngine.InputSystem;

public class MovementBloque : MonoBehaviour
{
    private Material originalMaterial;
    private Renderer blockRenderer;
    private bool isSelectedForHighlight = false;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 dragOffset;
    private bool isBeingDragged = false;

    [Tooltip("Layer mask for surfaces the block can be dragged over.")]
    [SerializeField] private LayerMask draggableSurfaceLayerMask;
    [Tooltip("Maximum distance for the raycast when dragging.")]
    [SerializeField] private float maxDragRaycastDistance = 100f;

    public bool IsBeingDragged => isBeingDragged;

    void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer != null)
            originalMaterial = blockRenderer.material;
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    public void PrepareForDrag()
    {
        isSelectedForHighlight = true;
        if (blockRenderer != null)
            blockRenderer.material.color = Color.green;
    }

    public void StartDrag()
    {
        if (rb == null || mainCamera == null) return;

        isBeingDragged = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDragRaycastDistance, draggableSurfaceLayerMask))
        {
            dragOffset = transform.position - hitInfo.point;
        }
        else
        {
            Plane plane = new Plane(mainCamera.transform.forward, transform.position);
            float distance;
            if (plane.Raycast(ray, out distance))
                dragOffset = transform.position - ray.GetPoint(distance);
            else
                dragOffset = Vector3.zero;
        }

        if (blockRenderer != null)
            blockRenderer.material.color = Color.yellow;
    }

    public void UpdateDrag()
    {
        if (!isBeingDragged || rb == null || mainCamera == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hitInfo;
        // El raycast debe detectar la superficie sobre la que quieres mover el bloque (por ejemplo, el suelo)
        if (Physics.Raycast(ray, out hitInfo, maxDragRaycastDistance, draggableSurfaceLayerMask))
        {
            Vector3 targetPosition = hitInfo.point + dragOffset;
            rb.MovePosition(targetPosition);
        }
        else
        {
            // Si no colisiona con la superficie, NO sueltes el bloque, simplemente no lo muevas
            // (No llames a EndDrag ni cambies estado aquí)
            // Así el bloque solo se mueve si el mouse apunta a una superficie válida
        }
    }

    public void EndDrag()
    {
        if (rb == null) return;

        isBeingDragged = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        if (isSelectedForHighlight && blockRenderer != null)
            blockRenderer.material.color = Color.green;
        else if (blockRenderer != null && originalMaterial != null)
            blockRenderer.material = originalMaterial;
    }

    public void ResetState()
    {
        if (isBeingDragged)
            EndDrag();
        isSelectedForHighlight = false;
        if (blockRenderer != null && originalMaterial != null)
            blockRenderer.material = originalMaterial;
    }
}