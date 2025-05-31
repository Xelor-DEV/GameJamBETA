using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ControladorPersonajeIndependiente : BasePlayerController
{
    [SerializeField] private float velocidadMovimiento = 5f;
    [SerializeField] private Transform plataformaTransform;
    [SerializeField] private Transform puntoDeAgarreEnPlataforma;
    [SerializeField] private string tagTriggerAgarrePlataforma = "PuntoDeAgarrePlataforma";
    [SerializeField] private string tagBloque = "Bloque";
    [SerializeField] private string tagZonaInteraccionPlataforma = "ZonaInteraccionPlataforma";

    [SerializeField] private PlayerInput playerInputPersonaje;
    [SerializeField] private PlayerInput playerInputPlataforma;

    private Vector2 moveInput;
    private bool puedeAgarrarsePlataforma = false;
    private GameObject objetoBloqueEnTrigger = null;
    private bool personajeEstaEnZonaPlataforma = false;
    private PlayerController plataformaActual = null;

    private BloqueInteractionHandler bloqueHandler;
    private Taiyoken taiyokenAbility;

    private int bloqueSeleccionadoIndex = 0;
    private HashSet<Rigidbody> bloquesEnSuelo = new HashSet<Rigidbody>();
    private HashSet<Rigidbody> bloquesFueraDePlataforma = new HashSet<Rigidbody>();

    protected override void Awake()
    {
        base.Awake();
        bloqueHandler = GetComponent<BloqueInteractionHandler>();
        taiyokenAbility = GetComponent<Taiyoken>();

        if (playerInputPersonaje == null)
        {
            Debug.LogWarning("PlayerInput del personaje no asignado en el Inspector. Intentando obtenerlo del GameObject.", this);
            playerInputPersonaje = GetComponent<PlayerInput>();
        }
        if (playerInputPersonaje == null)
        {
            Debug.LogError("PlayerInput del personaje NO ENCONTRADO. El control del personaje no funcionará.", this);
        }

        GameEventsManager.OnRequestControlPersonaje += HandleRequestControlPersonaje;

        if (playerInputPersonaje != null) playerInputPersonaje.enabled = true;
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;

        if (taiyokenAbility == null)
        {
            Debug.LogWarning("ControladorPersonajeIndependiente no encontró el componente Taiyoken. La habilidad no funcionará.", this);
        }
         if (bloqueHandler == null)
        {
            Debug.LogError("ControladorPersonajeIndependiente no encontró el componente BloqueInteractionHandler. El arrastre de bloques no funcionará.", this);
        }
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPersonaje -= HandleRequestControlPersonaje;
    }

    protected override void EnableInput(bool enable)
    {
        if (playerInputPersonaje != null)
        {
            playerInputPersonaje.enabled = enable;
        }
    }

    private void HandleRequestControlPersonaje()
    {
        Debug.Log("Manejando solicitud de control para el Personaje");
        if (plataformaTransform != null && transform.parent == plataformaTransform)
        {
            transform.SetParent(null, true);
        }
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        if (playerInputPersonaje != null) playerInputPersonaje.enabled = true;
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;
        bloqueHandler?.ResetBloque();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (playerInputPersonaje != null && playerInputPersonaje.enabled && (bloqueHandler == null || !bloqueHandler.IsDragging))
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed && playerInputPersonaje != null && playerInputPersonaje.enabled &&
            puedeAgarrarsePlataforma && puntoDeAgarreEnPlataforma != null && plataformaTransform != null)
        {
            Debug.Log("Cambiando control a Plataforma");
            bloqueHandler?.ResetBloque();
            transform.position = puntoDeAgarreEnPlataforma.position;
            transform.rotation = puntoDeAgarreEnPlataforma.rotation;
            transform.SetParent(plataformaTransform, true);

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            if (playerInputPersonaje != null) playerInputPersonaje.enabled = false;
            if (playerInputPlataforma != null) playerInputPlataforma.enabled = true;
            GameEventsManager.RequestControlPlataforma();
        }
    }

    public void OnActivarInteraccionConBloque(InputAction.CallbackContext context)
    {
        if (!context.performed || playerInputPersonaje == null || !playerInputPersonaje.enabled || bloqueHandler == null) return;

        if (bloqueHandler.IsInInteractMode)
        {
            bloqueHandler.ResetBloque();
            return;
        }

        Camera cam = Camera.main;
        if (cam != null && Mouse.current != null)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                GameObject bloqueGO = hit.collider.gameObject;
                if (bloqueGO.CompareTag(tagBloque))
                {
                    bloqueHandler.PrepareBloque(bloqueGO);
                    return;
                }
            }
        }
        bloqueHandler.ResetBloque();
    }

    public void OnArrastrarBloqueConClick(InputAction.CallbackContext context)
    {
        if (playerInputPersonaje == null || !playerInputPersonaje.enabled || bloqueHandler == null)
        {
            if (context.canceled) bloqueHandler?.EndDrag();
            return;
        }

        if (!bloqueHandler.IsInInteractMode) return;

        if (context.started)
        {
            bloqueHandler.StartDrag();
        }
        else if (context.canceled)
        {
            bloqueHandler.EndDrag();
        }
    }

    public void OnActivateTaiyoken(InputAction.CallbackContext context)
    {
        if (context.performed && playerInputPersonaje != null && playerInputPersonaje.enabled && taiyokenAbility != null)
        {
            taiyokenAbility.ActivateTaiyoken();
        }
        else if (context.performed && taiyokenAbility == null)
        {
            Debug.LogWarning("Se intentó activar Taiyoken, pero el componente no está asignado/encontrado.");
        }
    }

    void Update()
    {
        if (bloqueHandler != null && bloqueHandler.IsDragging)
        {
            bloqueHandler.UpdateDrag();
        }
    }

    void FixedUpdate()
    {
        if (rb == null || playerInputPersonaje == null || !playerInputPersonaje.enabled) return;

        if (moveInput != Vector2.zero && !rb.isKinematic && (bloqueHandler == null || !bloqueHandler.IsDragging))
        {
            Vector3 direccionInput = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            rb.MovePosition(rb.position + direccionInput * velocidadMovimiento * Time.fixedDeltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagTriggerAgarrePlataforma))
        {
            puedeAgarrarsePlataforma = true;
        }
        else if (other.CompareTag(tagBloque))
        {
            objetoBloqueEnTrigger = other.gameObject;
        }
        else if (other.CompareTag(tagZonaInteraccionPlataforma))
        {
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                plataformaActual = pc;
                personajeEstaEnZonaPlataforma = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagTriggerAgarrePlataforma))
        {
            puedeAgarrarsePlataforma = false;
        }
        else if (other.gameObject == objetoBloqueEnTrigger)
        {
            // Esta comparación es un poco torpe, idealmente tendrías una referencia directa
            // if (bloqueHandler != null && bloqueHandler.IsInInteractMode && bloqueHandler.IsDragging &&
            //     (bloqueHandler as BloqueInteractionHandler)?.ToString() == objetoBloqueEnTrigger?.GetComponent<MovementBloque>()?.ToString())
            // {
            //      // bloqueHandler.ResetBloque(); // Ojo: esto podría ser abrupto si el jugador lo saca del trigger mientras arrastra
            // }
            objetoBloqueEnTrigger = null;
        }
        else if (other.CompareTag(tagZonaInteraccionPlataforma))
        {
             PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (plataformaActual != null && pc == plataformaActual)
            {
                personajeEstaEnZonaPlataforma = false;
                plataformaActual = null;
            }
        }
    }
}