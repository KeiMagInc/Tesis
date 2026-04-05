using UnityEngine;

public class ZonaPlantado : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    public string tipoDeSemillaPermitida;
    public bool estaOcupada = false;

    public void DesactivarColision()
    {
        // 1. Apaga el trigger de siembra (rosa)
        Collider2D colPrincipal = GetComponent<Collider2D>();
        if (colPrincipal != null) colPrincipal.enabled = false;

        // 2. Apaga el muro invisible "Limite"
        ActivarLimite(false);
    }

    public void ResetearZona()
    {
        estaOcupada = false;

        // 1. Enciende el trigger de siembra (rosa)
        Collider2D colPrincipal = GetComponent<Collider2D>();
        if (colPrincipal != null) colPrincipal.enabled = true;

        // 2. Enciende el muro "Limite" para que Lupi no entre al pantano
        ActivarLimite(true);
    }

    private void ActivarLimite(bool estado)
    {
        // Busca al hijo llamado Limite y cambia su collider
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