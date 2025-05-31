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
    }

    void OnDisable()
    {
        moveInputPlataforma = Vector2.zero;
    }

    private void HandleRequestControlPlataforma()
    {
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
        if (transform.childCount > 0) // Si el personaje es hijo
        {
            bool isPlatformMoving = moveInputPlataforma != Vector2.zero;
            var character = transform.GetComponentInChildren<ControladorPersonajeIndependiente>();
            character.animator.SetBool("isMoving", isPlatformMoving);
        }
    }
}