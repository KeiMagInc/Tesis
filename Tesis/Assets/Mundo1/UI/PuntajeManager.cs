using UnityEngine;
using TMPro;

public class PuntajeManager : MonoBehaviour
{
    public TextMeshProUGUI textoPuntosHUD; // El del Canvas del Jugador (1000)
    
    private int puntosActuales = 0;
    private int aciertosTotales = 0;
    private int fallosTotales = 0;

    void Start() { ActualizarUI(); }

    public void ModificarPuntos(int cantidad, bool esAcierto)
    {
        puntosActuales += cantidad;
        if (puntosActuales < 0) puntosActuales = 0;

        if (esAcierto) aciertosTotales++;
        else fallosTotales++;

        ActualizarUI();
    }

    void ActualizarUI()
    {
        if(textoPuntosHUD != null) textoPuntosHUD.text = puntosActuales.ToString();
    }

    // Getters para la pantalla final
    public int GetPuntos() => puntosActuales;
    public int GetAciertos() => aciertosTotales;
    public int GetFallos() => fallosTotales;
}