using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_SettingsWindow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private AudioSliders audioSliders;
    
    [Header("Config")]
    [SerializeField] private int windowIndex;

    [Header("Resolution Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Toggle Settings")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle skipCutsceneToggle;

    private List<Resolution> availableResolutions = new List<Resolution>();

    private void Start()
    {
        InitializeResolutionDropdown();
    }

    private void InitializeResolutionDropdown()
    {
        // Obtener todas las resoluciones disponibles con RefreshRate
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        // Filtrar resoluciones únicas usando RefreshRate
        HashSet<string> uniqueResolutions = new HashSet<string>();
        int currentResolutionIndex = 0;
        List<string> options = new List<string>();
        availableResolutions.Clear();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            // Usar RefreshRate en lugar del campo obsoleto
            uint refreshRateValue = (uint)allResolutions[i].refreshRateRatio.value;
            string option = $"{allResolutions[i].width} x {allResolutions[i].height} @ {refreshRateValue}Hz";

            if (uniqueResolutions.Add(option))
            {
                options.Add(option);
                availableResolutions.Add(allResolutions[i]);

                // Comprobar si es la resolución actual usando RefreshRate
                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height &&
                    refreshRateValue == (uint)Screen.currentResolution.refreshRateRatio.value)
                {
                    currentResolutionIndex = availableResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void LoadCurrentSettings()
    {
        GameSettingsData settingsData = SaveManager.LoadGameSettings();
        if (settingsData != null)
        {
            gameSettings.LoadFromData(settingsData);
        }

        AudioConfigData audioData = SaveManager.LoadAudioConfig();
        if (audioData != null)
        {
            audioConfig.LoadFromData(audioData);
        }

        // Actualizar UI con los valores de los ScriptableObjects
        fullscreenToggle.isOn = gameSettings.fullscreen;
        skipCutsceneToggle.isOn = gameSettings.skipIntroCutscene!;

        // Buscar la resolución guardada en las disponibles
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            uint refreshRateValue = (uint)availableResolutions[i].refreshRateRatio.value;

            if (availableResolutions[i].width == gameSettings.resolutionWidth &&
                availableResolutions[i].height == gameSettings.resolutionHeight &&
                refreshRateValue == gameSettings.refreshRate)
            {
                resolutionDropdown.value = i;
                break;
            }
        }

        audioSliders.LoadAudioSettings();
    }

    // Método público para guardar todas las configuraciones
    public void SaveAllSettings()
    {
        // Actualizar GameSettings con los valores de la UI
        Resolution selectedRes = availableResolutions[resolutionDropdown.value];

        gameSettings.resolutionWidth = selectedRes.width;
        gameSettings.resolutionHeight = selectedRes.height;
        gameSettings.refreshRate = (int)selectedRes.refreshRateRatio.value;
        gameSettings.fullscreen = fullscreenToggle.isOn;
        gameSettings.skipIntroCutscene = skipCutsceneToggle.isOn;

        // Guardar ambas configuraciones (los ScriptableObjects ya están actualizados)
        SaveSystemManager.Instance.SaveAllConfigurations();

        GameSettingsData settingsData = SaveManager.LoadGameSettings();
        if (settingsData != null)
        {
            gameSettings.LoadFromData(settingsData);
        }

        AudioConfigData audioData = SaveManager.LoadAudioConfig();
        if (audioData != null)
        {
            audioConfig.LoadFromData(audioData);
        }

        SaveSystemManager.Instance.ApplyGraphicsSettings(gameSettings.ToData());
        audioSliders.LoadAudioSettings();
    }
}