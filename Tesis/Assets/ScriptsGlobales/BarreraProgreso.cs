using UnityEngine;

public class BarreraProgreso : MonoBehaviour
{
    public void Abrir()
    {
        // Buscamos el colisionador en este objeto (ya sea el Composite o el Tilemap)
        Collider2D colisionador = GetComponent<Collider2D>();

        if (colisionador != null)
        {
            colisionador.enabled = false; // Desactiva la colisión, pero el objeto sigue visible
            Debug.Log("Colisión de barrera desactivada. Lupi puede pasar.");
        }
    }
}