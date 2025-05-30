using UnityEngine;

public class ConfigLoader : MonoBehaviour
{
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private GameSettings gameSettings;

    private void Awake()
    {
        LoadGraphicsSettings(gameSettings);
    }

    private void Start()
    {
        LoadAudioConfig(audioConfig);
    }

    private void LoadGraphicsSettings(GameSettings gameSettings)
    {
        GameSettingsData settingsData = SaveManager.LoadGameSettings();
        if (settingsData != null)
        {
            gameSettings.LoadFromData(settingsData);

            ApplyGraphicsSettings(gameSettings.ToData());
        }
        else
        {
            SaveManager.SaveGameSettings(gameSettings);
            GameSettingsData settingsDataTmp = SaveManager.LoadGameSettings();
            if (settingsDataTmp != null)
            {
                gameSettings.LoadFromData(settingsDataTmp);
                ApplyGraphicsSettings(gameSettings.ToData());
            }
        }
    }

    private void LoadAudioConfig(AudioConfig audioConfig)
    {
        AudioConfigData audioData = SaveManager.LoadAudioConfig();
        if (audioData != null)
        {
            audioConfig.LoadFromData(audioData);
        }
        else
        {
            SaveManager.SaveAudioConfig(audioConfig);
            AudioConfigData audioDataTmp = SaveManager.LoadAudioConfig();
            if (audioData != null)
            {
                audioConfig.LoadFromData(audioDataTmp);
            }
        }
    }

    private void ApplyGraphicsSettings(GameSettingsData settings)
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
    }
}