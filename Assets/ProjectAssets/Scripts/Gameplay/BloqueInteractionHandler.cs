using UnityEngine;

public class BloqueInteractionHandler : MonoBehaviour
{
    private MovementBloque bloqueActual;
    private bool interactMode = false;

    // Selecciona solo un bloque para manipulación
    public void PrepareBloque(GameObject bloque)
    {
        if (bloqueActual != null && bloqueActual.gameObject != bloque)
            bloqueActual.ResetState();

        bloqueActual = bloque.GetComponent<MovementBloque>();
        if (bloqueActual != null)
        {
            interactMode = true;
            bloqueActual.PrepareForDrag();
        }
        else
        {
            ResetBloque();
        }
    }

    public void ResetBloque()
    {
        if (bloqueActual != null)
            bloqueActual.ResetState();
        bloqueActual = null;
        interactMode = false;
    }

    public void StartDrag()
    {
        if (bloqueActual != null && interactMode)
            bloqueActual.StartDrag();
    }

    public void EndDrag()
    {
        if (bloqueActual != null && bloqueActual.IsBeingDragged)
            bloqueActual.EndDrag();
    }

    public void UpdateDrag()
    {
        if (bloqueActual != null && bloqueActual.IsBeingDragged)
            bloqueActual.UpdateDrag();
    }

    public bool IsDragging => bloqueActual != null && bloqueActual.IsBeingDragged;
    public bool IsInInteractMode => interactMode;
}
