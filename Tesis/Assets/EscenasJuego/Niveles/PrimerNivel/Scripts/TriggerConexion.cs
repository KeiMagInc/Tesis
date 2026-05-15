using UnityEngine;

public class TriggerConexion : MonoBehaviour
{
    [Tooltip("Ej: Head, Null, EntradaAnterior, SalidaSiguiente, SalidaAnterior, EntradaSiguiente")]
    public string identificador;
    private bool estaCerca = false;

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) estaCerca = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) estaCerca = false; }

    void Update()
    {
        if (estaCerca && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("<color=yellow> Se presionó 'E' en el objeto: [" + gameObject.name + "] con identificador: [" + identificador + "]</color>", gameObject);

            if (UIManager.instancia != null && UIManager.instancia.logicaActiva != null)
            {
                UIManager.instancia.logicaActiva.AccionEnLetrero(identificador, gameObject);
            }
        }
    }
}