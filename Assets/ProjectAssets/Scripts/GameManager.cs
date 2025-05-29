using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEditor.Animations;

public class GameManager : MonoBehaviour
{
    [SerializeField][ReadOnly] private bool isGamePaused = false;

    [Header("Events")]
    public UnityEvent onGamePaused;
    public UnityEvent onGameUnpaused;


    public bool IsGamePaused
    {
        get
        {
            return IsGamePaused;
            
        }
        
    }

    public void TogglePause()
    {
        if (isGamePaused == true)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        Time.timeScale = 0f;
        AudioManager.Instance.PauseAllAudio();
        onGamePaused?.Invoke();
    }

    public void UnpauseGame()
    {
        if (!isGamePaused) return;

        isGamePaused = false;
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnpauseAllAudio(true); // Con fade
        }
        onGameUnpaused?.Invoke();
    }

    private void OnDestroy()
    {
        if (isGamePaused == true)
        {
            Time.timeScale = 1f;
        }
    }
}