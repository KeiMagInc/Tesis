using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel5 : MonoBehaviour, ILogicaNivel
{
    public static LogicaNivel5 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;
    public LineRenderer lineaFijaPrev;

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
    private string[] nombresNodos = { "Rabano", "Zanahoria", "Remolacha" };
    private Transform puntoOrigenActual;
    private List<Vector3> puntosAdelante = new List<Vector3>();
    private List<Vector3> puntosAtras = new List<Vector3>();
    private List<LineRenderer> lineasFijasActivas = new List<LineRenderer>();
    private LineRenderer enlaceActualAlNull;
    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        instancia = this;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabRabano, prefabZanahoria, prefabRemolacha);
        UIManager.instancia.ConfigurarBotonesUI(new Sprite[] { spriteRabano, spriteZanahoria, spriteRemolacha }, nombresNodos);
        ResetearNivel();
        StartCoroutine(IntroNivel5());
    }

    void CrearSegmentoFijo(Vector3 inicio, Vector3 fin)
    {
        // Clonamos la lineaFija que ya tienes configurada con el color y grosor que te gusta
        LineRenderer nuevaLinea = Instantiate(lineaFija, transform);
        nuevaLinea.positionCount = 2;
        nuevaLinea.SetPosition(0, inicio);
        nuevaLinea.SetPosition(1, fin);
        lineasFijasActivas.Add(nuevaLinea);
    }

    public void ResetearNivel()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0; lineaFija.positionCount = 0;
        listaNodos.Clear(); managerActual = null;
        UIManager.instancia.ResetBotones();
        UIManager.instancia.ConfigurarTextosChecklist("Sembrar rabano", "", "Sembrar zanahoria", "", "Sembrar remolacha");
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
        puntosAdelante.Clear();
        puntosAtras.Clear();
        foreach (LineRenderer l in lineasFijasActivas) Destroy(l.gameObject);
        lineasFijasActivas.Clear();
        if (enlaceActualAlNull != null) Destroy(enlaceActualAlNull.gameObject);
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0; // Esta ahora solo sirve como "plantilla"
        if (lineaFijaPrev != null) lineaFijaPrev.positionCount = 0;
    }

    IEnumerator IntroNivel5()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Algoritmo de Lista Doble! Vamos a insertar al final.");
        yield return new WaitForSeconds(2.5f);
        UIManager.instancia.MostrarMochilaSolo(true);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        if (fase < nombresNodos.Length)
        {
            andy.Decir("Siembra la semilla de " + nombresNodos[fase]);
            UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
            pasoConexion = 0;
        }
    }

    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        managerActual = ObtenerNodoReciente();

        if (fase == 0)
        {
            andy.Decir("Recoge agua del INICIO.");
            if (brilloHead) brilloHead.SetEncendido(true);
        }
        else
        {
            andy.Decir("Conecta la SALIDA SIGUIENTE de " + nombresNodos[fase - 1]);
            EncenderBrilloEnNodo(listaNodos[fase - 1].gameObject, "SalidaSiguiente", true);
            // NUEVO: Palpita el letrero derecho del nodo anterior
            SetPalpitarVisual(listaNodos[fase - 1].gameObject, "LetreroLigaDer", true);
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (managerActual == null) return;
        NodoManager nodoTocado = objetoTocado?.GetComponentInParent<NodoManager>();
        NodoManager nodoPrevio = (fase > 0) ? listaNodos[fase - 1] : null;

        if (!cargandoAgua)
        {
            if (pasoConexion == 0)
            {
                if (fase == 0 && tipo == "Head")
                {
                    IniciarCarga(puntoSalidaHead, "EntradaAnterior", managerActual.gameObject);
                    if (brilloHead) brilloHead.SetEncendido(false); // APAGAR INICIO
                }
                else if (fase > 0 && tipo == "SalidaSiguiente" && nodoTocado == nodoPrevio)
                {
                    IniciarCarga(nodoPrevio.puntoSalidaSiguiente, "EntradaAnterior", managerActual.gameObject);
                    SetPalpitarVisual(nodoPrevio.gameObject, "LetreroLigaDer", false); // APAGAR SIGUIENTE PREVIO
                }
            }
            else if (pasoConexion == 1 && fase > 0)
            {
                if (tipo == "SalidaAnterior" && nodoTocado == managerActual)
                {
                    IniciarCarga(managerActual.puntoSalidaAnterior, "EntradaSiguiente", nodoPrevio.gameObject);
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", false); // APAGAR ANTERIOR ACTUAL
                }
            }
            // CASO: Llevar agua al NULL
            else if ((fase == 0 && pasoConexion == 1) || (fase > 0 && pasoConexion == 2))
            {
                if (tipo == "SalidaSiguiente" && nodoTocado == managerActual)
                {
                    cargandoAgua = true;
                    puntoOrigenActual = managerActual.puntoSalidaSiguiente;

                    // GESTIÓN VISUAL NULL
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", false); // APAGAR ORIGEN
                    if (brilloNull) brilloNull.SetEncendido(true); // ENCENDER DESTINO (NULL)

                    andy.Decir("Lleva la salida a NULL.");
                }
            }
        }
        else
        {
            // ... dentro de AccionEnLetrero, en la parte final del else (cuando cargandoAgua es true) ...

            if (pasoConexion == 0 && tipo == "EntradaAnterior" && nodoTocado == managerActual)
            {
                FinalizarPaso("EntradaAnterior", managerActual.gameObject);
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", false); // Apagar destino al llegar

                managerActual.ActivarHuerto();

                if (fase == 0)
                {
                    pasoConexion = 1;
                    andy.Decir("Ahora conecta la SALIDA SIGUIENTE a NULL.");
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true); // Palpitar siguiente origen
                }
                else
                {
                    pasoConexion = 1;
                    andy.Decir("Ahora la liga hacia atrás: SALIDA ANTERIOR a la ENTRADA SIGUIENTE previa.");
                    SetPalpitarVisual(managerActual.gameObject, "LetreroLigaIzq", true); // Palpitar anterior origen
                }
            }
            else if (pasoConexion == 1 && fase > 0 && tipo == "EntradaSiguiente" && nodoTocado == nodoPrevio)
            {
                FinalizarPaso("EntradaSiguiente", nodoPrevio.gameObject);
                // El brillo del destino se apaga dentro de FinalizarPaso genérico o aquí:
                SetPalpitarVisual(nodoPrevio.gameObject, "LetreroLigaDer", false);

                pasoConexion = 2;
                andy.Decir("Bien. Cierra el nodo llevando la SALIDA SIGUIENTE a NULL.");
                SetPalpitarVisual(managerActual.gameObject, "LetreroLigaDer", true); // Palpitar origen final
            }
            else if (tipo == "Null")
            {
                FinalizarNodoCompleto();
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

        // NUEVO: Detectar qué letrero debe palpitar según el brillo
        if (proximoBrillo.Contains("Anterior")) SetPalpitarVisual(nodoDestino, "LetreroLigaIzq", true);
        else if (proximoBrillo.Contains("Siguiente")) SetPalpitarVisual(nodoDestino, "LetreroLigaDer", true);
        else if (proximoBrillo.Contains("Info")) SetPalpitarVisual(nodoDestino, "LetreroInfo", true);
    }

    void FinalizarPaso(string brilloApagar, GameObject nodo)
    {
        cargandoAgua = false;

        if (brilloApagar == "EntradaAnterior")
        {
            // Si el nodo anterior estaba conectado al NULL, borramos ese enlace viejo
            if (enlaceActualAlNull != null)
            {
                Destroy(enlaceActualAlNull.gameObject);
                enlaceActualAlNull = null;
            }

            // Creamos el enlace: desde la salida del anterior hasta la entrada de este
            CrearSegmentoFijo(puntoOrigenActual.position, managerActual.puntoEntradaAnterior.position);
        }
        else if (brilloApagar == "EntradaSiguiente")
        {
            // Enlace de retroceso: desde la salida anterior de este hasta la entrada siguiente del previo
            NodoManager nodoPrevio = listaNodos[fase - 1];
            CrearSegmentoFijo(managerActual.puntoSalidaAnterior.position, nodoPrevio.puntoEntradaSiguiente.position);
        }

        puntoOrigenActual = null;
        SumarPuntos(10);
        EncenderBrilloEnNodo(nodo, brilloApagar, false);
    }

    void FinalizarNodoCompleto()
    {
        cargandoAgua = false;

        // Creamos el enlace al NULL
        LineRenderer lineaNull = Instantiate(lineaFija, transform);
        lineaNull.positionCount = 2;
        lineaNull.SetPosition(0, managerActual.puntoSalidaSiguiente.position);
        lineaNull.SetPosition(1, puntoEntradaNull.position);

        // Lo guardamos por separado para poder quitarlo luego
        enlaceActualAlNull = lineaNull;

        puntoOrigenActual = null;
        if (brilloNull) brilloNull.SetEncendido(false);
        SumarPuntos(20);
        managerActual.DrenarAgua();

        listaNodos.Add(managerActual);
        managerActual = null;

        UIManager.instancia.MarcarTareaCompletada(fase * 2);
        fase++;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Lista Doblemente Ligada completada!");
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

    void ActualizarLineaFijaDoble()
    {
        // Dibuja el camino "Siguiente" (Next)
        lineaFija.positionCount = puntosAdelante.Count;
        lineaFija.SetPositions(puntosAdelante.ToArray());

        // Dibuja el camino "Anterior" (Prev) en el SEGUNDO objeto
        if (lineaFijaPrev != null)
        {
            lineaFijaPrev.positionCount = puntosAtras.Count;
            lineaFijaPrev.SetPositions(puntosAtras.ToArray());
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
        // Buscamos todos los Triggers en el nodo
        TriggerConexion[] triggers = n.GetComponentsInChildren<TriggerConexion>(true);
        foreach (var t in triggers)
        {
            // Si el identificador del script coincide con lo que buscamos
            if (t.identificador.Equals(identificadorBuscado, System.StringComparison.OrdinalIgnoreCase))
            {
                // Encendemos el EfectoLetrero que esté en ese mismo objeto
                EfectoLetrero ef = t.GetComponent<EfectoLetrero>();
                if (ef != null) ef.SetEncendido(estado);
            }
        }
    }

    void ApagarBrillosGlobales()
    {
        if (brilloHead) brilloHead.SetEncendido(false);
        if (brilloNull) brilloNull.SetEncendido(false);

        // Apaga absolutamente todos los EfectoLetrero en la escena
        EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None);
        foreach (var ef in todos) ef.SetEncendido(false);
    }

    void SetPalpitarVisual(GameObject n, string nombreLetrero, bool estado)
    {
        if (n == null) return;
        // Buscamos en los hijos el objeto que contenga el nombre (Der, Izq o Info)
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
    void LimpiarNodosEscena() { foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (n.name.Contains("(Clone)")) Destroy(n.gameObject); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}