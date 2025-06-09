using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Wander Center Reference")]
    [SerializeField] private Transform wanderCenterTransform;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private Color detectionColor = Color.red;
    [SerializeField] private Color noDetectionColor = Color.green;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolPointDistance = 5f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private Color patrolGizmoColor = Color.blue;

    [Header("Attack Settings")]
    [SerializeField] private float attackForce = 15f;
    [SerializeField] private float fleeTime = 2f;
    [SerializeField] private float destroyDelay = 1f;
    [SerializeField] private float fleeForceMultiplier = 1.5f; // Nueva variable para fuerza de huida

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform player;
    private Vector3 patrolCenter;
    private Vector3 wanderPoint;
    private bool isChasing = false;
    private bool isFleeing = false;
    private bool playerDetected = false;
    private bool isWaiting = false; // Flag para controlar estado de espera
    private bool isStunned;
    private Coroutine stunCoroutine;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        patrolCenter = wanderCenterTransform != null ?
            wanderCenterTransform.position :
            transform.position;

        GenerateNewWanderPoint();
    }

    public void StartStun(float duration)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        agent.isStopped = true; // Detiene el movimiento del NavMesh

        // Guarda el estado anterior para restaurarlo después
        bool wasChasing = isChasing;
        bool wasFleeing = isFleeing;

        // Resetea estados de movimiento
        isChasing = false;
        isFleeing = false;

        yield return new WaitForSeconds(duration);

        // Restaura el estado anterior
        agent.isStopped = false;
        isChasing = wasChasing;
        isFleeing = wasFleeing;
        isStunned = false;
    }

    private void Update()
    {
        if (isFleeing) return;

        CheckForPlayer();

        if (isChasing && player != null)
        {
            agent.SetDestination(player.position);
        }
        else if (!isChasing && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndWander());
        }
    }

    private void CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);
        playerDetected = false;

        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<PlatformMovement>() != null)
            {
                player = hit.transform;
                isChasing = true;
                playerDetected = true;
                isWaiting = false; // Resetear estado de espera
                return;
            }
        }

        isChasing = false;
    }

    private void GenerateNewWanderPoint()
    {
        if (wanderCenterTransform != null)
        {
            patrolCenter = wanderCenterTransform.position;
        }

        Vector3 randomPoint = patrolCenter + Random.insideUnitSphere * patrolRadius;
        randomPoint.y = transform.position.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolPointDistance, NavMesh.AllAreas))
        {
            wanderPoint = hit.position;
            agent.SetDestination(wanderPoint);
        }
    }

    private IEnumerator WaitAndWander()
    {
        isWaiting = true;
        yield return new WaitForSeconds(patrolWaitTime);
        GenerateNewWanderPoint();
        isWaiting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFleeing) return;

        PlatformMovement playerMovement = collision.gameObject.GetComponent<PlatformMovement>();
        if (playerMovement != null)
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 forceDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(forceDirection * attackForce, ForceMode.Impulse);
            }

            StartCoroutine(FleeAndDestroy());
        }
    }

    private IEnumerator FleeAndDestroy()
    {
        isFleeing = true;
        isChasing = false;
        playerDetected = false;

        // Deshabilitar completamente el NavMeshAgent
        agent.enabled = false;

        // Aplicar fuerza de huida con dirección aleatoria
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        fleeDirection += new Vector3(
            Random.Range(-0.5f, 0.5f),
            0,
            Random.Range(-0.5f, 0.5f)
        ).normalized * 0.5f;

        rb.AddForce(fleeDirection * attackForce * fleeForceMultiplier, ForceMode.Impulse);

        yield return new WaitForSeconds(fleeTime);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Vector3 center = wanderCenterTransform != null ?
            wanderCenterTransform.position :
            patrolCenter;

        Gizmos.color = playerDetected ? detectionColor : noDetectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = patrolGizmoColor;
        Gizmos.DrawWireSphere(center, patrolRadius);
        Gizmos.DrawSphere(wanderPoint, 0.5f);
        Gizmos.DrawLine(transform.position, wanderPoint);
    }
}