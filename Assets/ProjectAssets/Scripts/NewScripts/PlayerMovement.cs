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

    private Rigidbody rb;
    private Vector2 moveInput;
    private float currentSpeed;
    private bool isGrounded;
    private bool canMove = true;
    private bool canRotate = true;
    private PlayerCarryController carryController;

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
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            ApplyMovement();
        }
        
        if (canRotate)
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

    // Añade esta parte: Convertir dirección al espacio de la plataforma si está acarreando
    if (carryController != null && carryController.IsCarrying)
    {
        PlatformMovement platform = ChangeControlManager.Instance.CurrentPlatform;
        if (platform != null)
        {
            // Convertir de dirección mundial a local (respecto a la plataforma)
            moveDirection = platform.transform.InverseTransformDirection(moveDirection);
            // Mantener solo la componente horizontal
            moveDirection.y = 0;
        }
    }

    if (moveDirection != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        rb.rotation = Quaternion.RotateTowards(
            rb.rotation, 
            targetRotation, 
            rotationSpeed * Time.fixedDeltaTime
        );
    }
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