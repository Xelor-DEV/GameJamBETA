using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    [Tooltip("El Transform del personaje que la cámara debe seguir.")]
    public Transform target;

    [Tooltip("El desfase (offset) que la cámara mantendrá con respecto al personaje. Ajusta esto en el Inspector para definir la distancia y el ángulo de la cámara.")]
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Valores iniciales de ejemplo para una vista isométrica

    [Tooltip("Velocidad de suavizado del seguimiento. Un valor más alto significa un seguimiento más rápido y menos suave.")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f;


    // Si quieres que el offset se calcule automáticamente al inicio basado en la posición inicial de la cámara y el target:
    // private bool useInitialOffset = true; // Cambia a false si prefieres definir el offset manualmente en el Inspector

    void Start()
    {
        // if (target != null && useInitialOffset)
        // {
        //     offset = transform.position - target.position;
        // }
        // else if (target == null)
        // {
        //     Debug.LogError("IsometricCameraFollow: No se ha asignado un 'target' (personaje) para seguir.", this);
        //     enabled = false; // Desactiva el script si no hay target
        // }
        if (target == null)
        {
            Debug.LogError("IsometricCameraFollow: No se ha asignado un 'target' (personaje) para seguir.", this);
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Posición deseada de la cámara
        Vector3 desiredPosition = target.position + offset;

        // Interpolar suavemente hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Asegurarse de que la cámara siempre mire al personaje (o un punto ligeramente por encima/delante de él)
        // Esto es opcional y depende de si tu rotación isométrica ya está fija.
        // Si tu cámara ya tiene la rotación isométrica fija (ej. X:30, Y:45, Z:0),
        // no necesitas que mire directamente al target cada frame, ya que el offset define la vista.
        // transform.LookAt(target); // Descomenta si quieres que la cámara reoriente su 'forward' hacia el target.
                                  // ¡CUIDADO! Esto puede anular tu rotación isométrica fija si no se maneja bien.
                                  // Es más común para cámaras isométricas tener una rotación fija y solo mover la posición.
    }

    // Opcional: Para visualizar el offset en el editor
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 desiredPosition = target.position + offset;
            Gizmos.DrawLine(target.position, desiredPosition);
            Gizmos.DrawSphere(desiredPosition, 0.5f);
        }
    }
}