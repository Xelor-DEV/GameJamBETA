using UnityEngine;

public class PooledAudioSource : MonoBehaviour, IPoolable
{
    private AudioSource audioSource;
    private bool wasPaused = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnObjectSpawned()
    {
        audioSource.volume = AudioManager.Instance.AudioConfig.SfxVolume;
        audioSource.outputAudioMixerGroup = AudioManager.Instance.SfxAudioSource.outputAudioMixerGroup;
        audioSource.playOnAwake = false;
        wasPaused = false;
    }

    public void OnObjectReturned()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            wasPaused = true;
        }
    }

    public void UnPause()
    {
        if (wasPaused)
        {
            audioSource.UnPause();
            wasPaused = false;
        }
    }
}