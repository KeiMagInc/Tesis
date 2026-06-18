using UnityEngine;
using System.Collections;
using Mundo2;
using TMPro;
using UnityEngine.SceneManagement;
public class LogicaNivel1 : MonoBehaviour, ILogicaNivel
{
    private bool esModoRepaso = false;
    [Header("Pantalla Victoria")]
    public GameObject panelVictoria;
    public TextMeshProUGUI textoAciertosVictoria;
    public TextMeshProUGUI textoFallosVictoria;
    public TextMeshProUGUI textoPuntajeVictoria;
    public AudioClip sonidoFinDelJuego;
    [Header("Pantalla Derrota")]
    public GameObject panelDerrota;
    public TextMeshProUGUI textoAciertosDerrota;
    public TextMeshProUGUI textoFallosDerrota;
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
    public AudioClip audioErrorKaos1;
    public AudioClip audioErrorKaos2;
    public AudioClip audioErrorKaos3;
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoSeleccionar;
    public AudioClip sonidoSembrar; 
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoInsignia;
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
        if (PlayerPrefs.GetInt("EsPartidaNueva", 0) == 1)
        {
            UIManager.puntosGlobales = 0;
            UIManager.puntosTemporales = 0;
            if (KaosController.nivelesTerminados != null)
                KaosController.nivelesTerminados.Clear();
        }
        UIManager.instancia.ConfigurarCabeceraNivel(nombreDelNivel, operacionDelNivel);
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetSounds(sonidoSembrar, sonidoSembrar, sonidoSembrar);
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.panelParcelas.SetActive(false);
        var control = lupi.GetComponent<PlayerController>();
        if (control != null) control.controlesBloqueados = false;
        esModoRepaso = KaosController.nivelesTerminados.Contains("AnatomiaComponentes");
        if (!esModoRepaso)
        {
            puntosAlIniciarNivel = UIManager.puntosGlobales;
            KaosController kaos = Object.FindFirstObjectByType<KaosController>(FindObjectsInactive.Include);
            if (kaos != null)
            {
                kaos.gameObject.SetActive(true);
                kaos.ResetearEstadoNivel("AnatomiaComponentes");
            }
        }
        else
        {
            Debug.Log("Modo Repaso Nivel 1: Puntaje protegido.");
            KaosController kaos = Object.FindFirstObjectByType<KaosController>(FindObjectsInactive.Include);
            if (kaos != null) kaos.gameObject.SetActive(false);
        }
        ResetearNivel();
        ActualizarPuntos();
        Debug.Log("Modo Repaso: Manteniendo estado actual del nivel.");
    }
    void OnDisable()
    {
        if (!esModoRepaso && estado < 4)
        {
            UIManager.puntosGlobales = puntosAlIniciarNivel;
            UIManager.puntosTemporales = 0;
        }
        ResetearNivelSilencioso();
    }
    public void DesconectarEnlacePorKaos()
    {
        if (esModoRepaso) return;
        if (estado >= 4 || estado == 0) return;
        fallosContador++;
        int puntosARestar = 5;
        if (UIManager.puntosTemporales >= puntosARestar)
        {
            UIManager.puntosTemporales -= puntosARestar;
        }
        else
        {
            int sobrante = puntosARestar - UIManager.puntosTemporales;
            UIManager.puntosTemporales = 0;
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - sobrante);
        }
        ActualizarPuntos();
        UIManager.instancia.RevisarDerrotaPorPorcentaje(aciertosContador, fallosContador);
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (estado == 1)
        {
            estado = 0;
            ActualizarBrillos(true, false, false, false);
            if (andy != null)
                andy.Decir("¡Oh no! Kaos te ha tocado y ha desconectado la manguera del INICIO.", audioErrorKaos1);
        }
        else if (estado == 2)
        {
            estado = 0;
            if (huertoScript != null) huertoScript.ResetearNodo();
            ActualizarBrillos(true, false, false, false);
            if (andy != null)
                andy.Decir("¡Cuidado Lupi! Kaos desconectó tu manguera. Reconecta desde el INICIO.", audioErrorKaos2);
        }
        else if (estado == 3)
        {
            estado = 2;
            ActualizarBrillos(false, false, true, false);
            if (andy != null)
                andy.Decir("¡Kaos soltó la manguera de salida! Vuelve a conectarla desde P.LIGA.", audioErrorKaos3);
        }
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
    IEnumerator MostrarResumenFinal(bool esVictoria)
    {
        if (esVictoria)
        {
            if (audioCosechaASalvo != null)
            {
                yield return new WaitForSeconds(audioCosechaASalvo.length + 0.8f);
            }
            else
            {
                yield return new WaitForSeconds(4.0f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
        GameObject panelAActivar = esVictoria ? panelVictoria : panelDerrota;
        if (panelAActivar != null)
        {
            panelAActivar.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (esVictoria && fuenteAudio != null && sonidoFinDelJuego != null)
                fuenteAudio.PlayOneShot(sonidoFinDelJuego);
            if (esVictoria)
            {
                if (textoPuntajeVictoria) textoPuntajeVictoria.text = UIManager.puntosGlobales.ToString();
                if (textoAciertosVictoria) textoAciertosVictoria.text = aciertosContador.ToString();
                if (textoFallosVictoria) textoFallosVictoria.text = fallosContador.ToString();
            }
            else
            {
                if (textoAciertosDerrota) textoAciertosDerrota.text = aciertosContador.ToString();
                if (textoFallosDerrota) textoFallosDerrota.text = fallosContador.ToString();
            }
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
        UIManager.DescartarPuntos();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        StopAllCoroutines();
        if (!esModoRepaso)
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
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);
        CongelarLupi(false);
        ResetearNivel();
        if (lupi != null && puntoInicioNivel != null)
            lupi.position = puntoInicioNivel.position;
        ActualizarPuntos();
    }
    public void BotonSiguiente()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
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
        if (!esModoRepaso && KaosController.instancia != null)
            KaosController.instancia.ResetearEstadoNivel("AnatomiaComponentes");
        if (panelVictoria) panelVictoria.SetActive(false);
        tiempoInicioEstado = Time.time;
        if (lineaAgua != null) lineaAgua.positionCount = 0;
        if (huertoScript != null) huertoScript.ResetearNodo();
        ActualizarBrillos(true, false, false, false);
        if (andy != null)
            andy.Decir("¡Bienvenido Lupi! Identifica las partes del NODO, ve al poste INICIO. Usa 'E' para obtener la dirección de memoria del primer objeto de tipo NODO.", audioBienvenida);
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
        if (Time.time - tiempoUltimaAccion < 0.1f) return;
        if (fuenteAudio != null && sonidoSeleccionar != null)
            fuenteAudio.PlayOneShot(sonidoSeleccionar);
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
            if (!esModoRepaso)
                UIManager.ConfirmarPuntos();
            ActualizarBrillos(false, false, false, false);
            CongelarLupi(true);
            StartCoroutine(SecuenciaFinNivel());                       
        }
        else if (estado < 3)
        {
            ReproducirError("Ese es el pozo NULL. Solo debes apuntar aquí usando la válvula P.LIGA del huerto para cerrar la lista.", audioErrorNull);
        }
    }
    IEnumerator SecuenciaFinNivel()
    {
        if (UIManager.instancia != null && UIManager.instancia.fuenteVozAndy != null)
        {
            while (UIManager.instancia.fuenteVozAndy.isPlaying)
                yield return null;
        }
        if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null)
        {
            barreraSiguiente.Abrir();
            checkpointFinal.AparecerYActivar();
            controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
            if (fuenteAudio != null && sonidoInsignia != null)
                fuenteAudio.PlayOneShot(sonidoInsignia);
            if (!esModoRepaso && KaosController.instancia != null)
                KaosController.instancia.RecibirDanoYDesaparecer("AnatomiaComponentes");
        }
        float tiempoEsperaMedalla = (sonidoInsignia != null) ? sonidoInsignia.length : 2.0f;
        yield return new WaitForSeconds(tiempoEsperaMedalla);
        if (andy != null)
            andy.Decir("¡Excelente, Analista de estructuras! Has creado un NODO perfecto. Su P.INFO guarda un dato (int o string) y su P.LIGA (de tipo NODO) apunta a NULL. ¡Sin fugas de memoria!", audioCosechaASalvo);        
        StartCoroutine(MostrarResumenFinal(true));
    }
    void ReproducirError(string mensajePista, AudioClip audioExplicacion)
    {
        fallosContador++;
        if (andy != null) andy.Decir(mensajePista, audioExplicacion);
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (!esModoRepaso)
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            UIManager.instancia.RevisarDerrotaPorPorcentaje(aciertosContador, fallosContador);
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    void SumarPuntos(int cant, bool silencioso = false)
    {
        aciertosContador++;
        if (!esModoRepaso)
            UIManager.puntosTemporales += cant;
        ActualizarPuntos();
        if (prefabBurbuja != null)
        {
            Vector3 spawnPos;
            if (puntoCentroHuerto != null)
                spawnPos = puntoCentroHuerto.position;
            else
                spawnPos = huertoScript.transform.position;
            spawnPos.z = -1f;
            GameObject nuevaBurbuja = Instantiate(prefabBurbuja, spawnPos, Quaternion.identity);
            EfectoBurbuja scriptBurbuja = nuevaBurbuja.GetComponent<EfectoBurbuja>();
            if (scriptBurbuja != null)
                scriptBurbuja.Configurar(esModoRepaso ? 0 : cant);
        }
        if (!silencioso && UIManager.instancia.fuenteVozAndy && sonidoAcierto)
        {
            UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoAcierto);
            if (sonidoCuy) UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoCuy);
        }
    }
    void ActualizarPuntos()
    {
        if (textoPuntos)
            textoPuntos.text = (UIManager.puntosGlobales + UIManager.puntosTemporales).ToString();
    }
    void ActualizarBrillos(bool ini, bool dat, bool pun, bool nul)
    {
        if (brilloInicio) brilloInicio.SetEncendido(ini);
        if (brilloInfo) brilloInfo.SetEncendido(dat);
        if (brilloLiga) brilloLiga.SetEncendido(pun);
        if (brilloNull) brilloNull.SetEncendido(nul);
        if (andy != null)
        {
            if (ini && brilloInicio != null)
            {
                andy.CambiarObjetivo(brilloInicio.transform);
            }
            else if (dat && brilloInfo != null)
            {
                andy.CambiarObjetivo(brilloInfo.transform);
            }
            else if (pun && brilloLiga != null)
            {
                andy.CambiarObjetivo(brilloLiga.transform);
            }
            else if (nul && brilloNull != null)
            {
                andy.CambiarObjetivo(brilloNull.transform);
            }
            else if (lupi != null)
            {
                andy.CambiarObjetivo(lupi);
            }
        }
    }
    void Update()
    {
        if (panelVictoria != null && panelVictoria.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                BotonSiguiente();
        }
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