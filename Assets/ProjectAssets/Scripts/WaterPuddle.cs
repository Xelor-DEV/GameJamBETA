using UnityEngine;

public class WaterPuddle : MonoBehaviour
{
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float duration = 1f;
    private Vector3 slideDirection;
    private float slideEndTime;
    private Rigidbody playerRigidbody;

    private void OnTriggerEnter(Collider other)
    {
        PlatformMovement platform = other.GetComponent<PlatformMovement>();
        if (platform != null)
        {
            playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                // Usamos la velocidad actual del jugador para determinar la dirección
                Vector3 horizontalVelocity = playerRigidbody.linearVelocity;
                horizontalVelocity.y = 0; // Ignoramos el componente vertical

                // Comprobamos si hay movimiento horizontal significativo
                if (horizontalVelocity.sqrMagnitude > 0.1f)
                {
                    slideDirection = horizontalVelocity.normalized;
                    slideEndTime = Time.time + duration;
                }
                else
                {
                    // Si no hay movimiento, desactivamos el efecto
                    playerRigidbody = null;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerRigidbody != null && Time.time < slideEndTime)
        {
            playerRigidbody.AddForce(slideDirection * pushForce, ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlatformMovement platform = other.GetComponent<PlatformMovement>();
        if (platform != null)
        {
            playerRigidbody = null;
        }
    }

    private void OnValidate()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
}