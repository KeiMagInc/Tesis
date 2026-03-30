using UnityEngine;

public class TriggerHuertoSalida : MonoBehaviour
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
            // Llamamos a la función para recoger la manguera y llevarla al final
            Object.FindFirstObjectByType<LogicaNivel1>().AccionHuertoSalida();
        }
    }
}