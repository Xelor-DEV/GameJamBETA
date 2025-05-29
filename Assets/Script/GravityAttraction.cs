using UnityEngine;

public class GravityAttraction : MonoBehaviour
{
    [SerializeField] private Transform objetoConPeso;
    [SerializeField] private float fuerzaGravitacional = 10f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (objetoConPeso != null)
        {
            Vector3 direccion = (objetoConPeso.position - transform.position).normalized;
            float distancia = Vector3.Distance(transform.position, objetoConPeso.position);

            rb.AddForce(direccion * fuerzaGravitacional / Mathf.Pow(distancia, 2));
        }
    }
}