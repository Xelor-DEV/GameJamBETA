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
    private bool isBeingDragged = false; // Variable privada

    // Propiedad pública para acceder al estado de isBeingDragged
    public bool IsBeingDragged => isBeingDragged;

    [Tooltip("Layer mask for surfaces the block can be dragged over AND placed upon.")]
    [SerializeField] private LayerMask draggableSurfaceLayerMask;
    // ...existing code...
    [Tooltip("Maximum distance for the raycast when dragging (used for initial plane detection).")]
    [SerializeField] private float maxDragRaycastDistance = 100f;
    [Tooltip("Distance for the raycast downwards when trying to settle the block on a surface.")]
    [SerializeField] private float settleRaycastDistance = 2f;

    [Header("Mouse Wheel Zoom")]
    [Tooltip("Sensibilidad del zoom con la rueda del mouse mientras se arrastra (unidades por tick completo de rueda).")]
    [SerializeField] private float mouseScrollSensitivity = 0.5f;
    [Tooltip("Distancia mínima a la cámara al hacer zoom.")]
    [SerializeField] private float minZoomDistance = 1.5f;
    [Tooltip("Distancia máxima a la cámara al hacer zoom.")]
    [SerializeField] private float maxZoomDistance = 20f;

    private Plane dragPlane;
    private float currentDistanceAlongRay;

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

        isBeingDragged = true; // Asegúrate de que esto se establece
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        Vector3 cameraToBlock = transform.position - mainCamera.transform.position;
        currentDistanceAlongRay = Vector3.Dot(cameraToBlock, ray.direction);
        currentDistanceAlongRay = Mathf.Clamp(currentDistanceAlongRay, minZoomDistance, maxZoomDistance);

        Vector3 initialAnchorPointOnRay = ray.GetPoint(currentDistanceAlongRay);
        dragOffset = transform.position - initialAnchorPointOnRay;

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDragRaycastDistance, draggableSurfaceLayerMask))
        {
            dragPlane = new Plane(Vector3.up, hitInfo.point);
        }
        else
        {
            Plane tempPlane = new Plane(Vector3.up, transform.position.y);
            float enter;
            if(tempPlane.Raycast(ray, out enter))
            {
                dragPlane = new Plane(Vector3.up, ray.GetPoint(enter));
            }
            else
            {
                 dragPlane = new Plane(Vector3.up, transform.position);
            }
        }

        if (blockRenderer != null)
            blockRenderer.material.color = Color.yellow;
    }

    public void UpdateDrag()
    {
        if (!isBeingDragged || rb == null || mainCamera == null) return;

        float scrollInput = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            currentDistanceAlongRay += (scrollInput / 120f) * mouseScrollSensitivity;
            currentDistanceAlongRay = Mathf.Clamp(currentDistanceAlongRay, minZoomDistance, maxZoomDistance);
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Vector3 targetAnchorPointOnRay = ray.GetPoint(currentDistanceAlongRay);
        rb.MovePosition(targetAnchorPointOnRay + dragOffset);
    }

    public void EndDrag()
    {
        if (rb == null) return;

        isBeingDragged = false; // Asegúrate de que esto se establece

        RaycastHit hit;
        Collider blockCollider = GetComponent<Collider>();
        Vector3 rayStartPoint = transform.position + Vector3.up * 0.1f;

        if (blockCollider != null && Physics.Raycast(rayStartPoint, Vector3.down, out hit, settleRaycastDistance + 0.1f, draggableSurfaceLayerMask))
        {
            float blockBottomYOffset = blockCollider.bounds.extents.y;
            Vector3 newPosition = hit.point + (Vector3.up * blockBottomYOffset);
            transform.position = new Vector3(transform.position.x, newPosition.y, transform.position.z);
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        if (isSelectedForHighlight && blockRenderer != null)
        {
            blockRenderer.material.color = Color.green;
        }
        else if (blockRenderer != null && originalMaterial != null)
        {
            blockRenderer.material = originalMaterial;
        }
    }

    public void ResetState()
    {
        if (isBeingDragged)
            EndDrag();

        isSelectedForHighlight = false;
        if (blockRenderer != null && originalMaterial != null)
        {
            blockRenderer.material = originalMaterial;
        }
        currentDistanceAlongRay = 0f;
        dragOffset = Vector3.zero;
        // Asegúrate de que isBeingDragged también se resetee si es necesario, aunque EndDrag() ya lo hace.
        // isBeingDragged = false; // Ya se hace en EndDrag
    }
}