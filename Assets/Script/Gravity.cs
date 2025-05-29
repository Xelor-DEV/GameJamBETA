using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

public class Gravity : MonoBehaviour
{
    [SerializeField] float peso = 1f;
    [SerializeField] Vector3 centroDeGravedad = Vector3.zero; 
    [SerializeField] private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = peso;
        rb.centerOfMass = centroDeGravedad;
    }

    public void ModificarMasa(float nuevaMasa)
    {
        peso = nuevaMasa;
        rb.mass = nuevaMasa;
    }

    public void ModificarCentroDeGravedad(Vector3 nuevoCentro)
    {
        centroDeGravedad = nuevoCentro;
        rb.centerOfMass = nuevoCentro;
    }
}