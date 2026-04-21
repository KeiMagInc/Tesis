using UnityEngine;

public class ZonaPlantado : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    public string tipoDeSemillaPermitida;
    public bool estaOcupada = false;
    public void DesactivarColision()
    {
        Collider2D colPrincipal = GetComponent<Collider2D>();
        if (colPrincipal != null) colPrincipal.enabled = false;
        ActivarLimite(false);
    }

    public void ResetearZona()
    {
        estaOcupada = false;
        Collider2D colPrincipal = GetComponent<Collider2D>();
        if (colPrincipal != null) colPrincipal.enabled = true;
        ActivarLimite(true);
    }

    private void ActivarLimite(bool estado)
    {
        foreach (Transform hijo in transform)
        {
            if (hijo.name == "Limite")
            {
                Collider2D col = hijo.GetComponent<Collider2D>();
                if (col != null) col.enabled = estado;
            }
        }
    }
}