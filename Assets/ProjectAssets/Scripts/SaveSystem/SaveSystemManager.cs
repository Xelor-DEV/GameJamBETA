using UnityEngine;

public class SaveSystemManager : MonoBehaviour
{
    public static SaveSystemManager Instance { get; private set; }

    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private GameSettings gameSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Guarda ambas configuraciones simult�neamente
    public void SaveAllConfigurations()
    {
        SaveAudioConfiguration();
        SaveGameSettingsConfiguration();
    }

    // Guarda solo la configuraci�n de audio
    public void SaveAudioConfiguration()
    {
        if (audioConfig != null)
        {
            SaveManager.SaveAudioConfig(audioConfig.ToData());
            Debug.Log("Configuraci�n de audio guardada correctamente");
        }
        else
        {
            Debug.LogError("AudioConfig reference is missing in SaveSystemManager");
        }
    }

    // Guarda solo la configuraci�n del juego
    public void SaveGameSettingsConfiguration()
    {
        if (gameSettings != null)
        {
            SaveManager.SaveGameSettings(gameSettings.ToData());
            Debug.Log("Configuraci�n del juego guardada correctamente");
        }
        else
        {
            Debug.LogError("GameSettings reference is missing in SaveSystemManager");
        }
    }

    public void ApplyGraphicsSettings(GameSettingsData settings)
    {
        RefreshRate refreshRate = new RefreshRate();
        refreshRate.numerator = (uint)settings.refreshRate;
        refreshRate.denominator = 1;

        FullScreenMode fullscreenMode = new FullScreenMode();

        if (settings.fullscreen == true)
        {
            fullscreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            fullscreenMode = FullScreenMode.Windowed;
        }

        Screen.SetResolution(settings.resolutionWidth, settings.resolutionHeight, fullscreenMode, refreshRate);
        Debug.Log($"Configuraci�n gr�fica aplicada: {settings.resolutionWidth}x{settings.resolutionHeight} " + $"{fullscreenMode} @ {settings.refreshRate}Hz");
    }
}