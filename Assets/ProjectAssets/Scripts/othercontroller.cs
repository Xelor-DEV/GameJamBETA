using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class othercontroller : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 120f;

    private Rigidbody rb;
    private Vector3 movement;
    private float rotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevenir rotaciones no deseadas
    }

    void Update()
    {
        // Entrada de teclado en Update
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calcular movimiento y rotación
        movement = transform.forward * vertical * moveSpeed;
        rotation = horizontal * rotationSpeed;
    }

    void FixedUpdate()
    {
        // Aplicar movimiento en FixedUpdate (física)
        MoveCharacter();
        RotateCharacter();
    }

    void MoveCharacter()
    {
        // Convertir movimiento a velocidad
        Vector3 targetVelocity = movement;
        targetVelocity.y = rb.linearVelocity.y; // Mantener velocidad vertical original

        // Aplicar movimiento suavizado
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);
    }

    void RotateCharacter()
    {
        // Calcular rotación
        float rotationAngle = rotation * Time.fixedDeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0, rotationAngle, 0);

        // Aplicar rotación
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}