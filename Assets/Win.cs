using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    [Header("Configuraci�n")]
    [Tooltip("Tiempo m�nimo en minutos para ganar")]
    public float tiempoMinimoVictoriaMinutos = 1; // Tiempo en minutos

    [Header("UI")]
    public TMP_Text textoCronometro;
    public TMP_Text textoResultado;
    public GameObject Panel;
    public Image fondoFinal; // Nueva referencia a la imagen de fondo
    public Sprite spriteFinalBueno; // Sprite para final bueno
    public Sprite spriteFinalMalo;  // Sprite para final malo

    private float tiempoTranscurrido;
    private bool juegoActivo = false; // Inicia desactivado
    private bool victoria = false;
    private float tiempoMinimoSegundos; // Tiempo m�nimo convertido a segundos

    private void Start()
    {
        Panel.SetActive(false);
        fondoFinal.gameObject.SetActive(false); // Desactivar fondo al inicio
        textoCronometro.text = ""; // Texto vac�o inicialmente

        // Convertir minutos a segundos
        tiempoMinimoSegundos = tiempoMinimoVictoriaMinutos * 60;
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
        if (other.tag == "Player" && juegoActivo)
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

    // Nuevo m�todo para iniciar el juego
    public void IniciarJuego()
    {
        juegoActivo = true;
        tiempoTranscurrido = 0f;
    }

    void FinalizarJuego()
    {
        juegoActivo = false;

        // Nueva l�gica de victoria
        DraggableItem[] allItems = Object.FindObjectsByType<DraggableItem>(FindObjectsSortMode.None);
        int itemsNotOnGround = 0;

        foreach (DraggableItem item in allItems)
        {
            // Solo contar items que pueden ser arrastrados
            if (!item.IsOnGround())
            {
                itemsNotOnGround++;
            }
        }

        // Condici�n combinada: tiempo m�nimo Y al menos 4 items no en suelo
        victoria = (tiempoTranscurrido <= tiempoMinimoSegundos) && (itemsNotOnGround >= 4);

        Panel.SetActive(true);
        fondoFinal.gameObject.SetActive(true);

        // Cambiar sprite seg�n resultado
        fondoFinal.sprite = victoria ? spriteFinalBueno : spriteFinalMalo;

        // Actualizar texto con informaci�n adicional
        textoResultado.text = victoria ?
            $"You Win! Time: {tiempoTranscurrido:F2}s\n{itemsNotOnGround} items removed from ground" :
            $"You Lose!\nTime: {tiempoTranscurrido:F2}s ({(tiempoTranscurrido > tiempoMinimoSegundos ? "Over time limit" : "Within time")})\n" +
            $"Items removed: {itemsNotOnGround}/4 required";

        textoResultado.color = victoria ? Color.green : Color.red;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
        }

        Time.timeScale = 0;
    }

    public void mainmenu()
    {
        Time.timeScale = 1;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnpauseMusic();
        }
        SceneManager.LoadScene("StartCinematic");
    }
}