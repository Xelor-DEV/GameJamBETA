// PlayerMovement.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Game State")]
    [SerializeField] private bool movementEnabled = false;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float currentSpeed;
    private bool isGrounded;
    private bool canMove = true;
    private bool canRotate = true;
    private PlayerCarryController carryController;
    private bool wasMoving = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        carryController = GetComponent<PlayerCarryController>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            Debug.LogWarning("Camera reference not set, using Main Camera", this);
        }
    }

    private void Update()
    {
        if (!movementEnabled) return; // No hacer nada si el movimiento está deshabilitado

        CheckGrounded();
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        bool isMoving = currentSpeed > 0.1f;
        if (isMoving != wasMoving)
        {
            animator.SetBool("isMoving", isMoving);
            wasMoving = isMoving;
        }
    }

    public void EnableMovement(bool enable)
    {
        movementEnabled = enable;
        SetMovementEnabled(enable);
    }

    private void FixedUpdate()
    {
        if (!movementEnabled) return;

        if (canMove)
        {
            ApplyMovement();
        }

        if (canRotate && !carryController.IsCarrying)
        {
            ApplyRotation();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer
        );
    }

    public Vector3 GetCameraRelativeDirection()
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        return (cameraForward * moveInput.y + cameraRight * moveInput.x);
    }

    private void ApplyMovement()
    {
        Vector3 moveDirection = GetCameraRelativeDirection().normalized;

        // Calcular velocidad actual
        if (moveDirection != Vector3.zero && moveInput != Vector2.zero)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        // Aplicar movimiento
        if (isGrounded && moveInput != Vector2.zero)
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
        else if (moveInput == Vector2.zero)
        {
            // Frenar cuando no hay input
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void ApplyRotation()
    {
        if (moveInput == Vector2.zero) return;

        Vector3 moveDirection = GetCameraRelativeDirection();

        if (moveDirection != Vector3.zero)
        {
            RotateTowardsDirection(moveDirection);
        }
    }

    // Método público para rotación controlada externamente
    public void RotateTowardsDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.rotation = Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    public void ApplyFixedRotation(Vector2 input)
    {
        // Calcular ángulo en grados basado en la dirección
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

        // Convertir a sistema de 360 grados
        if (angle < 0) angle += 360;

        // Mapear a las direcciones específicas
        float targetAngle = 0f;

        if (angle >= 315 || angle < 45)    // Derecha
            targetAngle = -135f;
        else if (angle >= 45 && angle < 135)   // Arriba
            targetAngle = 135f;
        else if (angle >= 135 && angle < 225)  // Izquierda
            targetAngle = 45f;
        else if (angle >= 225 && angle < 315)  // Abajo
            targetAngle = -45f;

        // Calcular rotación objetivo
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        // Interpolar suavemente hacia la rotación objetivo
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            currentSpeed = 0f;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    public void SetRotationEnabled(bool enabled)
    {
        canRotate = enabled;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position + Vector3.up * 0.1f,
            transform.position + Vector3.up * 0.1f + Vector3.down * (groundCheckDistance + 0.1f)
        );
    }
}