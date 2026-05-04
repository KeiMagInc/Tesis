using UnityEngine;
using Mundo2;
using TMPro;
public class LogicaNivel1 : MonoBehaviour, ILogicaNivel
{
    [Header("Insignias")]
    public ControladorInsignia controladorInsignia;
    public Sprite insigniaDeEsteNivel;
    public Checkpoint checkpointFinal;
    [Header("Progreso")]
    public BarreraProgreso barreraSiguiente; 
    [Header("Audios Diálogos Andy")]
    public AudioClip audioBienvenida;
    public AudioClip audioDireccionObtenida;
    public AudioClip audioSemillasAsignadas;
    public AudioClip audioCanalActivado;
    public AudioClip audioCosechaASalvo;
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
    private float tiempoUltimaAccion = 0f;
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
            andy.Decir("¡Bienvenido Lupi! Identifica las partes del NODO, ve al poste INICIO (Puntero P). Usa 'E' para obtener la dirección de memoria del primer objeto de tipo Nodo.", audioBienvenida);
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
        if (Time.time - tiempoUltimaAccion < 1.5f) return;
        tiempoUltimaAccion = Time.time;

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
            estado = 1; SumarPuntos(10);
            ActualizarBrillos(false, true, false, false);
            andy.Decir("¡Dirección obtenida en P! Lleva la conexión al Almacén de la parcela INFO. Aquí guardaremos el valor de la semilla, que puede ser un dato de tipo int o string.", audioDireccionObtenida);
        }
    }
    void AccionHuertoEntrada()
    {
        if (estado == 1)
        {
            estado = 2;
            if (huertoScript) huertoScript.ActivarHuerto();
            SumarPuntos(10);
            ActualizarBrillos(false, false, true, false);
            andy.Decir("¡Dato guardado en INFO! Ahora ve a la válvula LIGA. Recuerda: LIGA no guarda textos ni números, es una variable de tipo Nodo que debe apuntar al siguiente huerto.", audioSemillasAsignadas);
        }
        else if (estado < 1)
        {
            ReproducirError("¡Lupi cuidado! No puedes guardar datos en INFO si no tienes una conexión desde el INICIO (P).", audioErrorInfo);
        }
    }
    void AccionHuertoSalida()
    {
        if (estado == 2)
        {
            estado = 3; SumarPuntos(10);
            ActualizarBrillos(false, false, false, true);
            andy.Decir("¡Referencia LIGA activada! Como no tenemos otro Nodo para conectar, arrastra la manguera al pozo final NULL. Así indicamos que este es el final de la lista.", audioCanalActivado);
        }
        else if (estado < 2)
        {
            ReproducirError("Secuencia incorrecta. Primero debes llenar el Almacén INFO antes de manipular la válvula de salida LIGA.", audioErrorLiga);
        }
    }
    void AccionNull()
    {
        if (estado == 3)
        {
            estado = 4;
            if (huertoScript) huertoScript.DrenarAgua();
            SumarPuntos(10, true);
            ActualizarBrillos(false, false, false, false);
            if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
            {
                barreraSiguiente.Abrir();
                checkpointFinal.AparecerYActivar();
                controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                KaosController.instancia.RecibirDanoYDesaparecer("AnatomiaComponentes");
            }
            andy.Decir("¡Excelente, Analista de estructuras! Has creado un Nodo perfecto. Su INFO guarda un dato (int o string) y su LIGA (de tipo Nodo) apunta a NULL. ¡Sin fugas de memoria!", audioCosechaASalvo); 
            ReproducirNivelCompleto();
        }
        else if (estado < 3)
        {
            ReproducirError("Ese es el pozo NULL. Solo debes apuntar aquí usando la válvula LIGA del huerto para cerrar la lista.", audioErrorNull);
        }
    }
    void ReproducirNivelCompleto()
    {
        if (fuenteAudio && sonidoCompletado)
            for (int i = 0; i < 2; i++) fuenteAudio.PlayOneShot(sonidoCompletado);
    }

    void ReproducirError(string mensajePista, AudioClip audioExplicacion)
    {
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError);
        if (andy != null) andy.Decir(mensajePista, audioExplicacion);
    }

    void SumarPuntos(int cant, bool silencioso = false)
    {
        UIManager.puntosGlobales += cant;
        ActualizarPuntos();
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