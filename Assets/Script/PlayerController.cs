using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : BasePlayerController
{
    [SerializeField] private float fuerzaMovimiento = 10f;
    [SerializeField] private PlayerInput playerInputPlataforma;
    [SerializeField] private PlayerInput playerInputPersonaje;

    private Vector2 moveInputPlataforma;
    [SerializeField] private List<Rigidbody> bloquesSobrePlataforma = new List<Rigidbody>();

    protected override void Awake()
{
    base.Awake(); // Obtiene this.rb
    if (playerInputPlataforma == null)
    {
        playerInputPlataforma = GetComponent<PlayerInput>(); // Intenta obtener el PlayerInput de la plataforma
    }

    if (playerInputPlataforma == null)
    {
        Debug.LogError("PlayerInput de la plataforma (playerInputPlataforma) NO ENCONTRADO en " + gameObject.name + ". El control de la plataforma no funcionará.", this);
    }
    // No se establece playerInputPlataforma.enabled aquí explícitamente.
    // Se asume que ControladorPersonajeIndependiente lo manejará si el personaje comienza agarrado,
    // o que el estado por defecto del componente PlayerInput en el Inspector es el deseado si no hay agarre inicial.

    // Lo mismo para playerInputPersonaje.enabled.
    // if (playerInputPersonaje != null)
    // {
    //     playerInputPersonaje.enabled = false; // El estado inicial del personaje lo maneja su propio script.
    // }

    GameEventsManager.OnRequestControlPlataforma += HandleRequestControlPlataforma;
}

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPlataforma -= HandleRequestControlPlataforma;
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        moveInputPlataforma = Vector2.zero;
    }

    private void HandleRequestControlPlataforma()
    {
        LiberarPlataformaYBloques();
        if (playerInputPlataforma != null) playerInputPlataforma.enabled = true;
        if (playerInputPersonaje != null) playerInputPersonaje.enabled = false;
    }

    public void OnMovePlataforma(InputAction.CallbackContext context)
    {
        if (playerInputPlataforma != null && playerInputPlataforma.enabled)
            moveInputPlataforma = context.ReadValue<Vector2>();
        else
            moveInputPlataforma = Vector2.zero;
    }

    public void OnChangeControlPlataforma(InputAction.CallbackContext context)
    {
        if (context.performed && playerInputPlataforma != null && playerInputPlataforma.enabled)
        {
            FijarPlataformaYBloques();
            if (playerInputPlataforma != null) playerInputPlataforma.enabled = false;
            if (playerInputPersonaje != null) playerInputPersonaje.enabled = true;
            GameEventsManager.RequestControlPersonaje();
        }
    }

    void FixedUpdate()
    {
        if (rb == null || playerInputPlataforma == null || !playerInputPlataforma.enabled)
            return;
        if (moveInputPlataforma != Vector2.zero && !rb.isKinematic)
        {
            Vector3 direccionInput = new Vector3(moveInputPlataforma.x, 0, moveInputPlataforma.y);
            rb.AddForce(direccionInput.normalized * fuerzaMovimiento, ForceMode.Force);
        }
    }

    void FijarPlataformaYBloques()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        foreach (Rigidbody bloqueRb in bloquesSobrePlataforma)
        {
            if (bloqueRb != null)
            {
                bloqueRb.linearVelocity = Vector3.zero;
                bloqueRb.angularVelocity = Vector3.zero;
            }
        }
    }

    void LiberarPlataformaYBloques()
    {
        if (rb != null)
            rb.isKinematic = false;
        foreach (Rigidbody bloqueRb in bloquesSobrePlataforma)
        {
            if (bloqueRb != null)
            {
                bloqueRb.isKinematic = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bloque"))
        {
            Rigidbody bloqueRb = other.GetComponent<Rigidbody>();
            if (bloqueRb == null) return;
            if (!bloquesSobrePlataforma.Contains(bloqueRb))
            {
                bloquesSobrePlataforma.Add(bloqueRb);
                if (rb != null && rb.isKinematic)
                {
                    bloqueRb.linearVelocity = Vector3.zero;
                    bloqueRb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bloque"))
        {
            Rigidbody bloqueRb = other.GetComponent<Rigidbody>();
            if (bloqueRb == null) return;
            if (bloquesSobrePlataforma.Contains(bloqueRb))
            {
                bloquesSobrePlataforma.Remove(bloqueRb);
                bloqueRb.isKinematic = false;
            }
        }
    }

    public bool EstaBloqueSobrePlataforma(Rigidbody bloqueConsultadoRb)
    {
        if (bloqueConsultadoRb == null) return false;
        return bloquesSobrePlataforma.Contains(bloqueConsultadoRb);
    }

    public List<Rigidbody> GetBloquesSobrePlataforma()
    {
        return bloquesSobrePlataforma;
    }
}