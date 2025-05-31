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
        if (other.CompareTag("Player"))
        {
            playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                // Usamos la dirección del jugador (su forward) al entrar
                slideDirection = other.transform.forward;
                slideDirection.y = 0; // Mantenemos el movimiento horizontal

                if (slideDirection != Vector3.zero)
                {
                    slideDirection.Normalize();
                }

                // Configuramos la duración del deslizamiento
                slideEndTime = Time.time + duration;
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerRigidbody != null && Time.time < slideEndTime)
        {
            // Aplicamos fuerza en la dirección de entrada del jugador
            playerRigidbody.AddForce(slideDirection * pushForce, ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Reseteamos cuando el jugador sale del trigger
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