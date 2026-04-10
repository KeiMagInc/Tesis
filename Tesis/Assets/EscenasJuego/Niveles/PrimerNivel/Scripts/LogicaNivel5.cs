using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel5 : MonoBehaviour, ILogicaNivel
{
    [Header("Sprites UI para Nivel 4")]
    public Sprite spriteRemolacha;
    public Sprite spriteZanahoria;
    public Sprite spriteRabano;

    public static LogicaNivel5 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;

    [Header("Prefabs Específicos Nivel 4")]
    public GameObject prefabRemolacha;
    public GameObject prefabZanahoria;
    public GameObject prefabRabano;

    [Header("Conexiones y Brillos Fijos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;

    private NodoManager managerActual;
    private List<NodoManager> listaNodos = new List<NodoManager>();
    private string[] nombresNodos = { "Remolacha", "Zanahoria", "Rabano" };

    void Awake() => instancia = this;

    // Reemplaza tu OnEnable por este:
    void OnEnable()
    {
        if (UIManager.instancia == null) return; // Seguridad

        LogicaNivel3 nivel3 = Object.FindAnyObjectByType<LogicaNivel3>();
        if (nivel3 != null) nivel3.gameObject.SetActive(false);

        instancia = this;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabRemolacha, prefabZanahoria, prefabRabano);

        UIManager.instancia.ConfigurarBotonesUI(
            spriteRemolacha, "Remolacha",
            spriteZanahoria, "Zanahoria",
            spriteRabano, "Rabano"
        );

        ResetearNivel();
        StartCoroutine(Intro());
    }

    public void ResetearNivel()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        listaNodos.Clear();
        managerActual = null;

        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist("Izquierda: sembrar rábano", "Centro: sembrar zanahoria", "Derecha: sembrar remolacha");
        }
        LimpiarNodosEscena();
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Algoritmo 5.16! Inserción al INICIO de una LISTA DOBLE.");
        yield return new WaitForSeconds(2.5f);
        andy.Decir("Siembra la " + nombresNodos[fase]);
        UIManager.instancia.MostrarMochilaSolo(true);
        UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
        UIManager.instancia.MostrarChecklistSolo(true);
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (managerActual == null) return;

        if (!cargandoAgua)
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                brilloHead.SetEncendido(false);
                cargandoAgua = true;
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
                andy.Decir("Lleva el agua al DATO del nuevo nodo.");
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Derecha", false);
                cargandoAgua = true;
                pasoConexion = 2;
                if (fase == 0) brilloNull.SetEncendido(true);
                else EncenderBrilloEnNodo(listaNodos[0].gameObject, "Dato", true);
                andy.Decir("Ahora conecta la liga DERECHA (LIGADER).");
            }
        }
        else
        {
            if (tipo == "EntradaHuerto" && pasoConexion == 0 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                cargandoAgua = false; pasoConexion = 1;
                managerActual.ActivarHuerto();
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
                EncenderBrilloEnNodo(managerActual.gameObject, "Derecha", true);
                andy.Decir("¡Bien! Ahora activa el puntero DERECHO.");
            }
            else if (pasoConexion == 2)
            {
                bool exito = false;
                if (fase == 0 && tipo == "Null") exito = true;
                else if (fase > 0 && tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[0]) exito = true;
                if (exito) FinalizarNodo();
            }
        }
    }

    void FinalizarNodo()
    {
        cargandoAgua = false; SumarPuntos(15);
        managerActual.DrenarAgua();
        ApagarBrillosGlobales();
        UIManager.instancia.MarcarTareaCompletada(fase);
        listaNodos.Insert(0, managerActual);
        ActualizarLineaFija();
        fase++; managerActual = null;
        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Estructura Doble completada!");
    }

    void ActualizarLineaFija()
    {
        List<Vector3> camino = new List<Vector3>();
        if (listaNodos.Count == 0) return;
        camino.Add(puntoSalidaHead.position);
        foreach (var n in listaNodos) { camino.Add(n.puntoEntrada.position); camino.Add(n.puntoSalida.position); }
        camino.Add(puntoEntradaNull.position);
        for (int i = listaNodos.Count - 1; i >= 0; i--) { camino.Add(listaNodos[i].puntoSalida.position); camino.Add(listaNodos[i].puntoEntrada.position); }
        camino.Add(puntoSalidaHead.position);
        lineaFija.positionCount = camino.Count;
        lineaFija.SetPositions(camino.ToArray());
    }

    void Update()
    {
        if (lupi == null || lineaAgua == null) return;
        List<Vector3> pts = new List<Vector3>() { puntoSalidaHead.position };
        if (managerActual != null)
        {
            if (pasoConexion == 1) pts.Add(managerActual.puntoEntrada.position);
            else if (pasoConexion == 2) { pts.Add(managerActual.puntoEntrada.position); pts.Add(managerActual.puntoSalida.position); }
        }
        if (cargandoAgua) pts.Add(lupi.position);
        lineaAgua.positionCount = pts.Count;
        lineaAgua.SetPositions(pts.ToArray());
    }

    public void AvanceSiembraExitosa() => StartCoroutine(AsignarNuevoNodo());

    IEnumerator AsignarNuevoNodo()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (nm.gameObject.name.Contains("(Clone)") && !listaNodos.Contains(nm)) { managerActual = nm; break; }
        }
        if (managerActual != null) { brilloHead.SetEncendido(true); andy.Decir("¡Semilla lista! Recoge agua del INICIO."); }
    }

    IEnumerator EsperarSiguiente()
    {
        yield return new WaitForSeconds(2f);
        andy.Decir("Siguiente: " + nombresNodos[fase]);
        UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
    }

    void EncenderBrilloEnNodo(GameObject n, string p, bool e) { foreach (var b in n.GetComponentsInChildren<EfectoLetrero>(true)) if (b.gameObject.name.ToUpper().Contains(p.ToUpper())) b.SetEncendido(e); }
    void ApagarBrillosGlobales() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); foreach (var b in Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None)) b.SetEncendido(false); }
    void SumarPuntos(int c) { UIManager.puntosGlobales += c; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    void LimpiarNodosEscena() { foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (n.name.Contains("(Clone)")) Destroy(n.gameObject); }
}