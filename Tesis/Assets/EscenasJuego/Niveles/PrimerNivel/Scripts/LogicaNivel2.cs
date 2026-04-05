using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel2 : MonoBehaviour
{
    public static LogicaNivel2 instancia;
    public AndyController andy;
    public UIManager ui;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public Transform lupi;

    [Header("Puntos de Conexión Fijos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;

    [Header("Efectos Brillo")]
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private int puntos = 0;
    private int fase = 0; // 0: Trigo, 1: Calabaza, 2: Papa
    private bool cargandoAgua = false;
    private int pasoConexion = 0;
    private Vector3 posOrigen;
    private NodoManager managerActual;

    void Awake() => instancia = this;

    void OnEnable()
    {
        lineaAgua.positionCount = 0;
        ActualizarPuntos();
        StartCoroutine(IntroNivel2());
    }

    IEnumerator IntroNivel2()
    {
        yield return new WaitForSeconds(1f);
        andy.Decir("¡Lupi! Abre tu mochila.");
        yield return ui.AparecerSuave(ui.groupIconoMochila);
        yield return new WaitUntil(() => ui.panelParcelas.activeSelf);
        yield return new WaitForSeconds(1f);
        andy.Decir("Revisa la lista de tareas.");
        yield return ui.AparecerSuave(ui.groupChecklist);
        yield return new WaitForSeconds(3f);
        andy.Decir("Hoy aprenderás que el ORDEN LÓGICO es más importante que el lugar donde siembras.");
        yield return new WaitForSeconds(4f);
        ProximoPasoSiembra();
    }

    void ProximoPasoSiembra()
    {
        string[] nombres = { "Trigo", "Calabaza", "Papa" };
        andy.Decir("Ahora siembra el " + nombres[fase] + ".");
        ui.SetSemillaPalpitar(nombres[fase]);
        pasoConexion = 0;
        lineaAgua.positionCount = 0;
    }

    public void AvanceSiembraExitosa()
    {
        ui.SetSemillaPalpitar("");
        andy.Decir("¡Bien! El nodo está plantado. Ahora busca la variable P (Inicio).");
        if (brilloHead) brilloHead.SetEncendido(true);
        pasoConexion = 0;
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        if (tipo == "Head" && pasoConexion == 0 && !cargandoAgua)
        {
            cargandoAgua = true;
            posOrigen = puntoSalidaHead.position;
            lineaAgua.positionCount = 2;
            lineaAgua.SetPosition(0, posOrigen);
            if (brilloHead) brilloHead.SetEncendido(false);
            andy.Decir("¡Dirección de P obtenida! Llévala al campo INFO (Dato).");

            // BUSCAR HUERTO ACTUAL
            GameObject huerto = BuscarHuertoPorFase();
            EncenderBrilloHijo(huerto, "Dato", true);
        }
        else if (tipo == "EntradaHuerto" && pasoConexion == 0 && cargandoAgua)
        {
            managerActual = objetoTocado.GetComponentInParent<NodoManager>();
            if (managerActual != null)
            {
                cargandoAgua = false;
                lineaAgua.SetPosition(1, managerActual.puntoEntrada.position);
                managerActual.ActivarHuerto();
                SumarPuntos(10);
                pasoConexion = 1;
                EncenderBrilloHijo(managerActual.gameObject, "Dato", false);
                andy.Decir("¡INFO asignada! Busca ahora la LIGA (Puntero).");
                EncenderBrilloHijo(managerActual.gameObject, "Puntero", true);
            }
        }
        else if (tipo == "SalidaHuerto" && pasoConexion == 1 && !cargandoAgua)
        {
            managerActual = objetoTocado.GetComponentInParent<NodoManager>();
            if (managerActual != null)
            {
                cargandoAgua = true;
                lineaAgua.positionCount = 4;
                lineaAgua.SetPosition(2, managerActual.puntoSalida.position);
                posOrigen = managerActual.puntoSalida.position;
                EncenderBrilloHijo(managerActual.gameObject, "Puntero", false);
                pasoConexion = 2;
                andy.Decir("Liga activa. Apunta el canal a NIL (Null).");
                if (brilloNull) brilloNull.SetEncendido(true);
            }
        }
        else if (tipo == "Null" && pasoConexion == 2 && cargandoAgua)
        {
            cargandoAgua = false;
            lineaAgua.SetPosition(3, puntoEntradaNull.position);
            if (managerActual != null) managerActual.DrenarAgua();
            SumarPuntos(10);
            if (brilloNull) brilloNull.SetEncendido(false);
            ui.MarcarTareaCompletada(fase);
            fase++;
            if (fase < 3) StartCoroutine(EsperarSiguiente());
            else andy.Decir("¡Excelente! Has restaurado el orden lógico de las parcelas.");
        }
    }

    GameObject BuscarHuertoPorFase()
    {
        string[] nombres = { "Trigo", "Calabaza", "Papa" };
        string buscado = nombres[fase].ToLower();
        NodoManager[] todos = Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None);
        foreach (var n in todos) { if (n.gameObject.name.ToLower().Contains(buscado)) return n.gameObject; }
        return null;
    }

    IEnumerator EsperarSiguiente()
    {
        andy.Decir("¡Estructura P -> INFO -> NIL completada!");
        yield return new WaitForSeconds(3.5f);
        ProximoPasoSiembra();
    }

    void Update()
    {
        if (cargandoAgua)
        {
            int ultimoIndice = lineaAgua.positionCount - 1;
            lineaAgua.SetPosition(ultimoIndice, lupi.position);
        }
    }

    void EncenderBrilloHijo(GameObject raiz, string nombre, bool estado)
    {
        if (raiz == null) return;
        EfectoLetrero[] brillos = raiz.GetComponentsInChildren<EfectoLetrero>(true);
        foreach (var b in brillos) { if (b.gameObject.name.ToLower().Contains(nombre.ToLower())) b.SetEncendido(estado); }
    }

    void SumarPuntos(int cant) { puntos += cant; if (textoPuntos) textoPuntos.text = puntos.ToString(); }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = puntos.ToString(); }
}