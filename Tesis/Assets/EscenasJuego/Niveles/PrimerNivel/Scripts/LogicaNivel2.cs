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
    public Transform puntoSalidaHead;   // Head -> PuntoSalida
    public Transform puntoEntradaNull;  // Null -> PuntoEntrada

    [Header("Efectos Brillo")]
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private int puntos = 0;
    private int fase = 0; // 0: Trigo, 1: Papa, 2: Calabaza
    private bool cargandoAgua = false;
    private int pasoConexion = 0; // 0: Head, 1: Dato, 2: Puntero, 3: Null

    // Guardamos las posiciones para la línea
    private Vector3 posOrigen;
    private Vector3 posDestinoFija;

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
        ProximoPasoSiembra();
    }

    void ProximoPasoSiembra()
    {
        string[] nombres = { "Trigo", "Papa", "Calabaza" };
        andy.Decir("Ahora siembra el " + nombres[fase] + ".");
        ui.SetSemillaPalpitar(nombres[fase]);
        pasoConexion = 0;
        lineaAgua.positionCount = 0; // Limpiamos la manguera para el nuevo producto
    }

    public void AvanceSiembraExitosa()
    {
        ui.SetSemillaPalpitar("");
        andy.Decir("¡Bien! Recoge agua del INICIO (Head).");
        if (brilloHead) brilloHead.SetEncendido(true);
        pasoConexion = 0;
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {

        // 1. RECOGER DEL INICIO (HEAD)
        if (tipo == "Head" && pasoConexion == 0 && !cargandoAgua)
        {
            cargandoAgua = true;
            posOrigen = puntoSalidaHead.position;
            lineaAgua.positionCount = 2;
            lineaAgua.SetPosition(0, posOrigen);

            if (brilloHead) brilloHead.SetEncendido(false);
            andy.Decir("¡Agua recogida! Llévala al DATO del huerto.");
        }

        // 2. CONECTAR AL DATO (ENTRADA)
        else if (tipo == "EntradaHuerto" && pasoConexion == 0 && cargandoAgua)
        {
            NodoManager manager = objetoTocado.GetComponentInParent<NodoManager>();
            if (manager != null)
            {
                cargandoAgua = false;
                posDestinoFija = manager.puntoEntrada.position;
                lineaAgua.SetPosition(1, posDestinoFija); // Anclamos la línea al punto

                manager.ActivarHuerto(); // Inundación
                SumarPuntos(10);
                pasoConexion = 1;

                andy.Decir("¡Inundado! Ahora recoge el PUNTERO.");
                EncenderBrilloHijo(manager.gameObject, "Puntero", true);
            }
        }

        // 3. RECOGER DEL PUNTERO (SALIDA)
        else if (tipo == "SalidaHuerto" && pasoConexion == 1 && !cargandoAgua)
        {
            NodoManager manager = objetoTocado.GetComponentInParent<NodoManager>();
            if (manager != null)
            {
                cargandoAgua = true;
                // La línea ahora tiene 4 puntos para mostrar el camino completo
                lineaAgua.positionCount = 4;
                lineaAgua.SetPosition(2, manager.puntoSalida.position);
                posOrigen = manager.puntoSalida.position;

                EncenderBrilloHijo(manager.gameObject, "Puntero", false);
                pasoConexion = 2;

                andy.Decir("¡Enlace recogido! Llévalo al pozo NULL.");
                if (brilloNull) brilloNull.SetEncendido(true);
            }
        }

        // 4. CONECTAR A NULL (FIN)
        else if (tipo == "Null" && pasoConexion == 2 && cargandoAgua)
        {
            cargandoAgua = false;
            lineaAgua.SetPosition(3, puntoEntradaNull.position); // Anclamos al pozo

            SumarPuntos(10);
            if (brilloNull) brilloNull.SetEncendido(false);

            ui.MarcarTareaCompletada(fase);
            fase++;

            if (fase < 3) StartCoroutine(EsperarSiguiente());
            else andy.Decir("¡Excelente! Has restaurado todo el sistema de riego.");
        }
    }

    IEnumerator EsperarSiguiente()
    {
        andy.Decir("¡Lista conectada correctamente!");
        yield return new WaitForSeconds(3.5f);
        ProximoPasoSiembra();
    }

    void Update()
    {
        if (cargandoAgua)
        {
            // Mientras Lupi camina, el último punto de la línea la sigue a ella
            int ultimoIndice = lineaAgua.positionCount - 1;
            lineaAgua.SetPosition(ultimoIndice, lupi.position);
        }
    }

    void EncenderBrilloHijo(GameObject raiz, string nombre, bool estado)
    {
        if (raiz == null) return;
        EfectoLetrero[] brillos = raiz.GetComponentsInChildren<EfectoLetrero>(true);
        foreach (var b in brillos)
        {
            if (b.gameObject.name.ToLower().Contains(nombre.ToLower())) b.SetEncendido(estado);
        }
    }

    void SumarPuntos(int cant) { puntos += cant; if (textoPuntos) textoPuntos.text = puntos.ToString(); }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = puntos.ToString(); }
}