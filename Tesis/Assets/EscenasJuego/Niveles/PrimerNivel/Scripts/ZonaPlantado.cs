using UnityEngine;

public class ZonaPlantado : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    public string tipoDeSemillaPermitida;
    public bool estaOcupada = false;

    // Esta función desactiva el trigger de siembra Y el muro invisible llamado "Limite"
    public void DesactivarColision()
    {
        // 1. Desactiva el collider de este objeto (el área rosa/Trigger)
        Collider2D colPrincipal = GetComponent<Collider2D>();
        if (colPrincipal != null) colPrincipal.enabled = false;

        // 2. Busca específicamente al hijo llamado "Limite" y desactiva su collider
        // transform.Find busca solo entre los hijos directos por nombre
        Transform objetoLimite = transform.Find("Limite");
        if (objetoLimite != null)
        {
            Collider2D colLimite = objetoLimite.GetComponent<Collider2D>();
            if (colLimite != null)
            {
                colLimite.enabled = false;
                Debug.Log("Muro 'Limite' desactivado en " + gameObject.name);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!estaOcupada)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, new Vector3(5, 5, 0));
        }
    }
}