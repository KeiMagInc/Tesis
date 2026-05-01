using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel5 : MonoBehaviour, ILogicaNivel
{
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

    // Listas de nombres según el modo para que al final quede la misma estructura
    private string[] nombresNodosInicio = { "Remolacha", "Zanahoria", "Rabano" };
    private string[] nombresNodosFinal = { "Rabano", "Zanahoria", "Remolacha" };

    private Transform puntoOrigenActual;
    private List<LineRenderer> lineasFijasActivas = new List<LineRenderer>();
    private LineRenderer enlaceActualAlNull;

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        instancia = this;
        UIManager.instancia.logicaActiva = this;

        // NUEVO: Asegurarnos de que inicie con todo oculto
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);

        ResetearNivel();
        StartCoroutine(IntroNivel5());
    }

    public void ResetearNivel()
    {
        modoActual = ModoOperacion.InsertarInicio;
        LimpiarDatosYEscena();
        ConfigurarUIParaModoActual();
    }

    void LimpiarDatosYEscena()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0; lineaFija.positionCount = 0;
        listaNodos.Clear(); managerActual = null;

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
            UIManager.instancia.ConfigurarTextosChecklist("Sembrar remolacha", "", "Sembrar zanahoria", "", "Sembrar rábano");
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.SetPrefabs(prefabRabano, prefabZanahoria, prefabRemolacha);
            UIManager.instancia.ConfigurarBotonesUI(new Sprite[] { spriteRabano, spriteZanahoria, spriteRemolacha }, nombresNodosFinal);
            UIManager.instancia.ConfigurarTextosChecklist("Sembrar rábano", "", "Sembrar zanahoria", "", "Sembrar remolacha");
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist("", "Eliminar Rábano", "", "", "");
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            UIManager.instancia.ConfigurarTextosChecklist("", "", "", "Eliminar Remolacha", "");
        }
    }

    IEnumerator IntroNivel5()
    {
        // NUEVO: Ocultar la interfaz en cada transición de algoritmo
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);

        yield return new WaitForSeconds(0.5f);
        if (modoActual == ModoOperacion.InsertarInicio)
            andy.Decir("¡Algoritmo de Lista Doble! Primero insertaremos al INICIO.");
        else if (modoActual == ModoOperacion.InsertarFinal)
            andy.Decir("¡Muy bien! Ahora aprenderemos a insertar al FINAL.");
        else if (modoActual == ModoOperacion.EliminarInicio)
            andy.Decir("¡Genial! Ahora vamos a ELIMINAR el primer nodo (Rábano).");

        yield return new WaitForSeconds(3.0f); // Un poco más de tiempo para que lean

        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            // NUEVO: Andy pide que abra la mochila ANTES de dar la instrucción de sembrar
            andy.Decir("Primero, abre tu mochila haciendo clic en ella para ver las semillas.");

            // Habilitamos el botón de la mochila
            UIManager.instancia.MostrarMochilaSolo(true);

            // NUEVO: El código se pausa aquí y no avanza hasta que el jugador abra la mochila
            yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        }

        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso(); // Recién aquí Andy da la instrucción de la primera semilla
    }

    void ProximoPaso()
    {
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
            if (fase < nombres.Length)
            {
                andy.Decir("Siembra la semilla de " + nombres[fase]);
                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
                pasoConexion = 0;
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("Recoge el agua del INICIO para saltar el Rábano y conectar directo a la Zanahoria.");
            if (brilloHead) brilloHead.SetEncendido(true);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            andy.Decir("Ahora recoge la SALIDA SIGUIENTE de la Zanahoria y llévala a NULL para desconectar la Remolacha.");
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "SalidaSiguiente", true);
            SetPalpitarVisual(listaNodos[1].gameObject, "LetreroLigaDer", true);
        }
    }

    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        managerActual = ObtenerNodoReciente();

        if (modoActual == ModoOperacion.InsertarInicio)
        {
            if (fase == 0)
            {
                andy.Decir("Recoge agua del INICIO.");
                if (brilloHead) brilloHead.SetEncendido(true);
            }
            else
            {
                andy.Decir("Recoge agua del INICIO para conectarlo al nuevo nodo.");
                if (brilloHead) brilloHead.SetEncendido(true);
            }
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            if (fase == 0)
            {
                andy.Decir("Recoge agua del INICIO.");
                if (brilloHead) brilloHead.SetEncendido(true);
            }
            else
            {
                andy.Decir("Recoge la SALIDA SIGUIENTE de " + nombresNodosFinal[fase - 1]);
                EncenderBrilloEnNodo(listaNodos[fase - 1].gameObject, "SalidaSiguiente", true);
                SetPalpitarVisual(listaNodos[fase - 1].gameObject, "LetreroLigaDer", true);
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

    // ==========================================
    // LOGICA: INSERTAR AL INICIO
    // ==========================================
    void LogicaInsertarInicio(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;
        NodoManager nodoViejoPrimero = (fase > 0) ? listaNodos[0] : null;

        if (!cargandoAgua)
        {
            if (pasoConexion == 0 && tipo == "Head")
            {
                IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                if (brilloHead) brilloHead.SetEncendido(false);
            }
            else if (pasoConexion == 1)
            {
                if (fase == 0 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    cargandoAgua = true;
                    puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                    if (brilloNull) brilloNull.SetEncendido(true);
                    andy.Decir("Lleva la salida a NULL.");
                }
                else if (fase > 0 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    IniciarCarga(managerActual.puntoSalidaSiguiente, "EntradaAnterior", nodoViejoPrimero.gameObject);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                }
            }
            else if (pasoConexion == 2 && fase > 0 && tipo == "SalidaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoPrimero)
            {
                IniciarCarga(nodoViejoPrimero.puntoSalidaAnterior, "EntradaSiguiente", managerActual.gameObject);
                SetPalpitarVisual(nodoViejoPrimero.gameObject, "LetreroLigaIzq", false);
            }
        }
        else
        {
            if (pasoConexion == 0 && tipo == "EntradaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                if (fase > 0) LimpiarSegmentoEspecifico(puntoSalidaHead.position, nodoViejoPrimero.puntoEntradaAnterior.position);
                FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                managerActual.ActivarHuerto();

                pasoConexion = 1;
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                if (fase == 0) andy.Decir("Ahora conecta la SALIDA SIGUIENTE a NULL.");
                else andy.Decir("Ahora enlaza la SALIDA SIGUIENTE a la entrada de " + nombresNodosInicio[fase - 1] + ".");
            }
            else if (pasoConexion == 1 && fase > 0 && tipo == "EntradaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoPrimero)
            {
                FinalizarPasoLigero(puntoOrigenActual.position, nodoViejoPrimero.puntoEntradaAnterior.position, "EntradaAnterior", nodoViejoPrimero.gameObject, "LetreroLigaIzq");

                pasoConexion = 2;
                andy.Decir("¡Bien! Ahora la liga hacia atrás: SALIDA ANTERIOR a la ENTRADA SIGUIENTE previa.");
                EncenderBrilloEnNodo(nodoViejoPrimero.gameObject, "SalidaAnterior", true);
                SetPalpitarVisual(nodoViejoPrimero.gameObject, "LetreroLigaIzq", true);
            }
            else if (pasoConexion == 2 && fase > 0 && tipo == "EntradaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaSiguiente.position, "EntradaSiguiente", managerActual.gameObject, "LetreroLigaDer");
                FinalizarNodoCompleto(true);
            }
            else if (fase == 0 && pasoConexion == 1 && tipo == "Null")
            {
                FinalizarNodoCompleto(true);
            }
        }
    }

    // ==========================================
    // LOGICA: INSERTAR AL FINAL
    // ==========================================
    void LogicaInsertarFinal(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;
        NodoManager nodoViejoUltimo = (fase > 0) ? listaNodos[fase - 1] : null;

        if (!cargandoAgua)
        {
            if (pasoConexion == 0)
            {
                if (fase == 0 && tipo == "Head")
                {
                    IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                    if (brilloHead) brilloHead.SetEncendido(false);
                }
                else if (fase > 0 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoUltimo)
                {
                    IniciarCarga(nodoViejoUltimo.puntoSalidaSiguiente, "EntradaAnterior", managerActual.gameObject);
                    SetPalpitarVisual(nodoViejoUltimo.gameObject, "LetreroLigaDer", false);
                }
            }
            else if (pasoConexion == 1)
            {
                if (fase == 0 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    cargandoAgua = true;
                    puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                    if (brilloNull) brilloNull.SetEncendido(true);
                    andy.Decir("Lleva la salida a NULL.");
                }
                else if (fase > 0 && tipo == "SalidaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    IniciarCarga(managerActual.puntoSalidaAnterior, "EntradaSiguiente", nodoViejoUltimo.gameObject);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", false);
                }
            }
            else if (pasoConexion == 2 && fase > 0 && tipo == "SalidaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                cargandoAgua = true;
                puntoOrigenActual = managerActual.puntoSalidaSiguiente;
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false);
                if (brilloNull) brilloNull.SetEncendido(true);
                andy.Decir("Cierra la lista conectando a NULL.");
            }
        }
        else
        {
            if (pasoConexion == 0 && tipo == "EntradaAnterior" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                // Si había conexión a Null del viejo último nodo, la borramos
                if (fase > 0 && enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);

                FinalizarPasoLigero(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position, "EntradaAnterior", managerActual.gameObject, "LetreroLigaIzq");
                managerActual.ActivarHuerto();

                pasoConexion = 1;
                if (fase == 0)
                {
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                    andy.Decir("Ahora conecta la SALIDA SIGUIENTE a NULL.");
                }
                else
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "SalidaAnterior", true);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", true);
                    andy.Decir("Crea el enlace hacia atrás: de SALIDA ANTERIOR a ENTRADA SIGUIENTE previa.");
                }
            }
            else if (pasoConexion == 1 && fase > 0 && tipo == "EntradaSiguiente" && objetoTocado.GetComponentInParent<NodoManager>() == nodoViejoUltimo)
            {
                FinalizarPasoLigero(puntoOrigenActual.position, nodoViejoUltimo.puntoEntradaSiguiente.position, "EntradaSiguiente", nodoViejoUltimo.gameObject, "LetreroLigaDer");

                pasoConexion = 2;
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true);
                andy.Decir("Cierra el nuevo nodo llevando la SALIDA SIGUIENTE a NULL.");
            }
            else if ((fase == 0 && pasoConexion == 1 || fase > 0 && pasoConexion == 2) && tipo == "Null")
            {
                FinalizarNodoCompleto(false);
            }
        }
    }

    // ==========================================
    // LOGICA: ELIMINAR INICIO Y FINAL
    // ==========================================
    void LogicaEliminarInicio(string tipo, GameObject objetoTocado)
    {
        NodoManager nodoTocado = objetoTocado != null ? objetoTocado.GetComponentInParent<NodoManager>() : null;
        NodoManager nodoZanahoria = listaNodos[1]; // Posición 1 es Zanahoria
        NodoManager nodoRabano = listaNodos[0];    // Posición 0 es Rábano

        if (!cargandoAgua)
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                brilloHead.SetEncendido(false);
                IniciarCarga(puntoSalidaHead, "EntradaAnterior", nodoZanahoria.gameObject);
                andy.Decir("Conecta el INICIO a la Zanahoria.");
            }
        }
        else
        {
            if (tipo == "EntradaAnterior" && nodoTocado == nodoZanahoria)
            {
                cargandoAgua = false;
                LimpiarSegmentosDeNodo(nodoRabano);
                CrearSegmentoFijo(puntoSalidaHead.position, nodoZanahoria.puntoEntradaAnterior.position);
                StartCoroutine(SecuenciaEliminacionExito(nodoRabano, 1));
            }
        }
    }

    void LogicaEliminarFinal(string tipo, GameObject objetoTocado)
    {
        NodoManager nodoTocado = objetoTocado != null ? objetoTocado.GetComponentInParent<NodoManager>() : null;
        NodoManager nodoZanahoria = listaNodos[1]; // Sigue siendo Zanahoria
        NodoManager nodoRemolacha = listaNodos[2]; // Posición 2 es Remolacha

        if (!cargandoAgua)
        {
            if (tipo == "SalidaSiguiente" && nodoTocado == nodoZanahoria && pasoConexion == 0)
            {
                cargandoAgua = true;
                puntoOrigenActual = nodoZanahoria.puntoSalidaSiguiente;
                SetPalpitarVisual(nodoZanahoria.gameObject, "LetreroLigaDer", false);
                if (brilloNull) brilloNull.SetEncendido(true);
                andy.Decir("Conecta a NULL.");
            }
        }
        else
        {
            if (tipo == "Null")
            {
                cargandoAgua = false;
                if (brilloNull) brilloNull.SetEncendido(false);
                if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);

                LimpiarSegmentosDeNodo(nodoRemolacha);

                LineRenderer lineaNull = Instantiate(lineaFija, transform);
                lineaNull.positionCount = 2;
                lineaNull.SetPosition(0, nodoZanahoria.puntoSalidaSiguiente.position);
                lineaNull.SetPosition(1, puntoEntradaNull.position);
                enlaceActualAlNull = lineaNull;

                StartCoroutine(SecuenciaEliminacionExito(nodoRemolacha, 2));
            }
        }
    }

    // ==========================================
    // METODOS AUXILIARES Y TRANSICIONES
    // ==========================================
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
        cargandoAgua = false;
        CrearSegmentoFijo(origen, destino);
        puntoOrigenActual = null;
        SumarPuntos(10);
        EncenderBrilloEnNodo(nodo, brilloApagar, false);
        SetPalpitarVisual(nodo, palpitarApagar, false);
    }

    void FinalizarNodoCompleto(bool insertarAlInicio)
    {
        cargandoAgua = false;

        if (insertarAlInicio && fase > 0)
        {
            // En insertar inicio, la conexión a NULL se mantiene intacta desde el primer nodo.
        }
        else
        {
            // En insertar final, O insertar inicio fase 0, creamos la línea al Null
            LineRenderer lineaNull = Instantiate(lineaFija, transform);
            lineaNull.positionCount = 2;
            lineaNull.SetPosition(0, managerActual.puntoSalidaSiguiente.position);
            lineaNull.SetPosition(1, puntoEntradaNull.position);
            enlaceActualAlNull = lineaNull;
        }

        if (brilloNull) brilloNull.SetEncendido(false);
        puntoOrigenActual = null;
        SumarPuntos(20);
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
        yield return new WaitForSeconds(3f);

        if (modoActual == ModoOperacion.InsertarInicio)
        {
            andy.Decir("¡Excelente! Has dominado la inserción al INICIO.");
            yield return new WaitForSeconds(3f);
            LimpiarDatosYEscena();
            modoActual = ModoOperacion.InsertarFinal;
            ConfigurarUIParaModoActual();
            StartCoroutine(IntroNivel5());
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            andy.Decir("¡Increíble! Ahora dominas la inserción al FINAL.");
            yield return new WaitForSeconds(3f);
            modoActual = ModoOperacion.EliminarInicio;
            pasoConexion = 0;
            cargandoAgua = false;
            ConfigurarUIParaModoActual();
            StartCoroutine(IntroNivel5());
        }
    }

    IEnumerator SecuenciaEliminacionExito(NodoManager nodo, int numeroTareaUI)
    {
        ApagarBrillosGlobales();
        SumarPuntos(30);
        nodo.IniciarSecuenciaEliminacion();

        yield return new WaitForSeconds(2f);
        UIManager.instancia.MarcarTareaCompletada(numeroTareaUI);

        if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("¡Rábano eliminado! Ahora vamos a quitar el último nodo.");
            yield return new WaitForSeconds(3f);
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
            andy.Decir("¡Felicidades! Has completado todas las operaciones de Listas Dobles.");
            ReproducirNivelCompleto();
        }
    }

    void ReproducirNivelCompleto()
    {
        if (fuenteAudio && sonidoCompletado)
            for (int i = 0; i < 2; i++) fuenteAudio.PlayOneShot(sonidoCompletado);
    }

    // Funciones Gráficas y Limpieza por Distancia
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

    void SumarPuntos(int c) { UIManager.puntosGlobales += c; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    void LimpiarNodosEscena()
    {
        // 1. Destruimos las plantas existentes (los clones)
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (n.name.Contains("(Clone)")) Destroy(n.gameObject);
        }

        // 2. Buscamos todas las zonas de plantado y las reseteamos
        // Esto desactivará el check de "ocupado" y reactivará los colliders
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas)
        {
            z.ResetearZona(); // Asegúrate de que este método existe en tu script ZonaPlantado
        }

        // 3. Opcional: Si tienes una lógica base en Nivel 1 que controla el estado global del huerto
        if (LogicaNivel1.instancia != null)
        {
            LogicaNivel1.instancia.ResetearNivelSilencioso();
        }
    }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}