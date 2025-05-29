using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Plataforma")]
    [SerializeField] private float fuerzaMovimiento = 10f;
    // Ya no se usa KeyCode teclaCambioControl

    private Rigidbody rb;
    private Vector2 moveInputPlataforma;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController necesita un Rigidbody en el mismo GameObject.", this);
        }
        // Este script (PlayerController) debe comenzar ACTIVADO en el Inspector.
    }

    void OnEnable()
    {
        GameEventsManager.OnRequestControlPlataforma += HandleRequestControlPlataforma;
        Debug.Log("PlayerController habilitado. Liberando plataforma y bloques.");
        LiberarPlataformaYBloques();
        // El componente PlayerInput en este GameObject se activará automáticamente.
    }

    void OnDisable()
    {
        GameEventsManager.OnRequestControlPlataforma -= HandleRequestControlPlataforma;
        Debug.Log("PlayerController deshabilitado.");
        moveInputPlataforma = Vector2.zero; // Resetea el input
        // El componente PlayerInput en este GameObject se desactivará.
    }

    private void HandleRequestControlPlataforma()
    {
        Debug.Log("PlayerController: Recibida solicitud para tomar control de plataforma.");
        this.enabled = true; // Se activa por el evento
    }

    // --- Métodos para ser llamados por los eventos del componente PlayerInput ---
    public void OnMovePlataforma(InputAction.CallbackContext context)
    {
        if (this.enabled) // Solo procesar si este script está activo
        {
            moveInputPlataforma = context.ReadValue<Vector2>();
        }
    }

    public void OnChangeControlPlataforma(InputAction.CallbackContext context)
    {
        if (context.performed && this.enabled) // Solo si se presiona y el script está activo
        {
            Debug.Log("PlayerController: Tecla/Botón de cambio presionado. Solicitando control de Personaje.");
            FijarPlataformaYBloques();
            GameEventsManager.RequestControlPersonaje(); // Dispara evento para activar el personaje
            this.enabled = false; // Desactiva este script
        }
    }
    // ---------------------------------------------------------------------------

    void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 direccionInput = new Vector3(moveInputPlataforma.x, 0, moveInputPlataforma.y);
        Vector3 direccionMovimientoNormalizada = direccionInput.normalized; // Normalizar aquí si es necesario
        rb.AddForce(direccionMovimientoNormalizada * fuerzaMovimiento, ForceMode.Force);
    }

    void FijarPlataformaYBloques()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        Rigidbody[] todosLosRigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody bloqueRb in todosLosRigidbodies)
        {
            if (bloqueRb.gameObject.CompareTag("Bloque") && bloqueRb != rb)
            {
                bloqueRb.isKinematic = true;
                bloqueRb.velocity = Vector3.zero;
                bloqueRb.angularVelocity = Vector3.zero;
            }
        }
        Debug.Log("Plataforma y bloques FIJADOS.");
    }

    void LiberarPlataformaYBloques()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        Rigidbody[] todosLosRigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody bloqueRb in todosLosRigidbodies)
        {
            if (bloqueRb.gameObject.CompareTag("Bloque") && bloqueRb != rb)
            {
                bloqueRb.isKinematic = false;
            }
        }
        Debug.Log("Plataforma y bloques LIBERADOS.");
    }
}