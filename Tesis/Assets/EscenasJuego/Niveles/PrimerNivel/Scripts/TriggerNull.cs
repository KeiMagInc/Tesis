using UnityEngine;

public class TriggerNull : MonoBehaviour
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
            // Buscamos el gestor del nivel y ejecutamos la acción de cierre (NULL)
            LogicaNivel1 gestor = Object.FindFirstObjectByType<LogicaNivel1>();
            if (gestor != null)
            {
                gestor.AccionNull();
            }
        }
    }
}