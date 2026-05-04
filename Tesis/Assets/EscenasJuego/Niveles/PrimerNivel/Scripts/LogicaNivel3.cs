using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class LogicaNivel3 : MonoBehaviour, ILogicaNivel
{
    [Header("Audios Diálogos Andy")]
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
    public AudioClip audioLlevaLigaANull;
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
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
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
    void Awake() => instancia = this;
    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabPapaN3, prefabTrigoN3, prefabCalabazaN3);
        Sprite[] imagenes = { spritePapa, spriteTrigo, spriteCalabaza };
        string[] nombres = { "Papa", "Trigo", "Calabaza" };
        UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        ResetearNivel();
        StartCoroutine(Intro());
    }
    public void ResetearNivel()
    {
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
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            clipReproducido = audioIntroInsertarFinal;
            andy.Decir("¡Lupifantástico! Ahora aprenderemos la INSERCIÓN AL FINAL. Debemos extender el rastro de la cosecha hasta el último rincón del valle.", clipReproducido);
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            clipReproducido = audioIntroEliminarInicio;
            andy.Decir("¡Alerta! El Kaos ha infectado el primer Nodo. Debemos realizar una Eliminación para proteger el resto de la estructura.", clipReproducido);
        }
        if (clipReproducido != null)
            yield return new WaitForSeconds(clipReproducido.length + 0.5f);
        else
            yield return new WaitForSeconds(3.5f);
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.MostrarMochilaSolo(true);
            yield return new WaitForSeconds(0.5f);
            andy.Decir("Primero, prepara el nuevo NODO. Abre la mochila y elije una semilla para asignar un valor al campo INFO de esta parcela.", audioPrepararNodo);
            if (audioPrepararNodo != null)
                yield return new WaitForSeconds(audioPrepararNodo.length + 0.2f);
            else
                yield return new WaitForSeconds(3.5f);
            yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        }
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }
    void ProximoPaso()
    {
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
            if (fase < nombres.Length)
            {
                andy.Decir("Siembra la semilla para definir el contenido INFO del NODO.", audioSiembraInstruccion);
                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("Enlaza el poste de INICIO. Vamos a liberar el enlace que apunta al NODO corrupto.", audioTocaInicioEliminar);
            brilloHead.SetEncendido(true);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            andy.Decir("El último NODO está perdido. Abre la válvula LIGA del Trigo para redirigir el flujo.", audioTocaLigaEliminar);
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
            LimpiarEscenaParaSiguienteAlgoritmo();
            ActualizarTextosChecklistSegunAlgoritmo();
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
            fase = 0;
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist("", "delete(Calabaza);", "", "delete(Papa);", "");
            StartCoroutine(Intro());
        }
    }
    void LogicaEliminarInicio(string tipo, GameObject objetoTocado)
    {
        if (tipo == "Head" && !cargandoAgua)
        {
            if (brilloHead) brilloHead.SetEncendido(false);
            cargandoAgua = true;
            andy.Decir("Apunta el puntero de INICIO directamente a la INFO del Trigo. Saltaremos el NODO infectado.", audioApuntaInicioEliminar);
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Info", true);
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[1])
            {
                cargandoAgua = false;
                EncenderBrilloEnNodo(listaNodos[1].gameObject, "Info", false);
                andy.Decir("¡Conexión segura! El NODO infectado ha sido desplazado de la secuencia lógica.", audioConexionSeguraEliminar);
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
                andy.Decir("Para eliminar el último Nodo, apunta la LIGA del elemento anterior hacia NULL.", audioApuntaNullEliminar);
            }
        }
        else if (tipo == "Null" && cargandoAgua)
        {
            cargandoAgua = false;
            brilloNull.SetEncendido(false);
            andy.Decir("¡Perfecto Lupi! Has cerrado la lista en el NODO anterior, eliminando el rastro de Kaos.", audioCerradoListaEliminar);
            StartCoroutine(SecuenciaEliminacionExitosa(2));
        }
        else if (cargandoAgua) { ReproducirError(); }
    }
    IEnumerator SecuenciaEliminacionExitosa(int indiceNodo)
    {
        AudioClip audioPrevio = (modoActual == ModoOperacion.EliminarInicio) ? audioConexionSeguraEliminar : audioCerradoListaEliminar;
        if (audioPrevio != null) yield return new WaitForSeconds(audioPrevio.length);
        ApagarBrillosGlobales();
        SumarPuntos(10, true);
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
            ProximoPaso(); 
        }
        else
        {
            if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
            {
                barreraSiguiente.Abrir();
                checkpointFinal.AparecerYActivar();
                controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                KaosController.instancia.RecibirDanoYDesaparecer("ListasSimples");
            }
            andy.Decir("¡Excelente Analista de Enlaces Simples! Has dominado las operaciones de Inserción y Eliminación en Listas Simples. El valle está a salvo.", audioExitoTotalNivel);
            ReproducirNivelCompleto();
            if (audioExitoTotalNivel != null)
                yield return new WaitForSeconds(audioExitoTotalNivel.length + 0.5f);
        }
    }
    void ReproducirNivelCompleto()
    {
        if (fuenteAudio && sonidoCompletado)
            for (int i = 0; i < 2; i++) fuenteAudio.PlayOneShot(sonidoCompletado);
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
        cargandoAgua = false;
        bool esUltimoDeFase = (fase == 2);
        SumarPuntos(10, esUltimoDeFase);
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
            andy.Decir("¡Despacio Lupi! Primero debemos definir el valor del campo INFO sembrando la semilla indicada.", audioFaltaSiembra);
            return;
        }
        if (cargandoAgua)
        {
            if (pasoConexion == 0)
            {
                if (tipo == "SalidaHuerto" || tipo == "Liga")
                {
                    andy.Decir("¡Error de direccionamiento! El puntero debe apuntar al campo INFO para entrar al nuevo NODO.", audioErrorLigaPorInfo);
                    ReproducirError();
                    return;
                }
                else if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(10);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", true);
                    andy.Decir("¡Contenido asignado! Ahora activa el puntero LIGA de este NODO para conectarlo con la lista existente.", audioActivaLigaNueva);
                }
                else
                {
                    andy.Decir("¡Cuidado Lupi! El enlace debe ir al campo INFO de la nueva parcela para inicializar el NODO.", audioErrorConexionInfo);
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
                        andy.Decir("Como este es el final de la estructura, su campo LIGA debe apuntar hacia NULL.", audioErrorLigaPorNull);
                    }
                    else
                    {
                        andy.Decir("¡Error de direccionamiento! El puntero LIGA debe conectarse al campo INFO de la siguiente parcela.", audioErrorLigaNuevaParcela);
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
                andy.Decir("Recogiste la dirección de memoria. Llévala al campo INFO de la nueva parcela para inicializar el NODO.", audioLlevaAguaAInfo);
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Liga", false);
                cargandoAgua = true;
                pasoConexion = 2;
                if (fase == 0)
                {
                    brilloNull.SetEncendido(true);
                    andy.Decir("Como es el primer elemento de la estructura, su campo LIGA debe apuntar hacia NULL.", audioLlevaLigaANull);
                }
                else
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Info", true);
                    andy.Decir("Ahora conecta este NODO con el resto de la estructura.", audioConectarLigaAInfo);
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
                    andy.Decir("¡Error de direccionamiento! El puntero debe apuntar al campo INFO para entrar al nuevo NODO.", audioErrorLigaPorInfo);
                    ReproducirError();
                    return; 
                }
                else if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(10);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", true);
                    andy.Decir("¡Contenido asignado! Ahora activa el puntero LIGA de este NODO para conectarlo con la lista existente.", audioActivaLigaNueva);
                }
                else
                {
                    andy.Decir("¡Cuidado Lupi! Para integrar el NODO, el enlace debe apuntar obligatoriamente a su campo INFO.", audioErrorConexionInfo);
                    ReproducirError();
                }
            }
            else if (pasoConexion == 2) 
            {
                if (tipo == "Null") FinalizarNodo();
                else
                {
                    andy.Decir("Como este es el final de la estructura, su campo LIGA debe apuntar hacia NULL.", audioLlevaLigaANullFinal);
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
                    andy.Decir("Recogiste la dirección de memoria. Llévala al campo INFO de la nueva parcela para inicializar el nodo.", audioLlevaAguaAInfo);
                }
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Liga", false);
                cargandoAgua = true;
                pasoConexion = 2;
                brilloNull.SetEncendido(true);
                andy.Decir("Como este es el final de la estructura, su campo LIGA debe apuntar hacia NULL.", audioLlevaLigaANullFinal);
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
                andy.Decir("¡Estructura física lista! Ahora recupera el puntero desde el poste de INICIO para establecer el enlace lógico.", audioNodoFisicoListo);
            }
            else if (modoActual == ModoOperacion.InsertarFinal && fase > 0)
            {
                EncenderBrilloEnNodo(managerAnterior.gameObject, "Liga", true);
                andy.Decir("¡Estructura física lista! Ahora recupera el puntero desde la LIGA de la parcela anterior para extender la lista.", audioNodoFisicoListoFinal);
            }
        }
        else
        {
            Debug.LogError("No se encontró el nodo en la escena.");
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
        UIManager.puntosGlobales += cant;
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        if (!silencioso && fuenteAudio && sonidoAcierto) fuenteAudio.PlayOneShot(sonidoAcierto);
    }
    void ReproducirError()
    {
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError);
    }
    void ApagarBrillosGlobales() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None); foreach (var b in todos) b.SetEncendido(false); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}