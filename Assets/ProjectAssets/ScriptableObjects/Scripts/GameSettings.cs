using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Configuration/Game Settings")]
public class GameSettings : ScriptableObject
{
    public bool skipIntroCutscene = false;
    public bool fullscreen = true;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int refreshRate = 60;

    public GameSettingsData ToData()
    {
        return new GameSettingsData
        {
            skipIntroCutscene = skipIntroCutscene,
            fullscreen = fullscreen,
            resolutionWidth = resolutionWidth,
            resolutionHeight = resolutionHeight,
            refreshRate = refreshRate
        };
    }

    public void LoadFromData(GameSettingsData data)
    {
        if (data == null) return;
        skipIntroCutscene = data.skipIntroCutscene;
        fullscreen = data.fullscreen;
        resolutionWidth = data.resolutionWidth;
        resolutionHeight = data.resolutionHeight;
        refreshRate = data.refreshRate;
    }
}

[System.Serializable]
public class GameSettingsData
{
    public bool skipIntroCutscene;
    public bool fullscreen;
    public int resolutionWidth;
    public int resolutionHeight;
    public int refreshRate;

    public GameSettingsData()
    {
        skipIntroCutscene = false;
        fullscreen = true;
        resolutionWidth = 1920;
        resolutionHeight = 1080;
        refreshRate = 60;
    }
}