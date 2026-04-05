using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel3 : MonoBehaviour
{
    public static LogicaNivel3 instancia;
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
    private int fase = 0;
    private bool cargandoAgua = false;
    private int pasoConexion = 0;

    private Vector3 posOrigen;
    private NodoManager managerActual;
    private NodoManager managerAnterior; // En Cairo, esto representa la dirección que tenía P antes de insertar

    void Awake() => instancia = this;

    void OnEnable()
    {
        lineaAgua.positionCount = 0;
        ActualizarPuntos();
        StartCoroutine(IntroNivel3());
    }

    IEnumerator IntroNivel3()
    {
        yield return new WaitForSeconds(1f);
        andy.Decir("¡Lupi! Hoy aplicaremos el algoritmo 'Crea_inicio' del libro de Cairo.");
        yield return new WaitForSeconds(3.5f);
        andy.Decir("Cada nuevo huerto será la nueva cabecera y apuntará al anterior.");

        yield return ui.AparecerSuave(ui.groupIconoMochila);
        yield return new WaitUntil(() => ui.panelParcelas.activeSelf);

        yield return ui.AparecerSuave(ui.groupChecklist);
        yield return new WaitForSeconds(3f);

        ProximoPasoSiembra();
    }

    void ProximoPasoSiembra()
    {
        string[] nombres = { "Trigo", "Papa", "Calabaza" };
        if (fase > 0)
            andy.Decir("Inserta la " + nombres[fase] + ". ¡Ella será el nuevo inicio de la lista!");
        else
            andy.Decir("Siembra el " + nombres[fase] + " para empezar.");

        ui.SetSemillaPalpitar(nombres[fase]);
        pasoConexion = 0;
    }

    public void AvanceSiembraExitosa()
    {
        ui.SetSemillaPalpitar("");
        // Según Cairo: Creamos el nodo y ahora asignamos la dirección de la cabecera P
        andy.Decir("¡Huerto listo! Recoge la dirección del Inicio (Variable P).");
        if (brilloHead) brilloHead.SetEncendido(true);
        pasoConexion = 0;
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        // 1. RECOGER (HEAD o PUNTERO)
        if ((tipo == "Head" || tipo == "SalidaHuerto") && !cargandoAgua)
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                cargandoAgua = true;
                posOrigen = puntoSalidaHead.position;
                lineaAgua.positionCount = 2; // P -> Lupi
                lineaAgua.SetPosition(0, posOrigen);

                if (brilloHead) brilloHead.SetEncendido(false);
                andy.Decir("Conecta el Inicio al campo INFO (Dato) del nuevo huerto.");

                GameObject huerto = BuscarHuertoPorFase();
                EncenderBrilloHijo(huerto, "Dato", true);
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1)
            {
                NodoManager manager = objetoTocado.GetComponentInParent<NodoManager>();
                if (manager == managerActual)
                {
                    cargandoAgua = true;
                    posOrigen = manager.puntoSalida.position;
                    // Mantenemos la conexión Head->Dato y ahora Puntero->Lupi
                    lineaAgua.positionCount = 4;
                    lineaAgua.SetPosition(2, posOrigen);

                    EncenderBrilloHijo(manager.gameObject, "Puntero", false);

                    if (fase == 0)
                    {
                        andy.Decir("Es el único nodo. Apunta su campo LIGA (Puntero) a NIL (Null).");
                        if (brilloNull) brilloNull.SetEncendido(true);
                    }
                    else
                    {
                        andy.Decir("¡Algoritmo 5.1! Apunta el campo LIGA al nodo que antes era el primero.");
                        EncenderBrilloHijo(managerAnterior.gameObject, "Dato", true);
                    }
                    pasoConexion = 2;
                }
            }
        }

        // 2. CONECTAR AL DATO (ENTRADA)
        else if (tipo == "EntradaHuerto" && cargandoAgua)
        {
            NodoManager managerTocado = objetoTocado.GetComponentInParent<NodoManager>();

            // Conectando el Inicio al nuevo nodo actual
            if (pasoConexion == 0 && managerTocado.gameObject.name.Contains(ObtenerNombreFase()))
            {
                cargandoAgua = false;
                managerActual = managerTocado;
                lineaAgua.SetPosition(1, managerActual.puntoEntrada.position);

                managerActual.ActivarHuerto(); // INFO asignada
                SumarPuntos(10);
                pasoConexion = 1;

                EncenderBrilloHijo(managerActual.gameObject, "Dato", false);
                andy.Decir("¡INFO asignada! Ahora activa su campo LIGA (Puntero).");
                EncenderBrilloHijo(managerActual.gameObject, "Puntero", true);
            }
            // Algoritmo 5.1: Q->LIGA = P (Conectar nuevo al viejo inicio)
            else if (pasoConexion == 2 && managerTocado == managerAnterior)
            {
                cargandoAgua = false;
                lineaAgua.SetPosition(3, managerAnterior.puntoEntrada.position);

                SumarPuntos(10);
                EncenderBrilloHijo(managerAnterior.gameObject, "Dato", false);
                FinalizarFase();
            }
        }

        // 3. CONECTAR A NULL (SOLO PARA EL PRIMER ELEMENTO DE LA HISTORIA)
        else if (tipo == "Null" && cargandoAgua && pasoConexion == 2 && fase == 0)
        {
            cargandoAgua = false;
            lineaAgua.SetPosition(3, puntoEntradaNull.position);
            if (managerActual != null) managerActual.DrenarAgua();

            SumarPuntos(10);
            if (brilloNull) brilloNull.SetEncendido(false);
            FinalizarFase();
        }
    }

    void FinalizarFase()
    {
        ui.MarcarTareaCompletada(fase);
        managerAnterior = managerActual; // El nodo que acabamos de poner se vuelve el "anterior" para el siguiente
        fase++;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Felicidades! Has creado una lista usando inserción al inicio.");
    }

    void Update()
    {
        if (cargandoAgua)
        {
            int ultimoIndice = lineaAgua.positionCount - 1;
            lineaAgua.SetPosition(ultimoIndice, lupi.position);
        }
    }

    // --- Métodos de búsqueda (iguales a tu lógica) ---
    string ObtenerNombreFase()
    {
        string[] nombres = { "Trigo", "Papa", "Calabaza" };
        return nombres[fase];
    }

    GameObject BuscarHuertoPorFase()
    {
        NodoManager[] todos = Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None);
        foreach (var n in todos)
        {
            if (n.gameObject.name.ToLower().Contains(ObtenerNombreFase().ToLower())) return n.gameObject;
        }
        return null;
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

    IEnumerator EsperarSiguiente()
    {
        andy.Decir("¡Enlace completado!");
        yield return new WaitForSeconds(3f);
        // NOTA: No borramos la línea para que el estudiante vea cómo se conectan los huertos
        ProximoPasoSiembra();
    }

    void SumarPuntos(int cant) { puntos += cant; if (textoPuntos) textoPuntos.text = puntos.ToString(); }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = puntos.ToString(); }
}