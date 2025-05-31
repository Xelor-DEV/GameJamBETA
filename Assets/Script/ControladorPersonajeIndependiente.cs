using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ControladorPersonajeIndependiente : BasePlayerController
{
    [Header("Movimiento Isométrico")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float accelerationFactor = 15f;
    [SerializeField] private float decelerationFactor = 20f;

    private float _currentSpeed;
    private Vector3 _inputDirection;

    [Header("Configuración General Anterior")]
    [SerializeField] private Transform plataformaTransform;
    [SerializeField] private Transform puntoDeAgarreEnPlataforma;
    [SerializeField] private string tagTriggerAgarrePlataforma = "PuntoDeAgarrePlataforma";
    [SerializeField] private string tagBloque = "Bloque";
    [SerializeField] private string tagZonaInteraccionPlataforma = "ZonaInteraccionPlataforma";

    [SerializeField] private PlayerInput playerInputPersonaje;
    [SerializeField] private PlayerInput playerInputPlataforma;

    private bool puedeAgarrarsePlataforma = false;
    private GameObject objetoBloqueEnTrigger = null;
    private bool personajeEstaEnZonaPlataforma = false;
    private PlayerController plataformaActual = null;

    private BloqueInteractionHandler bloqueHandler;
    private Taiyoken taiyokenAbility;

    protected override void Awake()
    {
        base.Awake(); // Esto obtiene this.rb y this.playerInput
        bloqueHandler = GetComponent<BloqueInteractionHandler>();
        taiyokenAbility = GetComponent<Taiyoken>();

        if (this.playerInput == null)
        {
            Debug.LogError("PlayerInput del personaje (this.playerInput) NO ENCONTRADO. El control del personaje no funcionará.", this);
        }

        if (playerInputPlataforma == null)
        {
            Debug.LogWarning("Referencia a playerInputPlataforma no asignada en ControladorPersonajeIndependiente. No se podrá activar el input de la plataforma desde aquí si el personaje comienza agarrado.", this);
        }

        GameEventsManager.OnRequestControlPersonaje += HandleRequestControlPersonaje;

        // Lógica para comenzar agarrado a la plataforma:
        // Verifica que las referencias necesarias estén asignadas en el Inspector.
        if (plataformaTransform != null && puntoDeAgarreEnPlataforma != null && this.playerInput != null && rb != null)
        {
            Debug.Log("Configurando personaje para comenzar agarrado a la plataforma.");

            // 1. Desactivar input del personaje
            this.playerInput.enabled = false;
            _inputDirection = Vector3.zero; // Resetea el vector de input si lo usas para movimiento
            _currentSpeed = 0f;             // Resetea la velocidad actual si la usas

            // 2. Posicionar y emparentar el personaje
            transform.position = puntoDeAgarreEnPlataforma.position;
            transform.rotation = puntoDeAgarreEnPlataforma.rotation;
            transform.SetParent(plataformaTransform, true);

            // 3. Hacer el Rigidbody del personaje Kinematic
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 4. Activar input de la plataforma
            if (playerInputPlataforma != null)
            {
                playerInputPlataforma.enabled = true;
                Debug.Log("Input de la plataforma activado por el personaje al inicio.");
            }
            else
            {
                Debug.LogError("No se puede activar el input de la plataforma porque playerInputPlataforma no está asignado en ControladorPersonajeIndependiente.", this);
            }
        }
        else
        {
            // Comportamiento por defecto si no se cumplen las condiciones para agarrar la plataforma:
            // El personaje comienza con el control.
            Debug.Log("Personaje comenzando de forma independiente. Faltan referencias (plataformaTransform, puntoDeAgarreEnPlataforma) o componentes (this.playerInput, rb).");
            if (this.playerInput != null)
            {
                this.playerInput.enabled = true; // Personaje toma control
            }
            if (playerInputPlataforma != null)
            {
                playerInputPlataforma.enabled = false; // Plataforma no tiene control
            }
        }

        if (taiyokenAbility == null)
        {
            Debug.LogWarning("ControladorPersonajeIndependiente no encontró el componente Taiyoken.", this);
        }
        if (bloqueHandler == null)
        {
            Debug.LogError("ControladorPersonajeIndependiente no encontró el componente BloqueInteractionHandler.", this);
        }
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPersonaje -= HandleRequestControlPersonaje;
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
        if (rb == null || playerInput == null || !playerInput.enabled) return;

        CalculateSpeedLogic(Time.fixedDeltaTime);
        LookLogic(Time.fixedDeltaTime);
        MoveLogic();
    }

    private void CalculateSpeedLogic(float deltaTime)
    {
        if (_inputDirection == Vector3.zero && _currentSpeed > 0)
        {
            _currentSpeed -= decelerationFactor * deltaTime;
        }
        else if (_inputDirection != Vector3.zero && _currentSpeed < maxSpeed)
        {
            _currentSpeed += accelerationFactor * deltaTime;
        }
        _currentSpeed = Mathf.Clamp(_currentSpeed, 0, maxSpeed);
    }

    private void LookLogic(float deltaTime)
    {
        if (_inputDirection == Vector3.zero) return;

        Matrix4x4 isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        Vector3 isometricDirection = isometricMatrix.MultiplyPoint3x4(_inputDirection);

        if (isometricDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(isometricDirection, Vector3.up);
            Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * deltaTime);
            rb.MoveRotation(newRotation);
        }
    }

    private void MoveLogic()
    {
        Vector3 moveVelocity = transform.forward * _currentSpeed;
        
        Vector3 targetVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        rb.linearVelocity = targetVelocity;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (playerInput == null || !playerInput.enabled)
        {
            _inputDirection = Vector3.zero;
            return;
        }
        Vector2 rawInput = context.ReadValue<Vector2>();
        _inputDirection = new Vector3(rawInput.x, 0, rawInput.y).normalized;
    }

    protected override void EnableInput(bool enable)
    {
        base.EnableInput(enable);
        if (!enable)
        {
            _inputDirection = Vector3.zero;
            _currentSpeed = 0f;
            if (rb != null) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
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
        EnableInput(true);
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;
        bloqueHandler?.ResetBloque();
    }

    public void OnChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed && playerInput != null && playerInput.enabled &&
            puedeAgarrarsePlataforma && puntoDeAgarreEnPlataforma != null && plataformaTransform != null)
        {
            Debug.Log("Cambiando control a Plataforma");
            EnableInput(false);
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
            if (playerInputPlataforma != null) playerInputPlataforma.enabled = true;
            GameEventsManager.RequestControlPlataforma();
        }
    }

    public void OnActivarInteraccionConBloque(InputAction.CallbackContext context)
    {
        if (!context.performed || playerInput == null || !playerInput.enabled || bloqueHandler == null) return;

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
        if (playerInput == null || !playerInput.enabled || bloqueHandler == null)
        {
            if (context.canceled) bloqueHandler?.EndDrag();
            return;
        }
        if (!bloqueHandler.IsInInteractMode) return;

        if (context.started) bloqueHandler.StartDrag();
        else if (context.canceled) bloqueHandler.EndDrag();
    }

    public void OnActivateTaiyoken(InputAction.CallbackContext context)
    {
        if (context.performed && playerInput != null && playerInput.enabled && taiyokenAbility != null)
        {
            taiyokenAbility.ActivateTaiyoken();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagTriggerAgarrePlataforma)) puedeAgarrarsePlataforma = true;
        else if (other.CompareTag(tagBloque)) objetoBloqueEnTrigger = other.gameObject;
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
        if (other.CompareTag(tagTriggerAgarrePlataforma)) puedeAgarrarsePlataforma = false;
        else if (other.gameObject == objetoBloqueEnTrigger) objetoBloqueEnTrigger = null;
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