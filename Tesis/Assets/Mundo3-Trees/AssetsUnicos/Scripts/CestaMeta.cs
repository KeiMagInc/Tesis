using UnityEngine;

public class CestaMeta : MonoBehaviour
{
    [Header("Configuración de la Cesta")]
    public ControladorNodo.TipoDeNodo tipoQueAcepto; // Raiz, Padre o Hijo

    // Se ejecuta cuando algo entra en el Collider (debe tener Is Trigger marcado)
    void OnTriggerStay2D(Collider2D otro) {
        // Buscamos si el objeto que entró tiene el script de Nodo
        ControladorNodo nodo = otro.GetComponent<ControladorNodo>();

        // Si soltamos el ratón justo encima de la cesta
        if (nodo != null && !Input.GetMouseButton(0)) {
            if (nodo.miTipo == tipoQueAcepto) {
                Debug.Log("<color=green>¡Correcto!</color> Nodo clasificado.");
                otro.gameObject.SetActive(false); // El nodo "entra" al cofre
                // Aquí podrías poner un sonido de moneda o victoria
            } else {
                Debug.Log("<color=red>Incorrecto.</color> Ese nodo no va aquí.");
                // El script del nodo lo regresará automáticamente al soltar
            }
        }
    }
}