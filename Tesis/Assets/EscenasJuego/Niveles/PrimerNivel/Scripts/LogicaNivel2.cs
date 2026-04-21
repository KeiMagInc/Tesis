using Mundo2;
using System.Collections;
using TMPro;
using UnityEngine;

public class LogicaNivel2 : MonoBehaviour, ILogicaNivel
{
    [Header("Sprites UI Originales")]
    public Sprite spriteTrigo;
    public Sprite spritePapa;
    public Sprite spriteCalabaza;
    private string[] nombresNodos = { "Trigo", "Calabaza", "Papa" };
    public static LogicaNivel2 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public Transform lupi;
    [Header("Prefabs Específicos Nivel 2")]
    public GameObject prefabTrigoN2;
    public GameObject prefabPapaN2;
    public GameObject prefabCalabazaN2;
    [Header("Conexiones y Brillos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;
    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;
    private NodoManager managerActual;
    private int[] mapaIndicesUI = { 0, 2, 4 };
    
    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabTrigoN2, prefabCalabazaN2, prefabPapaN2);
        Sprite[] imagenes = { spriteTrigo, spriteCalabaza, spritePapa };
        string[] nombres = { "Trigo", "Calabaza", "Papa" };
        UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);
        ResetearNivel();
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.ConfigurarTextosChecklist(
            "Izquierda: sembrar trigo",
            "",                          
            "Derecha: sembrar calabaza", 
            "",                          
            "Centro: sembrar papa"   
        );
        ActualizarPuntos();
        StartCoroutine(Intro());
    }

    public void ResetearNivel()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0;
        UIManager.instancia.ResetBotones();
        ApagarBrillos();
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();
        foreach (var p in GameObject.FindGameObjectsWithTag("Planta")) Destroy(p);
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(1f);
        andy.Decir("¡Lupi! Abre tu mochila para ver las semillas.");
        UIManager.instancia.MostrarMochilaSolo(true);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        andy.Decir("Bien. Ahora revisa las tareas pendientes.");
        UIManager.instancia.MostrarChecklistSolo(true);
        yield return new WaitForSeconds(2.5f);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        if (fase >= nombresNodos.Length) return;
        string semillaActual = nombresNodos[fase];
        UIManager.instancia.SetSemillaPalpitar(semillaActual);
        andy.Decir("Siembra el " + semillaActual);
        pasoConexion = 0;
        lineaAgua.positionCount = 0;
    }

    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        andy.Decir("Busca P (INICIO).");
        brilloHead.SetEncendido(true);
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (tipo == "Head" && pasoConexion == 0)
        {
            cargandoAgua = true;
            lineaAgua.positionCount = 2;
            lineaAgua.SetPosition(0, puntoSalidaHead.position);
            brilloHead.SetEncendido(false);
            andy.Decir("Lleva el flujo a la INFO.");
            GameObject huerto = BuscarHuerto();
            if (huerto != null) EncenderBrilloHijo(huerto, "Info", true);
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua)
        {
            managerActual = objetoTocado.GetComponentInParent<NodoManager>();
            cargandoAgua = false;
            lineaAgua.SetPosition(1, managerActual.puntoEntrada.position);
            managerActual.ActivarHuerto();
            pasoConexion = 1;
            SumarPuntos(10);
            EncenderBrilloHijo(managerActual.gameObject, "Info", false);
            EncenderBrilloHijo(managerActual.gameObject, "Liga", true);
            andy.Decir("Activa la LIGA.");
        }
        else if (tipo == "SalidaHuerto" && pasoConexion == 1)
        {
            cargandoAgua = true;
            lineaAgua.positionCount = 4;
            lineaAgua.SetPosition(2, managerActual.puntoSalida.position);
            EncenderBrilloHijo(managerActual.gameObject, "Liga", false);
            brilloNull.SetEncendido(true);
            pasoConexion = 2;
            andy.Decir("Apunta a NULL.");
        }
        else if (tipo == "Null" && pasoConexion == 2 && cargandoAgua)
        {
            cargandoAgua = false;
            lineaAgua.SetPosition(3, puntoEntradaNull.position);
            SumarPuntos(10);
            managerActual.DrenarAgua();
            brilloNull.SetEncendido(false);
            UIManager.instancia.MarcarTareaCompletada(mapaIndicesUI[fase]);
            fase++;
            if (fase < 3) StartCoroutine(EsperarSiguiente());
            else andy.Decir("¡Nodos completados!");
        }
    }

    void Update() { if (cargandoAgua) lineaAgua.SetPosition(lineaAgua.positionCount - 1, lupi.position); }
    void SumarPuntos(int cant) { UIManager.puntosGlobales += cant; ActualizarPuntos(); }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }

    GameObject BuscarHuerto()
    {
        string buscado = nombresNodos[fase].ToLower();
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (n.gameObject.name.ToLower().Contains(buscado) && n.gameObject.name.Contains("(Clone)"))
                return n.gameObject;
        }
        return null;
    }

    void EncenderBrilloHijo(GameObject r, string n, bool e)
    {
        if (!r) return;
        foreach (var b in r.GetComponentsInChildren<EfectoLetrero>(true))
        {
            if (b.gameObject.name.ToUpper().Contains(n.ToUpper()))
                b.SetEncendido(e);
        }
    }
    void ApagarBrillos() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}