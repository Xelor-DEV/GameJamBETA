using UnityEngine;
using System.Collections;

public class Taiyoken : MonoBehaviour
{
    public float stunDuration = 3f;
    public float effectRange = 10f;
    public string enemyTag = "Enemy";
    public GameObject taiyokenVisualEffect;
    public float visualEffectDuration = 1.5f;
    public float cooldown = 10f;
    public float visualEffectYPosition = -4.94f;

    private bool canUseAbility = true;

    public void ActivateTaiyoken()
    {
        if (!canUseAbility)
        {
            Debug.Log("Taiyoken en enfriamiento.");
            return;
        }

        Debug.Log("¡TAIYOKEN activado por el personaje!");

        if (taiyokenVisualEffect != null)
        {
            Vector3 effectPosition = new Vector3(transform.position.x, visualEffectYPosition, transform.position.z);
            GameObject effectInstance = Instantiate(taiyokenVisualEffect, effectPosition, Quaternion.identity);
            Destroy(effectInstance, visualEffectDuration);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, effectRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(enemyTag))
            {
                Enemy enemyScript = hitCollider.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.Stun(stunDuration);
                }
                else
                {
                    Debug.LogWarning($"El objeto {hitCollider.name} con tag '{enemyTag}' no tiene el script 'Enemy'.");
                }
            }
        }
        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        canUseAbility = false;
        Debug.Log($"Taiyoken en enfriamiento por {cooldown} segundos.");
        yield return new WaitForSeconds(cooldown);
        canUseAbility = true;
        Debug.Log("Taiyoken listo para usarse.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectRange);
    }
}