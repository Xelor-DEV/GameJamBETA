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

    [Tooltip("Layer mask for surfaces the block can be dragged over AND placed upon.")]
    [SerializeField] private LayerMask draggableSurfaceLayerMask;
    [Tooltip("Maximum distance for the raycast when dragging.")]
    [SerializeField] private float maxDragRaycastDistance = 100f;
    [Tooltip("Distance for the raycast downwards when trying to settle the block on a surface.")]
    [SerializeField] private float settleRaycastDistance = 2f;

    private Plane dragPlane;

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
            dragPlane = new Plane(Vector3.up, hitInfo.point);
            dragOffset = transform.position - hitInfo.point;
        }
        else
        {
            dragPlane = new Plane(Vector3.up, transform.position);
            float distanceToPlane;
            if (dragPlane.Raycast(ray, out distanceToPlane))
            {
                dragOffset = transform.position - ray.GetPoint(distanceToPlane);
            }
            else
            {
                dragOffset = Vector3.zero;
            }
        }

        if (blockRenderer != null)
            blockRenderer.material.color = Color.yellow;
    }

    public void UpdateDrag()
    {
        if (!isBeingDragged || rb == null || mainCamera == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        float distanceToPlane;

        if (dragPlane.Raycast(ray, out distanceToPlane))
        {
            Vector3 pointOnPlane = ray.GetPoint(distanceToPlane);
            rb.MovePosition(pointOnPlane + dragOffset);
        }
    }

    public void EndDrag()
    {
        if (rb == null) return;

        isBeingDragged = false;

        RaycastHit hit;
        Collider blockCollider = GetComponent<Collider>();

        if (blockCollider != null && Physics.Raycast(transform.position, Vector3.down, out hit, settleRaycastDistance, draggableSurfaceLayerMask))
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
    }
}