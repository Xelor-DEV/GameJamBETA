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
        base.Awake();
        if (playerInputPlataforma == null)
            playerInputPlataforma = GetComponent<PlayerInput>();
        GameEventsManager.OnRequestControlPlataforma += HandleRequestControlPlataforma;
    }

    void OnDestroy()
    {
        GameEventsManager.OnRequestControlPlataforma -= HandleRequestControlPlataforma;
    }

    void OnEnable()
    {
        // Elimina la llamada masiva para evitar lag al iniciar
        // if (rb != null && rb.isKinematic)
        //     LiberarPlataformaYBloques();
    }

    void OnDisable()
    {
        moveInputPlataforma = Vector2.zero;
    }

    private void HandleRequestControlPlataforma()
    {
        // Solo aquí se libera la plataforma y los bloques
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
                bloqueRb.isKinematic = true;
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
                    bloqueRb.isKinematic = true;
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