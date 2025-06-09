using UnityEngine;
using System.Collections;
using TMPro;

public class Taiyoken : MonoBehaviour
{
    public float stunDuration = 3f;
    public float effectRange = 10f;
    public string enemyTag = "Enemy";
    public GameObject taiyokenVisualEffect;
    public float visualEffectDuration = 1.5f;
    public float cooldown = 10f;
    public float visualEffectYPosition = -4.94f;
    public TMP_Text cooldownText;
    public int maxCharges = 2; // Máximo de usos

    private int currentCharges; // Usos actuales
    private float remainingCooldown;
    private bool isCooldownActive;

    void Start()
    {
        currentCharges = maxCharges; // Inicializar con todos los usos disponibles
        UpdateChargeText();
    }

    public void ActivateTaiyoken()
    {
        // Solo activar si hay cargas y no está en cooldown
        if (currentCharges <= 0 || isCooldownActive) return;

        Debug.Log("¡TAIYOKEN activado por el personaje!");
        currentCharges--;

        // Visual effect
        if (taiyokenVisualEffect != null)
        {
            Vector3 effectPosition = new Vector3(transform.position.x, visualEffectYPosition, transform.position.z);
            GameObject effectInstance = Instantiate(taiyokenVisualEffect, effectPosition, Quaternion.identity);
            Destroy(effectInstance, visualEffectDuration);
        }

        // Aplicar stun a enemigos
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, effectRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(enemyTag))
            {
                EnemyController enemyController = hitCollider.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.StartStun(stunDuration);
                }
                else
                {
                    Debug.LogWarning($"Objeto {hitCollider.name} no tiene EnemyController");
                }
            }
        }

        // Iniciar cooldown solo si quedan cargas disponibles
        if (currentCharges > 0)
        {
            StartCoroutine(CooldownCoroutine());
        }

        UpdateChargeText();
    }

    IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        remainingCooldown = cooldown;

        // Mostrar cooldown
        while (remainingCooldown > 0)
        {
            if (cooldownText != null)
            {
                cooldownText.text = $"Flash Cooldown: {Mathf.CeilToInt(remainingCooldown)} s";
            }
            remainingCooldown -= Time.deltaTime;
            yield return null;
        }

        // Finalizar cooldown (sin recargar cargas)
        isCooldownActive = false;
        UpdateChargeText();
    }

    void UpdateChargeText()
    {
        if (cooldownText == null) return;

        if (isCooldownActive && currentCharges > 0)
        {
            // Durante cooldown se muestra el tiempo (ya actualizado en la corrutina)
        }
        else
        {
            // Mostrar cargas disponibles
            cooldownText.text = $"Flash Charges: {currentCharges}/{maxCharges}";
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectRange);
    }
}