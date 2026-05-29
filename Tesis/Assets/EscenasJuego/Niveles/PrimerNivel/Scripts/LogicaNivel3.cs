using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LogicaNivel3 : MonoBehaviour, ILogicaNivel
{
    private float tiempoUltimaAccion = 0f;
    [Header("Efectos Burbuja")]
    public GameObject prefabBurbuja;
    [Header("Información del Nivel UI")]
    public string nombreDelNivel = "Listas Simples";
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
    [Header("Audios Diálogos Andy")]
    public AudioClip audioNodoListoPInicio;
    public AudioClip audioNodoListoQInicio;
    public AudioClip audioNodoListoPFinal;
    public AudioClip audioNodoListoQFinal;
    public AudioClip audioEliminarInicioP;
    public AudioClip audioEliminarFinalLiga;
    public AudioClip audioLlevaQAInfo;
    public AudioClip audioActivaLigaPFinal;
    public AudioClip audioActivaLigaQFinal;
    public AudioClip audioLlevaPANull;
    public AudioClip audioLlevaQANull;
    public AudioClip audioSiembraPInfo;
    public AudioClip audioSiembraQInfo;
    public AudioClip audioLlevaPAInfo;
    public AudioClip audioFaltaSiembraP;
    public AudioClip audioFaltaSiembraQ;
    public AudioClip audioLlevaAguaAInfoP;
    public AudioClip audioLlevaAguaAInfoQ;
    public AudioClip audioActivaLigaP;
    public AudioClip audioActivaLigaQ;
    public AudioClip audioLlevaLigaANull;
    public AudioClip audioConectarLigaAInfoQ;
    public AudioClip audioLlevaLigaANullFinal;
    public AudioClip audioNodoFisicoListoFinal;
    public AudioClip audioConectarLigaAInfo;
    public AudioClip audioErrorConexionInfo;
    public AudioClip audioErrorLigaNuevaParcela;
    public AudioClip audioErrorLigaPorNull;
    public AudioClip audioFaltaSiembra;
    public AudioClip audioErrorLigaPorInfo;
    public AudioClip audioLlevaAguaAInfo;
    public AudioClip audioActivaLigaNueva;
    public AudioClip audioNodoFisicoListo;
    public AudioClip audioIntroInsertarInicio;
    public AudioClip audioIntroInsertarFinal;
    public AudioClip audioIntroEliminarInicio;
    public AudioClip audioPrepararNodo;
    public AudioClip audioSiembraInstruccion;
    public AudioClip audioTocaInicioEliminar;
    public AudioClip audioTocaLigaEliminar;
    public AudioClip audioExitoReasignarHead;
    public AudioClip audioExitoFlujoFinal;
    public AudioClip audioApuntaInicioEliminar;
    public AudioClip audioConexionSeguraEliminar;
    public AudioClip audioApuntaNullEliminar;
    public AudioClip audioCerradoListaEliminar;
    public AudioClip audioAdiosNodo;
    public AudioClip audioExitoTotalNivel;
    [Header("Insignias")]
    public ControladorInsignia controladorInsignia;
    public Sprite insigniaDeEsteNivel;
    public Checkpoint checkpointFinal;
    [Header("Progreso")]
    public BarreraProgreso barreraSiguiente;
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoSeleccionar;
    public AudioClip sonidoSembrar;
    public AudioClip sonidoAlerta;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
    public AudioClip sonidoCuy;
    [Header("Sprites UI Originales")]
    public Sprite spriteTrigo;
    public Sprite spritePapa;
    public Sprite spriteCalabaza;
    public static LogicaNivel3 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;
    private enum ModoOperacion { InsertarInicio, InsertarFinal, EliminarInicio, EliminarFinal }
    private ModoOperacion modoActual = ModoOperacion.InsertarInicio;
    [Header("Prefabs Específicos Nivel 3")]
    public GameObject prefabPapaN3;
    public GameObject prefabTrigoN3;
    public GameObject prefabCalabazaN3;
    [Header("Conexiones y Brillos Fijos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;
    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;
    private NodoManager managerActual;
    private NodoManager managerAnterior;
    private List<NodoManager> listaNodos = new List<NodoManager>();
    private List<Vector3> puntosCadenaFija = new List<Vector3>();
    private string[] nombresNodosInicio = { "Papa", "Trigo", "Calabaza" };
    private string[] nombresNodosFinal = { "Calabaza", "Trigo", "Papa" };
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
        puntosAlIniciarNivel = UIManager.puntosGlobales;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabPapaN3, prefabTrigoN3, prefabCalabazaN3);
        UIManager.instancia.SetSounds(sonidoSembrar, sonidoSembrar, sonidoSembrar);
        Sprite[] imagenes = { spritePapa, spriteTrigo, spriteCalabaza };
        string[] nombres = { "Papa", "Trigo", "Calabaza" };
        UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);
        ResetearNivel();
        ActualizarCabeceraSegunModo();
        UIManager.instancia.SetMochilaHabilitada(true);
        StartCoroutine(Intro());
    }
    void ActualizarCabeceraSegunModo()
    {
        if (UIManager.instancia == null) return;
        string operacionTexto = "";
        switch (modoActual)
        {
            case ModoOperacion.InsertarInicio: operacionTexto = "Inserción al inicio de la lista"; break;
            case ModoOperacion.InsertarFinal: operacionTexto = "Inserción al final de la lista"; break;
            case ModoOperacion.EliminarInicio: operacionTexto = "Eliminación por el inicio de la lista"; break;
            case ModoOperacion.EliminarFinal: operacionTexto = "Eliminación por el final de la lista"; break;
        }
        UIManager.instancia.ConfigurarCabeceraNivel(nombreDelNivel, operacionTexto);
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
            KaosController.instancia.ResetearEstadoNivel("ListasSimples");
        if (checkpointFinal != null)
            checkpointFinal.ResetearCheckpoint();
        if (barreraSiguiente != null)
            barreraSiguiente.Cerrar();
        if (controladorInsignia != null)
            controladorInsignia.ResetearInsignia();
        if (panelFinal != null) panelFinal.SetActive(false);
        CongelarLupi(false);
        ResetearNivel();
        ActualizarCabeceraSegunModo();
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
        aciertosContador = 0;
        fallosContador = 0;
        if (panelFinal) panelFinal.SetActive(false);
        modoActual = ModoOperacion.InsertarInicio;
        fase = 0;
        pasoConexion = 0;
        cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        listaNodos.Clear();
        managerAnterior = null;
        managerActual = null;
        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            ActualizarTextosChecklistSegunAlgoritmo();
        }
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }

    void ActualizarTextosChecklistSegunAlgoritmo()
    {
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            UIManager.instancia.ConfigurarTextosChecklist(
                "new Nodo(\"Papa\");",
                "",
                "new Nodo(\"Trigo\");",
                "",
                "new Nodo(\"Calabaza\");"
            );
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.ConfigurarTextosChecklist(
                "new Nodo(\"Calabaza\");",
                "",
                "new Nodo(\"Trigo\");",
                "",
                "new Nodo(\"Papa\");"
            );
        }
    }
    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        AudioClip clipReproducido = null;
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            clipReproducido = audioIntroInsertarInicio;
            andy.Decir("¡Atención Lupi! Kaos ha borrado todas las parcelas. Debemos aplicar una INSERCIÓN AL INICIO para expandir la lista desde su cabecera.", clipReproducido);
            UIManager.instancia.SetMochilaHabilitada(true);
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            clipReproducido = audioIntroInsertarFinal;
            andy.Decir("¡Lupifantástico! Ahora aprenderemos la INSERCIÓN AL FINAL. Debemos extender el rastro de la cosecha hasta el último rincón del valle.", clipReproducido);
            UIManager.instancia.SetMochilaHabilitada(true);
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            if (fuenteAudio != null && sonidoAlerta != null)
                fuenteAudio.PlayOneShot(sonidoAlerta);
            clipReproducido = audioIntroEliminarInicio;
            andy.Decir("¡Alerta! El Kaos ha infectado el primer NODO. Debemos realizar una ELIMINACIÓN para proteger el resto de la estructura.", clipReproducido);
            UIManager.instancia.SetMochilaHabilitada(false);
        }
        if (clipReproducido != null)
            yield return new WaitForSeconds(clipReproducido.length + 0.5f);
        else
            yield return new WaitForSeconds(3.5f);
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.MostrarMochilaSolo(true);
            yield return new WaitForSeconds(0.5f);
            andy.Decir("Primero, prepara el nuevo NODO. Abre la mochila y elije una semilla para asignar un valor al campo P.INFO de esta parcela.", audioPrepararNodo);
            yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
            UIManager.instancia.MostrarChecklistSolo(true);
            if (audioPrepararNodo != null)
                yield return new WaitForSeconds(audioPrepararNodo.length + 0.2f);
            else
                UIManager.instancia.MostrarChecklistSolo(true);
            yield return new WaitForSeconds(3.5f);
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
                if (fase == 0)
                {
                    andy.Decir("Siembra la semilla para definir el contenido P^.INFO de la cabecera.", audioSiembraPInfo);
                }
                else
                {
                    andy.Decir("Siembra la semilla para definir el contenido Q^.INFO del nuevo NODO.", audioSiembraQInfo);
                }

                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("Enlaza el poste inicial P. Vamos a redirigir el puntero hacia P^.LIGA para liberar el NODO corrupto.", audioEliminarInicioP);
            brilloHead.SetEncendido(true);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            if (fuenteAudio != null && sonidoAlerta != null)
                fuenteAudio.PlayOneShot(sonidoAlerta);
            andy.Decir("El último NODO está perdido. Debemos modificar el campo LIGA del penúltimo NODO para que apunte a NULL.", audioEliminarFinalLiga);
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Liga", true);
        }
        pasoConexion = 0;
    }
    IEnumerator CambiarDeModo()
    {
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Lo lograste! Has reasignado el puntero INICIO correctamente. La lista ahora comienza con un nuevo elemento.", audioExitoReasignarHead);
            if (audioExitoReasignarHead != null)
                yield return new WaitForSeconds(audioExitoReasignarHead.length + 0.5f);
            else
                yield return new WaitForSeconds(5.0f);
            modoActual = ModoOperacion.InsertarFinal;
            ActualizarCabeceraSegunModo();
            LimpiarEscenaParaSiguienteAlgoritmo();
            ActualizarTextosChecklistSegunAlgoritmo();
            UIManager.instancia.SetMochilaHabilitada(true);
            StartCoroutine(Intro());
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Fantástico Lupi! Ahora el flujo recorre toda la estructura hasta el nuevo NODO final.", audioExitoFlujoFinal);
            if (audioExitoFlujoFinal != null)
                yield return new WaitForSeconds(audioExitoFlujoFinal.length + 0.5f);
            else
                yield return new WaitForSeconds(5.0f);
            modoActual = ModoOperacion.EliminarInicio;
            ActualizarCabeceraSegunModo();
            fase = 0;
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist("", "delete(Calabaza);", "", "delete(Papa);", "");
            UIManager.instancia.SetMochilaHabilitada(false);
            StartCoroutine(Intro());
        }
    }
    void LogicaEliminarInicio(string tipo, GameObject objetoTocado)
    {
        if (tipo == "Head" && !cargandoAgua)
        {
            if (brilloHead) brilloHead.SetEncendido(false);
            cargandoAgua = true;
            andy.Decir("Haremos que la variable auxiliar Q apunte a P. Ahora redefinamos el inicio.", audioApuntaInicioEliminar);
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Info", true);
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[1])
            {
                cargandoAgua = false;
                EncenderBrilloEnNodo(listaNodos[1].gameObject, "Info", false);
                andy.Decir("¡Puntero redefinido! P ahora contiene la dirección de Q^.LIGA.", audioConexionSeguraEliminar);

                StartCoroutine(SecuenciaEliminacionExitosa(0));
            }
            else { ReproducirError(); }
        }
        else if (tipo != "Head") { ReproducirError(); }
    }
    void LogicaEliminarFinal(string tipo, GameObject objetoTocado)
    {
        if (tipo == "SalidaHuerto" && !cargandoAgua)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[1])
            {
                EncenderBrilloEnNodo(listaNodos[1].gameObject, "Liga", false);
                cargandoAgua = true;
                brilloNull.SetEncendido(true);
                andy.Decir("Hemos recorrido la lista hasta que Q es el último y T el penúltimo. Ahora, para desconectar a Q, haz que T^.LIGA apunte a NULL.", audioApuntaNullEliminar);
            }
            else { ReproducirError(); }
        }
        else if (tipo == "Null" && cargandoAgua)
        {
            cargandoAgua = false;
            brilloNull.SetEncendido(false);
            andy.Decir("¡Asignación exitosa! T^.LIGA ahora es NULL El nodo Q ha sido aislado de la estructura.", audioCerradoListaEliminar);
            StartCoroutine(SecuenciaEliminacionExitosa(2));
        }
        else if (cargandoAgua) { ReproducirError(); }
    }
    IEnumerator SecuenciaEliminacionExitosa(int indiceNodo)
    {
        int puntos = CalcularPuntosDinamicos();
        AudioClip audioPrevio = (modoActual == ModoOperacion.EliminarInicio) ? audioConexionSeguraEliminar : audioCerradoListaEliminar;
        if (audioPrevio != null) yield return new WaitForSeconds(audioPrevio.length);
        ApagarBrillosGlobales();
        SumarPuntos(puntos, true);
        listaNodos[indiceNodo].IniciarSecuenciaEliminacion();
        yield return new WaitForSeconds(1.5f);
        ActualizarLineaFijaPostEliminacion();
        UIManager.instancia.MarcarTareaCompletada((fase * 2) + 1);
        fase++;
        if (modoActual == ModoOperacion.EliminarInicio)
        {
            ReproducirNivelCompleto();
            andy.Decir("El primer NODO ha sido removido con éxito. ¡Vamos por el último paso Lupi!", audioAdiosNodo);
            if (audioAdiosNodo != null)
                yield return new WaitForSeconds(audioAdiosNodo.length + 0.5f);
            else
                yield return new WaitForSeconds(3.5f);
            modoActual = ModoOperacion.EliminarFinal;
            ActualizarCabeceraSegunModo();
            UIManager.instancia.SetMochilaHabilitada(false);
            ProximoPaso(); 
        }
        else
        {
            UIManager.instancia.DesactivarTodoPostNivel();
            if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
            {
                barreraSiguiente.Abrir();
                checkpointFinal.AparecerYActivar();
                controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                KaosController.instancia.RecibirDanoYDesaparecer("ListasSimples");
            }
            CongelarLupi(true);
            ReproducirNivelCompleto();
            andy.Decir("¡Excelente Analista de Enlaces Simples! Has dominado las operaciones de INSERCIÓN y ELIMINACIÓN en Listas Simples. El valle está a salvo.", audioExitoTotalNivel);
            if (audioExitoTotalNivel != null)
                yield return new WaitForSeconds(audioExitoTotalNivel.length + 0.5f);
            StartCoroutine(MostrarResumenFinal());

        }
    }
    void ReproducirNivelCompleto()
    {
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoCompletado)
            masterSFX.PlayOneShot(sonidoCompletado);
    }
    void LimpiarEscenaParaSiguienteAlgoritmo()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        if (lineaAgua) lineaAgua.positionCount = 0;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        listaNodos.Clear();
        managerAnterior = null;
        managerActual = null;
        if (UIManager.instancia != null) UIManager.instancia.ResetBotones();
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }
    void FinalizarNodo()
    {
        int puntos = CalcularPuntosDinamicos();
        cargandoAgua = false;
        bool esUltimoDeFase = (fase == 2);
        SumarPuntos(puntos, esUltimoDeFase);
        managerActual.DrenarAgua();
        ApagarBrillosGlobales();
        UIManager.instancia.MarcarTareaCompletada(fase * 2);
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            listaNodos.Insert(0, managerActual);
            List<Vector3> nuevaRuta = new List<Vector3>() { managerActual.puntoEntrada.position, managerActual.puntoSalida.position };
            if (fase == 0) nuevaRuta.Add(puntoEntradaNull.position);
            else nuevaRuta.AddRange(puntosCadenaFija);
            puntosCadenaFija = nuevaRuta;
        }
        else
        {
            listaNodos.Add(managerActual);
            if (fase == 0) puntosCadenaFija = new List<Vector3>() { puntoSalidaHead.position, managerActual.puntoEntrada.position, managerActual.puntoSalida.position, puntoEntradaNull.position };
            else
            {
                puntosCadenaFija.RemoveAt(puntosCadenaFija.Count - 1);
                puntosCadenaFija.Add(managerActual.puntoEntrada.position);
                puntosCadenaFija.Add(managerActual.puntoSalida.position);
                puntosCadenaFija.Add(puntoEntradaNull.position);
            }
        }
        lineaFija.positionCount = puntosCadenaFija.Count;
        lineaFija.SetPositions(puntosCadenaFija.ToArray());
        managerAnterior = managerActual;
        managerActual = null;
        fase++;
        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else StartCoroutine(CambiarDeModo());
    }
    void Update()
    {
        if (panelFinal != null && panelFinal.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                BotonSiguiente();
        }
        if (puntoSalidaHead == null || lupi == null || puntoEntradaNull == null) return;
        ActualizarVisualManguera();
    }
    void ActualizarVisualManguera()
    {
        List<Vector3> puntosActivos = new List<Vector3>();
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            puntosActivos.Add(puntoSalidaHead.position);
            if (managerActual != null)
            {
                if (pasoConexion == 0)
                {
                    if (cargandoAgua) puntosActivos.Add(lupi.position);
                    else if (managerAnterior != null) puntosActivos.Add(managerAnterior.puntoEntrada.position);
                }
                else if (pasoConexion == 1) puntosActivos.Add(managerActual.puntoEntrada.position);
                else if (pasoConexion == 2)
                {
                    puntosActivos.Add(managerActual.puntoEntrada.position);
                    puntosActivos.Add(managerActual.puntoSalida.position);
                    if (cargandoAgua) puntosActivos.Add(lupi.position);
                }
            }
            else if (managerAnterior != null) puntosActivos.Add(managerAnterior.puntoEntrada.position);
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            if (managerActual != null)
            {
                Vector3 origen = (fase == 0 || managerAnterior == null) ? puntoSalidaHead.position : managerAnterior.puntoSalida.position;
                puntosActivos.Add(origen);
                if (pasoConexion == 0) { if (cargandoAgua) puntosActivos.Add(lupi.position); }
                else if (pasoConexion == 1) puntosActivos.Add(managerActual.puntoEntrada.position);
                else if (pasoConexion == 2) { puntosActivos.Add(managerActual.puntoEntrada.position); puntosActivos.Add(managerActual.puntoSalida.position); if (cargandoAgua) puntosActivos.Add(lupi.position); }
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            puntosActivos.Add(puntoSalidaHead.position);
            if (cargandoAgua) puntosActivos.Add(lupi.position);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            if (listaNodos.Count > 1 && listaNodos[1] != null)
                puntosActivos.Add(listaNodos[1].puntoSalida.position);
            if (cargandoAgua) puntosActivos.Add(lupi.position);
        }
        if (lineaAgua != null)
        {
            lineaAgua.positionCount = puntosActivos.Count;
            lineaAgua.SetPositions(puntosActivos.ToArray());
        }
    }
    void ActualizarLineaFijaPostEliminacion()
    {
        puntosCadenaFija.Clear();
        if (modoActual == ModoOperacion.EliminarInicio)
        {
            puntosCadenaFija.Add(puntoSalidaHead.position);
            puntosCadenaFija.Add(listaNodos[1].puntoEntrada.position);
            puntosCadenaFija.Add(listaNodos[1].puntoSalida.position);
            puntosCadenaFija.Add(listaNodos[2].puntoEntrada.position);
            puntosCadenaFija.Add(listaNodos[2].puntoSalida.position);
            puntosCadenaFija.Add(puntoEntradaNull.position);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            puntosCadenaFija.Add(puntoSalidaHead.position);
            puntosCadenaFija.Add(listaNodos[1].puntoEntrada.position);
            puntosCadenaFija.Add(listaNodos[1].puntoSalida.position);
            puntosCadenaFija.Add(puntoEntradaNull.position);
        }
        lineaFija.positionCount = puntosCadenaFija.Count;
        lineaFija.SetPositions(puntosCadenaFija.ToArray());
    }
    void LimpiarNodosEscena()
    {
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (n.gameObject.name.Contains("(Clone)")) Destroy(n.gameObject);
        }
        if (LogicaNivel1.instancia != null)
        {
            LogicaNivel1.instancia.ResetearNivelSilencioso();
        }
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas)
        {
            z.ResetearZona();
        }
    }
    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (Time.time - tiempoUltimaAccion < 0.1f) return;
        tiempoUltimaAccion = Time.time;
        if (fuenteAudio != null && sonidoSeleccionar != null)
            fuenteAudio.PlayOneShot(sonidoSeleccionar);
        switch (modoActual)
        {
            case ModoOperacion.InsertarInicio: LogicaInsertarInicio(tipo, objetoTocado); break;
            case ModoOperacion.InsertarFinal: LogicaInsertarFinal(tipo, objetoTocado); break;
            case ModoOperacion.EliminarInicio: LogicaEliminarInicio(tipo, objetoTocado); break;
            case ModoOperacion.EliminarFinal: LogicaEliminarFinal(tipo, objetoTocado); break;
        }
    }
    void LogicaInsertarInicio(string tipo, GameObject objetoTocado)
    {
        Debug.Log("Objeto tocado: " + objetoTocado.name + " | Tipo recibido: " + tipo);
        if (managerActual == null && (tipo == "Head" || tipo == "EntradaHuerto"))
        {
            if (fase == 0)
                andy.Decir("¡Despacio Lupi! Primero debemos definir el valor del campo P^.INFO sembrando la semilla indicada.", audioFaltaSiembraP);
            else
                andy.Decir("¡Despacio Lupi! Primero debemos definir el valor del campo Q^.INFO sembrando la semilla para el nuevo NODO.", audioFaltaSiembraQ);
            return;
        }
        if (cargandoAgua)
        {
            if (pasoConexion == 0)
            {
                if (tipo == "SalidaHuerto" || tipo == "Liga")
                {
                    andy.Decir("¡Error de direccionamiento! El puntero debe apuntar al campo P.INFO para entrar al nuevo NODO.", audioErrorLigaPorInfo);
                    ReproducirError();
                    return;
                }
                else if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    int puntos = CalcularPuntosDinamicos();
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(puntos);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    tiempoInicioEstado = Time.time;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", true);
                    if (fase == 0)
                        andy.Decir("¡Contenido asignado! Ahora activa el puntero P^.LIGA de la cabecera para definir el final de la estructura.", audioActivaLigaP);
                    else
                        andy.Decir("¡Contenido asignado! Ahora activa el puntero Q^.LIGA de este nuevo NODO para conectarlo con la lista existente.", audioActivaLigaQ);
                }
                else
                {
                    andy.Decir("¡Cuidado Lupi! El enlace debe ir al campo P.INFO de la nueva parcela para inicializar el NODO.", audioErrorConexionInfo);
                    ReproducirError();
                }
            }
            else if (pasoConexion == 2)
            {
                if (tipo == "Null" && fase == 0)
                {
                    FinalizarNodo();
                }
                else if (tipo == "EntradaHuerto" && fase > 0 && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior)
                {
                    FinalizarNodo();
                }
                else
                {
                    if (fase == 0)
                    {
                        andy.Decir("Como este es el final de la estructura, su campo P.LIGA debe apuntar hacia NULL.", audioErrorLigaPorNull);
                    }
                    else
                    {
                        andy.Decir("¡Error de direccionamiento! El puntero P.LIGA debe conectarse al campo P.INFO de la siguiente parcela.", audioErrorLigaNuevaParcela);
                    }
                    ReproducirError();
                }
            }
        }
        else
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                brilloHead.SetEncendido(false);
                cargandoAgua = true;
                EncenderBrilloEnNodo(managerActual.gameObject, "Info", true);

                if (fase == 0)
                    andy.Decir("Recogiste la dirección de memoria para el puntero inicial P. Llévala al campo P^.INFO.", audioLlevaAguaAInfoP);
                else
                    andy.Decir("Recogiste una dirección para el nuevo NODO Q. Llévala al campo Q^.INFO para crear el nuevo elemento.", audioLlevaAguaAInfoQ);
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Liga", false);
                cargandoAgua = true;
                pasoConexion = 2;
                if (fase == 0)
                {
                    brilloNull.SetEncendido(true);
                    andy.Decir("Como es el primer elemento de la estructura, su campo P.LIGA debe apuntar hacia NULL.", audioLlevaLigaANull);
                }
                else
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Info", true);
                    andy.Decir("Ahora conecta el puntero Q^.LIGA al campo INFO de la cabecera actual (P).", audioConectarLigaAInfoQ);
                }
            }
        }
    }
    void LogicaInsertarFinal(string tipo, GameObject objetoTocado)
    {
        Debug.Log("InsertarFinal -> Objeto tocado: " + objetoTocado.name + " | Tipo recibido: " + tipo);
        if (managerActual == null) return;
        if (cargandoAgua)
        {
            if (pasoConexion == 0) 
            {
                if (tipo == "SalidaHuerto" || tipo == "Liga")
                {
                    andy.Decir("¡Error de direccionamiento! El puntero debe apuntar al campo P.INFO para entrar al nuevo NODO.", audioErrorLigaPorInfo);
                    ReproducirError();
                    return; 
                }
                else if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    int puntos = CalcularPuntosDinamicos();
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(puntos);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    tiempoInicioEstado = Time.time;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", true);
                    if (fase == 0)
                        andy.Decir("¡Contenido asignado a P^.INFO! Ahora activa P^.LIGA. Como es el primer NODO, también inicializamos P en T.", audioActivaLigaPFinal);
                    else
                        andy.Decir("¡Contenido asignado a Q^.INFO! Ahora activa Q^.LIGA para cerrar el nuevo NODO.", audioActivaLigaQFinal);
                }
                else
                {
                    andy.Decir("¡Cuidado Lupi! Para integrar el NODO, el enlace debe apuntar obligatoriamente a su campo P.INFO.", audioErrorConexionInfo);
                    ReproducirError();
                }
            }
            else if (pasoConexion == 2) 
            {
                if (tipo == "Null") FinalizarNodo();
                else
                {
                    andy.Decir("Como este es el final de la estructura, su campo P.LIGA debe apuntar hacia NULL.", audioLlevaLigaANullFinal);
                    ReproducirError();
                }
            }
        }
        else 
        {
            if (pasoConexion == 0)
            {
                if (fase == 0 && tipo == "Head") { brilloHead.SetEncendido(false); cargandoAgua = true; }
                else if (fase > 0 && tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior)
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Liga", false);
                    cargandoAgua = true;
                }
                if (cargandoAgua)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", true);
                    if (fase == 0)
                        andy.Decir("Recogiste la dirección para la cabecera P. Llévala al campo P^.INFO.", audioLlevaPAInfo);
                    else
                        andy.Decir("Recogiste la dirección para el nuevo NODO Q. Llévala al campo Q^.INFO.", audioLlevaQAInfo);
                }
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Liga", false);
                cargandoAgua = true;
                pasoConexion = 2;
                brilloNull.SetEncendido(true);
                if (fase == 0)
                    andy.Decir("Este es el final actual. Lleva la conexión al pozo NULL.", audioLlevaPANull);
                else
                    andy.Decir("Este nuevo NODO será el último. Lleva la conexión a NULL.", audioLlevaQANull);
            }
        }
    }
    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        StartCoroutine(EsperarYAsignarNodo());
    }
    IEnumerator EsperarYAsignarNodo()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForFixedUpdate();
        managerActual = BuscarNuevoNodoEnEscena();

        if (managerActual != null)
        {
            if (modoActual == ModoOperacion.InsertarInicio || (modoActual == ModoOperacion.InsertarFinal && fase == 0))
            {
                brilloHead.SetEncendido(true);
                if (fase == 0)
                {
                    andy.Decir("¡Estructura física de la cabecera P lista! Ahora recupera la dirección desde el poste para inicializar la Lista.", audioNodoListoPInicio);
                }
                else
                {
                    andy.Decir("¡Estructura física de Q lista! Recupera una nueva dirección desde el poste para este nuevo NODO.", audioNodoListoQInicio);
                }
            }
            else if (modoActual == ModoOperacion.InsertarFinal)
            {
                if (fase == 0)
                {
                    brilloHead.SetEncendido(true);
                    andy.Decir("¡Estructura física de la cabecera P lista! Recupera la dirección desde el poste de inicio.", audioNodoListoPFinal);
                }
                else
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Liga", true);
                    andy.Decir("¡Estructura física de Q lista! Ahora activa el puntero LIGA del último nodo (T) para enlazarlo con el nuevo elemento.", audioNodoListoQFinal);
                }
            }
        }
        else
        {
            Debug.LogError("No se encontró el NODO en la escena.");
        }
    }
    NodoManager BuscarNuevoNodoEnEscena()
    {
        string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
        if (fase >= nombres.Length) return null;
        string buscado = nombres[fase].ToLower();
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (nm.gameObject.name.ToLower().Contains(buscado) &&
                nm.gameObject.name.Contains("(Clone)") &&
                !listaNodos.Contains(nm))
            {
                return nm;
            }
        }
        return null;
    }
    void EncenderBrilloEnNodo(GameObject nodo, string parte, bool encender) { if (nodo == null) return; foreach (var b in nodo.GetComponentsInChildren<EfectoLetrero>(true)) if (b.gameObject.name.ToUpper().Contains(parte.ToUpper())) b.SetEncendido(encender); }
    void SumarPuntos(int cant, bool silencioso = false)
    {
        if (KaosController.nivelesTerminados.Contains("ListasSimples")) return;

        if (prefabBurbuja != null)
        {
            Vector3 posicionAparicion = Vector3.zero;
            bool hayObjetivo = false;
            if (managerActual != null)
            {
                posicionAparicion = managerActual.transform.position;
                hayObjetivo = true;
            }
            else if (modoActual == ModoOperacion.EliminarInicio && listaNodos.Count > 0 && listaNodos[0] != null)
            {
                posicionAparicion = listaNodos[0].transform.position;
                hayObjetivo = true;
            }
            else if (modoActual == ModoOperacion.EliminarFinal && listaNodos.Count > 0)
            {
                posicionAparicion = listaNodos[listaNodos.Count - 1].transform.position;
                hayObjetivo = true;
            }

            if (hayObjetivo)
            {
                posicionAparicion.z = -1f;
                GameObject nuevaBurbuja = Instantiate(prefabBurbuja, posicionAparicion, Quaternion.identity);
                if (nuevaBurbuja.TryGetComponent<EfectoBurbuja>(out var efecto))
                {
                    efecto.Configurar(cant);
                    Debug.Log($"[Nivel 3] Burbuja +{cant} en modo {modoActual}");
                }
            }
        }
        aciertosContador++;
        UIManager.puntosGlobales += cant;
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        if (!silencioso && UIManager.instancia.fuenteVozAndy && sonidoAcierto)
        {
            if (sonidoAcierto) UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoAcierto);
            if (sonidoCuy) UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoCuy);
        }
    }
    void ReproducirError()
    {
        fallosContador++;
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (!KaosController.nivelesTerminados.Contains("ListasSimples"))
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    void ApagarBrillosGlobales() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None); foreach (var b in todos) b.SetEncendido(false); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}