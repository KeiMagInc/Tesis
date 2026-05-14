using UnityEngine;
public class BarreraProgreso : MonoBehaviour
{
    public void Abrir()
    {
        Collider2D colisionador = GetComponent<Collider2D>();
        if (colisionador != null)
        {
            colisionador.enabled = false;
            Debug.Log("Colisión de barrera desactivada. Lupi puede pasar.");
        }
    }
    public void Cerrar()
    {
        Collider2D colisionador = GetComponent<Collider2D>();
        if (colisionador != null)
        {
            colisionador.enabled = true;
            Debug.Log("Barrera activada: El paso está bloqueado nuevamente.");
        }
    }
}