using UnityEngine;
using Mundo2;
using TMPro;

public class LogicaNivel1 : MonoBehaviour, ILogicaNivel
{
    [Header("Audios Diálogos Andy")]
    public AudioClip audioBienvenida;
    public AudioClip audioDireccionObtenida;
    public AudioClip audioSemillasAsignadas;
    public AudioClip audioCanalActivado;
    public AudioClip audioCosechaASalvo;
    public AudioClip audioErrorHead;
    public AudioClip audioErrorInfo;
    public AudioClip audioErrorLiga;
    public AudioClip audioErrorNull;
    [Header("Sonidos")]
    public AudioSource fuenteAudio; 
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
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
    public EfectoLetrero brilloInfo;
    public EfectoLetrero brilloLiga;
    public EfectoLetrero brilloNull;
    public NodoManager huertoScript;
    private int estado = 0;

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.panelParcelas.SetActive(false);
        ResetearNivel();
    }

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
            // Fusión GDD (Poste, manguera) + Cairo (Puntero P, Lista vacía)
            andy.Decir("¡Bienvenido Lupi! Para crear nuestra primera Lista Enlazada, ve al poste principal.\nUsa 'E' en INICIO (Puntero P) para tomar la manguera de memoria.", audioBienvenida);
        }
        ActualizarPuntos();
    }

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
            // Teoría Cairo: P debe apuntar al nodo.
            andy.Decir("¡Dirección base obtenida en P! Ahora lleva el canal de luz hacia el Almacén de la parcela (Campo INFO) para asegurar nuestros datos.", audioDireccionObtenida);
        }
        else if (estado < 0)
        {
            ReproducirError("Error lógico: Una lista siempre nace de un Puntero principal (Inicio/P).", audioErrorHead);
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
            // GDD: El agua no va a ningún lado. Cairo: El campo LIGA debe definirse.
            andy.Decir("¡Semillas en INFO asignadas! Pero cuidado, si la válvula (Campo LIGA) queda suelta, habrá fuga de memoria. Activa la LIGA.", audioSemillasAsignadas);
        }
        else if (estado < 1)
        {
            ReproducirError("¡El Kaos acecha! No puedes guardar datos en INFO si no tienes una conexión desde el INICIO (P).", audioErrorInfo);
        }
    }

    void AccionHuertoSalida()
    {
        if (estado == 2)
        {
            estado = 3; GanarPuntos(10);
            ActualizarBrillos(false, false, false, true);
            // Cairo: Si no hay más nodos, apuntar a NIL/NULL.
            andy.Decir("¡Canal LIGA activado! Como es nuestro único huerto, arrastra la manguera al pozo seco (NULL) para indicar el final de la lista.", audioCanalActivado);
        }
        else if (estado < 2)
        {
            ReproducirError("Secuencia incorrecta. Primero debes llenar el Almacén (INFO) antes de manipular la válvula de salida (LIGA).", audioErrorLiga);
        }
    }

    void AccionNull()
    {
        if (estado == 3)
        {
            estado = 4;
            if (huertoScript) huertoScript.DrenarAgua();
            GanarPuntos(10, true);
            ActualizarBrillos(false, false, false, false);
            // Celebración final mezclando ambos conceptos.
            andy.Decir("¡Cosecha a salvo! Has creado un nodo perfecto: Puntero P -> INFO -> LIGA apuntando a NULL. ¡Sin referencias sueltas!", audioCosechaASalvo);
            ReproducirNivelCompleto();
        }
        else if (estado < 3)
        {
            ReproducirError("Ese es el pozo NULL. Solo debes apuntar aquí usando la válvula (LIGA) del huerto para cerrar la lista.", audioErrorNull);
        }
    }

    void ReproducirNivelCompleto()
    {
        if (fuenteAudio && sonidoCompletado)
            for (int i = 0; i < 2; i++) fuenteAudio.PlayOneShot(sonidoCompletado);
    }

    void ReproducirError(string mensajePista, AudioClip audioExplicacion)
    {
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError); // Suena el "Bip"
        if (andy != null) andy.Decir(mensajePista, audioExplicacion); // Andy habla y se escribe el texto
    }

    void GanarPuntos(int cant, bool silencioso = false)
    {
        UIManager.puntosGlobales += cant;
        ActualizarPuntos();
        // Solo suena si NO es silencioso
        if (!silencioso && fuenteAudio && sonidoAcierto) fuenteAudio.PlayOneShot(sonidoAcierto);
    }

    void ActualizarPuntos()
    {
        if (textoPuntos != null) textoPuntos.text = UIManager.puntosGlobales.ToString();
    }

    void ActualizarBrillos(bool ini, bool dat, bool pun, bool nul)
    {
        if (brilloInicio) brilloInicio.SetEncendido(ini);
        if (brilloInfo) brilloInfo.SetEncendido(dat);
        if (brilloLiga) brilloLiga.SetEncendido(pun);
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