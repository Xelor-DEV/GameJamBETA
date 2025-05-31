using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public bool isStunned = false;
    private float stunTimer = 0f;

    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 3f;
    private Transform currentTarget;

    // Guarda aquí referencias a componentes que controlan el comportamiento del enemigo
    // Ejemplo: private UnityEngine.AI.NavMeshAgent agent;
    // Ejemplo: private Animator animator;
    // Ejemplo: private EnemyAttack enemyAttackScript;

    void Awake()
    {
        // Obtén referencias a los componentes de tu enemigo aquí
        // agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        // animator = GetComponent<Animator>();
        // enemyAttackScript = GetComponent<EnemyAttack>();

        if (pointA != null)
        {
            currentTarget = pointA; // Empezar moviéndose hacia el punto A
        }
        else if (pointB != null)
        {
            currentTarget = pointB;
        }
        else
        {
            Debug.LogError("Los puntos de patrulla A y B no están asignados en el enemigo: " + gameObject.name);
            // Desactivar el movimiento si no hay puntos
        }
    }

    void Update()
    {
        if (isStunned)
        {
            // Lógica cuando está aturdido (generalmente, no hacer nada o animación de aturdimiento)
            // Por ejemplo, detener el NavMeshAgent:
            // if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            return; // No hacer nada más si está aturdido
        }

        // Lógica normal del enemigo (movimiento, ataque, etc.)
        // Asegúrate de que esta lógica solo se ejecute si no está aturdido.
        // Por ejemplo, reactivar el NavMeshAgent si se detuvo:
        // if (agent != null && agent.isOnNavMesh && agent.isStopped) agent.isStopped = false;

        // Movimiento de patrulla simple
        if (currentTarget != null)
        {
            MoveTowardsTarget();
        }
        else
        {
            // Aquí iría tu IA normal si no hay patrulla o si la patrulla es solo una parte
            // Debug.Log(gameObject.name + " está activo pero sin target de patrulla.");
        }
    }

    void MoveTowardsTarget()
    {
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            // Ha llegado al punto, cambiar de objetivo
            if (currentTarget == pointA && pointB != null)
            {
                currentTarget = pointB;
            }
            else if (currentTarget == pointB && pointA != null)
            {
                currentTarget = pointA;
            }
            // Si solo hay un punto asignado, se quedará quieto después de alcanzarlo.
        }
        else
        {
            // Moverse hacia el objetivo
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            // Opcional: hacer que el enemigo mire hacia el objetivo
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }


    public void Stun(float duration)
    {
        if (isStunned) // Si ya está aturdido, podrías reiniciar el timer o ignorar
        {
            // Opcional: reiniciar duración si se aturde de nuevo mientras ya está aturdido
            // stunTimer = duration;
            // Debug.Log(gameObject.name + " ya estaba aturdido, se reinicia duración a: " + duration + "s");
            return;
        }
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        Debug.Log(gameObject.name + " ATURDIDO por " + duration + " segundos!");

        // Aquí desactivas los componentes de IA del enemigo:
        // if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        // if (animator != null) animator.speed = 0; // O cambia a una animación de "aturdido"
        // if (enemyAttackScript != null) enemyAttackScript.enabled = false;
        // También podrías cambiar el color, añadir un efecto de partículas sobre el enemigo, etc.

        // Para el movimiento simple, no necesitamos desactivar nada más explícitamente
        // porque el Update() ya verifica isStunned.

        while (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            yield return null;
        }

        isStunned = false;
        Debug.Log(gameObject.name + " ya NO está aturdido.");

        // Aquí reactivas los componentes de IA del enemigo:
        // if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        // if (animator != null) animator.speed = 1;
        // if (enemyAttackScript != null) enemyAttackScript.enabled = true;
        // Restaurar color, quitar efectos, etc.
    }
}