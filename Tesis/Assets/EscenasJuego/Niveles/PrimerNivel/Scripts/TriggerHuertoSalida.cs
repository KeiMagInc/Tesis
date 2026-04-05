using UnityEngine;

public class TriggerHuertoSalida : MonoBehaviour
{
    private bool estaCerca = false;

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) estaCerca = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) estaCerca = false; }

    void Update()
    {
        if (estaCerca && Input.GetKeyDown(KeyCode.E))
        {
            // CAMBIO CLAVE: Debe decir .instancia (sin el UI al final)
            if (UIManager.instancia != null && UIManager.instancia.logicaActiva != null)
            {
                UIManager.instancia.logicaActiva.AccionEnLetrero("SalidaHuerto", gameObject);
                // Cambia "Head" por "EntradaHuerto", "SalidaHuerto" o "Null" según el trigger
            }
        }
    }
}