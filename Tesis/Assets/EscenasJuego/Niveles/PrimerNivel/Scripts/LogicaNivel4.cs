using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    [Header("Configuración de Semillas (5 semillas)")]
    public Sprite[] misSprites;
    public GameObject[] misPrefabs;
    private string[] nombres = { "Rábano", "Zanahoria", "Trigo", "Papa", "Calabaza" };

    [Header("Referencias de Escena")]
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;
    public Transform puntoAccesoLC;
    public EfectoLetrero brilloLC;

    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;

    private NodoManager primero;
    private NodoManager lc;
    private NodoManager managerActual;
    private List<NodoManager> listaNodos = new List<NodoManager>();

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;
        ResetearNivel();
        StartCoroutine(Intro());
    }

    public void ResetearNivel()
    {
        fase = 0;
        pasoConexion = 0;
        cargandoAgua = false;
        listaNodos.Clear();
        if (lineaAgua) lineaAgua.positionCount = 0;
        if (lineaFija) lineaFija.positionCount = 0;

        // CONFIGURAMOS LA MOCHILA
        // Asegúrate de que misSprites y misPrefabs tengan tamaño 5 en el Inspector
        UIManager.instancia.ConfigurarMochila(misSprites, nombres, misPrefabs);
        UIManager.instancia.ConfigurarTextosChecklist(nombres);
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Nivel 4! Listas Circulares. Aquí el último siempre apunta al primero.");
        yield return new WaitForSeconds(2.5f);
        UIManager.instancia.MostrarMochilaSolo(true);
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        if (fase < nombres.Length)
        {
            andy.Decir("Siembra " + nombres[fase] + " para continuar el círculo.");
            UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
        }
        pasoConexion = 0;
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;

        if (!cargandoAgua)
        {
            if (fase == 0 && tipo == "LC")
            {
                cargandoAgua = true;
                if (brilloLC) brilloLC.SetEncendido(false);
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
            }
            else if (fase > 0 && tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == lc)
            {
                cargandoAgua = true;
                EncenderBrilloEnNodo(lc.gameObject, "Puntero", false);
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
            }
        }
        else
        {
            if (pasoConexion == 0 && tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                cargandoAgua = false; pasoConexion = 1;
                managerActual.ActivarHuerto();
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
                EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", true);
                andy.Decir("¡Bien! Ahora que apunte al inicio (Rábano) para cerrar el círculo.");
            }
            else if (pasoConexion == 1 && tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == primero)
            {
                FinalizarNodo();
            }
        }
    }

    void FinalizarNodo()
    {
        cargandoAgua = false;
        if (fase == 0) primero = managerActual;
        lc = managerActual;
        listaNodos.Add(lc);

        UIManager.instancia.MarcarTareaCompletada(fase);
        DibujarCirculoFijo();

        fase++;
        managerActual = null;

        if (fase < nombres.Length) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Lista Circular completada!");
    }

    void DibujarCirculoFijo()
    {
        if (puntoAccesoLC == null || primero == null) return;
        List<Vector3> pts = new List<Vector3>();
        pts.Add(puntoAccesoLC.position);
        foreach (var n in listaNodos)
        {
            pts.Add(n.puntoEntrada.position);
            pts.Add(n.puntoSalida.position);
        }
        pts.Add(primero.puntoEntrada.position);

        if (lineaFija)
        {
            lineaFija.positionCount = pts.Count;
            lineaFija.SetPositions(pts.ToArray());
        }
    }

    void Update()
    {
        if (lineaAgua == null || lupi == null) return;
        if (!cargandoAgua || managerActual == null) { lineaAgua.positionCount = 0; return; }

        Vector3 origen = (pasoConexion == 0) ?
            (fase == 0 ? puntoAccesoLC.position : lc.puntoSalida.position) :
            managerActual.puntoSalida.position;

        lineaAgua.positionCount = 2;
        lineaAgua.SetPositions(new Vector3[] { origen, lupi.position });
    }

    public void AvanceSiembraExitosa() => StartCoroutine(AsignarNodo());
    IEnumerator AsignarNodo()
    {
        yield return new WaitForSeconds(0.2f);
        managerActual = BuscarNuevoNodo();
        if (fase == 0) { if (brilloLC) brilloLC.SetEncendido(true); }
        else EncenderBrilloEnNodo(lc.gameObject, "Puntero", true);
    }

    NodoManager BuscarNuevoNodo()
    {
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
            if (nm.name.Contains("(Clone)") && !listaNodos.Contains(nm)) return nm;
        return null;
    }

    void EncenderBrilloEnNodo(GameObject n, string p, bool e)
    {
        if (n == null) return;
        foreach (var b in n.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.name.ToUpper().Contains(p.ToUpper())) b.SetEncendido(e);
    }

    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}