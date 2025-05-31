using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Win : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float tiempoMinimoVictoria = 60; // Tiempo m�nimo para ganar

    [Header("UI")]
    public TMP_Text textoCronometro;
    public TMP_Text textoResultado;
    public GameObject Panel;

    private float tiempoTranscurrido;
    private bool juegoActivo = true;
    private bool victoria = false;

    private void Start()
    {
        Panel.SetActive(false);
    }

    void Update()
    {
        if (juegoActivo)
        {
            // Actualizar el cron�metro
            tiempoTranscurrido += Time.deltaTime;
            ActualizarCronometroUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            FinalizarJuego();
        }
    }
    void ActualizarCronometroUI()
    {
        // Formatear tiempo como minutos:segundos:cent�simas
        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60);
        int centesimas = Mathf.FloorToInt((tiempoTranscurrido * 100) % 100);

        textoCronometro.text = $"{minutos:00}:{segundos:00}:{centesimas:00}";
    }

    void FinalizarJuego()
    {
        juegoActivo = false;
        victoria = (tiempoTranscurrido >= tiempoMinimoVictoria);
        Panel.SetActive(true);

        // Mostrar resultado
        textoResultado.text = victoria ?
            $"�Ganaste! ({tiempoTranscurrido:F2}s)" :
            $"Perdiste. Necesitabas {tiempoMinimoVictoria}s ({tiempoTranscurrido:F2}s)";

        textoResultado.color = victoria ? Color.green : Color.red;

        Time.timeScale = 0;
    }

    public void mainmenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("StartCinematic");
    }
}