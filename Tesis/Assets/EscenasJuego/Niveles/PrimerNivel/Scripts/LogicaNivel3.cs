using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel3 : MonoBehaviour, ILogicaNivel
{
    public static LogicaNivel3 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;      // La línea que manejas con Lupi
    public LineRenderer lineaFija;      // La cadena que ya está terminada
    public Transform lupi;

    [Header("Prefabs Específicos Nivel 3")]
    public GameObject prefabPapaN3;      // Fase 0
    public GameObject prefabTrigoN3;     // Fase 1
    public GameObject prefabCalabazaN3;  // Fase 2

    [Header("Conexiones y Brillos Fijos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private int fase = 0;
    private int pasoConexion = 0; // 0: Buscando Dato, 1: En Dato (esperando click en Puntero), 2: Buscando Siguiente
    private bool cargandoAgua = false;

    private NodoManager managerActual;
    private NodoManager managerAnterior;

    private List<Vector3> puntosCadenaFija = new List<Vector3>();
    private string[] nombresNodos = { "Papa", "Trigo", "Calabaza" };

    void Awake() => instancia = this;

    void OnEnable()
    {
        // 1. BUSCAR Y DESACTIVAR EL NIVEL 1 SI EXISTE
        LogicaNivel1 nivel1 = Object.FindAnyObjectByType<LogicaNivel1>();
        if (nivel1 != null)
        {
            // Desactivamos el objeto completo para que sus Update/OnEnable no corran
            nivel1.gameObject.SetActive(false);
        }

        instancia = this;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabTrigoN3, prefabPapaN3, prefabCalabazaN3);

        ResetearNivel();

        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.ConfigurarTextosChecklist("Derecha: sembrar papa", "Centro: sembrar trigo", "Izquierda: sembrar calabaza");

        ActualizarPuntos();
        StartCoroutine(Intro());
    }

    public void ResetearNivel()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        managerAnterior = null;
        managerActual = null;

        if (UIManager.instancia != null) UIManager.instancia.ResetBotones();
        ApagarBrillosGlobales();

        NodoManager[] nodosViejos = Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var n in nodosViejos)
        {
            n.gameObject.name = "DESTRUIDO_VIEJO";
            n.gameObject.tag = "Untagged";
            Destroy(n.gameObject);
        }

        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("Algoritmo 5.1: CREA_INICIO. Las nuevas plantas van al principio.");
        UIManager.instancia.MostrarMochilaSolo(true);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        UIManager.instancia.MostrarChecklistSolo(true);
        yield return new WaitForSeconds(1f);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        andy.Decir("Siembra " + nombresNodos[fase]);
        UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
        pasoConexion = 0;
        managerActual = null;
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
            andy.Decir("El INICIO (P) debe apuntar a la " + nombresNodos[fase] + ". Toca P.");
            brilloHead.SetEncendido(true);
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (managerActual == null && tipo != "Head") return;

        // 1. CLICK EN INICIO (P)
        if (tipo == "Head" && pasoConexion == 0 && !cargandoAgua)
        {
            brilloHead.SetEncendido(false);
            cargandoAgua = true;
            EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
            andy.Decir("Lleva P al DATO de la " + nombresNodos[fase]);
        }

        // 2. CLICK EN DATO (INFO)
        else if (tipo == "EntradaHuerto" && cargandoAgua && pasoConexion == 0)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() != managerActual) return;

            EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
            managerActual.ActivarHuerto();
            SumarPuntos(10);
            cargandoAgua = false;
            pasoConexion = 1; // Ahora la manguera se quedará pegada aquí

            EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", true);
            andy.Decir("Ahora activa su PUNTERO (LIGA).");
        }

        // 3. CLICK EN PUNTERO (LIGA)
        else if (tipo == "SalidaHuerto" && pasoConexion == 1)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() != managerActual) return;

            EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", false);
            cargandoAgua = true;
            pasoConexion = 2;

            if (fase == 0)
            {
                brilloNull.SetEncendido(true);
                andy.Decir("Como es la primera, apunta a NIL.");
            }
            else
            {
                EncenderBrilloEnNodo(managerAnterior.gameObject, "Dato", true);
                andy.Decir("Apunta al dato de la " + nombresNodos[fase - 1]);
            }
        }

        // 4. CIERRE (NIL O NODO ANTERIOR)
        else if (pasoConexion == 2 && cargandoAgua)
        {
            if (tipo == "Null" && fase == 0)
            {
                brilloNull.SetEncendido(false);
                FinalizarNodo(puntoEntradaNull.position);
            }
            else if (tipo == "EntradaHuerto" && fase > 0)
            {
                if (objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior)
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Dato", false);
                    FinalizarNodo(managerAnterior.puntoEntrada.position);
                }
            }
        }
    }

    void FinalizarNodo(Vector3 puntoFinalConexion)
    {
        cargandoAgua = false;
        SumarPuntos(10);
        managerActual.DrenarAgua();
        UIManager.instancia.MarcarTareaCompletada(fase);

        // 1. Construimos la ruta de ESTE nodo específico
        List<Vector3> rutaNodoActual = new List<Vector3>();
        rutaNodoActual.Add(managerActual.puntoEntrada.position);
        rutaNodoActual.Add(managerActual.puntoSalida.position);

        // 2. Si es el primero, apunta a Null. Si no, une con lo que ya había.
        if (fase == 0) rutaNodoActual.Add(puntoEntradaNull.position);
        else rutaNodoActual.AddRange(puntosCadenaFija);

        // 3. Guardamos esto como la nueva "Cadena Fija"
        puntosCadenaFija = rutaNodoActual;

        // 4. ACTUALIZAMOS LA LÍNEA VISUAL ESTÁTICA
        lineaFija.positionCount = puntosCadenaFija.Count;
        lineaFija.SetPositions(puntosCadenaFija.ToArray());

        managerAnterior = managerActual;
        fase++;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Lista completada al inicio exitosamente!");
    }

    void Update() => ActualizarVisualManguera();

    void ActualizarVisualManguera()
    {
        // 1. DIBUJO DE LA LÍNEA ACTIVA (Inicio -> Nodo Nuevo -> Lupi)
        List<Vector3> puntosActivos = new List<Vector3>();
        puntosActivos.Add(puntoSalidaHead.position);

        if (managerActual != null)
        {
            // Caso: Estamos operando con un nodo nuevo recién sembrado
            if (pasoConexion == 0)
            {
                if (cargandoAgua)
                {
                    // Estamos moviendo el puntero INICIO con Lupi
                    puntosActivos.Add(lupi.position);
                }
                else if (managerAnterior != null)
                {
                    // SOLUCIÓN: Si aún no tocamos el INICIO, mantenemos la conexión 
                    // con la planta que actualmente es la cabeza de la lista.
                    puntosActivos.Add(managerAnterior.puntoEntrada.position);
                }
            }
            else if (pasoConexion == 1)
            {
                // El Inicio ya está conectado al Dato del nuevo nodo
                puntosActivos.Add(managerActual.puntoEntrada.position);
            }
            else if (pasoConexion == 2)
            {
                // El flujo pasa por el nuevo nodo y el puntero busca a quién apuntar
                puntosActivos.Add(managerActual.puntoEntrada.position);
                puntosActivos.Add(managerActual.puntoSalida.position);

                if (cargandoAgua)
                {
                    puntosActivos.Add(lupi.position);
                }
                else
                {
                    // Conexión finalizada antes de pasar a la siguiente fase
                    puntosActivos.AddRange(puntosCadenaFija);
                }
            }
        }
        else if (managerAnterior != null)
        {
            // Estado de espera entre siembras: mantiene el Inicio conectado a la cabeza actual
            puntosActivos.Add(managerAnterior.puntoEntrada.position);
        }

        lineaAgua.positionCount = puntosActivos.Count;
        lineaAgua.SetPositions(puntosActivos.ToArray());

        // 2. DIBUJO DE LA LÍNEA FIJA (La cadena de nodos ya establecida)
        if (puntosCadenaFija.Count > 0)
        {
            lineaFija.positionCount = puntosCadenaFija.Count;
            lineaFija.SetPositions(puntosCadenaFija.ToArray());
        }
    }

    NodoManager BuscarNuevoNodoEnEscena()
    {
        string nombreBuscado = nombresNodos[fase].ToLower();
        NodoManager[] todos = Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None);
        foreach (var nm in todos)
        {
            if (nm != null && nm.gameObject.name != "DESTRUIDO_VIEJO" &&
                nm.gameObject.name.ToLower().Contains(nombreBuscado) && nm != managerAnterior)
                return nm;
        }
        return null;
    }

    void EncenderBrilloEnNodo(GameObject nodo, string parte, bool encender)
    {
        if (nodo == null) return;
        foreach (var b in nodo.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.gameObject.name.ToUpper().Contains(parte.ToUpper())) b.SetEncendido(encender);
    }

    void SumarPuntos(int cant) { UIManager.puntosGlobales += cant; ActualizarPuntos(); }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    void ApagarBrillosGlobales()
    {
        // Busca TODOS los letreros con brillo en la jerarquía y los apaga
        EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None);
        foreach (var b in todos)
        {
            b.SetEncendido(false);
        }
    }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}