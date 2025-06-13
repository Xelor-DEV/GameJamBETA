using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tiempo mínimo en minutos para ganar")]
    public float tiempoMinimoVictoriaMinutos = 1;

    [Header("UI")]
    public TMP_Text textoCronometro;
    public TMP_Text textoResultado;
    public GameObject Panel;
    public Image fondoFinal;
    public Sprite spriteFinalBueno;
    public Sprite spriteFinalMalo;

    [Header("Requisitos Victoria")]
    [Tooltip("Número mínimo de objetos que deben estar fuera del suelo")]
    public int objetosRequeridosFueraSuelo = 4;

    private float tiempoTranscurrido;
    private bool juegoActivo = false;
    private bool victoria = false;
    private float tiempoMinimoSegundos;

    private void Start()
    {
        Panel.SetActive(false);
        fondoFinal.gameObject.SetActive(false);
        textoCronometro.text = "";
        tiempoMinimoSegundos = tiempoMinimoVictoriaMinutos * 60;
    }

    void Update()
    {
        if (juegoActivo)
        {
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
        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60);
        int centesimas = Mathf.FloorToInt((tiempoTranscurrido * 100) % 100);
        textoCronometro.text = $"{minutos:00}:{segundos:00}:{centesimas:00}";
    }

    public void IniciarJuego()
    {
        juegoActivo = true;
        tiempoTranscurrido = 0f;
    }

    void FinalizarJuego()
    {
        juegoActivo = false;
        DraggableItem[] allItems = Object.FindObjectsByType<DraggableItem>(FindObjectsSortMode.None);
        int itemsNotOnGround = 0;

        foreach (DraggableItem item in allItems)
        {
            if (!item.IsOnGround())
            {
                itemsNotOnGround++;
            }
        }

        // Nueva condición de victoria: tiempo mínimo + objetos requeridos fuera del suelo
        victoria = (tiempoTranscurrido <= tiempoMinimoSegundos) &&
                   (itemsNotOnGround >= objetosRequeridosFueraSuelo);

        Panel.SetActive(true);
        fondoFinal.gameObject.SetActive(true);
        fondoFinal.sprite = victoria ? spriteFinalBueno : spriteFinalMalo;

        // Actualizar texto con información adicional
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