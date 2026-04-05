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

    [Header("Puntos de Conexión (Anatomía Cairo)")]
    public Transform puntoSalidaHead;    // Variable P
    public Transform puntoEntradaHuerto; // Campo INFO
    public Transform puntoSalidaHuerto;  // Campo LIGA
    public Transform puntoEntradaNull;   // Valor NIL

    [Header("Efectos de Brillo (Letreros)")]
    public EfectoLetrero brilloInicio;
    public EfectoLetrero brilloDato;
    public EfectoLetrero brilloPuntero;
    public EfectoLetrero brilloNull;

    public NodoManager huertoScript;
    private int estado = 0;

    void OnEnable()
    {
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui != null) ui.OcultarInterfazNivel2Snappy();

        lineaAgua.positionCount = 0;
        lineaAgua.sortingOrder = 25;
        ActualizarTextoPuntos();

        ActualizarBrillos(true, false, false, false);

        if (andy != null)
            andy.Decir("¡Lupi! El río fluye de P (Inicio) a NULL.\nUsa 'E' en INICIO para obtener la dirección del primer nodo.");
    }

    void OnDisable()
    {
        ActualizarBrillos(false, false, false, false);
    }

    void Update()
    {
        if (lupi == null) return;

        // Visualización del flujo de memoria dinámica
        switch (estado)
        {
            case 1: // P apuntando a Lupi
                lineaAgua.positionCount = 2;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, lupi.position);
                break;
            case 2: // P -> primer nodo (INFO)
                lineaAgua.positionCount = 2;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, puntoEntradaHuerto.position);
                break;
            case 3: // P -> Nodo.INFO y Nodo.LIGA -> Lupi
                lineaAgua.positionCount = 4;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, puntoEntradaHuerto.position);
                lineaAgua.SetPosition(2, puntoSalidaHuerto.position);
                lineaAgua.SetPosition(3, lupi.position);
                break;
            case 4: // Estructura Completa: P -> INFO -> LIGA -> NIL
                lineaAgua.positionCount = 4;
                lineaAgua.SetPosition(0, puntoSalidaHead.position);
                lineaAgua.SetPosition(1, puntoEntradaHuerto.position);
                lineaAgua.SetPosition(2, puntoSalidaHuerto.position);
                lineaAgua.SetPosition(3, puntoEntradaNull.position);
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
            andy.Decir("¡Dirección obtenida de P! Llévala al campo DATO del huerto.");
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
            andy.Decir("¡Campo INFO asignado!\nAhora activa el campo PUNTERO para ver el enlace.");
        }
    }

    public void AccionHuertoSalida()
    {
        if (estado == 2)
        {
            estado = 3;
            GanarPuntos(10);
            ActualizarBrillos(false, false, false, true);
            andy.Decir("¡Campo PUNTERO activo! Como es el último nodo, debe apuntar a NULL (Null).");
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
            andy.Decir("¡Excelente! Has creado la estructura básica según Cairo:\nVariable P -> INFO -> LIGA -> NULL.");
        }
    }

    // Funciones de soporte
    void GanarPuntos(int cant) { puntosTotales += cant; ActualizarTextoPuntos(); }
    void ActualizarTextoPuntos() { if (textoPuntos != null) textoPuntos.text = puntosTotales.ToString(); }
    void ActualizarBrillos(bool ini, bool dat, bool pun, bool nul)
    {
        if (brilloInicio) brilloInicio.SetEncendido(ini);
        if (brilloDato) brilloDato.SetEncendido(dat);
        if (brilloPuntero) brilloPuntero.SetEncendido(pun);
        if (brilloNull) brilloNull.SetEncendido(nul);
    }
}