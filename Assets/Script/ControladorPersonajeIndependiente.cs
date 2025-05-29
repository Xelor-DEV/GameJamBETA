using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorPersonajeIndependiente : MonoBehaviour
{
    [Header("Configuración de Personaje")]
    [SerializeField] private float velocidadMovimiento = 5f;
    [Tooltip("Arrastra aquí el GameObject de la Plataforma")]
    [SerializeField] private Transform plataformaTransform;

    private Rigidbody rbPersonaje;
    private Vector2 moveInput;

    void Awake()
    {
        rbPersonaje = GetComponent<Rigidbody>();
        if (rbPersonaje == null)
        {
            Debug.LogError("ControladorPersonajeIndependiente necesita un Rigidbody.", this);
        }
        if (plataformaTransform == null)
        {
            Debug.LogWarning("ControladorPersonajeIndependiente: 'plataformaTransform' no asignada en el Inspector. El emparentamiento no funcionará.", this);
        }
        GameEventsManager.OnRequestControlPersonaje += HandleRequestControlPersonaje;
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPersonaje -= HandleRequestControlPersonaje;
    }

    void OnEnable()
    {
        Debug.Log("ControladorPersonajeIndependiente: Script HABILITADO (OnEnable).");
        if (rbPersonaje != null)
        {
            rbPersonaje.useGravity = true; // Asumir gravedad propia al ser independiente
        }
    }

    void OnDisable()
    {
        Debug.Log("ControladorPersonajeIndependiente: Script DESHABILITADO (OnDisable).");
        moveInput = Vector2.zero;
        if (rbPersonaje != null)
        {
            rbPersonaje.linearVelocity = Vector3.zero;
            rbPersonaje.angularVelocity = Vector3.zero;
        }
    }

    private void HandleRequestControlPersonaje()
    {
        Debug.Log("ControladorPersonajeIndependiente: Recibida solicitud para tomar control.");
        if (plataformaTransform != null && transform.parent == plataformaTransform)
        {
            transform.SetParent(null, true); // Desparentar, manteniendo posición global
            Debug.Log("ControladorPersonajeIndependiente: Cápsula desparentada de la plataforma.");
        }
        this.enabled = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (this.enabled)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    public void OnChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed && this.enabled)
        {
            Debug.Log("ControladorPersonajeIndependiente: Tecla/Botón de cambio presionado. Solicitando control de Plataforma.");
            if (plataformaTransform != null && transform.parent == null)
            {
                transform.SetParent(plataformaTransform, true); // Re-emparentar, manteniendo posición global
                Debug.Log("ControladorPersonajeIndependiente: Cápsula re-emparentada a la plataforma.");
                // Opcional: Ajustar gravedad si la plataforma "sostiene" al personaje
                // if (rbPersonaje != null) rbPersonaje.useGravity = false;
            }
            GameEventsManager.RequestControlPlataforma();
            this.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (rbPersonaje == null || !this.enabled) return;

        Vector3 direccionInput = new Vector3(moveInput.x, 0, moveInput.y);
        rbPersonaje.MovePosition(rbPersonaje.position + direccionInput.normalized * velocidadMovimiento * Time.fixedDeltaTime);
    }
}