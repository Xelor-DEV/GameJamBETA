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
        SetActive(false);
    }

    public void Move(Vector3 direction)
    {
        if (!isActive) return;
        moveDirection = direction.normalized;
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        if (moveDirection != Vector3.zero)
        {
            Vector3 targetVelocity = moveDirection * maxVelocity;
            Vector3 force = (targetVelocity - rb.linearVelocity) * moveForce;
            force.y = 0;

            rb.AddForce(force, ForceMode.Force);

            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVel.magnitude > maxVelocity)
            {
                horizontalVel = horizontalVel.normalized * maxVelocity;
                rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
            }
        }
        else
        {
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 brakeForce = -horizontalVel * deceleration;
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