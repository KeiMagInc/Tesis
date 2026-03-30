using UnityEngine;

public class TriggerHuerto : MonoBehaviour
{
    private bool estaCerca = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) estaCerca = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) estaCerca = false;
    }

    void Update()
    {
        if (estaCerca && Input.GetKeyDown(KeyCode.E))
        {
            // CAMBIO AQUÍ: Llamamos a AccionHuertoEntrada
            Object.FindFirstObjectByType<LogicaNivel1>().AccionHuertoEntrada();
        }
    }
}