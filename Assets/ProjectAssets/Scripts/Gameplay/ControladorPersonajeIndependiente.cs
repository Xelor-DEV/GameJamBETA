using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; // <-- Agrega esta línea

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

    private int bloqueSeleccionadoIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        bloqueHandler = GetComponent<BloqueInteractionHandler>();
        if (playerInputPersonaje == null)
            playerInputPersonaje = GetComponent<PlayerInput>();
        GameEventsManager.OnRequestControlPersonaje += HandleRequestControlPersonaje;
        if (playerInputPersonaje != null) playerInputPersonaje.enabled = true;
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPersonaje -= HandleRequestControlPersonaje;
    }

    void OnEnable()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
        }
    }

    void OnDisable()
    {
        moveInput = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        puedeAgarrarsePlataforma = false;
        bloqueHandler?.ResetBloque();
        personajeEstaEnZonaPlataforma = false;
        plataformaActual = null;
    }

    private void HandleRequestControlPersonaje()
    {
        if (plataformaTransform != null && transform.parent == plataformaTransform)
            transform.SetParent(null, true);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
        }
        if (playerInputPersonaje != null) playerInputPersonaje.enabled = true;
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;
        bloqueHandler?.ResetBloque();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (playerInputPersonaje != null && playerInputPersonaje.enabled && (bloqueHandler == null || !bloqueHandler.IsDragging))
            moveInput = context.ReadValue<Vector2>();
        else
            moveInput = Vector2.zero;
    }

    public void OnChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed && playerInputPersonaje != null && playerInputPersonaje.enabled && puedeAgarrarsePlataforma && puntoDeAgarreEnPlataforma != null && plataformaTransform != null)
        {
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
        if (!context.performed || playerInputPersonaje == null || !playerInputPersonaje.enabled) return;
        if (!personajeEstaEnZonaPlataforma || plataformaActual == null) return;

        var bloques = plataformaActual.GetBloquesSobrePlataforma();
        if (bloques != null && bloques.Count > 0)
        {
            // Si ya está en modo interactuar, al presionar E se desmarca el bloque seleccionado
            if (bloqueHandler != null && bloqueHandler.IsInInteractMode)
            {
                bloqueHandler.ResetBloque();
                return;
            }

            // Si NO quieres usar raycast y quieres que se pueda marcar cualquier bloque (todos los bloques sobre la plataforma):
            // Marca todos los bloques para manipulación
            foreach (var bloque in bloques)
            {
                if (bloque != null)
                    bloqueHandler.PrepareBloque(bloque.gameObject);
            }
        }
        else
        {
            bloqueHandler?.ResetBloque();
        }
    }

    public void OnArrastrarBloqueConClick(InputAction.CallbackContext context)
    {
        if (playerInputPersonaje == null || !playerInputPersonaje.enabled || bloqueHandler == null || !bloqueHandler.IsInInteractMode)
        {
            if (context.canceled)
                bloqueHandler?.EndDrag();
            return;
        }

        if (context.started)
            bloqueHandler.StartDrag();
        else if (context.canceled)
            bloqueHandler.EndDrag();
    }

    void Update()
    {
        if (bloqueHandler != null && bloqueHandler.IsDragging)
        {
            bloqueHandler.UpdateDrag();
            moveInput = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || playerInputPersonaje == null || !playerInputPersonaje.enabled) return;
        if (moveInput != Vector2.zero && !rb.isKinematic && (bloqueHandler == null || !bloqueHandler.IsDragging))
        {
            Vector3 direccionInput = new Vector3(moveInput.x, 0, moveInput.y);
            rb.MovePosition(rb.position + direccionInput.normalized * velocidadMovimiento * Time.fixedDeltaTime);
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
            else
            {
                personajeEstaEnZonaPlataforma = false;
                plataformaActual = null;
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
            bloqueHandler?.ResetBloque();
            objetoBloqueEnTrigger = null;
        }
        else if (other.CompareTag(tagZonaInteraccionPlataforma))
        {
            if (plataformaActual != null && other.GetComponentInParent<PlayerController>() == plataformaActual)
            {
                personajeEstaEnZonaPlataforma = false;
                plataformaActual = null;
                bloqueHandler?.ResetBloque();
                bloqueSeleccionadoIndex = 0;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!personajeEstaEnZonaPlataforma || plataformaActual == null) return;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Ray ray = cam.ScreenPointToRay(mousePos);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * 100f);
        }
    }
}