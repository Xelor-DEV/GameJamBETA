using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    public float waitTimeAtPoint = 1.5f;

    [Header("Chase Settings")]
    public float detectionRange = 8f;
    public float chaseSpeed = 6f;
    public float chaseRange = 15f;
    public float rotationSpeed = 5f;

    [Header("Attack Settings")]
    public float pushForce = 10f;
    public float pushHeight = 2f;
    public float attackCooldown = 2f;

    [Header("References")]
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Transform player;
    private Rigidbody rb;
    private int currentPatrolIndex = 0;
    private bool isChasing = false;
    private bool isWaiting = false;
    private bool canAttack = true;
    private bool isStunned = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag(playerTag).transform;

        // Configuración inicial
        agent.speed = patrolSpeed;
        SetNextPatrolPoint();
    }

    void Update()
    {
        if (isStunned) return;

        // Manejo de patrulla y persecución
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
            CheckForPlayer();
        }
    }

    void Patrol()
    {
        if (isWaiting || patrolPoints.Length == 0) return;

        // Verificar si ha llegado al punto de patrulla
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtPoint);
        SetNextPatrolPoint();
        isWaiting = false;
    }

    void SetNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    void CheckForPlayer()
    {
        // Detección del jugador usando OverlapSphere
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(playerTag))
            {
                StartChase();
                return;
            }
        }
    }

    void StartChase()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
    }

    void ChasePlayer()
    {
        // Perseguir al jugador
        agent.SetDestination(player.position);

        // Rotación suave hacia el jugador
        FacePlayer();

        // Verificar si el jugador se ha escapado
        if (Vector3.Distance(transform.position, player.position) > chaseRange)
        {
            StopChase();
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void StopChase()
    {
        isChasing = false;
        agent.speed = patrolSpeed;

        // Volver a patrullar
        SetNextPatrolPoint();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Empujar al jugador al colisionar
        if (collision.gameObject.CompareTag(playerTag) && canAttack && !isStunned)
        {
            ApplyPushForce(collision.gameObject);
            StartCoroutine(AttackCooldown());
        }
    }

    void ApplyPushForce(GameObject player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Calcular dirección del empuje
            Vector3 pushDirection = player.transform.position - transform.position;
            pushDirection.y = 0; // Mantener dirección horizontal
            pushDirection.Normalize();

            // Aplicar fuerza con componente vertical
            Vector3 forceVector = pushDirection * pushForce;
            forceVector.y = pushHeight;

            playerRb.AddForce(forceVector, ForceMode.Impulse);
        }
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Método para ser aturdido por el Taiyoken
    public void Stun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(duration);

        agent.isStopped = false;
        isStunned = false;
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de persecución
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Ruta de patrulla
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }
}