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
            LogicaNivel1 l1 = Object.FindFirstObjectByType<LogicaNivel1>();
            if (l1 != null) l1.AccionHuertoSalida();

            LogicaNivel2 l2 = Object.FindFirstObjectByType<LogicaNivel2>();
            if (l2 != null) l2.AccionEnLetrero("SalidaHuerto", this.gameObject);
        }
    }
}