using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [BoxGroup("Audio Mixer Settings")]
    [SerializeField, Required] private AudioMixer audioMixer;

    [BoxGroup("Audio Sources")]
    [SerializeField, Required] private AudioSource musicAudioSource;

    [BoxGroup("Audio Sources")]
    [SerializeField, Required] private AudioSource sfxAudioSource;

    [BoxGroup("Audio Clips")]
    [ReorderableList]
    [SerializeField] private AudioClip[] musicClips;

    [BoxGroup("Audio Clips")]
    [ReorderableList]
    [SerializeField] private AudioClip[] sfxClips;

    [BoxGroup("Audio Configuration")]
    [Expandable]
    [SerializeField] private AudioConfig audioConfig;

    [BoxGroup("Object Pooling")]
    [SerializeField] private ObjectPool sfxObjectPool;

    [BoxGroup("Fade Settings")]
    [Range(0.1f, 5.0f)]
    [SerializeField, Tooltip("Duracion del fade de musica en segundos")]
    private float fadeDuration = 1.0f;

    [BoxGroup("Fade Settings")]
    [ReadOnly]
    [SerializeField] private bool isFading = false;

    [BoxGroup("Pause Settings")]
    [ReadOnly]
    [SerializeField] private bool isPaused = false;

    [BoxGroup("Pause Settings")]
    [SerializeField, Tooltip("Si se pausan los efectos de sonido cuando el juego está en pausa")]
    private bool pauseSfxOnGamePause = true;

    private List<AudioSource> pausedSfxSources = new List<AudioSource>();
    private float prePauseMusicVolume;
    private float prePauseMusicTime;

    public AudioConfig AudioConfig
    {
        get
        {
            return audioConfig;
        }
    }

    public AudioSource SfxAudioSource
    {
        get
        {
            return sfxAudioSource;
        }
    }

    public bool IsPaused
    {
        get
        {
            return isPaused;
        }
        set
        {
            isPaused = value;
        }
    }

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

    // Configuracion de volumenes
    public void SetVolumeOfMusic(float newVolume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(newVolume) * 20f);
        audioConfig.MusicVolume = newVolume;
    }

    public void SetVolumeOfSfx(float newVolume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(newVolume) * 20f);
        audioConfig.SfxVolume = newVolume;
    }

    public void SetVolumeOfMaster(float newVolume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(newVolume) * 20f);
        audioConfig.MasterVolume = newVolume;
    }

    // Reproducir musica sin transicion
    public void PlayMusic(int index)
    {
        // No permitir cambio de musica si hay un fade en curso
        if (isFading == false && index >= 0 && index < musicClips.Length)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = musicClips[index];
            musicAudioSource.Play();
        }
    }

    // Reproducir musica con transicion (fade out)
    public void PlayMusicWithTransition(int index)
    {
        if (isFading == false && index >= 0 && index < musicClips.Length)
        {
            StartCoroutine(FadeOutMusicAndPlayNew(index));
        }
    }

    private IEnumerator FadeOutMusicAndPlayNew(int newMusicIndex)
    {
        isFading = true; // Se esta marcando que se inicio el fade

        // Realizar el fade out
        float currentVolume;
        audioMixer.GetFloat("MusicVolume", out currentVolume);
        float startVolume = Mathf.Pow(10f, currentVolume / 20f); // Convertir dB a valor lineal

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Lerp(startVolume, 0.001f, normalizedTime)) * 20f);
            yield return null;
        }

        // Parar la m�sica anterior y reproducir la nueva
        musicAudioSource.Stop();
        musicAudioSource.clip = musicClips[newMusicIndex];
        musicAudioSource.Play();

        // Realizar fade in de la nueva m�sica
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Lerp(0.001f, startVolume, normalizedTime)) * 20f);
            yield return null;
        }

        isFading = false; // El fade ha terminado
    }

    // Reproducir SFX usando object pooling
    public void PlaySfx(int index)
    {
        if (index < 0 || index >= sfxClips.Length) return;
        if (isPaused == true) return;

        // Usar AudioSource principal si está disponible
        if (!sfxAudioSource.isPlaying)
        {
            sfxAudioSource.PlayOneShot(sfxClips[index]);
            return;
        }

        // Obtener AudioSource del pool
        GameObject pooledObject = sfxObjectPool.RequestPoolObject();
        if (pooledObject != null && pooledObject.TryGetComponent(out AudioSource audioSource))
        {
            audioSource.PlayOneShot(sfxClips[index]);
            StartCoroutine(ReturnToPoolAfterPlay(audioSource, pooledObject));
        }
    }


    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, GameObject pooledObject)
    {
        yield return new WaitWhile(() => source.isPlaying && !isPaused);

        if (isPaused == false)
        {
            sfxObjectPool.ReturnPoolObject(pooledObject);
        }
    }

    // ========================
    // SISTEMA DE PAUSA
    // ========================

    // Pausar todo el audio (música y SFX)
    public void PauseAllAudio()
    {
        if (isPaused) return;

        isPaused = true;

        // Guardar estado de la música
        prePauseMusicTime = musicAudioSource.time;
        prePauseMusicVolume = GetMusicVolumeLinear();

        // Pausar música con fade
        StartCoroutine(FadeAndPauseMusic());

        // Pausar SFX si está configurado
        if (pauseSfxOnGamePause)
        {
            PauseAllSfx();
        }
    }

    // Despausar todo el audio
    public void UnpauseAllAudio(bool withFade = true)
    {
        if (!isPaused) return;

        isPaused = false;

        // Despausar música con o sin fade
        if (withFade)
        {
            StartCoroutine(FadeInAndUnpauseMusic());
        }
        else
        {
            musicAudioSource.Play();
            musicAudioSource.time = prePauseMusicTime;
            SetVolumeOfMusic(prePauseMusicVolume);
        }

        // Despausar SFX si estaban pausados
        if (pauseSfxOnGamePause)
        {
            UnpauseAllSfx();
        }
    }

    // Pausar solo la música
    public void PauseMusic()
    {
        if (!musicAudioSource.isPlaying || isPaused) return;

        // Guardar estado de la música
        prePauseMusicTime = musicAudioSource.time;
        prePauseMusicVolume = GetMusicVolumeLinear();

        StartCoroutine(FadeAndPauseMusic());
    }

    // Despausar solo la música
    public void UnpauseMusic(bool withFade = true)
    {
        if (musicAudioSource.isPlaying || isPaused) return;

        if (withFade)
        {
            StartCoroutine(FadeInAndUnpauseMusic());
        }
        else
        {
            musicAudioSource.Play();
            musicAudioSource.time = prePauseMusicTime;
            SetVolumeOfMusic(prePauseMusicVolume);
        }
    }

    // Pausar todos los efectos de sonido
    public void PauseAllSfx()
    {
        pausedSfxSources.Clear();

        // Pausar SFX principal
        if (sfxAudioSource.isPlaying)
        {
            sfxAudioSource.Pause();
            pausedSfxSources.Add(sfxAudioSource);
        }

        // Pausar todos los SFX del pool que estén activos
        foreach (var obj in sfxObjectPool.GetAllActiveObjects())
        {
            if (obj.TryGetComponent(out AudioSource audioSource) && audioSource.isPlaying)
            {
                audioSource.Pause();
                pausedSfxSources.Add(audioSource);
            }
        }
    }

    // Despausar todos los efectos de sonido
    public void UnpauseAllSfx()
    {
        foreach (AudioSource source in pausedSfxSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
        pausedSfxSources.Clear();
    }

    // Corutina para pausar música con fade
    private IEnumerator FadeAndPauseMusic()
    {
        // Fade out
        float currentVolume = GetMusicVolumeLinear();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            SetVolumeOfMusic(Mathf.Lerp(currentVolume, 0.001f, normalizedTime));
            yield return null;
        }

        // Pausar después del fade
        musicAudioSource.Pause();
    }

    // Corutina para despausar música con fade
    private IEnumerator FadeInAndUnpauseMusic()
    {
        // Reanudar antes del fade
        musicAudioSource.UnPause();
        musicAudioSource.time = prePauseMusicTime;

        // Fade in
        float startVolume = 0.001f;
        SetVolumeOfMusic(startVolume);

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            SetVolumeOfMusic(Mathf.Lerp(startVolume, prePauseMusicVolume, normalizedTime));
            yield return null;
        }

        SetVolumeOfMusic(prePauseMusicVolume);
    }

    // Obtener volumen de música en escala lineal
    private float GetMusicVolumeLinear()
    {
        audioMixer.GetFloat("MusicVolume", out float dbVolume);
        return Mathf.Pow(10f, dbVolume / 20f);
    }
}