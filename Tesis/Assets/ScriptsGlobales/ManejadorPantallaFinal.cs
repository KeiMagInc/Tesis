using UnityEngine;
using Mundo2;
public class ManejadorPantallaFinal : MonoBehaviour
{
    public GameObject panelPropio;
    public void ClickReintentar()
    {
        if (UIManager.instancia != null)
            UIManager.instancia.ReproducirSonidoClick();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (panelPropio != null)
            panelPropio.SetActive(false);
        if (UIManager.instancia != null && UIManager.instancia.logicaActiva != null)
            UIManager.instancia.logicaActiva.BotonReintentar();
        else
            Debug.LogWarning("No se encontró lógica de nivel activa para reintentar.");
    }
    public void ClickSiguiente()
    {
        if (UIManager.instancia != null)
            UIManager.instancia.ReproducirSonidoClick();
        if (UIManager.instancia != null && UIManager.instancia.logicaActiva != null)
            UIManager.instancia.logicaActiva.BotonSiguiente();
    }
}