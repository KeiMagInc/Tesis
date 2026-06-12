using UnityEngine;
public class GameInitializer : MonoBehaviour
{
    public Transform playerLupi;
    public Transform puntoInicio;
    void Start()
    {
        if (PlayerPrefs.GetInt("EsPartidaNueva", 0) == 1)
        {
            UIManager.puntosGlobales = 0;
            UIManager.puntosTemporales = 0;
            if (KaosController.nivelesTerminados != null)
                KaosController.nivelesTerminados.Clear(); 
            if (playerLupi != null && puntoInicio != null)
                playerLupi.position = puntoInicio.position;
            PlayerPrefs.SetInt("EsPartidaNueva", 0);
            Debug.Log("Partida nueva detectada: Puntaje en 0 y Lupi en inicio.");
        }
    }
}