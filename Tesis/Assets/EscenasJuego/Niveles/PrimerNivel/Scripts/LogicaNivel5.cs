using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicaNivel5 : MonoBehaviour, ILogicaNivel
{
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
    public int puntosMinimos = 1;
    public float tiempoLimite = 60f;
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
    public AudioClip audioConectarZanahoriaDesdeHead;
    public AudioClip audioRecogerNullEliminar;
    public AudioClip audioConectarZanahoriaDesdeNull;
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
    [Header("Insignias")]
    public ControladorInsignia controladorInsignia;
    public Sprite insigniaDeEsteNivel;
    public Checkpoint checkpointFinal;
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
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
    public GameObject prefabRabano;
    public GameObject prefabZanahoria;
    public GameObject prefabRemolacha;
    public Sprite spriteRabano;
    public Sprite spriteZanahoria;
    public Sprite spriteRemolacha;
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
    private string[] nombresNodosInicio = { "Remolacha", "Zanahoria", "Rabano" };
    private string[] nombresNodosFinal = { "Rabano", "Zanahoria", "Remolacha" };
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
        puntosAlIniciarNivel = UIManager.puntosGlobales;
        instancia = this;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        ResetearNivel();
        StartCoroutine(IntroNivel5());
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
        if (lupi != null && puntoInicioNivel != null)
            lupi.position = puntoInicioNivel.position;
        StartCoroutine(IntroNivel5());
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
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
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
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            UIManager.instancia.SetPrefabs(prefabRemolacha, prefabZanahoria, prefabRabano);
            UIManager.instancia.ConfigurarBotonesUI(new Sprite[] { spriteRemolacha, spriteZanahoria, spriteRabano }, nombresNodosInicio);
            UIManager.instancia.ConfigurarTextosChecklist("new Nodo(\"Remolacha\");", "", "new Nodo(\"Zanahoria\");", "", "new Nodo(\"Rábano\");");
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.SetPrefabs(prefabRabano, prefabZanahoria, prefabRemolacha);
            UIManager.instancia.ConfigurarBotonesUI(new Sprite[] { spriteRabano, spriteZanahoria, spriteRemolacha }, nombresNodosFinal);
            UIManager.instancia.ConfigurarTextosChecklist("new Nodo(\"Rábano\");", "", "new Nodo(\"Zanahoria\");", "", "new Nodo(\"Remolacha\");");
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist("", "delete(Rábano)", "", "delete(Remolacha)", "");
        }
    }
    IEnumerator IntroNivel5()
    {
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        yield return new WaitForSeconds(0.5f);
        AudioClip clipReproducido = null;
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            clipReproducido = audioInsertarInicio;
            andy.Decir("¡Lupi! Las Listas Dobles permiten que el agua fluya hacia adelante y hacia atrás. ¡Insertemos al INICIO!", clipReproducido);
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            clipReproducido = audioInsertarFinal;
            andy.Decir("¡Excelente! Ahora extenderemos el canal hacia el horizonte. Aplicaremos la Inserción al FINAL de la estructura.", clipReproducido);
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            clipReproducido = audioIntroEliminarDoble;
            andy.Decir("¡Alerta Lupi! El Kaos ha infectado la cabecera. Debemos eliminar el primer NODO para sanar el flujo bidireccional.", clipReproducido);
        }
        if (clipReproducido != null)
            yield return new WaitForSeconds(clipReproducido.length + 0.5f);
        else
            yield return new WaitForSeconds(3.0f);
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            andy.Decir("Cada semilla es un NODO con dos enlaces. Abre tu mochila para preparar la siembra.", audioPrepararNodoDoble);
            UIManager.instancia.MostrarMochilaSolo(true);
            yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        }
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }
    void ProximoPaso()
    {
        tiempoInicioEstado = Time.time;
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
            if (fase < nombres.Length)
            {
                andy.Decir("¡Lupi! prepara la tierra y siembra la semilla indicada.", audioSiembraInstruccion);
                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
                pasoConexion = 0;
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("¡El primer nodo está infectado! Recoge la dirección del INICIO; debemos reasignar la cabecera directamente a la Zanahoria para saltar el Rábano corrupto.", audioEliminarInicio);
            if (brilloHead) brilloHead.SetEncendido(true);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            andy.Decir("Sanaremos el final de la estructura. Activa la SALIDA SIGUIENTE de la Zanahoria para redirigir su flujo hacia NULL y desvincular la Remolacha.", audioEliminarFinal);
            EncenderBrilloEnNodo(puntoEntradaNull.parent.gameObject, "Null", true);
            SetPalpitarVisual(puntoEntradaNull.parent.gameObject, "LetreroNull", true);
        }
    }
    public void AvanceSiembraExitosa()
    {
        tiempoInicioEstado = Time.time;
        UIManager.instancia.SetSemillaPalpitar("");
        managerActual = ObtenerNodoReciente();
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            if (fase == 0)
            {
                andy.Decir("Al ser el primer NODO, su campo ANTERIOR debe apuntar a la dirección de memoria de INICIO.", audioPrimerNodoDoble);
                if (brilloHead) brilloHead.SetEncendido(true);
            }
            else
            {
                andy.Decir("¡Muy bien Lupi! El antiguo primer NODO ahora tiene un nuevo predecesor. Conecta su SALIDA ANTERIOR a la ENTRADA del nuevo cultivo.", audioConectarAnterior);
                EncenderBrilloEnNodo(listaNodos[0].gameObject, "SalidaAnterior", true);
                SetPalpitarVisual(listaNodos[0].gameObject, "LetreroLigaIzq", true);
            }
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            if (fase == 0)
            {
                andy.Decir("¡Primer NODO del canal! Recoge el flujo del INICIO para establecer el punto de partida de nuestra lista doble.", audioPrimerNodoDobleInicio);
                if (brilloHead) brilloHead.SetEncendido(true);
            }
            else
            {
                andy.Decir("Cada nuevo integrante debe conocer sus raíces. Activa la SALIDA ANTERIOR de este para enlazarlo con su predecesor.", audioConectarAnteriorSalida);
                EncenderBrilloEnNodo(managerActual.gameObject, "SalidaAnterior", true);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", true);
            }
        }
    }
    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
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
                IniciarCarga(nodoViejoPrimero.puntoSalidaAnterior, "EntradaSiguiente", managerActual.gameObject);
                SetPalpitarVisual(nodoViejoPrimero.gameObject, "LetreroLigaIzq", false);
                andy.Decir("Recogiste el enlace de retroceso. Llévalo a la ENTRADA SIGUIENTE de la nueva parcela.", audioConectarASiguiente);
                return;
            }
            else if (fase > 0 && pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                ultimoTiempoClic = Time.time;
                IniciarCarga(managerActual.puntoSalidaSiguiente, "EntradaAnterior", nodoViejoPrimero.gameObject);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                andy.Decir("Ahora establezcamos el camino de ida. Lleva la SALIDA SIGUIENTE al campo ANTERIOR que te indica.", audioConectarSiguiente);
                return;
            }
            else if (((fase == 0 && pasoConexion == 0) || (fase > 0 && pasoConexion == 2)) && tipo == "Head")
            {
                ultimoTiempoClic = Time.time;
                IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                if (brilloHead) brilloHead.SetEncendido(false);
                andy.Decir("Recogiste la dirección de la cabecera. Llévala al campo ANTERIOR del nuevo NODO.", audioMoverInicio);
                return;
            }
            else if (fase == 0 && pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                ultimoTiempoClic = Time.time;
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                cargandoAgua = true;
                puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                if (brilloNull) brilloNull.SetEncendido(true);
                andy.Decir("Como solo hay un NODO, su flujo de salida debe descansar en NULL para cerrar la estructura.", audioCerrarConNull);
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
                andy.Decir("¡Excelente equilibrio Lupi! Los NODOS están unidos en ambos sentidos. Por último, mueve el puntero de INICIO a nuestra nueva cabecera.", audioMoverInicioACabecera);
                if (brilloHead) brilloHead.SetEncendido(true);
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
                        andy.Decir("¡Inserción completa Lupi! El nuevo NODO es ahora la cabecera de nuestra Lista Doble.", audioExitoInicio);
                        StartCoroutine(EsperarYFinalizar(true, audioExitoInicio.length));
                    }
                    return;
                }
            }
            else if (fase == 0 && pasoConexion == 1 && tipo == "Null")
            {
                ultimoTiempoClic = Time.time;
                FinalizarPasoLigero(puntoOrigenActual.position, puntoEntradaNull.position, "", null, "");
                andy.Decir("¡Lista Doble creada! El flujo ahora puede ir y venir en armonía.", audioExitoTotalInicio);
                StartCoroutine(EsperarYFinalizar(true, audioExitoTotalInicio.length));
                return;
            }
            if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        }
        if (objetoTocado != null && objetoTocado.transform == puntoOrigenActual) return;
        if (!cargandoAgua && (tipo.Contains("Entrada") || tipo == "Null")) return;
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
    IEnumerator EsperarYFinalizar(bool exito, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
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
                    andy.Decir("Iniciemos el canal recogiendo la dirección desde INICIO.", audioPrimerNodoDobleInsertarFinal);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    ultimoTiempoClic = Time.time;
                    cargandoAgua = true;
                    puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                    if (brilloNull) brilloNull.SetEncendido(true);
                    andy.Decir("Establece el final del flujo llevando la SALIDA SIGUIENTE al cartel de NULL.", audioCerrarSiguienteANull);
                    return;
                }
            }
            else 
            {
                if (pasoConexion == 0 && tipo == "SalidaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    ultimoTiempoClic = Time.time;
                    IniciarCarga(managerActual.puntoSalidaAnterior, "EntradaSiguiente", nodoViejoUltimo.gameObject);
                    andy.Decir("Para que el nuevo nodo reconozca a su antecesor, recoge su SALIDA ANTERIOR.", audioConectarSalidaAnterior);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoUltimo)
                {
                    ultimoTiempoClic = Time.time;
                    IniciarCarga(nodoViejoUltimo.puntoSalidaSiguiente, "EntradaAnterior", managerActual.gameObject);
                    andy.Decir("Ahora completa la dualidad lleva la SALIDA SIGUIENTE del antiguo nodo hacia el nuevo integrante.", audioConectarSiguienteAlNuevo);
                    return;
                }
                else if (pasoConexion == 2 && tipo == "Head")
                {
                    ultimoTiempoClic = Time.time;
                    IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                    return;
                }
                else if (pasoConexion == 2 && tipo == "Null")
                {
                    ultimoTiempoClic = Time.time;
                    cargandoAgua = true;
                    puntoOrigenActual = puntoEntradaNull;
                    if (brilloNull) brilloNull.SetEncendido(false);
                    EncenderBrilloEnNodo(managerActual.gameObject, "EntradaSiguiente", true);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                    andy.Decir("Finalmente, recoge la dirección de NULL y llévala al nuevo NODO.", audioDeNullANodo);
                    return;
                }
            }
        }
        else 
        {
            if (fase == 0)
            {
                if (pasoConexion == 0 && tipo == "EntradaAnterior")
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                    managerActual.ActivarHuerto();
                    pasoConexion = 1;
                    andy.Decir("¡Primer NODO vinculado! Ahora, definamos el fin del camino conectando la SALIDA SIGUIENTE a NULL.", audioSiguienteANull);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "Null")
                {
                    ultimoTiempoClic = Time.time;
                    if (brilloNull) brilloNull.SetEncendido(false);
                    FinalizarPasoLigero(puntoOrigenActual.position, puntoEntradaNull.position, "", null, "");
                    andy.Decir("¡LupiFantástico! La lista doble ha comenzado en el valle.", audioExitoFinal);
                    StartCoroutine(EsperarYFinalizar(false, audioExitoFinal.length));
                    return;
                }
            }
            else
            {
                if (pasoConexion == 0 && tipo == "EntradaSiguiente")
                {
                    ultimoTiempoClic = Time.time;
                    if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);
                    FinalizarPasoLigero(puntoOrigenActual.position, nodoViejoUltimo.puntoEntradaSiguiente.position, "EntradaSiguiente", nodoViejoUltimo.gameObject, "LetreroLigaDer");
                    pasoConexion = 1;
                    andy.Decir("¡Vínculo de retroceso creado! Ahora establece el flujo de avance hacia la nueva parcela.", audioVinculoRetroceso);
                    SetPalpitarVisual(nodoViejoUltimo.gameObject, "LetreroLigaDer", true);
                    return;
                }
                else if (pasoConexion == 1 && tipo == "EntradaAnterior")
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                    managerActual.ActivarHuerto();
                    pasoConexion = 2;
                    andy.Decir("Por último, actualiza la referencia de NULL hacia este nuevo final.", audioActualizarNull);
                    if (brilloNull) brilloNull.SetEncendido(true);
                    return;
                }
                else if (pasoConexion == 2 && tipo == "EntradaSiguiente") 
                {
                    ultimoTiempoClic = Time.time;
                    FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaSiguiente.position, "EntradaSiguiente", managerActual.gameObject, "LetreroLigaDer");
                    andy.Decir("¡Inserción al final completada! El canal se ha extendido en perfecta armonía Bidireccional.", audioExitoInsercionFinal);
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
        NodoManager nodoZanahoria = listaNodos[1];
        NodoManager nodoRabano = listaNodos[0];
        if (!cargandoAgua)
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                ultimoTiempoClic = Time.time;
                brilloHead.SetEncendido(false);
                IniciarCarga(puntoSalidaHead, "EntradaAnterior", nodoZanahoria.gameObject);
                andy.Decir("¡Alerta Lupi! Recoge la esencia del INICIO. Debemos reasignar el puntero para que salte al Rábano marchito y busque una nueva cabecera.", audioRecogerHeadEliminar);
                return;
            }
        }
        else
        {
            if (tipo == "EntradaAnterior" && nodoTocado == nodoZanahoria)
            {
                ultimoTiempoClic = Time.time; 
                cargandoAgua = false;
                LimpiarSegmentosDeNodo(nodoRabano);
                CrearSegmentoFijo(puntoSalidaHead.position, nodoZanahoria.puntoEntradaAnterior.position);
                andy.Decir("¡Perfecto! El flujo ahora reconoce a la Zanahoria como su nuevo origen. El NODO infectado ha sido aislado.", audioConectarZanahoriaDesdeHead);
                StartCoroutine(EsperarYFinalizar(false, audioConectarZanahoriaDesdeHead.length));
                StartCoroutine(SecuenciaEliminacionExito(nodoRabano, 1));
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
        NodoManager nodoZanahoria = listaNodos[1];
        NodoManager nodoRemolacha = listaNodos[2];
        if (!cargandoAgua)
        {
            if (tipo == "Null" && pasoConexion == 0)
            {
                ultimoTiempoClic = Time.time;
                cargandoAgua = true;
                puntoOrigenActual = puntoEntradaNull;
                if (brilloNull) brilloNull.SetEncendido(false);
                SetPalpitarVisual(puntoEntradaNull.parent.gameObject, "LetreroNull", false);
                SetPalpitarVisual(nodoZanahoria.gameObject, "LetreroLigaDer", true);
                andy.Decir("Recoge la dirección de NULL para cerrar el canal antes de que llegue a la Remolacha.", audioRecogerNullEliminar);
                return;
            }
        }
        else
        {
            if (tipo == "EntradaSiguiente" && nodoTocado == nodoZanahoria)
            {
                ultimoTiempoClic = Time.time;
                cargandoAgua = false;
                SetPalpitarVisual(nodoZanahoria.gameObject, "LetreroLigaDer", false);
                if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);
                LimpiarSegmentosDeNodo(nodoRemolacha);
                LineRenderer lineaNull = Instantiate(lineaFija, transform);
                lineaNull.positionCount = 2;
                lineaNull.SetPosition(0, puntoEntradaNull.position);
                lineaNull.SetPosition(1, nodoZanahoria.puntoSalidaSiguiente.position);
                enlaceActualAlNull = lineaNull;
                andy.Decir("¡Perfecto Lupi! La Zanahoria ahora apunta directamente al vacío de NULL, liberando la memoria de la parcela infectada.", audioConectarZanahoriaDesdeNull);
                StartCoroutine(EsperarYFinalizar(false, audioConectarZanahoriaDesdeNull.length));
                StartCoroutine(SecuenciaEliminacionExito(nodoRemolacha, 2));
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
        yield return new WaitForSeconds(1f);
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
            ConfigurarUIParaModoActual();
            StartCoroutine(IntroNivel5());
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Asombroso Lupi! Ahora el canal es fuerte hasta su último nodo. Has completado con éxito la Inserción al Final de la estructura.", audioFelicidadesInsertarFinal);
            if (audioFelicidadesInsertarFinal != null)
                yield return new WaitForSeconds(audioFelicidadesInsertarFinal.length + 0.5f);
            else
                yield return new WaitForSeconds(4f);
            modoActual = ModoOperacion.EliminarInicio;
            fase = 0;
            pasoConexion = 0;
            cargandoAgua = false;
            ConfigurarUIParaModoActual();
            StartCoroutine(IntroNivel5());
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
        if (modoActual == ModoOperacion.EliminarInicio)        {
            andy.Decir("¡El Rábano ha sido purificado! Ahora desvincularemos el último NODO infectado para sanar el valle.", audioFelicidadesEliminarInicio);
            if (audioFelicidadesEliminarInicio != null) yield return new WaitForSeconds(audioFelicidadesEliminarInicio.length + 0.5f);
            else yield return new WaitForSeconds(4f);
            modoActual = ModoOperacion.EliminarFinal;
            pasoConexion = 0;
            ConfigurarUIParaModoActual();
            ProximoPaso();
        }
        else
        {
            if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
            {
                barreraSiguiente.Abrir();
                checkpointFinal.AparecerYActivar();
                controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                KaosController.instancia.RecibirDanoYDesaparecer("ListasDobles");
            }
            CongelarLupi(true);
            ReproducirNivelCompleto();
            andy.Decir("¡Victoria total Técnico de Caminos Dobles! Eres un maestro del flujo Bidireccional.", audioFelicidadesFinalNivel);
            StartCoroutine(MostrarResumenFinal());
        }
    }
    void ReproducirAcierto()
    {
        if (fuenteAudio && sonidoAcierto) fuenteAudio.PlayOneShot(sonidoAcierto);
    }
    void ReproducirError()
    {
        fallosContador++;
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError);
        if (!KaosController.nivelesTerminados.Contains("ListasDobles"))
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    void ReproducirNivelCompleto()
    {
        if (fuenteAudio && sonidoCompletado)
            for (int i = 0; i < 2; i++) fuenteAudio.PlayOneShot(sonidoCompletado);
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
        if (cargandoAgua && puntoOrigenActual != null)
        {
            lineaAgua.positionCount = 2;
            lineaAgua.SetPosition(0, puntoOrigenActual.position);
            lineaAgua.SetPosition(1, lupi.position);
        }
        else lineaAgua.positionCount = 0;
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
                if (ef != null) ef.SetEncendido(estado);
            }
        }
    }
    void ApagarBrillosGlobales()
    {
        if (brilloHead) brilloHead.SetEncendido(false);
        if (brilloNull) brilloNull.SetEncendido(false);
        foreach (var ef in Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None)) ef.SetEncendido(false);
    }
    void SetPalpitarVisual(GameObject n, string nombreLetrero, bool estado)
    {
        if (n == null) return;
        foreach (Transform hijo in n.GetComponentsInChildren<Transform>(true))
        {
            if (hijo.name.Contains(nombreLetrero))
            {
                EfectoLetrero ef = hijo.GetComponent<EfectoLetrero>();
                if (ef != null) ef.SetEncendido(estado);
            }
        }
    }
    void SumarPuntos(int c) {
        if (KaosController.nivelesTerminados.Contains("ListasDobles")) return;
        aciertosContador++;
        UIManager.puntosGlobales += c;
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        ReproducirAcierto();
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
        {
            LogicaNivel1.instancia.ResetearNivelSilencioso();
        }
    }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}