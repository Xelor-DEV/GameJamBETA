using System;

public static class GameEventsManager
{
    public static event Action OnRequestControlPersonaje;
    public static void RequestControlPersonaje()
    {
        OnRequestControlPersonaje?.Invoke();
    }

    public static event Action OnRequestControlPlataforma;
    public static void RequestControlPlataforma()
    {
        OnRequestControlPlataforma?.Invoke();
    }
}