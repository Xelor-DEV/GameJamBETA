using UnityEngine;
using System.Collections;

public class ObstacleCar : MonoBehaviour
{
    public enum State
    {
        MovingToEnd,
        WaitingAtEnd,
        TeleportedToStart,
        WaitingForOtherCar
    }

    [Header("Waypoints")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Steering Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float slowingRadius = 3f;
    [SerializeField] private float arrivalThreshold = 0.1f;
    [SerializeField] private float waitTime = 2f;

    [Header("Dependency")]
    public ObstacleCar otherCar;

    private Rigidbody rb;
    private State currentState;
    private float otherCarHalfDistance;
    private Vector3 currentTarget;
    private Vector3 desiredVelocity;
    private Vector3 steeringForce;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.position = startPoint.position;
        currentState = State.MovingToEnd;
        currentTarget = endPoint.position;

        if (otherCar != null)
        {
            otherCarHalfDistance = Vector3.Distance(otherCar.startPoint.position, otherCar.endPoint.position) * 0.5f;
        }
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.MovingToEnd:
                Seek(currentTarget);
                //OrientTowards(rb.linearVelocity);

                // Check if arrived at end point
                if (Vector3.Distance(transform.position, endPoint.position) <= arrivalThreshold)
                {
                    rb.linearVelocity = Vector3.zero;
                    currentState = State.WaitingAtEnd;
                    StartCoroutine(WaitAtEnd());
                }
                break;

            case State.TeleportedToStart:
                // Wait for other car to reach halfway
                if (otherCar != null &&
                    Vector3.Distance(otherCar.startPoint.position, otherCar.transform.position) >= otherCarHalfDistance)
                {
                    currentState = State.MovingToEnd;
                    currentTarget = endPoint.position;
                }
                break;
        }
    }

    private void Seek(Vector3 targetPosition)
    {
        // Calculate desired velocity
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        // If within slowing radius, slow down
        if (distance <= slowingRadius)
        {
            desiredVelocity = toTarget.normalized * maxSpeed * (distance / slowingRadius);
        }
        else
        {
            desiredVelocity = toTarget.normalized * maxSpeed;
        }

        // Calculate steering force
        steeringForce = desiredVelocity - rb.linearVelocity;
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        // Apply steering force
        rb.AddForce(steeringForce, ForceMode.Acceleration);

        // Clamp velocity
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private IEnumerator WaitAtEnd()
    {
        yield return new WaitForSeconds(waitTime);

        // Teleport to start point
        rb.position = startPoint.position;
        transform.position = startPoint.position;
        rb.linearVelocity = Vector3.zero;

        // Orient towards end point
        OrientTowards(endPoint.position - startPoint.position);

        currentState = State.TeleportedToStart;
    }

    private void OrientTowards(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            direction.y = 0; // Maintain horizontal orientation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(startPoint.position, 0.3f);
            Gizmos.DrawSphere(endPoint.position, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }

        // Draw steering vectors
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + desiredVelocity);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + rb.linearVelocity);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + steeringForce);
        }
    }
}