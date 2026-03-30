using UnityEngine;

public class TriggerHead : MonoBehaviour
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
        // Solo si está en el área y presiona E
        if (estaCerca && Input.GetKeyDown(KeyCode.E))
        {
            LogicaNivel1 gestor = Object.FindFirstObjectByType<LogicaNivel1>();
            if (gestor != null)
            {
                gestor.AccionHead(); // Esta función activa el estado 1 (manguera)
            }
        }
    }
}