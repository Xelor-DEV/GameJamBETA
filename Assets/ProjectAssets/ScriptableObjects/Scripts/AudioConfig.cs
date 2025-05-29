using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "ScriptableObjects/Audio/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [BoxGroup("Volume Settings")]
    [Range(0.001f, 1f), Tooltip("Controla el volumen de la música (0.001f = silencio, 1 = volumen máximo)")]
    [SerializeField] private float musicVolume = 0.5f;

    [BoxGroup("Volume Settings")]
    [Range(0.001f, 1f), Tooltip("Controla el volumen de los efectos de sonido (0.001f = silencio, 1 = volumen máximo)")]
    [SerializeField] private float sfxVolume = 0.5f;

    [BoxGroup("Volume Settings")]
    [Range(0.001f, 1f), Tooltip("Controla el volumen general del audio (0.001f = silencio, 1 = volumen máximo)")]
    [SerializeField] private float masterVolume = 0.5f;

    public float MusicVolume
    {
        get
        {
            return musicVolume;
        }
        set
        {
            musicVolume = value;
        }
    }
    public float SfxVolume
    {
        get
        {
            return sfxVolume;
        }
        set
        {
            sfxVolume = value;
        }
    }
    public float MasterVolume
    {
        get
        {
            return masterVolume;
        }
        set
        {
            masterVolume = value;
        }
    }
}