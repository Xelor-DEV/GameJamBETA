// PlatformMovement.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxVelocity = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float drag = 5f;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isActive = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = drag;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        SetActive(false);
    }

    public void Move(Vector3 direction)
    {
        if (!isActive) return;
        moveDirection = direction.normalized;
        
        // Calcular velocidad actual
        if (moveDirection != Vector3.zero)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        if (moveDirection != Vector3.zero && currentSpeed > 0)
        {
            // Aplicar fuerza en la dirección calculada
            Vector3 targetVelocity = moveDirection * currentSpeed;
            Vector3 force = (targetVelocity - rb.linearVelocity) * moveForce;
            force.y = 0;
            
            rb.AddForce(force, ForceMode.Force);
            
            // Limitar velocidad
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVel.magnitude > maxVelocity)
            {
                horizontalVel = horizontalVel.normalized * maxVelocity;
                rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
            }
        }
        else if (currentSpeed <= 0)
        {
            // Frenar cuando no hay input
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 brakeForce = -horizontalVel * deceleration * 0.1f;
            rb.AddForce(brakeForce, ForceMode.Force);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        rb.isKinematic = !active;
        
        if (!active)
        {
            moveDirection = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            currentSpeed = 0f;
        }
    }
}