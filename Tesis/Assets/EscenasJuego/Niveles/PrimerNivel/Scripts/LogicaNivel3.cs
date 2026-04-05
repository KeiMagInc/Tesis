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
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;

    private enum ModoOperacion { InsertarInicio, InsertarFinal }
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

    private List<Vector3> puntosCadenaFija = new List<Vector3>();
    private string[] nombresNodosInicio = { "Papa", "Trigo", "Calabaza" };
    private string[] nombresNodosFinal = { "Calabaza", "Trigo", "Papa" };

    void Awake() => instancia = this;

    void OnEnable()
    {
        LogicaNivel1 nivel1 = Object.FindAnyObjectByType<LogicaNivel1>();
        if (nivel1 != null) nivel1.gameObject.SetActive(false);

        instancia = this;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabTrigoN3, prefabPapaN3, prefabCalabazaN3);

        ResetearNivel();
        StartCoroutine(Intro());
    }

    // INTERFAZ: Este reset ahora siempre vuelve al inicio del Nivel 3 (Algoritmo 5.1)
    public void ResetearNivel()
    {
        modoActual = ModoOperacion.InsertarInicio; // Forzamos el regreso al primer algoritmo
        fase = 0;
        pasoConexion = 0;
        cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        managerAnterior = null;
        managerActual = null;

        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            // CORRECCIÓN DE TEXTO: Orden solicitado para el inicio
            UIManager.instancia.ConfigurarTextosChecklist("Derecha: sembrar papa", "Centro: sembrar trigo", "Izquierda: sembrar calabaza");
        }

        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }

    // Función interna para no perder el "modoActual" durante la transición fluida
    void LimpiarEscenaParaSiguienteAlgoritmo()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        managerAnterior = null;
        managerActual = null;
        if (UIManager.instancia != null) UIManager.instancia.ResetBotones();
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }

    void LimpiarNodosEscena()
    {
        NodoManager[] nodosViejos = Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var n in nodosViejos) Destroy(n.gameObject);
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        if (modoActual == ModoOperacion.InsertarInicio)
            andy.Decir("Algoritmo 5.1: CREA_INICIO. Las nuevas plantas van al principio.");
        else
            andy.Decir("Algoritmo 5.2: CREA_FINAL. Las nuevas plantas van al final.");

        UIManager.instancia.MostrarMochilaSolo(true);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
        if (fase < nombres.Length)
        {
            andy.Decir("Siembra " + nombres[fase]);
            UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
        }
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
            if (modoActual == ModoOperacion.InsertarInicio)
            {
                andy.Decir("Toca INICIO (P) para apuntar a la nueva planta.");
                brilloHead.SetEncendido(true);
            }
            else
            {
                if (fase == 0) { andy.Decir("La primera planta. Toca INICIO (P)."); brilloHead.SetEncendido(true); }
                else { andy.Decir("Toca el PUNTERO de la planta anterior."); EncenderBrilloEnNodo(managerAnterior.gameObject, "Puntero", true); }
            }
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (managerActual == null) return;
        if (modoActual == ModoOperacion.InsertarInicio) LogicaInsertarInicio(tipo, objetoTocado);
        else LogicaInsertarFinal(tipo, objetoTocado);
    }

    void LogicaInsertarInicio(string tipo, GameObject objetoTocado)
    {
        if (tipo == "Head" && pasoConexion == 0 && !cargandoAgua)
        {
            brilloHead.SetEncendido(false); cargandoAgua = true;
            EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
            andy.Decir("Lleva P al DATO.");
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua && pasoConexion == 0)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() != managerActual) return;
            EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
            managerActual.ActivarHuerto(); SumarPuntos(10); cargandoAgua = false; pasoConexion = 1;
            EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", true);
            andy.Decir("Activa su PUNTERO.");
        }
        else if (tipo == "SalidaHuerto" && pasoConexion == 1)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() != managerActual) return;
            EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", false);
            cargandoAgua = true; pasoConexion = 2;
            if (fase == 0) { brilloNull.SetEncendido(true); andy.Decir("A NIL."); }
            else { EncenderBrilloEnNodo(managerAnterior.gameObject, "Dato", true); andy.Decir("Apunta a la planta anterior."); }
        }
        else if (pasoConexion == 2 && cargandoAgua)
        {
            if (tipo == "Null" && fase == 0) FinalizarNodo();
            else if (tipo == "EntradaHuerto" && fase > 0 && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior) FinalizarNodo();
        }
    }

    void LogicaInsertarFinal(string tipo, GameObject objetoTocado)
    {
        if (pasoConexion == 0 && !cargandoAgua)
        {
            if (fase == 0 && tipo == "Head") { brilloHead.SetEncendido(false); cargandoAgua = true; }
            else if (fase > 0 && tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior)
            { EncenderBrilloEnNodo(managerAnterior.gameObject, "Puntero", false); cargandoAgua = true; }

            if (cargandoAgua) { EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true); andy.Decir("Conecta al DATO."); }
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua && pasoConexion == 0)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() != managerActual) return;
            EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
            managerActual.ActivarHuerto(); SumarPuntos(10); cargandoAgua = false; pasoConexion = 1;
            EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", true);
            andy.Decir("Ahora su PUNTERO a NIL.");
        }
        else if (tipo == "SalidaHuerto" && pasoConexion == 1)
        {
            EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", false);
            cargandoAgua = true; pasoConexion = 2; brilloNull.SetEncendido(true);
        }
        else if (tipo == "Null" && pasoConexion == 2 && cargandoAgua) FinalizarNodo();
    }

    void FinalizarNodo()
    {
        cargandoAgua = false;
        SumarPuntos(10);
        managerActual.DrenarAgua();
        ApagarBrillosGlobales();
        UIManager.instancia.MarcarTareaCompletada(fase);

        if (modoActual == ModoOperacion.InsertarInicio)
        {
            List<Vector3> nuevaRuta = new List<Vector3>() { managerActual.puntoEntrada.position, managerActual.puntoSalida.position };
            if (fase == 0) nuevaRuta.Add(puntoEntradaNull.position);
            else nuevaRuta.AddRange(puntosCadenaFija);
            puntosCadenaFija = nuevaRuta;
        }
        else
        {
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
        fase++;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else StartCoroutine(CambiarDeModo());
    }

    IEnumerator CambiarDeModo()
    {
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            andy.Decir("¡Excelente! Has dominado la inserción al INICIO.");
            yield return new WaitForSeconds(3f);
            andy.Decir("Ahora aprenderemos el Algoritmo 5.2: CREA_FINAL.");
            yield return new WaitForSeconds(3f);
            modoActual = ModoOperacion.InsertarFinal;

            LimpiarEscenaParaSiguienteAlgoritmo(); // Limpiamos sin resetear el ModoActual

            UIManager.instancia.ConfigurarTextosChecklist("Izquierda: sembrar calabaza", "Centro: sembrar trigo", "Derecha: sembrar papa");
            StartCoroutine(Intro());
        }
        else andy.Decir("¡Felicidades! Dominas las inserciones de Cairo.");
    }

    void Update() => ActualizarVisualManguera();

    void ActualizarVisualManguera()
    {
        List<Vector3> puntosActivos = new List<Vector3>();

        if (modoActual == ModoOperacion.InsertarInicio)
        {
            puntosActivos.Add(puntoSalidaHead.position);
            if (managerActual != null)
            {
                if (pasoConexion == 0) { if (cargandoAgua) puntosActivos.Add(lupi.position); else if (managerAnterior != null) puntosActivos.Add(managerAnterior.puntoEntrada.position); }
                else if (pasoConexion == 1) puntosActivos.Add(managerActual.puntoEntrada.position);
                else if (pasoConexion == 2) { puntosActivos.Add(managerActual.puntoEntrada.position); puntosActivos.Add(managerActual.puntoSalida.position); if (cargandoAgua) puntosActivos.Add(lupi.position); }
            }
            else if (managerAnterior != null) puntosActivos.Add(managerAnterior.puntoEntrada.position);
        }
        else
        {
            if (managerActual != null)
            {
                Vector3 origen = (fase == 0) ? puntoSalidaHead.position : managerAnterior.puntoSalida.position;
                puntosActivos.Add(origen);
                if (pasoConexion == 0) { if (cargandoAgua) puntosActivos.Add(lupi.position); }
                else if (pasoConexion == 1) puntosActivos.Add(managerActual.puntoEntrada.position);
                else if (pasoConexion == 2) { puntosActivos.Add(managerActual.puntoEntrada.position); puntosActivos.Add(managerActual.puntoSalida.position); if (cargandoAgua) puntosActivos.Add(lupi.position); }
            }
        }

        lineaAgua.positionCount = puntosActivos.Count;
        lineaAgua.SetPositions(puntosActivos.ToArray());
    }

    NodoManager BuscarNuevoNodoEnEscena()
    {
        string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
        if (fase >= nombres.Length) return null;
        string buscado = nombres[fase].ToLower();
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
            if (nm.gameObject.name.ToLower().Contains(buscado) && nm != managerAnterior) return nm;
        return null;
    }

    void EncenderBrilloEnNodo(GameObject nodo, string parte, bool encender)
    {
        if (nodo == null) return;
        foreach (var b in nodo.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.gameObject.name.ToUpper().Contains(parte.ToUpper())) b.SetEncendido(encender);
    }

    void SumarPuntos(int cant) { UIManager.puntosGlobales += cant; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    void ApagarBrillosGlobales()
    {
        if (brilloHead) brilloHead.SetEncendido(false);
        if (brilloNull) brilloNull.SetEncendido(false);
        EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None);
        foreach (var b in todos) b.SetEncendido(false);
    }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}