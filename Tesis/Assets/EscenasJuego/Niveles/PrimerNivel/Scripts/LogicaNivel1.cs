using UnityEngine;
using Mundo2;
using TMPro;

public class LogicaNivel1 : MonoBehaviour, ILogicaNivel
{
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

    public NodoManager huertoScript; // El nodo estático del nivel 1
    private int estado = 0;

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia != null)
        {
            UIManager.instancia.logicaActiva = this;
            UIManager.instancia.MostrarInterfaz(false); // Oculta mochila en N1
        }

        // RESET TOTAL AL ENTRAR
        ResetearNivel();
    }

    public void ResetearNivel()
    {
        estado = 0;

        // 1. Limpiar la línea azul
        if (lineaAgua != null) lineaAgua.positionCount = 0;

        // 2. Resetear el nodo (Agua desaparece y sembríos secos)
        if (huertoScript != null)
        {
            huertoScript.ResetearNodo();
        }

        // 3. Resetear brillos de letreros
        ActualizarBrillos(true, false, false, false);

        // 4. Repetir diálogo de Andy
        if (andy != null)
        {
            andy.Decir("¡Lupi! El río fluye de P (Inicio) a NULL.\nUsa 'E' en INICIO para obtener la dirección.");
        }

        ActualizarPuntos();
    }

    public void AvanceSiembraExitosa() { } // No se usa en N1

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

    // Lógica de estados...
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