using UnityEngine;
using System.Collections;
using Mundo2;
using TMPro;
using UnityEngine.SceneManagement;
public class LogicaNivel1 : MonoBehaviour, ILogicaNivel
{
    [Header("Efectos Burbuja")]
    public GameObject prefabBurbuja;
    public Transform puntoCentroHuerto;
    [Header("Información del Nivel UI")]
    public string nombreDelNivel = "Anatomía y Componentes";
    public string operacionDelNivel = "Identificar las partes del nodo";
    private Color colorOriginalPuntos;
    private Vector3 escalaOriginalPuntos;
    private Coroutine rutinaEfectoPuntos;
    [Header("Posicionamiento")]
    public Transform puntoInicioNivel;
    [Header("Pantalla Final")]
    public GameObject panelFinal;
    public TextMeshProUGUI textoPuntajeFinal;
    public TextMeshProUGUI textoAciertos;
    public TextMeshProUGUI textoFallos;
    private int aciertosContador = 0;
    private int fallosContador = 0;
    private int puntosAlIniciarNivel;
    [Header("Configuración de Tiempo")]
    public int puntosMaximos = 10;
    public int puntosMinimos = 0;
    public float tiempoLimite = 120f;
    private float tiempoInicioEstado;
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
    public AudioClip sonidoCuy;
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
    void Awake()
    {
        instancia = this;
        if (textoPuntos != null)
        {
            colorOriginalPuntos = textoPuntos.color;
            escalaOriginalPuntos = textoPuntos.transform.localScale;
        }
    }
    void OnEnable()
    {
        UIManager.instancia.ConfigurarCabeceraNivel(nombreDelNivel, operacionDelNivel);
        if (UIManager.instancia == null) return;
        puntosAlIniciarNivel = UIManager.puntosGlobales;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.panelParcelas.SetActive(false);
        var control = lupi.GetComponent<PlayerController>();
        if (control != null) control.controlesBloqueados = false;
        ResetearNivel();
    }
    void OnDisable()
    {
        ResetearNivelSilencioso();
    }
    IEnumerator AnimacionPuntos(bool esAumento)
    {
        textoPuntos.color = esAumento ? Color.green : Color.red;
        float tiempoPaso = 0.07f;
        Vector3 escalaFlash = escalaOriginalPuntos * 1.3f;
        for (int i = 0; i < 3; i++)
        {
            textoPuntos.transform.localScale = escalaFlash;
            yield return new WaitForSeconds(tiempoPaso);
            textoPuntos.transform.localScale = escalaOriginalPuntos;
            yield return new WaitForSeconds(tiempoPaso);
        }
        textoPuntos.transform.localScale = escalaOriginalPuntos;
        textoPuntos.color = colorOriginalPuntos;
    }
    IEnumerator MostrarResumenFinal()
    {
        yield return new WaitForSeconds(3.5f);
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (textoPuntajeFinal) textoPuntajeFinal.text = UIManager.puntosGlobales.ToString();
            if (textoAciertos) textoAciertos.text = aciertosContador.ToString();
            if (textoFallos) textoFallos.text = fallosContador.ToString();
            Debug.Log("Panel Final activado y Lupi congelado.");
        }
        else
        {
            Debug.LogError("¡No has asignado el Panel Final en el Inspector!");
        }
    }
    void CongelarLupi(bool congelar)
    {
        if (lupi != null)
        {
            var controlMovimiento = lupi.GetComponent<PlayerController>();
            if (controlMovimiento != null) controlMovimiento.enabled = !congelar;

            Rigidbody2D rb = lupi.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
    public void BotonReintentar()
    {
        StopAllCoroutines();
        UIManager.puntosGlobales = puntosAlIniciarNivel;
        ActualizarPuntos();
        if (KaosController.instancia != null)
            KaosController.instancia.ResetearEstadoNivel("AnatomiaComponentes");
        if (checkpointFinal != null)
            checkpointFinal.ResetearCheckpoint();
        if (barreraSiguiente != null)
            barreraSiguiente.Cerrar();
        if (controladorInsignia != null)
            controladorInsignia.ResetearInsignia();
        if (panelFinal != null) panelFinal.SetActive(false);
        CongelarLupi(false);
        ResetearNivel();
        if (lupi != null && puntoInicioNivel != null)
            lupi.position = puntoInicioNivel.position;
    }
    public void BotonSiguiente()
    {
        if (panelFinal != null) panelFinal.SetActive(false);
        if (lupi != null)
        {
            var controlMovimiento = lupi.GetComponent<PlayerController>();
            if (controlMovimiento != null) controlMovimiento.enabled = true;
        }
        Debug.Log("Lupi descongelado, puede avanzar al siguiente nivel en la misma escena.");
    }
    int CalcularPuntosDinamicos()
    {
        float tiempoTranscurrido = Time.time - tiempoInicioEstado;
        float t = Mathf.Clamp01(tiempoTranscurrido / tiempoLimite);
        int puntos = Mathf.RoundToInt(Mathf.Lerp(puntosMaximos, puntosMinimos, t));
        return puntos;
    }
    public void ResetearNivel()
    {
        estado = 0;
        aciertosContador = 0;
        fallosContador = 0;
        if (panelFinal) panelFinal.SetActive(false);
        tiempoInicioEstado = Time.time;
        if (lineaAgua != null) lineaAgua.positionCount = 0;
        if (huertoScript != null) huertoScript.ResetearNodo();
        ActualizarBrillos(true, false, false, false);
        if (andy != null)
        {
            andy.Decir("¡Bienvenido Lupi! Identifica las partes del NODO, ve al poste INICIO. Usa 'E' para obtener la dirección de memoria del primer objeto de tipo NODO.", audioBienvenida);
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
            int puntosGanados = CalcularPuntosDinamicos();
            estado = 1; 
            SumarPuntos(puntosGanados);
            tiempoInicioEstado = Time.time;
            ActualizarBrillos(false, true, false, false);
            andy.Decir("¡Dirección obtenida en P! Lleva la conexión al P.INFO, aquí guardaremos el valor de la semilla, que puede ser un dato de tipo int o string.", audioDireccionObtenida);
        }
    }
    void AccionHuertoEntrada()
    {
        if (estado == 1)
        {
            int puntosGanados = CalcularPuntosDinamicos();
            estado = 2;
            if (huertoScript) huertoScript.ActivarHuerto();
            SumarPuntos(puntosGanados);
            tiempoInicioEstado = Time.time;
            ActualizarBrillos(false, false, true, false);
            andy.Decir("¡Dato guardado en P.INFO! Ahora ve a la válvula P.LIGA. Recuerda: P.LIGA no guarda textos ni números, es una variable de tipo NODO que debe apuntar al siguiente huerto.", audioSemillasAsignadas);
        }
        else if (estado < 1)
        {
            ReproducirError("¡Lupi cuidado! No puedes guardar datos en P.INFO si no tienes una conexión desde el INICIO.", audioErrorInfo);
        }
    }
    void AccionHuertoSalida()
    {
        if (estado == 2)
        {
            int puntosGanados = CalcularPuntosDinamicos();
            estado = 3;
            SumarPuntos(puntosGanados);
            tiempoInicioEstado = Time.time;
            ActualizarBrillos(false, false, false, true);
            andy.Decir("¡Referencia P.LIGA activada! Como no tenemos otro NODO para conectar, arrastra la manguera al pozo final NULL. Así indicamos que este es el final de la lista.", audioCanalActivado);
        }
        else if (estado < 2)
        {
            ReproducirError("Secuencia incorrecta. Primero debes llenar el P.INFO antes de manipular la válvula de salida P.LIGA.", audioErrorLiga);
        }
    }
    void AccionNull()
    {
        if (estado == 3)
        {
            int puntosGanados = CalcularPuntosDinamicos();
            estado = 4;
            if (huertoScript) huertoScript.DrenarAgua();
            SumarPuntos(puntosGanados, true);
            ActualizarBrillos(false, false, false, false);
            if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
            {
                barreraSiguiente.Abrir();
                checkpointFinal.AparecerYActivar();
                controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                KaosController.instancia.RecibirDanoYDesaparecer("AnatomiaComponentes");
            }
            CongelarLupi(true);
            ReproducirNivelCompleto();
            andy.Decir("¡Excelente, Analista de estructuras! Has creado un NODO perfecto. Su P.INFO guarda un dato (int o string) y su P.LIGA (de tipo NODO) apunta a NULL. ¡Sin fugas de memoria!", audioCosechaASalvo); 
            StartCoroutine(MostrarResumenFinal());
        }
        else if (estado < 3)
        {
            ReproducirError("Ese es el pozo NULL. Solo debes apuntar aquí usando la válvula P.LIGA del huerto para cerrar la lista.", audioErrorNull);
        }
    }
    void ReproducirNivelCompleto()
    {
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoCompletado)
            masterSFX.PlayOneShot(sonidoCompletado);
    }

    void ReproducirError(string mensajePista, AudioClip audioExplicacion)
    {
        fallosContador++;
        if (andy != null) andy.Decir(mensajePista, audioExplicacion);
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (!KaosController.nivelesTerminados.Contains("AnatomiaComponentes"))
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    void SumarPuntos(int cant, bool silencioso = false)
    {
        if (KaosController.nivelesTerminados.Contains("AnatomiaComponentes")) return;
        if (prefabBurbuja != null)
        {
            Vector3 spawnPos;
            if (puntoCentroHuerto != null)
            {
                spawnPos = puntoCentroHuerto.position;
            }
            else
            {
                spawnPos = huertoScript.transform.position;
            }
            spawnPos.z = -1f;
            GameObject nuevaBurbuja = Instantiate(prefabBurbuja, spawnPos, Quaternion.identity);
            EfectoBurbuja scriptBurbuja = nuevaBurbuja.GetComponent<EfectoBurbuja>();
            if (scriptBurbuja != null)
                scriptBurbuja.Configurar(cant);
        }
        UIManager.puntosGlobales += cant;
        ActualizarPuntos();
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        if (!silencioso && UIManager.instancia.fuenteVozAndy && sonidoAcierto)
        {
            UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoAcierto);
            if (sonidoCuy) UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoCuy);
        }
    }

    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }

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