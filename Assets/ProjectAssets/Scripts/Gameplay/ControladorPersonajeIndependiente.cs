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
    private bool juegoIniciado = false;

    [Header("Detección de Bloques")]
    [SerializeField] private float blockDetectionRadius = 5f;
    [SerializeField] private LayerMask blockDetectionLayer;
    [SerializeField] private Color detectionGizmoColor = new Color(0, 1, 0, 0.25f);
    [SerializeField] private Color activeDetectionGizmoColor = new Color(1, 0, 0, 0.5f);

    private List<GameObject> nearbyBlocks = new List<GameObject>();
    private bool isDetectionActive = false;



    [Header("Configuración General Anterior")]
    [SerializeField] private Transform plataformaTransform;
    [SerializeField] private Transform puntoDeAgarreEnPlataforma;
    [SerializeField] private string tagTriggerAgarrePlataforma = "PuntoDeAgarrePlataforma";
    [SerializeField] private string tagBloque = "Bloque";
    [SerializeField] private string tagZonaInteraccionPlataforma = "ZonaInteraccionPlataforma";

    [SerializeField] private PlayerInput playerInputPersonaje; // Usará this.playerInput de BasePlayerController
    [SerializeField] private PlayerInput playerInputPlataforma;

    [SerializeField] public Animator animator;

    private bool wasMoving = false;
    private bool wasCarrying = false;

    private bool puedeAgarrarsePlataforma = false;
    private GameObject objetoBloqueEnTrigger = null;
    private bool personajeEstaEnZonaPlataforma = false;
    private PlayerController plataformaActual = null;

    private Taiyoken taiyokenAbility;

    public bool JuegoIniciado
    {
        get
        {
            return juegoIniciado;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        taiyokenAbility = GetComponent<Taiyoken>();
      

        if (this.playerInput == null)
        {
            Debug.LogError("PlayerInput del personaje NO ENCONTRADO. El control del personaje no funcionará.", this);
        }

        GameEventsManager.OnRequestControlPersonaje += HandleRequestControlPersonaje;

        if (this.playerInput != null) this.playerInput.enabled = true;
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;

        if (taiyokenAbility == null)
        {
            Debug.LogWarning("ControladorPersonajeIndependiente no encontró el componente Taiyoken.", this);
        }

        ToggleBlockDetection(false);
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPersonaje -= HandleRequestControlPersonaje;
    }

    public void HabilitarMovimiento()
    {
        juegoIniciado = true;
        if (this.playerInput != null)
        {
            this.playerInput.enabled = true;
        }
    }

    void Update()
    {

        // Actualizar solo cuando cambia el estado de movimiento
        bool isMoving = _currentSpeed > 0.1f;
        if (isMoving != wasMoving)
        {
            animator.SetBool("isMoving", isMoving);
            wasMoving = isMoving;
        }

        // Actualizar solo cuando cambia el estado de carga
        bool isCarrying = (transform.parent == plataformaTransform);
        if (isCarrying != wasCarrying)
        {
            animator.SetBool("isCarrying", isCarrying);
            wasCarrying = isCarrying;
        }
    }

    void FixedUpdate()
    {
        if (!juegoIniciado || rb == null || playerInput == null || !playerInput.enabled) return;


        // Calcular velocidad y rotación en FixedUpdate para sincronizar con la física.
        // Usar Time.fixedDeltaTime para los cálculos dependientes del tiempo en FixedUpdate.
        CalculateSpeedLogic(Time.fixedDeltaTime);
        LookLogic(Time.fixedDeltaTime);
        MoveLogic();

        if (isDetectionActive)
        {
            DetectNearbyBlocks();
        }
    }
    public void ToggleBlockDetection(bool active)
    {
        isDetectionActive = active;
        if (!active) nearbyBlocks.Clear();
    }

    private void DetectNearbyBlocks()
    {
        nearbyBlocks.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, blockDetectionRadius, blockDetectionLayer);
        foreach (var hitCollider in hitColliders)
        {
            // Solo detectar bloques en el suelo
            if (hitCollider.CompareTag("Suelo"))
            {
                nearbyBlocks.Add(hitCollider.gameObject);
            }
        }
    }

    public bool CanDragBlock(GameObject block)
    {
        // En fase de juego, solo permitir arrastrar bloques cercanos
        return juegoIniciado && nearbyBlocks.Contains(block);
    }
    void OnDrawGizmos()
    {
        if (!isDetectionActive) return;

        Gizmos.color = nearbyBlocks.Count > 0 ? activeDetectionGizmoColor : detectionGizmoColor;
        Gizmos.DrawSphere(transform.position, blockDetectionRadius);
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
        // rb.MoveRotation en LookLogic debería haber actualizado la orientación del Rigidbody para este paso de física.
        // transform.forward ahora debería reflejar esa nueva orientación.
        Vector3 moveVelocity = transform.forward * _currentSpeed;
        
        // Mantenemos la velocidad vertical actual del Rigidbody (manejada por la gravedad de Unity)
        Vector3 targetVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        rb.linearVelocity = targetVelocity;

        // Para depuración, puedes añadir esto:
        // Debug.Log($"Input: {_inputDirection}, Speed: {_currentSpeed}, transform.forward: {transform.forward}, TargetVel: {targetVelocity}");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!juegoIniciado || playerInput == null || !playerInput.enabled)
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
        animator.SetTrigger("Drop");
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
    }

    public void OnChangeControl(InputAction.CallbackContext context)
    {
        if (context.performed && playerInput != null && playerInput.enabled &&
            puedeAgarrarsePlataforma && puntoDeAgarreEnPlataforma != null && plataformaTransform != null)
        {
            animator.SetTrigger("Pickup");
            Debug.Log("Cambiando control a Plataforma");
            EnableInput(false);
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