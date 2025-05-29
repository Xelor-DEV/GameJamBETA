using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Plataforma")]
    [SerializeField] private float fuerzaMovimiento = 10f;
    // No necesita referencia al personaje para el emparentamiento, el personaje se maneja a sí mismo.

    private Rigidbody rb;
    private Vector2 moveInputPlataforma;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController necesita un Rigidbody en el mismo GameObject.", this);
        }
        GameEventsManager.OnRequestControlPlataforma += HandleRequestControlPlataforma;
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPlataforma -= HandleRequestControlPlataforma;
    }

    void OnEnable()
    {
        Debug.Log("PlayerController: Script HABILITADO (OnEnable). Liberando plataforma y bloques.");
        LiberarPlataformaYBloques();
        // El personaje se emparentará a la plataforma a través de su propio script
        // cuando ceda el control.
    }

    void OnDisable()
    {
        Debug.Log("PlayerController: Script DESHABILITADO (OnDisable).");
        moveInputPlataforma = Vector2.zero;
        // FijarPlataformaYBloques() se llama antes de deshabilitar en OnChangeControlPlataforma
    }

    private void HandleRequestControlPlataforma()
    {
        Debug.Log("PlayerController: Recibida solicitud para tomar control de plataforma.");
        this.enabled = true;
    }

    public void OnMovePlataforma(InputAction.CallbackContext context)
    {
        if (this.enabled)
        {
            moveInputPlataforma = context.ReadValue<Vector2>();
        }
    }

    public void OnChangeControlPlataforma(InputAction.CallbackContext context)
    {
        if (context.performed && this.enabled)
        {
            Debug.Log("PlayerController: Tecla/Botón de cambio presionado. Solicitando control de Personaje.");
            FijarPlataformaYBloques(); // Fija la plataforma antes de ceder control
            GameEventsManager.RequestControlPersonaje(); // El personaje se desparentará a sí mismo
            this.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || !this.enabled) return;

        Vector3 direccionInput = new Vector3(moveInputPlataforma.x, 0, moveInputPlataforma.y);
        rb.AddForce(direccionInput.normalized * fuerzaMovimiento, ForceMode.Force);
    }

    void FijarPlataformaYBloques()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero; // Usar velocity en lugar de linearVelocity para mayor compatibilidad
            rb.angularVelocity = Vector3.zero;
        }
        Rigidbody[] todosLosRigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody bloqueRb in todosLosRigidbodies)
        {
            if (bloqueRb.gameObject.CompareTag("Bloque") && bloqueRb != rb)
            {
                bloqueRb.isKinematic = true;
                bloqueRb.linearVelocity = Vector3.zero;
                bloqueRb.angularVelocity = Vector3.zero;
            }
        }
        Debug.Log("Plataforma y bloques FIJADOS.");
    }

    void LiberarPlataformaYBloques()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        Rigidbody[] todosLosRigidbodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody bloqueRb in todosLosRigidbodies)
        {
            if (bloqueRb.gameObject.CompareTag("Bloque") && bloqueRb != rb)
            {
                bloqueRb.isKinematic = false;
            }
        }
        Debug.Log("Plataforma y bloques LIBERADOS.");
    }
}