using UnityEngine;

public class MuelleTrigger : MonoBehaviour
{
    private bool activado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si Lupe entra y aún no hemos activado las misiones
        if (other.CompareTag("Player") && !activado)
        {
            activado = true;
            
            // 1. Buscamos el MisionManager e iniciamos
            MisionManager manager = Object.FindFirstObjectByType<MisionManager>();
            if (manager != null) manager.IniciarMisiones();
            
            // 2. Opcional: Desactiva el objeto para que no vuelva a ocurrir
            // gameObject.SetActive(false); 
        }
    }
}