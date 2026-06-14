using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicaNivel5 : MonoBehaviour, ILogicaNivel
{
    private bool esModoRepaso = false;
    private bool nivelCompletado = false;
    private bool completandoNodo = false;
    [Header("Pantalla Derrota")]
    public GameObject panelDerrota;
    public TextMeshProUGUI textoAciertosDerrota;
    public TextMeshProUGUI textoFallosDerrota;
    [Header("Efectos Burbuja")]
    public GameObject prefabBurbuja;
    private bool esperandoCierreNivel = false;
    [Header("Información del Nivel UI")]
    public string nombreDelNivel = "Listas Dobles";
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
    float ultimoTiempoClic;
    [Header("Audios Diálogos Andy - Nivel 5")]
    public AudioClip audioConectarSalidaAnterior;
    public AudioClip audioErrorEliminacionInicio;
    public AudioClip audioErrorEliminacion;
    public AudioClip audioErrorEliminacionFinalNull;
    public AudioClip audioErrorEliminacionFinal;
    public AudioClip audioErrorNoRio;
    public AudioClip audioErrorNoLigaAnterior;
    public AudioClip audioMoverInicioError;
    public AudioClip audioErrorNoInfoNueva;
    public AudioClip audioFelicidadesInsertarInicio;
    public AudioClip audioFelicidadesInsertarFinal;
    public AudioClip audioFelicidadesEliminarInicio;
    public AudioClip audioFelicidadesFinalNivel;
    public AudioClip audioRecogerHeadEliminar;
    public AudioClip audioConectarCerdoDesdeHead;
    public AudioClip audioRecogerNullEliminar;
    public AudioClip audioConectarCerdoDesdeNull;
    public AudioClip audioPrimerNodoDobleInsertarFinal;
    public AudioClip audioVinculoRetroceso;
    public AudioClip audioCerrarSiguienteANull;
    public AudioClip audioDeNullANodo;
    public AudioClip audioSiguienteANull;
    public AudioClip audioConectarASiguiente;
    public AudioClip audioActualizarNull;
    public AudioClip audioExitoInsercionFinal;
    public AudioClip audioConectarSiguienteAlNuevo;
    public AudioClip audioMoverInicioACabecera;
    public AudioClip audioCerrarSiguienteConNull;
    public AudioClip audioConectarSiguienteAlNodo;
    public AudioClip audioExitoTotalInicio;
    public AudioClip audioEliminarInicio;
    public AudioClip audioEliminarFinal;
    public AudioClip audioPrimerNodoDobleInicio;
    public AudioClip audioConectarAnteriorSalida;
    public AudioClip audioInsertarInicio;
    public AudioClip audioInsertarFinal;
    public AudioClip audioPrepararNodoDoble;
    public AudioClip audioSiembraInstruccion;
    public AudioClip audioPrimerNodoDoble;
    public AudioClip audioConectarAnterior;
    public AudioClip audioConectarSiguiente;
    public AudioClip audioMoverInicio;
    public AudioClip audioCerrarConNull;
    public AudioClip audioExitoInicio;
    public AudioClip audioExitoFinal;
    public AudioClip audioIntroEliminarDoble;
    public AudioClip audioErrorKaos1;
    public AudioClip audioErrorKaos2;
    public AudioClip audioErrorKaos3;
    public AudioClip audioErrorNullLiga;
    public AudioClip audioErrorInicioLiga;
    public AudioClip audioErrorLigaInicio;
    [Header("Insignias")]
    public ControladorInsignia controladorInsignia;
    public Sprite insigniaDeEsteNivel;
    public Checkpoint checkpointFinal;
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoSeleccionar;
    public AudioClip sonidoAlerta;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
    public AudioClip sonidoCuy;
    [Header("Progreso")]
    public BarreraProgreso barreraSiguiente;
    public static LogicaNivel5 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;
    public LineRenderer lineaFijaPrev;
    private enum ModoOperacion { InsertarInicio, InsertarFinal, EliminarInicio, EliminarFinal }
    private ModoOperacion modoActual = ModoOperacion.InsertarInicio;
    [Header("Prefabs y Sprites")]
    public GameObject prefabVaca;
    public GameObject prefabCerdo;
    public GameObject prefabOveja;
    public Sprite spriteVaca;
    public Sprite spriteCerdo;
    public Sprite spriteOveja;
    public AudioClip sonidoVaca;
    public AudioClip sonidoCerdo;
    public AudioClip sonidoOveja;
    [Header("Referencias de Escena")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;
    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;
    private NodoManager managerActual;
    private List<NodoManager> listaNodos = new List<NodoManager>();
    private string[] nombresNodosInicio = { "Oveja", "Cerdo", "Vaca" };
    private string[] nombresNodosFinal = { "Vaca", "Cerdo", "Oveja" };
    private Transform puntoOrigenActual;
    private List<LineRenderer> lineasFijasActivas = new List<LineRenderer>();
    private LineRenderer enlaceActualAlNull;
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
        esModoRepaso = KaosController.nivelesTerminados.Contains("ListasDobles");
        nivelCompletado = false;
        completandoNodo = false;
        if (!esModoRepaso)
        {
            puntosAlIniciarNivel = UIManager.puntosGlobales;
            UIManager.puntosTemporales = 0;
            KaosController kaos = Object.FindFirstObjectByType<KaosController>(FindObjectsInactive.Include);
            if (kaos != null)
            {
                kaos.gameObject.SetActive(true);
                kaos.ResetearEstadoNivel("ListasDobles");
            }
        }
        else
        {
            Debug.Log("Modo Repaso Nivel 5: Puntaje protegido.");
            KaosController kaos = Object.FindFirstObjectByType<KaosController>(FindObjectsInactive.Include);
            if (kaos != null) kaos.gameObject.SetActive(false);
        }
        instancia = this;
        UIManager.instancia.logicaActiva = this;
        ResetearNivel();
        ActualizarCabeceraNivel5();
        UIManager.instancia.SetMochilaHabilitada(true);
        ActualizarPuntos();
        StartCoroutine(Intro());
    }
    void OnDisable()
    {
        if (UIManager.instancia != null && UIManager.instancia.logicaActiva == (ILogicaNivel)this)
            UIManager.instancia.logicaActiva = null;
        if (!esModoRepaso && !nivelCompletado)
        {
            UIManager.puntosGlobales = puntosAlIniciarNivel;
            UIManager.puntosTemporales = 0;
        }
        ResetearNivel();
    }
    public void DesconectarEnlacePorKaos()
    {
        if (esModoRepaso) return;
        if (completandoNodo) return;
        bool debePenalizar = false;
        if ((modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal) && managerActual != null) debePenalizar = true;
        if ((modoActual == ModoOperacion.EliminarInicio || modoActual == ModoOperacion.EliminarFinal) && cargandoAgua) debePenalizar = true;
        if (!debePenalizar) return;
        if (fase >= 3) return;
        fallosContador++;
        int puntosARestar = 5;
        if (UIManager.puntosTemporales >= puntosARestar)
        {
            UIManager.puntosTemporales -= puntosARestar;
        }
        else
        {
            puntosARestar -= UIManager.puntosTemporales;
            UIManager.puntosTemporales = 0;
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - puntosARestar);
        }
        ActualizarPuntos();
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError) masterSFX.PlayOneShot(sonidoError);
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            cargandoAgua = false;
            lineaAgua.positionCount = 0;
            ApagarBrillosGlobales();
            pasoConexion = 0;
            if (managerActual != null)
            {
                managerActual.ResetearNodo();
                EncenderBrilloEnNodo(managerActual.gameObject, "EntradaAnterior", false);
                EncenderBrilloEnNodo(managerActual.gameObject, "EntradaSiguiente", false);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", false);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
            }
            int limiteLineas = fase * 2;
            while (lineasFijasActivas.Count > limiteLineas)
            {
                LineRenderer ultima = lineasFijasActivas[lineasFijasActivas.Count - 1];
                lineasFijasActivas.RemoveAt(lineasFijasActivas.Count - 1);
                if (ultima != null) Destroy(ultima.gameObject);
            }
            if (modoActual == ModoOperacion.InsertarInicio)
            {
                if (fase == 0)
                {
                    if (brilloHead)
                    {
                        brilloHead.SetEncendido(true);
                        if (andy != null) andy.CambiarObjetivo(brilloHead.transform);
                    }
                }
                else
                {
                    EncenderBrilloEnNodo(listaNodos[0].gameObject, "SalidaAnterior", true);
                    SetPalpitarVisual(listaNodos[0].gameObject, "LetreroLigaIzq", true);
                }
            }
            else if (modoActual == ModoOperacion.InsertarFinal)
            {
                if (fase == 0)
                {
                    if (brilloHead)
                    {
                        brilloHead.SetEncendido(true);
                        if (andy != null) andy.CambiarObjetivo(brilloHead.transform);
                    }
                }
                else
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "SalidaAnterior", true);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", true);
                }
            }
            if (andy != null)
                andy.Decir("¡Cuidado! El golpe de Kaos rompió la secuencia. Debes rehacer el último paso.", audioErrorKaos2);
        }
        else
        {
            cargandoAgua = false;
            ApagarBrillosGlobales();
            ProximoPaso();
            if (andy != null) andy.Decir("¡Kaos interrumpió la purificación! Intenta reasignar los enlaces de nuevo.", audioErrorKaos3);
        }
    }
    void ActualizarCabeceraNivel5()
    {
        if (UIManager.instancia == null) return;
        string operacionTexto = "";
        switch (modoActual)
        {
            case ModoOperacion.InsertarInicio: operacionTexto = "Inserción al inicio de la lista doble"; break;
            case ModoOperacion.InsertarFinal: operacionTexto = "Inserción al final de la lista doble"; break;
            case ModoOperacion.EliminarInicio: operacionTexto = "Eliminación por el inicio de la lista doble"; break;
            case ModoOperacion.EliminarFinal: operacionTexto = "Eliminación por el final de la lista doble"; break;
        }
        UIManager.instancia.ConfigurarCabeceraNivel(nombreDelNivel, operacionTexto);
    }
    IEnumerator AnimacionPuntos(bool esAumento)
    {
        textoPuntos.color = esAumento ? Color.green : Color.red;
        Vector3 escalaMax = escalaOriginalPuntos * 1.5f;
        float tiempo = 0f;
        float duracionPop = 0.08f;
        while (tiempo < duracionPop)
        {
            textoPuntos.transform.localScale = Vector3.Lerp(escalaOriginalPuntos, escalaMax, tiempo / duracionPop);
            tiempo += Time.deltaTime;
            yield return null;
        }
        textoPuntos.transform.localScale = escalaMax;
        tiempo = 0f;
        float duracionRetorno = 0.18f;
        Color colorInicialEfecto = textoPuntos.color;
        while (tiempo < duracionRetorno)
        {
            float t = tiempo / duracionRetorno;
            textoPuntos.transform.localScale = Vector3.Lerp(escalaMax, escalaOriginalPuntos, t);
            textoPuntos.color = Color.Lerp(colorInicialEfecto, colorOriginalPuntos, t);
            tiempo += Time.deltaTime;
            yield return null;
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
        UIManager.DescartarPuntos();
        StopAllCoroutines();
        if (!esModoRepaso)
            UIManager.puntosGlobales = puntosAlIniciarNivel;
        ActualizarPuntos();
        if (KaosController.instancia != null && !esModoRepaso)
            KaosController.instancia.ResetearEstadoNivel("ListasDobles");
        if (checkpointFinal != null)
            checkpointFinal.ResetearCheckpoint();
        if (barreraSiguiente != null)
            barreraSiguiente.Cerrar();
        if (controladorInsignia != null)
            controladorInsignia.ResetearInsignia();
        if (panelFinal != null) panelFinal.SetActive(false);
        CongelarLupi(false);
        ResetearNivel();
        ActualizarCabeceraNivel5();
        UIManager.instancia.SetMochilaHabilitada(true);
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
    void ActualizarPuntos()
    {
        if (textoPuntos)
            textoPuntos.text = (UIManager.puntosGlobales + UIManager.puntosTemporales).ToString();
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
        modoActual = ModoOperacion.InsertarInicio; 
        LimpiarDatosYEscena();
        ConfigurarUIParaModoActual();
    }
    void LimpiarDatosYEscena()
    {
        aciertosContador = 0;
        fallosContador = 0;
        if (panelFinal) panelFinal.SetActive(false);
        fase = 0; pasoConexion = 0; 
        cargandoAgua = false;
        lineaAgua.positionCount = 0; 
        lineaFija.positionCount = 0;
        listaNodos.Clear(); 
        managerActual = null;
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
        foreach (LineRenderer l in lineasFijasActivas) if (l != null) Destroy(l.gameObject);
        lineasFijasActivas.Clear();
        if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        if (lineaFijaPrev != null) lineaFijaPrev.positionCount = 0;
    }
    void ConfigurarUIParaModoActual()
    {
        if (UIManager.instancia == null) return;
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            UIManager.instancia.SetPrefabs(prefabOveja, prefabCerdo, prefabVaca);
            UIManager.instancia.SetSounds(sonidoOveja, sonidoCerdo, sonidoVaca);
            UIManager.instancia.ConfigurarBotonesUI(new Sprite[] { spriteOveja, spriteCerdo, spriteVaca }, nombresNodosInicio);
            UIManager.instancia.ConfigurarTextosChecklist("new Nodo(\"Oveja\");", "", "new Nodo(\"Cerdo\");", "", "new Nodo(\"Vaca\");");
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.SetPrefabs(prefabVaca, prefabCerdo, prefabOveja);
            UIManager.instancia.SetSounds(sonidoVaca, sonidoCerdo, sonidoOveja);
            UIManager.instancia.ConfigurarBotonesUI(new Sprite[] { spriteVaca, spriteCerdo, spriteOveja }, nombresNodosFinal);
            UIManager.instancia.ConfigurarTextosChecklist("new Nodo(\"Vaca\");", "", "new Nodo(\"Cerdo\");", "", "new Nodo(\"Oveja\");");
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist("", "delete(Vaca)", "", "delete(Oveja)", "");
        }
    }
    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        AudioClip clipReproducido = null;
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            clipReproducido = audioInsertarInicio;
            andy.Decir("¡Lupi! Las Listas Doblemente Ligadas utilizan punteros LIGADER y LIGAIZQ. Empezamos con la Inserción al Inicio para actualizar el puntero de acceso P.", clipReproducido);
            UIManager.instancia.SetMochilaHabilitada(true);
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            clipReproducido = audioInsertarFinal;
            andy.Decir("¡Excelente! Aplicaremos el algoritmo de Inserción al Final para actualizar el puntero F.", clipReproducido);
            UIManager.instancia.SetMochilaHabilitada(true);
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            if (fuenteAudio != null && sonidoAlerta != null)
                fuenteAudio.PlayOneShot(sonidoAlerta);
            clipReproducido = audioIntroEliminarDoble;
            andy.Decir("¡Alerta Lupi! El Kaos ha infectado el campo INFO de la cabecera. Eliminaremos el NODO en P reasignando las ligas bidireccionales.", clipReproducido);
            UIManager.instancia.SetMochilaHabilitada(false);
        }
        if (clipReproducido != null)
            yield return new WaitForSeconds(clipReproducido.length + 0.5f);
        else
            yield return new WaitForSeconds(3.0f);
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.MostrarMochilaSolo(true);
            andy.Decir("Cada animal es un NODO Q con dos campos de LIGA. Abre tu mochila para preparar la asignación de memoria.", audioPrepararNodoDoble);
            yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
            UIManager.instancia.MostrarChecklistSolo(true);
        }
        else
        {
            UIManager.instancia.MostrarChecklistSolo(true);
        }
        ProximoPaso();
    }
    void ProximoPaso()
    {
        tiempoInicioEstado = Time.time;
        ApagarBrillosGlobales();
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
            if (fase < nombres.Length)
            {
                andy.Decir("¡Lupi! Crea el NODO Q y define su campo INFO con el animal indicado.", audioSiembraInstruccion);
                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
                if (UIManager.instancia != null)
                    UIManager.instancia.MarcarTareaEnProgreso(fase * 2);
                pasoConexion = 0;
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            if (fuenteAudio != null && sonidoAlerta != null)
                fuenteAudio.PlayOneShot(sonidoAlerta);
            andy.Decir("¡La Vaca en P está infectada! Recoge la dirección de P; debemos reasignar la cabecera al Cerdo para aislar el NODO corrupto.", audioEliminarInicio);
            if (brilloHead)
            {
                brilloHead.SetEncendido(true);
                if (andy != null) andy.CambiarObjetivo(brilloHead.transform);
            }
            if (UIManager.instancia != null)
                UIManager.instancia.MarcarTareaEnProgreso(1);
            if (listaNodos.Count > 0 && listaNodos[0] != null)
                listaNodos[0].InfectarNodo();
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            andy.Decir("Sanaremos el final. El campo LIGADER de la Oveja debe apuntar a NULL para desvincular al Cerdo infectado en F.", audioEliminarFinal);
            EncenderBrilloEnNodo(puntoEntradaNull.parent.gameObject, "Null", true);
            SetPalpitarVisual(puntoEntradaNull.parent.gameObject, "LetreroNull", true);
            if (UIManager.instancia != null)
                UIManager.instancia.MarcarTareaEnProgreso(2);
            if (listaNodos.Count > 2 && listaNodos[2] != null)
                listaNodos[2].InfectarNodo();
        }
    }
    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        StartCoroutine(EsperarParaAsignarNodoDoble());        
    }

    IEnumerator EsperarParaAsignarNodoDoble()
    {
        yield return new WaitForSeconds(0.5f);
        managerActual = ObtenerNodoReciente();
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            if (fase == 0)
            {
                andy.Decir("Al ser el primer NODO de la lista, el puntero P debe apuntar a Q, y su campo LIGAIZQ debe inicializarse en NULL.", audioPrimerNodoDoble);
                if (brilloHead)
                {
                    brilloHead.SetEncendido(true);
                    if (andy != null) andy.CambiarObjetivo(brilloHead.transform);
                }
            }
            else
            {
                andy.Decir("¡Muy bien Lupi! El antiguo NODO en P tiene un nuevo predecesor. Conecta su LIGAIZQ a la dirección de memoria del nuevo NODO Q.", audioConectarAnterior);
                EncenderBrilloEnNodo(listaNodos[0].gameObject, "SalidaAnterior", true);
                SetPalpitarVisual(listaNodos[0].gameObject, "LetreroLigaIzq", true);
            }
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            if (fase == 0)
            {
                andy.Decir("Iniciemos la lista doble. El puntero P y el puntero F deben apuntar a este primer NODO Q.", audioPrimerNodoDobleInicio);
                if (brilloHead)
                {
                    brilloHead.SetEncendido(true);
                    if (andy != null) andy.CambiarObjetivo(brilloHead.transform);
                }
            }
            else
            {
                andy.Decir("El nuevo NODO Q debe conocer a su predecesor. Conecta su LIGAIZQ a la dirección del NODO que apunta F.", audioConectarAnteriorSalida);
                EncenderBrilloEnNodo(managerActual.gameObject, "SalidaAnterior", true);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", true);
            }
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (Time.time - ultimoTiempoClic < 0.1f) return;
        if (modoActual == ModoOperacion.InsertarFinal && fase > 0 && tipo == "Head")
        {
            ReproducirError();
            andy.Decir("¡Lupi, detente! Estamos insertando al final. El puntero de INICIO (P) ya está fijo y no debe modificarse.", audioErrorLigaInicio);
            return;
        }
        if (cargandoAgua && tipo == "Head")
        {
            ReproducirError();
            andy.Decir("¡Movimiento inválido! El puntero de Inicio (P) no puede recibir una conexión de una Liga; él es quien define dónde empieza la lista.", audioErrorNullLiga);
            return;
        }
        if (fuenteAudio != null && sonidoSeleccionar != null)
            fuenteAudio.PlayOneShot(sonidoSeleccionar);
        if (modoActual == ModoOperacion.InsertarInicio) LogicaInsertarInicio(tipo, objetoTocado);
        else if (modoActual == ModoOperacion.InsertarFinal) LogicaInsertarFinal(tipo, objetoTocado);
        else if (modoActual == ModoOperacion.EliminarInicio) LogicaEliminarInicio(tipo, objetoTocado);
        else if (modoActual == ModoOperacion.EliminarFinal) LogicaEliminarFinal(tipo, objetoTocado);
    }
    void LogicaInsertarInicio(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;
        if (Time.time - ultimoTiempoClic < 0.5f) return;
        NodoManager nodoViejoPrimero = (fase > 0) ? listaNodos[0] : null;
        if (!cargandoAgua)
        {
            if (fase > 0 && pasoConexion == 0 && tipo == "SalidaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoPrimero)
            {
                ultimoTiempoClic = Time.time;
                ApagarBrillosGlobales(); 
                IniciarCarga(nodoViejoPrimero.puntoSalidaAnterior, "EntradaSiguiente", managerActual.gameObject);
                SetPalpitarVisual(nodoViejoPrimero.gameObject, "LetreroLigaIzq", false);
                andy.Decir("Recogiste la liga de retroceso. Llévala al campo LIGADER del nuevo NODO Q para mantener la continuidad.", audioConectarASiguiente);
                return;
            }
            else if (fase > 0 && pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                ultimoTiempoClic = Time.time;
                ApagarBrillosGlobales(); 
                IniciarCarga(managerActual.puntoSalidaSiguiente, "EntradaAnterior", nodoViejoPrimero.gameObject);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                andy.Decir("Establezcamos el camino de ida. El campo LIGADER del nuevo NODO Q debe apuntar a la dirección que antes tenía P.", audioConectarSiguiente);
                return;
            }
            else if (((fase == 0 && pasoConexion == 0) || (fase > 0 && pasoConexion == 2)) && tipo == "Head")
            {
                ultimoTiempoClic = Time.time;
                ApagarBrillosGlobales();
                IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                if (brilloHead) brilloHead.SetEncendido(false);
                andy.Decir("Dirección de memoria recogida. Actualiza el puntero P para que apunte al nuevo NODO Q, convirtiéndolo en la nueva cabecera.", audioMoverInicio);
                return;
            }
            else if (fase == 0 && pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                ultimoTiempoClic = Time.time;
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                cargandoAgua = true;
                puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                if (brilloNull)
                {
                    brilloNull.SetEncendido(true);
                    if (andy != null) andy.CambiarObjetivo(brilloNull.transform);
                }
                andy.Decir("Como solo existe el NODO Q en la lista, su campo LIGADER debe apuntar a NULL para indicar el fin de la estructura.", audioCerrarConNull);
                return;
            }
        }
        else
        {
            if (fase > 0 && pasoConexion == 0 && tipo == "EntradaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                ultimoTiempoClic = Time.time;
                FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaSiguiente.position, "EntradaSiguiente", managerActual.gameObject, "LetreroLigaDer");
                pasoConexion = 1;
                ApagarBrillosGlobales();
                andy.Decir("¡Enlace de vuelta creado! Ahora conecta la SALIDA SIGUIENTE del nuevo NODO a la entrada de la Lista.", audioConectarSiguienteAlNodo);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                return;
            }
            else if (fase > 0 && pasoConexion == 1 && tipo == "EntradaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoPrimero)
            {
                ultimoTiempoClic = Time.time;
                LimpiarSegmentoEspecifico(puntoSalidaHead.position, nodoViejoPrimero.puntoEntradaAnterior.position);
                FinalizarPasoLigero(puntoOrigenActual.position, nodoViejoPrimero.puntoEntradaAnterior.position, "EntradaAnterior", nodoViejoPrimero.gameObject, "LetreroLigaIzq");
                pasoConexion = 2;
                andy.Decir("¡Excelente equilibrio Lupi! Los NODOS están vinculados por LIGADER y LIGAIZQ. Finalmente, mueve el puntero P a nuestra nueva cabecera.", audioMoverInicioACabecera);
                if (brilloHead)
                {
                    brilloHead.SetEncendido(true);
                    if (andy != null) andy.CambiarObjetivo(brilloHead.transform);
                }
                return;
            }
            else if (tipo == "EntradaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                if ((fase == 0 && pasoConexion == 0) || (fase > 0 && pasoConexion == 2))
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                    managerActual.ActivarHuerto();
                    if (fase == 0)
                    {
                        pasoConexion = 1;
                        andy.Decir("¡NODO inicializado! Guía la SALIDA SIGUIENTE hacia el cartel de NULL.", audioCerrarSiguienteConNull);
                        SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                    }
                    else
                    {
                        completandoNodo = true;
                        andy.Decir("¡Inserción completa Lupi! El NODO Q es ahora el primer elemento (P) de nuestra Lista Doblemente Ligada.", audioExitoInicio);
                        StartCoroutine(EsperarYFinalizar(true, audioExitoInicio.length));
                    }
                    return;
                }
            }
            else if (fase == 0 && pasoConexion == 1 && tipo == "Null")
            {
                ultimoTiempoClic = Time.time;
                FinalizarPasoLigero(puntoOrigenActual.position, puntoEntradaNull.position, "", null, "");
                completandoNodo = true;
                andy.Decir("¡Lista Doble creada!  El flujo bidireccional entre P y NULL está en perfecta armonía técnica.", audioExitoTotalInicio);
                StartCoroutine(EsperarYFinalizar(true, audioExitoTotalInicio.length));
                return;
            }
            if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        }
        if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        if (!cargandoAgua && (tipo.Contains("Entrada") || tipo == "Null")) return;
        if (cargandoAgua && (tipo.Contains("Salida") || tipo == "Head")) return;
        ReproducirError();
        if (fase == 0 && pasoConexion == 0)
            andy.Decir("¡Cuidado Lupi! Para iniciar la Lista Doble, debemos recoger el flujo directamente del INICIO.", audioErrorNoRio);
            else if (pasoConexion == 0 || pasoConexion == 1)
            andy.Decir("El algoritmo indica que debemos activar los enlaces bidireccionales del NODO.", audioErrorNoLigaAnterior);
            else
            andy.Decir("Para completar la inserción, activa el poste de INICIO.", audioMoverInicioError);
    }
    IEnumerator EsperarYFinalizar(bool exito, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (andy != null && andy.fuenteVoz != null)
        {
            while (andy.fuenteVoz.isPlaying)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.5f);
        FinalizarNodoCompleto(exito);
    }
    void LogicaInsertarFinal(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;
        if (Time.time - ultimoTiempoClic < 0.5f) return;
        NodoManager nodoViejoUltimo = (fase > 0) ? listaNodos[fase - 1] : null;
        if (!cargandoAgua)
        {
            if (fase == 0) 
            {
                if (pasoConexion == 0 && tipo == "Head")
                {
                    ultimoTiempoClic = Time.time;
                    IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                    andy.Decir("El puntero P debe reconocer a este primer NODO como el origen.", audioPrimerNodoDobleInsertarFinal);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    ultimoTiempoClic = Time.time;
                    cargandoAgua = true;
                    puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                    if (brilloNull) brilloNull.SetEncendido(true);
                    andy.Decir("Establece el final de la lista. El campo LIGADER del último NODO debe apuntar siempre al valor NULL.", audioCerrarSiguienteANull);
                    return;
                }
            }
            else 
            {
                if (pasoConexion == 0 && tipo == "SalidaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    ultimoTiempoClic = Time.time;
                    IniciarCarga(managerActual.puntoSalidaAnterior, "EntradaSiguiente", nodoViejoUltimo.gameObject);
                    andy.Decir("Para que el NODO Q reconozca al NODO anterior en la secuencia, recoge la dirección desde su campo LIGAIZQ.", audioConectarSalidaAnterior);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoUltimo)
                {
                    ultimoTiempoClic = Time.time;
                    IniciarCarga(nodoViejoUltimo.puntoSalidaSiguiente, "EntradaAnterior", managerActual.gameObject);
                    andy.Decir("Completa la dualidad: el campo LIGADER del antiguo NODO F debe apuntar ahora hacia el nuevo NODO Q.", audioConectarSiguienteAlNuevo);
                    return;
                }
                else if (pasoConexion == 2 && tipo == "Null")
                {
                    ultimoTiempoClic = Time.time;
                    cargandoAgua = true;
                    puntoOrigenActual = puntoEntradaNull;
                    ApagarBrillosGlobales();
                    EncenderBrilloEnNodo(managerActual.gameObject, "EntradaSiguiente", true);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                    andy.Decir("Recoge la dirección NULL y asígnala al LIGADER del NODO Q para terminar con la inserción al final.", audioDeNullANodo);
                    return;
                }
            }
        }
        else 
        {
            if (puntoOrigenActual == puntoSalidaHead && tipo == "EntradaSiguiente")
            {
                ReproducirError();
                andy.Decir("¡Error de lógica! El puntero de Inicio (P) define el comienzo de la lista, debe conectarse a la LIGA IZQUIERDA del primer nodo.", audioErrorInicioLiga);
                return;
            }
            if (fase == 0)
            {
                if (pasoConexion == 0 && tipo == "EntradaAnterior")
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                    managerActual.ActivarHuerto();
                    pasoConexion = 1;
                    andy.Decir("¡Primer NODO vinculado! Definamos el límite de la lista conectando el campo LIGADER hacia NULL.", audioSiguienteANull);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "Null")
                {
                    ultimoTiempoClic = Time.time;
                    if (brilloNull) brilloNull.SetEncendido(false);
                    FinalizarPasoLigero(puntoOrigenActual.position, puntoEntradaNull.position, "", null, "");
                    completandoNodo = true;
                    andy.Decir("¡LupiFantástico! La lista doble ha sido inicializada con P y F apuntando al mismo NODO.", audioExitoFinal);
                    StartCoroutine(EsperarYFinalizar(false, audioExitoFinal.length));
                    return;
                }
            }
            else
            {
                if (pasoConexion == 0 && tipo == "EntradaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoUltimo)
                {
                    ultimoTiempoClic = Time.time;
                    if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);
                    FinalizarPasoLigero(puntoOrigenActual.position, nodoViejoUltimo.puntoEntradaSiguiente.position, "EntradaSiguiente", nodoViejoUltimo.gameObject, "LetreroLigaDer");
                    pasoConexion = 1;
                    andy.Decir("¡Vínculo LIGAIZQ creado! Establece el flujo de avance actualizando el campo LIGADER del NODO previo.", audioVinculoRetroceso);
                    SetPalpitarVisual(nodoViejoUltimo.gameObject, "LetreroLigaDer", true);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "EntradaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                    managerActual.ActivarHuerto();
                    pasoConexion = 2;
                    andy.Decir("Finalmente, el campo LIGADER del nuevo NODO Q debe apuntar a NULL, y actualizaremos F hacia este nuevo final.", audioActualizarNull);
                    if (brilloNull)
                    {
                        brilloNull.SetEncendido(true);
                        if (andy != null) andy.CambiarObjetivo(brilloNull.transform);
                    }
                    return;
                }
                else if (pasoConexion == 2 && tipo == "EntradaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaSiguiente.position, "EntradaSiguiente", managerActual.gameObject, "LetreroLigaDer");
                    completandoNodo = true;
                    andy.Decir("¡Inserción al final completada! El puntero F se ha desplazado y el campo LIGADER del predecesor ha sido actualizado.", audioExitoInsercionFinal);
                    StartCoroutine(EsperarYFinalizar(false, audioExitoInsercionFinal.length));
                    return;
                }
            }
            if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        }
        if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        if (!cargandoAgua && tipo.Contains("Entrada")) return;
        if (!cargandoAgua && tipo == "Null" && !(fase > 0 && pasoConexion == 2)) return;
        if (cargandoAgua && (tipo.Contains("Salida") || tipo == "Head")) return;
        if (!andy.fuenteVoz.isPlaying)
        {
            ReproducirError();
            if (fase == 0 && pasoConexion == 0)
                andy.Decir("¡Cuidado Lupi! Para iniciar la Lista Doble, debemos recoger el flujo directamente del INICIO.", audioErrorNoRio);
            else if (pasoConexion == 0 || pasoConexion == 1)
                andy.Decir("El algoritmo indica que debemos activar los enlaces bidireccionales del NODO.", audioErrorNoLigaAnterior);
            else
                andy.Decir("Para completar la inserción, activa el poste de INICIO.", audioMoverInicioError);
        }
    }
    void LogicaEliminarInicio(string tipo, GameObject objetoTocado)
    {
        if (Time.time - ultimoTiempoClic < 0.5f) return;
        NodoManager nodoTocado = objetoTocado != null ? objetoTocado.GetComponentInParent<NodoManager>() : null;
        if (listaNodos.Count < 2) return; 
        NodoManager nodoCerdo = listaNodos[1];
        NodoManager nodoVaca = listaNodos[0];
        if (!cargandoAgua)
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                ultimoTiempoClic = Time.time;
                brilloHead.SetEncendido(false);
                IniciarCarga(puntoSalidaHead, "EntradaAnterior", nodoCerdo.gameObject);
                andy.Decir("Recogiste el puntero P. Reasígnalo al sucesor de la Vaca para que la lista comience en el Cerdo.", audioRecogerHeadEliminar);
                return;
            }
        }
        else
        {
            if (tipo == "EntradaAnterior" && nodoTocado == nodoCerdo)
            {
                ultimoTiempoClic = Time.time; 
                cargandoAgua = false;
                LimpiarSegmentosDeNodo(nodoVaca);
                CrearSegmentoFijo(puntoSalidaHead.position, nodoCerdo.puntoEntradaAnterior.position);
                andy.Decir("¡Perfecto! P ahora apunta al Cerdo. El campo LIGAIZQ del nuevo P debe ser NULL para liberar la memoria de la Vaca.", audioConectarCerdoDesdeHead);
                StartCoroutine(EsperarYFinalizar(false, audioConectarCerdoDesdeHead.length));
                StartCoroutine(SecuenciaEliminacionExito(nodoVaca, 1));
                return;
            }
            if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        }
        if (!cargandoAgua && (tipo.Contains("Entrada") || tipo == "Null")) return;
        if (cargandoAgua && (tipo.Contains("Salida") || tipo == "Head")) return;
        if (!andy.fuenteVoz.isPlaying)
        {
            if (!cargandoAgua)
            {
                ReproducirError();
                andy.Decir("Para eliminar un NODO de la cabecera, primero debemos recoger la dirección del poste de INICIO.", audioErrorEliminacionInicio);
            }
            else
            {
                ReproducirError();
                andy.Decir("Para eliminar un NODO sin romper el flujo, debemos reasignar el enlace de la parcela anterior.", audioErrorEliminacion);
            }
        }
    }
    void LogicaEliminarFinal(string tipo, GameObject objetoTocado)
    {
        if (Time.time - ultimoTiempoClic < 0.5f) return;
        NodoManager nodoTocado = objetoTocado != null ? objetoTocado.GetComponentInParent<NodoManager>() : null;
        if (listaNodos.Count < 3) return; 
        NodoManager nodoCerdo = listaNodos[1];
        NodoManager nodoOveja = listaNodos[2];
        if (!cargandoAgua)
        {
            if (tipo == "Null" && pasoConexion == 0)
            {
                ultimoTiempoClic = Time.time;
                cargandoAgua = true;
                puntoOrigenActual = puntoEntradaNull;
                if (brilloNull) brilloNull.SetEncendido(false);
                SetPalpitarVisual(puntoEntradaNull.parent.gameObject, "LetreroNull", false);
                SetPalpitarVisual(nodoCerdo.gameObject, "LetreroLigaDer", true);
                andy.Decir("Recoge el valor NULL para cerrar el flujo antes de que el puntero F alcance a la Oveja infectada.", audioRecogerNullEliminar);
                return;
            }
        }
        else
        {
            if (tipo == "EntradaSiguiente" && nodoTocado == nodoCerdo)
            {
                ultimoTiempoClic = Time.time;
                cargandoAgua = false;
                SetPalpitarVisual(nodoCerdo.gameObject, "LetreroLigaDer", false);
                if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);
                LimpiarSegmentosDeNodo(nodoOveja);
                LineRenderer lineaNull = Instantiate(lineaFija, transform);
                lineaNull.positionCount = 2;
                lineaNull.SetPosition(0, puntoEntradaNull.position);
                lineaNull.SetPosition(1, nodoCerdo.puntoSalidaSiguiente.position);
                enlaceActualAlNull = lineaNull;
                andy.Decir("¡Perfecto Lupi! El Cerdo ahora apunta a NULL. El campo LIGADER de la Oveja ha sido anulado y el NODO infectado está fuera de la memoria.", audioConectarCerdoDesdeNull);
                StartCoroutine(EsperarYFinalizar(false, audioConectarCerdoDesdeNull.length));
                StartCoroutine(SecuenciaEliminacionExito(nodoOveja, 2));
                return;
            }
            if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        }
        if (!cargandoAgua && tipo.Contains("Entrada")) return;
        if (cargandoAgua && (tipo == "Null" || tipo.Contains("Salida") || tipo == "Head")) return;
        if (!andy.fuenteVoz.isPlaying)
        {
            if (!cargandoAgua)
            {
                ReproducirError();
                andy.Decir("Para cerrar la lista, el algoritmo indica que debemos recoger el valor NULL.", audioErrorEliminacionFinalNull);
            }
            else
            {
                ReproducirError();
                andy.Decir("¡Lupi cuidado! Si conectas al puntero equivocado, el flujo se perderá en el vacío.", audioErrorEliminacionFinal);
            }
        }
    }
    void IniciarCarga(Transform origen, string proximoBrillo, GameObject nodoDestino)
    {
        if (origen == null) return;
        cargandoAgua = true;
        puntoOrigenActual = origen;
        ApagarBrillosGlobales();
        EncenderBrilloEnNodo(nodoDestino, proximoBrillo, true);
        if (proximoBrillo.Contains("Anterior")) SetPalpitarVisual(nodoDestino, "LetreroLigaIzq", true);
        else if (proximoBrillo.Contains("Siguiente")) SetPalpitarVisual(nodoDestino, "LetreroLigaDer", true);
    }
    void FinalizarPasoLigero(Vector3 origen, Vector3 destino, string brilloApagar, GameObject nodo, string palpitarApagar)
    {
        int puntos = CalcularPuntosDinamicos();
        cargandoAgua = false;
        CrearSegmentoFijo(origen, destino);
        puntoOrigenActual = null;
        SumarPuntos(puntos);
        tiempoInicioEstado = Time.time;
        EncenderBrilloEnNodo(nodo, brilloApagar, false);
        SetPalpitarVisual(nodo, palpitarApagar, false);
    }
    void FinalizarNodoCompleto(bool insertarAlInicio)
    {
        completandoNodo = false;
        cargandoAgua = false;
        if (modoActual != ModoOperacion.InsertarFinal)
        {
            if (!(insertarAlInicio && fase > 0))
            {
                LineRenderer lineaNull = Instantiate(lineaFija, transform);
                lineaNull.positionCount = 2;
                lineaNull.SetPosition(0, managerActual.puntoSalidaSiguiente.position);
                lineaNull.SetPosition(1, puntoEntradaNull.position);
                enlaceActualAlNull = lineaNull;
            }
        }
        else
        {
            enlaceActualAlNull = lineasFijasActivas[lineasFijasActivas.Count - 1];
        }
        if (brilloNull) brilloNull.SetEncendido(false);
        puntoOrigenActual = null;
        managerActual.DrenarAgua(); 
        if (insertarAlInicio) listaNodos.Insert(0, managerActual);
        else listaNodos.Add(managerActual);
        managerActual = null;
        UIManager.instancia.MarcarTareaCompletada(fase * 2);
        fase++;
        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else StartCoroutine(CambiarDeFaseAlgoritmo());
    }
    IEnumerator CambiarDeFaseAlgoritmo()
    {
        yield return new WaitForSeconds(0.3f);
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Lupifantástico! Has dominado la Inserción al Inicio. Protegiste el origen de nuestra Lista Doble con flujos de ida y vuelta.", audioFelicidadesInsertarInicio);
            if (audioFelicidadesInsertarInicio != null)
                yield return new WaitForSeconds(audioFelicidadesInsertarInicio.length + 0.5f);
            else
                yield return new WaitForSeconds(4f);
            LimpiarDatosYEscena();
            modoActual = ModoOperacion.InsertarFinal;
            ActualizarCabeceraNivel5();
            ConfigurarUIParaModoActual();
            UIManager.instancia.SetMochilaHabilitada(true); 
            StartCoroutine(Intro());
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Asombroso Lupi! Ahora el canal es fuerte hasta su último NODO. Has completado con éxito la Inserción al Final de la estructura.", audioFelicidadesInsertarFinal);
            if (audioFelicidadesInsertarFinal != null)
                yield return new WaitForSeconds(audioFelicidadesInsertarFinal.length + 0.5f);
            else
                yield return new WaitForSeconds(4f);
            modoActual = ModoOperacion.EliminarInicio;
            ActualizarCabeceraNivel5();
            fase = 0;
            pasoConexion = 0;
            cargandoAgua = false;
            ConfigurarUIParaModoActual();
            UIManager.instancia.SetMochilaHabilitada(false); 
            StartCoroutine(Intro());
        }
    }
    IEnumerator SecuenciaEliminacionExito(NodoManager nodo, int numeroTareaUI)
    {
        int puntos = CalcularPuntosDinamicos();
        ApagarBrillosGlobales();
        SumarPuntos(puntos);
        nodo.IniciarSecuenciaEliminacion();
        yield return new WaitForSeconds(2f);
        UIManager.instancia.MarcarTareaCompletada(numeroTareaUI);
        if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("¡La Vaca ha sido purificada y su memoria liberada! Ahora eliminaremos el NODO en F para sanar el final de la lista.", audioFelicidadesEliminarInicio);
            if (audioFelicidadesEliminarInicio != null)
                yield return new WaitForSeconds(audioFelicidadesEliminarInicio.length + 0.8f);
            else
                yield return new WaitForSeconds(4f);
            modoActual = ModoOperacion.EliminarFinal;
            if (fuenteAudio != null && sonidoAlerta != null)
                fuenteAudio.PlayOneShot(sonidoAlerta);
            ActualizarCabeceraNivel5();
            pasoConexion = 0;
            ConfigurarUIParaModoActual();
            UIManager.instancia.SetMochilaHabilitada(false);
            ProximoPaso();
        }
        else
        {
            nivelCompletado = true;
            UIManager.instancia.DesactivarTodoPostNivel();
            if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null)
            {
                barreraSiguiente.Abrir();
                checkpointFinal.AparecerYActivar();
                controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                if (!esModoRepaso && KaosController.instancia != null)
                    KaosController.instancia.RecibirDanoYDesaparecer("ListasDobles");
                esperandoCierreNivel = true;
            }
            if (!esModoRepaso)
                UIManager.ConfirmarPuntos();
            ActualizarPuntos();
            CongelarLupi(true);
            ReproducirNivelCompleto();
            andy.Decir("¡Victoria total Técnico de Caminos Dobles! Eres un maestro del flujo Bidireccional.", audioFelicidadesFinalNivel);
            StartCoroutine(MostrarResumenFinal());
        }
    }
    void ReproducirAcierto()
    {
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX != null)
        {
            if (sonidoAcierto != null) masterSFX.PlayOneShot(sonidoAcierto);
            if (sonidoCuy != null) masterSFX.PlayOneShot(sonidoCuy);
        }
    }
    public void MostrarDerrota()
    {
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
            if (textoAciertos) textoAciertos.text = aciertosContador.ToString();
            if (textoFallos) textoFallos.text = fallosContador.ToString();
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
    }
    void ReproducirError()
    {
        fallosContador++;
        if (UIManager.instancia != null)
            UIManager.instancia.RevisarDerrotaPorPorcentaje(aciertosContador, fallosContador);
        if (Time.timeScale == 0f)
        {
            CongelarLupi(true);
            return;
        }
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (!esModoRepaso)
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
        aciertosContador++;
        if (!esModoRepaso)
            UIManager.puntosTemporales += cant;
        ActualizarPuntos();
        if (textoPuntos != null)
        {
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        }
        if (prefabBurbuja != null)
        {
            Vector3 posicionAparicion = Vector3.zero;
            bool hayObjetivo = false;
            if (managerActual != null)
            {
                posicionAparicion = managerActual.transform.position;
                hayObjetivo = true;
            }
            else if (modoActual == ModoOperacion.EliminarInicio && listaNodos.Count > 0)
            {
                if (listaNodos[0] != null)
                {
                    posicionAparicion = listaNodos[0].transform.position;
                    hayObjetivo = true;
                }
            }
            else if (modoActual == ModoOperacion.EliminarFinal && listaNodos.Count > 2)
            {
                if (listaNodos[2] != null)
                {
                    posicionAparicion = listaNodos[2].transform.position;
                    hayObjetivo = true;
                }
            }
            if (hayObjetivo)
            {
                posicionAparicion.z = -1f;
                GameObject nuevaBurbuja = Instantiate(prefabBurbuja, posicionAparicion, Quaternion.identity);
                if (nuevaBurbuja.TryGetComponent<EfectoBurbuja>(out var efecto))
                {
                    efecto.Configurar(esModoRepaso ? 0 : cant);
                    Debug.Log($"[Nivel 5] Burbuja +{(esModoRepaso ? 0 : cant)} en {modoActual}");
                }
            }
        }
        ReproducirAcierto();
    }
    void ReproducirNivelCompleto()
    {
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoCompletado)
            masterSFX.PlayOneShot(sonidoCompletado);
    }
    void CrearSegmentoFijo(Vector3 inicio, Vector3 fin)
    {
        LineRenderer nuevaLinea = Instantiate(lineaFija, transform);
        nuevaLinea.positionCount = 2;
        nuevaLinea.SetPosition(0, inicio);
        nuevaLinea.SetPosition(1, fin);
        lineasFijasActivas.Add(nuevaLinea);
    }
    bool EsPuntoCercano(Vector3 a, Vector3 b) { return Vector3.Distance(a, b) < 0.1f; }
    void LimpiarSegmentoEspecifico(Vector3 pA, Vector3 pB)
    {
        for (int i = lineasFijasActivas.Count - 1; i >= 0; i--)
        {
            LineRenderer lr = lineasFijasActivas[i];
            if (lr == null) continue;
            Vector3 p0 = lr.GetPosition(0);
            Vector3 p1 = lr.GetPosition(1);
            if ((EsPuntoCercano(p0, pA) && EsPuntoCercano(p1, pB)) || (EsPuntoCercano(p0, pB) && EsPuntoCercano(p1, pA)))
            {
                Destroy(lr.gameObject);
                lineasFijasActivas.RemoveAt(i);
            }
        }
    }
    void LimpiarSegmentosDeNodo(NodoManager nodo)
    {
        Vector3 entAnt = nodo.puntoEntradaAnterior.position;
        Vector3 salSig = nodo.puntoSalidaSiguiente.position;
        Vector3 salAnt = nodo.puntoSalidaAnterior.position;
        Vector3 entSig = nodo.puntoEntradaSiguiente.position;
        for (int i = lineasFijasActivas.Count - 1; i >= 0; i--)
        {
            LineRenderer lr = lineasFijasActivas[i];
            if (lr == null) continue;
            Vector3 p0 = lr.GetPosition(0);
            Vector3 p1 = lr.GetPosition(1);
            if (EsPuntoCercano(p0, entAnt) || EsPuntoCercano(p1, entAnt) ||
                EsPuntoCercano(p0, salSig) || EsPuntoCercano(p1, salSig) ||
                EsPuntoCercano(p0, salAnt) || EsPuntoCercano(p1, salAnt) ||
                EsPuntoCercano(p0, entSig) || EsPuntoCercano(p1, entSig))
            {
                Destroy(lr.gameObject);
                lineasFijasActivas.RemoveAt(i);
            }
        }
    }
    void Update()
    {
        if (panelFinal != null && panelFinal.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                BotonSiguiente();
        }
        if (cargandoAgua && puntoOrigenActual != null)
        {
            lineaAgua.positionCount = 2;
            lineaAgua.SetPosition(0, puntoOrigenActual.position);
            lineaAgua.SetPosition(1, lupi.position);
        }
        else lineaAgua.positionCount = 0;
        if (esperandoCierreNivel && checkpointFinal != null && lupi != null)
        {
            if (Vector3.Distance(lupi.position, checkpointFinal.transform.position) < 1.5f)
                SceneManager.LoadScene("HistoriaFin");
        }
    }
    private NodoManager ObtenerNodoReciente()
    {
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
            if (nm.gameObject.name.Contains("(Clone)") && !listaNodos.Contains(nm)) return nm;
        return null;
    }
    void EncenderBrilloEnNodo(GameObject n, string identificadorBuscado, bool estado)
    {
        if (n == null) return;
        foreach (var t in n.GetComponentsInChildren<TriggerConexion>(true))
        {
            if (t.identificador.Equals(identificadorBuscado, System.StringComparison.OrdinalIgnoreCase))
            {
                EfectoLetrero ef = t.GetComponent<EfectoLetrero>();
                if (ef != null)
                {
                    ef.SetEncendido(estado);
                    if (estado && andy != null)
                        andy.CambiarObjetivo(ef.transform);
                }
            }
        }
    }
    void ApagarBrillosGlobales()
    {
        if (brilloHead) brilloHead.SetEncendido(false);
        if (brilloNull) brilloNull.SetEncendido(false);
        EfectoLetrero[] todosLosBrillos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None);
        foreach (var ef in todosLosBrillos)
            ef.SetEncendido(false);
        if (andy != null && lupi != null) andy.CambiarObjetivo(lupi);
    }
    private bool EsConexionValida(string tipoDestino)
    {
        if (cargandoAgua && tipoDestino == "Head")
        {
            return false;
        }
        return true;
    }
    void SetPalpitarVisual(GameObject n, string nombreLetrero, bool estado)
    {
        if (n == null) return;
        foreach (Transform hijo in n.GetComponentsInChildren<Transform>(true))
        {
            if (hijo.name.Contains(nombreLetrero))
            {
                EfectoLetrero ef = hijo.GetComponent<EfectoLetrero>();
                if (ef != null)
                {
                    ef.SetEncendido(estado);
                    if (estado && andy != null)
                        andy.CambiarObjetivo(ef.transform);
                }
            }
        }
    }
    void LimpiarNodosEscena()
    {
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (n.name.Contains("(Clone)")) Destroy(n.gameObject);
        }
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas)
        {
            z.ResetearZona(); 
        }
        if (LogicaNivel1.instancia != null)
            LogicaNivel1.instancia.ResetearNivelSilencioso();
    }
    IEnumerator EsperarSiguiente() { 
        yield return new WaitForSeconds(1.0f); 
        ProximoPaso(); 
    }
}