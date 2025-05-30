using UnityEngine;
using UnityEngine.Video;

public class IntroCutsceneController : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private VideoPlayer introCinematic;
    [SerializeField] private string nextSceneName = "MainMenu"; // Nombre de la siguiente escena

    void Start()
    {
        if (gameSettings.skipIntroCutscene == true)
        {
            SkipCutscene();
        }
        else
        {
            StartCutscene();
        }
    }

    private void SkipCutscene()
    {
        introCinematic.Stop();
        Debug.Log("Skipping intro cutscene");
        LoadNextScene();
    }

    private void StartCutscene()
    {
        Debug.Log("Starting intro cutscene");

        // Configurar eventos
        introCinematic.loopPointReached += EndReached;
        introCinematic.prepareCompleted += Prepared;

        // Preparar video (reproduce automáticamente al estar listo)
        introCinematic.Prepare();
    }

    private void Prepared(VideoPlayer source)
    {
        // Iniciar reproducción cuando esté preparado
        introCinematic.Play();
    }

    private void EndReached(VideoPlayer source)
    {
        Debug.Log("Cutscene finished");
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneLoader.Instance.LoadScene(nextSceneName);
    }
}