using UnityEngine;

public class ArrayManager : MonoBehaviour
{
    public Transform[] posicionesBalsas; 
    public ItemRecogible[] arregloLogico; 
    public int capacidad = 7;

    void Awake()
    {
        arregloLogico = new ItemRecogible[capacidad];
    }

    // Función auxiliar para contar cuántos elementos reales hay (sin contar nulos)
    public int GetCantidadActual()
    {
        int contador = 0;
        for (int i = 0; i < capacidad; i++)
        {
            if (arregloLogico[i] != null) contador++;
        }
        return contador;
    }

    public bool InsertarEn(int indiceDeseado, ItemRecogible nuevoItem)
    {
        int tamanoActual = GetCantidadActual();

        // 1. Verificar Overflow
        if (tamanoActual >= capacidad)
        {
            Debug.Log("¡OVERFLOW! El arreglo está lleno.");
            return false;
        }

        // 2. LÓGICA DE CONTIGÜIDAD:
        // Si el jugador intenta insertar en el índice 6 pero solo hay 2 elementos (0 y 1),
        // el sistema lo obligará a irse al índice 2 (la siguiente posición disponible).
        int indiceFinal = Mathf.Min(indiceDeseado, tamanoActual);

        if (indiceFinal != indiceDeseado)
        {
            Debug.Log($"Índice {indiceDeseado} no válido para mantener contigüidad. Redirigiendo a {indiceFinal}");
        }

        // 3. Lógica de SHIFTING hacia la DERECHA
        // Solo desplazamos si estamos insertando en una posición que ya está ocupada
        for (int i = capacidad - 1; i > indiceFinal; i--)
        {
            arregloLogico[i] = arregloLogico[i - 1];
            if (arregloLogico[i] != null)
            {
                arregloLogico[i].MoverA(posicionesBalsas[i].position);
            }
        }

        // 4. Insertar el nuevo elemento en el índice corregido
        arregloLogico[indiceFinal] = nuevoItem;
        nuevoItem.estaEnBalsa = true;
        nuevoItem.MoverA(posicionesBalsas[indiceFinal].position);
        
        return true;
    }

    public void EliminarDe(int indice)
    {
        if (arregloLogico[indice] == null) return;

        ItemRecogible item = arregloLogico[indice];
        item.estaEnBalsa = false;
        arregloLogico[indice] = null;

        // Shifting a la izquierda (Esto ya mantenía la contigüidad)
        for (int i = indice; i < capacidad - 1; i++)
        {
            arregloLogico[i] = arregloLogico[i + 1];
            if (arregloLogico[i] != null)
            {
                arregloLogico[i].MoverA(posicionesBalsas[i].position);
            }
        }
        arregloLogico[capacidad - 1] = null;
    }
}