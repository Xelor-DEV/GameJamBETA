using System;
using UnityEngine;

public static class GameEventsManager
{
    public static event Action OnRequestControlPersonaje;
    public static void RequestControlPersonaje()
    {
        Debug.Log("GameEventsManager: Solicitud de control para Personaje.");
        OnRequestControlPersonaje?.Invoke();
    }

    public static event Action OnRequestControlPlataforma;
    public static void RequestControlPlataforma()
    {
        Debug.Log("GameEventsManager: Solicitud de control para Plataforma.");
        OnRequestControlPlataforma?.Invoke();
    }
}