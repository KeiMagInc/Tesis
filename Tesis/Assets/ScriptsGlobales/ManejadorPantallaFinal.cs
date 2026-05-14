using UnityEngine;
using Mundo2;
public class ManejadorPantallaFinal : MonoBehaviour
{
    public void ClickReintentar()
    {
        if (UIManager.instancia.logicaActiva != null)
        {
            UIManager.instancia.logicaActiva.BotonReintentar();
        }
    }
    public void ClickSiguiente()
    {
        if (UIManager.instancia.logicaActiva != null)
        {
            UIManager.instancia.logicaActiva.BotonSiguiente();
        }
    }
}