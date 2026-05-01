using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel3 : MonoBehaviour, ILogicaNivel
{
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
                "Derecha: sembrar papa",  
                "",                       
                "Centro: sembrar trigo",  
                "",                       
                "Izquierda: sembrar calabaza"
            );
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.ConfigurarTextosChecklist(
                "Izquierda: sembrar calabaza", 
                "",                            
                "Centro: sembrar trigo",      
                "",                           
                "Derecha: sembrar papa"        
            );
        }
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        if (modoActual == ModoOperacion.InsertarInicio)
            andy.Decir("¡Algoritmo 5.1! Vamos a insertar al INICIO.");
        else if (modoActual == ModoOperacion.InsertarFinal)
            andy.Decir("¡Algoritmo 5.2! Ahora insertaremos al FINAL.");
        else if (modoActual == ModoOperacion.EliminarInicio)
            andy.Decir("¡Algoritmo 5.9! Vamos a eliminar el PRIMER nodo.");
        yield return new WaitForSeconds(2.5f);
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            andy.Decir("Primero, abre tu mochila para elegir la semilla que vamos a plantar.");
            UIManager.instancia.MostrarMochilaSolo(true);
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
                andy.Decir("Busca la semilla de " + nombres[fase] + " y siémbrala. \nLuego recoge el agua del letrero que indica.");
                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("Toca el INICIO para soltar la conexión del primer NODO.");
            brilloHead.SetEncendido(true);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            andy.Decir("Toca el LIGA del Trigo. Vamos a desconectar el último NODO de la lista.");
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Liga", true);
        }
        pasoConexion = 0;
    }

    IEnumerator CambiarDeModo()
    {
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Excelente trabajo! Has dominado la inserción por el frente de la lista.");
            yield return new WaitForSeconds(3f);
            modoActual = ModoOperacion.InsertarFinal;
            LimpiarEscenaParaSiguienteAlgoritmo();
            ActualizarTextosChecklistSegunAlgoritmo();
            StartCoroutine(Intro());
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            ReproducirNivelCompleto();
            andy.Decir("¡Increíble! Ya sabes cómo construir una lista añadiendo elementos al final.");
            yield return new WaitForSeconds(3f);
            modoActual = ModoOperacion.EliminarInicio;
            fase = 0;
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist(
                "", 
                "Eliminar Calabaza",     
                "",                  
                "Eliminar Papa",                 
                ""                 
            );
            StartCoroutine(Intro());
        }
    }

    void LogicaEliminarInicio(string tipo, GameObject objetoTocado)
    {
        if (tipo == "Head" && !cargandoAgua)
        {
            // APAGAR INMEDIATAMENTE
            if (brilloHead) brilloHead.SetEncendido(false);

            cargandoAgua = true;
            andy.Decir("Conecta el INICIO directamente a la INFO del Trigo. Así saltaremos la Calabaza.");
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Info", true);
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[1])
            {
                cargandoAgua = false;
                // Apagamos el brillo de Info del Trigo
                EncenderBrilloEnNodo(listaNodos[1].gameObject, "Info", false);
                andy.Decir("¡Bien! Al quitarle el agua desaparecerá de la lista.");
                StartCoroutine(SecuenciaEliminacionExitosa(0));
            }
            else { ReproducirError(); }
        }
        // Evitamos que el error suene si simplemente tocamos INICIO otra vez
        else if (tipo != "Head")
        {
            ReproducirError();
        }
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
                andy.Decir("Lleva el LIGA del Trigo hacia NULL. Esto dejará a la Papa fuera de la lista.");
            }
        }
        else if (tipo == "Null" && cargandoAgua)
        {
            cargandoAgua = false;
            brilloNull.SetEncendido(false);
            andy.Decir("¡Perfecto! El Trigo ahora es el final y la Papa ha sido eliminada.");
            StartCoroutine(SecuenciaEliminacionExitosa(2));
        }
        else if (cargandoAgua) { ReproducirError(); }
    }

    IEnumerator SecuenciaEliminacionExitosa(int indiceNodo)
    {
        ApagarBrillosGlobales();

        // CAMBIO 1: Silenciar el acierto para que no tape al de completado
        SumarPuntos(20, true);

        listaNodos[indiceNodo].IniciarSecuenciaEliminacion();
        yield return new WaitForSeconds(1.5f);
        ActualizarLineaFijaPostEliminacion();
        UIManager.instancia.MarcarTareaCompletada((fase * 2) + 1);
        fase++;

        if (modoActual == ModoOperacion.EliminarInicio)
        {
            // CAMBIO 2: Añadir sonido aquí para que suene al terminar de eliminar el primero
            ReproducirNivelCompleto();

            andy.Decir("¡Adiós calabaza! Último paso...");
            yield return new WaitForSeconds(1.5f);
            modoActual = ModoOperacion.EliminarFinal;
            ProximoPaso();
        }
        else
        {            
            if (barreraSiguiente != null)
            {
                barreraSiguiente.Abrir();
            }
            andy.Decir("¡Perfecto! Dominas las listas de Cairo.");
            ReproducirNivelCompleto();
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
        // 1. Destruimos las plantas anteriores (los clones)
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (n.gameObject.name.Contains("(Clone)")) Destroy(n.gameObject);
        }

        // 2. ¡LA CLAVE! Reiniciar la lógica base del Huerto (LogicaNivel1)
        if (LogicaNivel1.instancia != null)
        {
            LogicaNivel1.instancia.ResetearNivelSilencioso();
        }

        // 3. Reactivamos los colliders de las parcelas quitando el check
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
        if (managerActual == null && (tipo == "Head" || tipo == "EntradaHuerto"))
        {
            andy.Decir("Primero siembra la semilla de " + nombresNodosInicio[fase]);
            return;
        }
        if (cargandoAgua)
        {
            if (pasoConexion == 0) 
            {
                if (tipo == "SalidaHuerto")
                {
                    andy.Decir("¡No! El INICIO debe conectarse a la INFO (izquierda) para entrar al NODO.");
                    ReproducirError();
                    return;
                }
                if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(10);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", true);
                    andy.Decir("¡Muy bien! Ahora activa su LIGA.");
                }
            }
            else if (pasoConexion == 2) 
            {
                if (tipo == "Null" && fase == 0) FinalizarNodo();
                else if (tipo == "EntradaHuerto" && fase > 0 && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior) FinalizarNodo();
            }
        }
        else 
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                brilloHead.SetEncendido(false);
                cargandoAgua = true;
                EncenderBrilloEnNodo(managerActual.gameObject, "Info", true);
                andy.Decir("Lleva el agua al INFO de la planta nueva.");
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Liga", false);
                cargandoAgua = true;
                pasoConexion = 2;
                if (fase == 0) brilloNull.SetEncendido(true);
                else EncenderBrilloEnNodo(managerAnterior.gameObject, "Info", true);
            }
        }
    }

    void LogicaInsertarFinal(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;
        if (cargandoAgua)
        {
            if (pasoConexion == 0) 
            {
                if (tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    andy.Decir("¡Error! El flujo de la lista debe entrar por la INFO (izquierda) del nuevo nodo.");
                    ReproducirError();
                    return;
                }
                if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Info", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(10);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", true);
                    andy.Decir("¡Bien! Ahora lleva a la LIGA de esta nueva planta hacia NULL.");
                }
                else
                {
                    andy.Decir("Conecta la manguera al letrero de INFO (izquierdo) de la nueva planta.");
                    ReproducirError();
                }
            }
            else if (pasoConexion == 2)
            {
                if (tipo == "Null") FinalizarNodo();
                else andy.Decir("Como es el final de la lista, la LIGA debe ir a NULL.");
            }
        }
        else
        {
            if (pasoConexion == 0)
            {
                if (fase == 0 && tipo == "Head")
                {
                    brilloHead.SetEncendido(false);
                    cargandoAgua = true;
                }
                else if (fase > 0 && tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior)
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Liga", false);
                    cargandoAgua = true;
                }
                if (cargandoAgua) EncenderBrilloEnNodo(managerActual.gameObject, "Info", true);
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1)
            {
                if (objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Liga", false);
                    cargandoAgua = true;
                    pasoConexion = 2;
                    brilloNull.SetEncendido(true);
                }
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
            if (modoActual == ModoOperacion.InsertarInicio)
            {
                brilloHead.SetEncendido(true);
                andy.Decir("¡NODO listo! Ahora recoge el agua del INICIO para conectarla.");
            }
            else if (modoActual == ModoOperacion.InsertarFinal)
            {
                if (fase == 0)
                {
                    brilloHead.SetEncendido(true);
                    andy.Decir("¡NODO listo! Ahora recoge el agua del INICIO.");
                }
                else
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Liga", true);
                    andy.Decir("¡NODO listo! Recoge el agua de la LIGA del NODO anterior.");
                }
            }
        }
        else
        {
            Debug.LogError("No se encontró el nodo en la escena. Revisa que el nombre del Prefab contenga el nombre de la semilla.");
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
        UIManager.puntosGlobales += cant;
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        // Sonido de acierto
        if (!silencioso && fuenteAudio && sonidoAcierto) fuenteAudio.PlayOneShot(sonidoAcierto);
    }


    void ReproducirError()
    {
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError);
    }
    void ApagarBrillosGlobales() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None); foreach (var b in todos) b.SetEncendido(false); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}
