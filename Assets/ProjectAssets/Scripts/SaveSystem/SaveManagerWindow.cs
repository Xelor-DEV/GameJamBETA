#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveSystemEditorWindow : EditorWindow
{
    [SerializeField] private AudioConfigData audioData;
    [SerializeField] private GameSettingsData settingsData;

    private string audioPath;
    private string settingsPath;
    private string statusMessage = "";
    private MessageType statusType = MessageType.None;
    private bool showOverwriteWarning = false;
    private Vector2 scrollPosition;
    private int currentTab = 0;

    [MenuItem("Tools/Save System Manager")]
    public static void ShowWindow()
    {
        GetWindow<SaveSystemEditorWindow>("Save Manager");
    }

    private void OnEnable()
    {
        audioPath = Application.persistentDataPath + "/audioConfig.dat";
        settingsPath = Application.persistentDataPath + "/gameSettings.dat";

        InitializeData();
        TryLoadFiles();
    }

    private void OnDisable()
    {
        showOverwriteWarning = false;
        statusMessage = "";
        statusType = MessageType.None;
    }

    private void InitializeData()
    {
        audioData = new AudioConfigData();
        settingsData = new GameSettingsData();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Space(10);

        // Mostrar rutas de archivos
        EditorGUILayout.LabelField("Rutas de archivos:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Audio Config: {audioPath}");
        EditorGUILayout.LabelField($"Game Settings: {settingsPath}");

        GUILayout.Space(20);

        // Pestañas
        currentTab = GUILayout.Toolbar(currentTab, new string[] { "Configuración de Audio", "Configuración del Juego" });
        GUILayout.Space(10);

        switch (currentTab)
        {
            case 0:
                DrawAudioConfigTab();
                break;
            case 1:
                DrawGameSettingsTab();
                break;
        }

        GUILayout.Space(20);
        DrawActionButtons();
        GUILayout.Space(20);

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, statusType);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawAudioConfigTab()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Volúmenes", EditorStyles.boldLabel);
        audioData.masterVolume = EditorGUILayout.Slider("Volumen Principal", audioData.masterVolume, 0.001f, 1f);
        audioData.musicVolume = EditorGUILayout.Slider("Volumen Música", audioData.musicVolume, 0.001f, 1f);
        audioData.sfxVolume = EditorGUILayout.Slider("Volumen Efectos", audioData.sfxVolume, 0.001f, 1f);

        if (EditorGUI.EndChangeCheck())
        {
            statusMessage = "Cambios en audio realizados (no guardados)";
            statusType = MessageType.Info;
        }
    }

    private void DrawGameSettingsTab()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Preferencias", EditorStyles.boldLabel);
        settingsData.skipIntroCutscene = EditorGUILayout.Toggle("Saltar Cinemática", settingsData.skipIntroCutscene);
        settingsData.fullscreen = EditorGUILayout.Toggle("Pantalla Completa", settingsData.fullscreen);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Resolución", EditorStyles.boldLabel);
        settingsData.resolutionWidth = EditorGUILayout.IntField("Ancho", settingsData.resolutionWidth);
        settingsData.resolutionHeight = EditorGUILayout.IntField("Alto", settingsData.resolutionHeight);
        settingsData.refreshRate = EditorGUILayout.IntSlider("Tasa de Refresco", settingsData.refreshRate, 30, 240);

        if (EditorGUI.EndChangeCheck())
        {
            statusMessage = "Cambios en configuración realizados (no guardados)";
            statusType = MessageType.Info;
        }
    }

    private void DrawActionButtons()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Cargar Archivos", GUILayout.Height(30)))
        {
            TryLoadFiles();
        }

        if (GUILayout.Button("Guardar Cambios", GUILayout.Height(30)))
        {
            SaveFiles();
        }

        if (GUILayout.Button("Crear Nuevos", GUILayout.Height(30)))
        {
            if (File.Exists(audioPath) || File.Exists(settingsPath))
            {
                showOverwriteWarning = true;
                statusMessage = "¡Advertencia! Los archivos ya existen. ¿Sobreescribir?";
                statusType = MessageType.Warning;
            }
            else
            {
                CreateNewFiles();
            }
        }

        GUILayout.EndHorizontal();

        if (showOverwriteWarning)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("¡Archivos existentes! Se perderán los datos actuales.", MessageType.Warning);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirmar Sobreescritura"))
            {
                CreateNewFiles();
                showOverwriteWarning = false;
            }
            if (GUILayout.Button("Cancelar"))
            {
                showOverwriteWarning = false;
                statusMessage = "Operación cancelada";
                statusType = MessageType.Info;
            }
            GUILayout.EndHorizontal();
        }
    }

    private void TryLoadFiles()
    {
        try
        {
            AudioConfigData loadedAudio = SaveManager.LoadAudioConfig();
            if (loadedAudio != null)
            {
                audioData = loadedAudio;
                statusMessage = "Configuración de audio cargada!";
                statusType = MessageType.Info;
            }
            else
            {
                statusMessage = "Archivo de audio no encontrado";
                statusType = MessageType.Warning;
            }

            GameSettingsData loadedSettings = SaveManager.LoadGameSettings();
            if (loadedSettings != null)
            {
                settingsData = loadedSettings;
                statusMessage += "\nConfiguración del juego cargada!";
            }
            else
            {
                statusMessage += "\nArchivo de configuración no encontrado";
            }
        }
        catch (Exception e)
        {
            statusMessage = $"Error al cargar: {e.Message}";
            statusType = MessageType.Error;
        }

        Repaint();
    }

    private void SaveFiles()
    {
        try
        {
            SaveManager.SaveAudioConfig(audioData);
            SaveManager.SaveGameSettings(settingsData);
            statusMessage = "¡Cambios guardados correctamente!";
            statusType = MessageType.Info;
        }
        catch (Exception e)
        {
            statusMessage = $"Error al guardar: {e.Message}";
            statusType = MessageType.Error;
        }
    }

    private void CreateNewFiles()
    {
        try
        {
            audioData = new AudioConfigData();
            settingsData = new GameSettingsData();
            SaveManager.SaveAudioConfig(audioData);
            SaveManager.SaveGameSettings(settingsData);
            statusMessage = "¡Archivos nuevos creados!";
            statusType = MessageType.Info;
            showOverwriteWarning = false;
        }
        catch (Exception e)
        {
            statusMessage = $"Error al crear: {e.Message}";
            statusType = MessageType.Error;
        }

        Repaint();
    }
}
#endif