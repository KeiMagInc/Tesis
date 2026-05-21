using Mundo2;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LogicaNivel2 : MonoBehaviour, ILogicaNivel
{
    [Header("Información del Nivel UI")]
    public string nombreDelNivel = "Creación y Referencia";
    public string operacionDelNivel = "Identificar la dirección física y lógica";
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
    public AudioClip audioErrorEntradaLiga;
    public AudioClip audioIntroMochila;
    public AudioClip audioIntroChecklist;
    public AudioClip audioInstruccionSiembra;
    public AudioClip audioHuertoListo;
    public AudioClip audioDireccionHead;
    public AudioClip audioDatoInfo;
    public AudioClip audioLigaAbierta;
    public AudioClip audioFinalNivel;
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
    [Header("Sprites UI Originales")]
    public Sprite spriteTrigo;
    public Sprite spritePapa;
    public Sprite spriteCalabaza;
    private string[] nombresNodos = { "Trigo", "Calabaza", "Papa" };
    public static LogicaNivel2 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public Transform lupi;
    [Header("Prefabs Específicos Nivel 2")]
    public GameObject prefabTrigoN2;
    public GameObject prefabPapaN2;
    public GameObject prefabCalabazaN2;
    [Header("Conexiones y Brillos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;
    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;
    private NodoManager managerActual;
    private int[] mapaIndicesUI = { 0, 2, 4 };
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
        if (UIManager.instancia == null) return;
        UIManager.instancia.DesactivarTodoPostNivel();
        UIManager.instancia.ConfigurarCabeceraNivel(nombreDelNivel, operacionDelNivel);
        puntosAlIniciarNivel = UIManager.puntosGlobales;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabTrigoN2, prefabCalabazaN2, prefabPapaN2);
        Sprite[] imagenes = { spriteTrigo, spriteCalabaza, spritePapa };
        string[] nombres = { "Trigo", "Calabaza", "Papa" };
        UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);
        ResetearNivel();
        UIManager.instancia.ConfigurarTextosChecklist(
            "new Nodo(\"Trigo\");",
            "",
            "new Nodo(\"Calabaza\");",
            "",
            "new Nodo(\"Papa\");"
        );
        ActualizarPuntos();
        StartCoroutine(Intro());
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
            KaosController.instancia.ResetearEstadoNivel("CreacionReferencias");
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
        StartCoroutine(Intro());
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
        fase = 0; 
        pasoConexion = 0; 
        cargandoAgua = false;
        aciertosContador = 0;
        fallosContador = 0;
        if (panelFinal) panelFinal.SetActive(false);
        lineaAgua.positionCount = 0;
        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist(
                "new Nodo(\"Trigo\");",
                "",
                "new Nodo(\"Calabaza\");",
                "",
                "new Nodo(\"Papa\");"
            );
        }
        UIManager.instancia.ResetBotones();
        ApagarBrillos();
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();
        foreach (var p in GameObject.FindGameObjectsWithTag("Planta")) Destroy(p);
    }
    IEnumerator Intro()
    {
        yield return new WaitForSeconds(1f);
        UIManager.instancia.MostrarMochilaSolo(true);
        andy.Decir("¡Lupi! El Kaos ha borrado el rastro de la cosecha. Abre tu mochila y revisa las semillas para restaurar los Nodos de este valle.", audioIntroMochila);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        if (audioIntroMochila != null) yield return new WaitForSeconds(audioIntroMochila.length);
        andy.Decir("¡Lupifantástico!. Consulta el pergamino de objetivos. Debemos plantar los huertos en el orden lógico para que la Lista no se pierda en la memoria.", audioIntroChecklist);
        UIManager.instancia.MostrarChecklistSolo(true);
        if (audioIntroChecklist != null)
            yield return new WaitForSeconds(audioIntroChecklist.length + 0.5f);
        else
            yield return new WaitForSeconds(2.5f); 
        ProximoPaso();
    }

    void ProximoPaso()
    {
        if (fase >= nombresNodos.Length) return;
        string semillaActual = nombresNodos[fase];
        UIManager.instancia.SetSemillaPalpitar(semillaActual);
        andy.Decir("Planta el huerto. Recuerda que cada huerto es un NODO que necesita un valor en su P.INFO.", audioInstruccionSiembra);
        pasoConexion = 0;
        lineaAgua.positionCount = 0;
        tiempoInicioEstado = Time.time;
    }
    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        andy.Decir("¡Huerto listo! Ahora busca el poste de INICIO para obtener la dirección de memoria inicial.", audioHuertoListo);
        brilloHead.SetEncendido(true);
    }
    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        switch (tipo)
        {
            case "Head":
                if (pasoConexion == 0 && !cargandoAgua)
                {
                    cargandoAgua = true;
                    lineaAgua.positionCount = 2;
                    lineaAgua.SetPosition(0, puntoSalidaHead.position);
                    brilloHead.SetEncendido(false);
                    andy.Decir("¡Dirección obtenida! Conecta la manguera de luz a P.INFO para asignar el dato al NODO actual.", audioDireccionHead);
                    GameObject huerto = BuscarHuerto();
                    if (huerto != null) EncenderBrilloHijo(huerto, "Info", true);
                }
                break;
            case "EntradaHuerto":
                if (cargandoAgua && pasoConexion == 0)
                {
                    int puntosDinamicos = CalcularPuntosDinamicos();
                    managerActual = objetoTocado.GetComponentInParent<NodoManager>();
                    cargandoAgua = false;
                    lineaAgua.SetPosition(1, managerActual.puntoEntrada.position);
                    managerActual.ActivarHuerto();
                    pasoConexion = 1;
                    SumarPuntos(puntosDinamicos); 
                    tiempoInicioEstado = Time.time;
                    EncenderBrilloHijo(managerActual.gameObject, "Info", false);
                    EncenderBrilloHijo(managerActual.gameObject, "Liga", true);
                    andy.Decir("Dato guardado con éxito. Ahora abre la válvula P.LIGA, este canal de riego es el puntero que conectará con el siguiente destino.", audioDatoInfo);
                }
                else if (!cargandoAgua && pasoConexion == 0) ReproducirError();
                break;
            case "EntradaLiga":
                if (cargandoAgua && pasoConexion == 0)
                {
                    ReproducirError();
                    andy.Decir("¡Cuidado Lupi! El puntero de INICIO debe apuntar a P.INFO para inicializar el NODO, no a su válvula P.LIGA.", audioErrorEntradaLiga);
                }
                break;
            case "SalidaHuerto":
                if (pasoConexion == 1)
                {
                    cargandoAgua = true;
                    lineaAgua.positionCount = 4;
                    lineaAgua.SetPosition(2, managerActual.puntoSalida.position);
                    EncenderBrilloHijo(managerActual.gameObject, "Liga", false);
                    andy.Decir("El canal está abierto. Arrastra el enlace hasta el pozo NULL para finalizar esta secuencia.", audioLigaAbierta);
                    brilloNull.SetEncendido(true);
                    pasoConexion = 2;
                }
                else if (pasoConexion < 1) ReproducirError();
                break;
            case "Null":
                if (pasoConexion == 2 && cargandoAgua)
                {
                    int puntosDinamicos = CalcularPuntosDinamicos();
                    cargandoAgua = false;
                    lineaAgua.positionCount = 0;
                    if (managerActual != null)
                    {
                        managerActual.DrenarAgua();
                    }
                    lineaAgua.SetPosition(3, puntoEntradaNull.position);
                    brilloNull.SetEncendido(false);
                    UIManager.instancia.MarcarTareaCompletada(mapaIndicesUI[fase]);
                    fase++;
                    if (fase < 3)
                    {
                        SumarPuntos(puntosDinamicos); 
                        StartCoroutine(EsperarSiguiente());
                    }
                    else
                    {
                        SumarPuntos(puntosDinamicos, true);
                        UIManager.instancia.DesactivarTodoPostNivel();
                        if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
                        {
                            barreraSiguiente.Abrir();
                            checkpointFinal.AparecerYActivar();
                            controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                            KaosController.instancia.RecibirDanoYDesaparecer("CreacionReferencias");
                        }
                        CongelarLupi(true);
                        ReproducirNivelCompleto();
                        andy.Decir("¡Excelente trabajo, Arquitecto de referencias! Has creado tres Nodos perfectos y el flujo llega al pozo NULL sin fugas de memoria.", audioFinalNivel);
                        StartCoroutine(MostrarResumenFinal());
                    }
                }
                else if (pasoConexion < 2) ReproducirError();
                break;            
        }
    }
    void ReproducirError()
    {
        fallosContador++;
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (!KaosController.nivelesTerminados.Contains("CreacionReferencias"))
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    void Update() { if (cargandoAgua) lineaAgua.SetPosition(lineaAgua.positionCount - 1, lupi.position); }
    void SumarPuntos(int cant, bool silencioso = false)
    {
        if (KaosController.nivelesTerminados.Contains("CreacionReferencias")) return;
        aciertosContador++;
        UIManager.puntosGlobales += cant;
        ActualizarPuntos();
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        if (!silencioso && UIManager.instancia.fuenteVozAndy && sonidoAcierto)
            UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoAcierto);
    }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    GameObject BuscarHuerto()
    {
        string buscado = nombresNodos[fase].ToLower();
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (n.gameObject.name.ToLower().Contains(buscado) && n.gameObject.name.Contains("(Clone)"))
                return n.gameObject;
        }
        return null;
    }
    void EncenderBrilloHijo(GameObject r, string n, bool e)
    {
        if (!r) return;
        foreach (var b in r.GetComponentsInChildren<EfectoLetrero>(true))
        {
            if (b.gameObject.name.ToUpper().Contains(n.ToUpper()))
                b.SetEncendido(e);
        }
    }
    void ReproducirNivelCompleto()
    {
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoCompletado)
            masterSFX.PlayOneShot(sonidoCompletado);
    }
    void ApagarBrillos() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}