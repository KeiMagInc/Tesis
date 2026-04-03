using UnityEngine;

public class ZonaPlantado : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    public string tipoDeSemillaPermitida; // Escribe en Unity: Trigo, Papas o Zanahoria
    public bool estaOcupada = false;

    private void OnDrawGizmos()
    {
        if (!estaOcupada)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, new Vector3(3, 3, 0));
        }
    }
}