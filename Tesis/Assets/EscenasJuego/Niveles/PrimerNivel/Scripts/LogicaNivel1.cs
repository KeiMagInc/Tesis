using UnityEngine;
using Mundo2;
using TMPro;

public class LogicaNivel1 : MonoBehaviour, ILogicaNivel
{
    [Header("Sprites UI Originales")]
    public Sprite spriteTrigo;
    public Sprite spritePapa;
    public Sprite spriteCalabaza;

    public static LogicaNivel1 instancia;
    public AndyController andy;
    public LineRenderer lineaAgua;
    public Transform lupi;

    [Header("Interfaz")]
    public TextMeshProUGUI textoPuntos;

    [Header("Puntos de Conexión")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaHuerto;
    public Transform puntoSalidaHuerto;
    public Transform puntoEntradaNull;

    [Header("Efectos")]
    public EfectoLetrero brilloInicio;
    public EfectoLetrero brilloDato;
    public EfectoLetrero brilloPuntero;
    public EfectoLetrero brilloNull;

    public NodoManager huertoScript;
    private int estado = 0;

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;

        UIManager.instancia.logicaActiva = this;

        // OCULTAR TODO AL EMPEZAR
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.panelParcelas.SetActive(false);

        ResetearNivel();
    }

    // ESTO SE EJECUTA AL SALIR DEL NIVEL 1 (Cuando se activa el Nivel 2 o 3)
    // Agregamos esta función para que el Nivel 1 se limpie solo al cerrarse
    void OnDisable()
    {
        ResetearNivelSilencioso();
    }

    public void ResetearNivel()
    {
        estado = 0;
        if (lineaAgua != null) lineaAgua.positionCount = 0;
        if (huertoScript != null) huertoScript.ResetearNodo();

        ActualizarBrillos(true, false, false, false);

        if (andy != null)
        {
            andy.Decir("¡Lupi! El río fluye de P (Inicio) a NULL.\nUsa 'E' en INICIO para obtener la dirección.");
        }
        ActualizarPuntos();
    }

    // Nueva función para limpiar el nivel sin que Andy hable (útil al salir)
    public void ResetearNivelSilencioso()
    {
        estado = 0;
        if (lineaAgua != null) lineaAgua.positionCount = 0;
        if (huertoScript != null) huertoScript.ResetearNodo();
        ActualizarBrillos(false, false, false, false);
    }

    public void AvanceSiembraExitosa() { }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        switch (tipo)
        {
            case "Head": AccionHead(); break;
            case "EntradaHuerto": AccionHuertoEntrada(); break;
            case "SalidaHuerto": AccionHuertoSalida(); break;
            case "Null": AccionNull(); break;
        }
    }

    void AccionHead()
    {
        if (estado == 0)
        {
            estado = 1; GanarPuntos(10);
            ActualizarBrillos(false, true, false, false);
            andy.Decir("¡Dirección obtenida de P! Llévala al campo DATO.");
        }
    }

    void AccionHuertoEntrada()
    {
        if (estado == 1)
        {
            estado = 2;
            if (huertoScript) huertoScript.ActivarHuerto();
            GanarPuntos(10);
            ActualizarBrillos(false, false, true, false);
            andy.Decir("¡Campo INFO asignado! Activa el PUNTERO.");
        }
    }

    void AccionHuertoSalida()
    {
        if (estado == 2)
        {
            estado = 3; GanarPuntos(10);
            ActualizarBrillos(false, false, false, true);
            andy.Decir("Apunta a NULL.");
        }
    }

    void AccionNull()
    {
        if (estado == 3)
        {
            estado = 4;
            if (huertoScript) huertoScript.DrenarAgua();
            GanarPuntos(10);
            ActualizarBrillos(false, false, false, false);
            andy.Decir("¡Excelente! Estructura básica creada.");
        }
    }

    void GanarPuntos(int cant)
    {
        UIManager.puntosGlobales += cant;
        ActualizarPuntos();
    }

    void ActualizarPuntos()
    {
        if (textoPuntos != null) textoPuntos.text = UIManager.puntosGlobales.ToString();
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
        if (lupi == null || lineaAgua == null) return;
        switch (estado)
        {
            case 1: lineaAgua.positionCount = 2; lineaAgua.SetPosition(0, puntoSalidaHead.position); lineaAgua.SetPosition(1, lupi.position); break;
            case 2: lineaAgua.positionCount = 2; lineaAgua.SetPosition(0, puntoSalidaHead.position); lineaAgua.SetPosition(1, puntoEntradaHuerto.position); break;
            case 3: lineaAgua.positionCount = 4; lineaAgua.SetPosition(0, puntoSalidaHead.position); lineaAgua.SetPosition(1, puntoEntradaHuerto.position); lineaAgua.SetPosition(2, puntoSalidaHuerto.position); lineaAgua.SetPosition(3, lupi.position); break;
            case 4: lineaAgua.positionCount = 4; lineaAgua.SetPosition(0, puntoSalidaHead.position); lineaAgua.SetPosition(1, puntoEntradaHuerto.position); lineaAgua.SetPosition(2, puntoSalidaHuerto.position); lineaAgua.SetPosition(3, puntoEntradaNull.position); break;
            default: lineaAgua.positionCount = 0; break;
        }
    }
}