using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Rigidbody rb;
    private bool isBeingDragged = false;
    private float currentHeight;
    private float heightAdjustmentSpeed = 5f;
    private const float minHeight = 0.5f;
    private const float maxHeight = 10f;
    private float horizontalSpeedMultiplier = 0.2f; // Nuevo multiplicador para velocidad horizontal

    public void SetRigidbody(Rigidbody rigidbody)
    {
        rb = rigidbody;
        currentHeight = transform.position.y;
    }

    void OnMouseDown()
    {
        if (rb != null)
        {
            isBeingDragged = true;
            rb.isKinematic = true;
        }
    }

    void OnMouseDrag()
    {
        if (isBeingDragged && rb != null)
        {
            // Calcular movimiento horizontal con multiplicador
            float mouseX = Input.GetAxis("Mouse X") * horizontalSpeedMultiplier;
            float mouseZ = Input.GetAxis("Mouse Y") * horizontalSpeedMultiplier; // Usamos Mouse Y para Z

            // Convertir movimiento de pantalla a movimiento en el mundo
            Vector3 cameraRight = Camera.main.transform.right;
            Vector3 cameraUp = Camera.main.transform.up;
            Vector3 moveDirection = (cameraRight * mouseX + cameraUp * mouseZ);

            // Mantener movimiento solo en plano horizontal
            moveDirection.y = 0;

            // Aplicar movimiento horizontal
            Vector3 newPosition = transform.position + moveDirection;
            newPosition.y = currentHeight;
            rb.MovePosition(newPosition);

            // Ajustar altura con la rueda del mouse
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentHeight = Mathf.Clamp(currentHeight + scroll * heightAdjustmentSpeed, minHeight, maxHeight);
                newPosition.y = currentHeight;
                rb.MovePosition(newPosition);
            }
        }
    }

    void OnMouseUp()
    {
        if (isBeingDragged && rb != null)
        {
            isBeingDragged = false;
            rb.isKinematic = false;
        }
    }
}