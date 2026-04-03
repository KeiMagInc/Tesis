using UnityEngine;
using Mundo2;
using TMPro;

public class LogicaNivel1 : MonoBehaviour
{
    public AndyController andy;
    public LineRenderer lineaAgua;
    public Transform lupi;

    [Header("Interfaz")]
    public TextMeshProUGUI textoPuntos;
    private int puntosTotales = 0;

    [Header("Puntos de Conexión")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaHuerto;
    public Transform puntoSalidaHuerto;
    public Transform puntoNull;

    [Header("Efectos de Brillo (Letreros)")]
    public EfectoLetrero brilloInicio;
    public EfectoLetrero brilloDato;
    public EfectoLetrero brilloPuntero;
    public EfectoLetrero brilloNull;

    public NodoManager huertoScript;
    private int estado = 0;

    void OnEnable()
    {
        // CORRECCIÓN AQUÍ: Usamos el nuevo nombre de la función del UIManager
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui != null) ui.OcultarInterfazNivel2Snappy();

        lineaAgua.positionCount = 0;
        lineaAgua.sortingOrder = 25;
        ActualizarTextoPuntos();

        ActualizarBrillos(true, false, false, false);

        if (andy != null)
            andy.Decir("¡Lupi! El río fluye de INICIO a NULL, pero el sembrío está seco.\nPulsa 'E' en INICIO para interceptar el agua.");
    }

    void OnDisable()
    {
        ActualizarBrillos(false, false, false, false);
    }

    void GanarPuntos(int cantidad)
    {
        puntosTotales += cantidad;
        ActualizarTextoPuntos();
    }

    void ActualizarTextoPuntos()
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = puntosTotales.ToString();
        }
    }

    void ActualizarBrillos(bool ini, bool dat, bool pun, bool nul)
    {
        if (brilloInicio) brilloInicio.SetEncendido(ini);
        if (brilloDato) brilloDato.SetEncendido(dat);
        if (brilloPuntero) brilloPuntero.SetEncendido(pun);
        if (brilloNull) brilloNull.SetEncendido(nul);
    }

    void Update()
    {
        if (lupi == null) return;

        switch (estado)
        {
            case 1:
                lineaAgua.positionCount = 2;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, lupi.position);
                break;
            case 2:
                lineaAgua.positionCount = 2;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, puntoEntradaHuerto.position);
                break;
            case 3:
                lineaAgua.positionCount = 4;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, puntoEntradaHuerto.position);
                lineaAgua.SetPosition(2, puntoSalidaHuerto.position);
                lineaAgua.SetPosition(3, lupi.position);
                break;
            case 4:
                lineaAgua.positionCount = 4;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, puntoEntradaHuerto.position);
                lineaAgua.SetPosition(2, puntoSalidaHuerto.position);
                lineaAgua.SetPosition(3, puntoNull.position);
                break;
        }
    }

    public void AccionHead()
    {
        if (estado == 0)
        {
            estado = 1;
            GanarPuntos(10);
            ActualizarBrillos(false, true, false, false);
            andy.Decir("¡Agua recogida! Llévala al DATO.");
        }
    }

    public void AccionHuertoEntrada()
    {
        if (estado == 1)
        {
            estado = 2;
            huertoScript.ActivarHuerto();
            GanarPuntos(10);
            ActualizarBrillos(false, false, true, false);
            andy.Decir("¡DATO conectado!\nAhora recoge el PUNTERO en la salida.");
        }
    }

    public void AccionHuertoSalida()
    {
        if (estado == 2)
        {
            estado = 3;
            GanarPuntos(10);
            ActualizarBrillos(false, false, false, true);
            andy.Decir("¡Bien! El PUNTERO indica el siguiente camino.\nLleva el enlace a NULL.");
        }
    }

    public void AccionNull()
    {
        if (estado == 3)
        {
            estado = 4;
            huertoScript.DrenarAgua();
            GanarPuntos(10);
            ActualizarBrillos(false, false, false, false);
            andy.Decir("¡Excelente! Has creado una LISTA SIMPLE:\nINICIO -> DATO -> PUNTERO -> NULL.");
        }
    }
}