using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

public class ControladorPersonajeIndependiente : MonoBehaviour
{
    [Header("Configuración de Personaje")]
    [SerializeField] private float velocidadMovimiento = 5f;
    // Ya no necesitas KeyCode teclaCambioControl

    private Rigidbody rbPersonaje;
    private Vector2 moveInput;

    void Awake()
    {
        rbPersonaje = GetComponent<Rigidbody>();
        if (rbPersonaje == null)
        {
            Debug.LogError("ControladorPersonajeIndependiente necesita un Rigidbody.", this);
        }
        // Este script (ControladorPersonajeIndependiente) debe comenzar DESACTIVADO en el Inspector.
    }

    void OnEnable()
    {
        GameEventsManager.OnRequestControlPersonaje += HandleRequestControlPersonaje;
        Debug.Log("ControladorPersonajeIndependiente habilitado.");
        // El componente PlayerInput en este GameObject se activará automáticamente.
    }

    void OnDisable()
    {
        GameEventsManager.OnRequestControlPersonaje -= HandleRequestControlPersonaje;
        Debug.Log("ControladorPersonajeIndependiente deshabilitado.");
        moveInput = Vector2.zero; // Resetea el input
        // El componente PlayerInput en este GameObject se desactivará.
    }

    private void HandleRequestControlPersonaje()
    {
        Debug.Log("ControladorPersonajeIndependiente: Recibida solicitud para tomar control.");
        this.enabled = true; // Se activa por el evento
    }

    // --- Métodos para ser llamados por los eventos del componente PlayerInput ---
    public void OnMove(InputAction.CallbackContext context)
    {
        if (this.enabled) // Solo procesar si este script está activo
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    public void OnChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed && this.enabled) // Solo si se presiona y el script está activo
        {
            Debug.Log("ControladorPersonajeIndependiente: Tecla/Botón de cambio presionado. Solicitando control de Plataforma.");
            GameEventsManager.RequestControlPlataforma(); // Dispara evento para activar la plataforma
            this.enabled = false; // Desactiva este script
        }
    }
    // ---------------------------------------------------------------------------

    void FixedUpdate()
    {
        if (rbPersonaje == null) return;

        Vector3 direccionInput = new Vector3(moveInput.x, 0, moveInput.y);
        // Vector3 direccionMovimientoNormalizada = direccionInput.normalized; // Normalizar aquí si es necesario
        rbPersonaje.MovePosition(rbPersonaje.position + direccionInput * velocidadMovimiento * Time.fixedDeltaTime);
    }
}