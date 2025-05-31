using UnityEngine;

public class BloqueInteractionHandler : MonoBehaviour
{
    private MovementBloque bloqueActual;
    private bool interactMode = false;

    public bool IsDragging => bloqueActual != null && bloqueActual.IsBeingDragged;
    public bool IsInInteractMode => interactMode;

    public void PrepareBloque(GameObject bloqueGO)
    {
        if (bloqueGO == null) return;

        MovementBloque mb = bloqueGO.GetComponent<MovementBloque>();
        if (mb != null)
        {
            if (bloqueActual != null && bloqueActual != mb)
            {
                bloqueActual.ResetState();
            }

            bloqueActual = mb;
            interactMode = true;
            bloqueActual.PrepareForDrag();
            Debug.Log($"Bloque '{bloqueGO.name}' preparado para interacción.");
        }
        else
        {
            Debug.LogWarning($"El objeto '{bloqueGO.name}' no tiene el script MovementBloque.");
            ResetBloque();
        }
    }

    public void StartDrag()
    {
        if (interactMode && bloqueActual != null)
        {
            bloqueActual.StartDrag();
            Debug.Log($"Iniciando arrastre de '{bloqueActual.name}'.");
        }
        else
        {
            Debug.Log("Intento de StartDrag sin bloque preparado o no en modo interacción.");
        }
    }

    public void UpdateDrag()
    {
        if (IsDragging && bloqueActual != null)
        {
            bloqueActual.UpdateDrag();
        }
    }

    public void EndDrag()
    {
        if (bloqueActual != null)
        {
            bloqueActual.EndDrag();
            Debug.Log($"Finalizando arrastre de '{bloqueActual.name}'.");
            // No reseteamos interactMode aquí, el bloque sigue "seleccionado" hasta que se elija otro o se deseleccione explícitamente.
            // Si quieres que se deseleccione al soltar, mueve interactMode = false; y bloqueActual = null; aquí desde ResetBloque.
        }
        else
        {
             Debug.Log("Intento de EndDrag sin bloque actual.");
        }
    }

    public void ResetBloque()
    {
        if (bloqueActual != null)
        {
            bloqueActual.ResetState();
        }
        bloqueActual = null;
        interactMode = false;
        Debug.Log("Interacción con bloque reseteada.");
    }
}