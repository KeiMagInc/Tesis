using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel5 : MonoBehaviour, ILogicaNivel
{
    [Header("Sprites UI")]
    public Sprite spriteRabano;
    public Sprite spriteZanahoria;
    public Sprite spriteRemolacha;

    public static LogicaNivel5 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;

    [Header("Prefabs Nivel 5 (Doble)")]
    public GameObject prefabRabano;
    public GameObject prefabZanahoria;
    public GameObject prefabRemolacha;

    [Header("Conexiones y Brillos Fijos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private int fase = 0;
    private int pasoConexion = 0; // 0: Inicio->Dato, 1: Siguiente->Destino, 2: Anterior de la lista->NuevoNodo
    private bool cargandoAgua = false;

    private NodoManager managerActual;
    private List<NodoManager> listaNodos = new List<NodoManager>();
    private string[] nombresNodos = { "Rabano", "Zanahoria", "Remolacha" };

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;

        if (LogicaNivel3.instancia != null) LogicaNivel3.instancia.gameObject.SetActive(false);
        if (LogicaNivel4.instancia != null) LogicaNivel4.instancia.gameObject.SetActive(false);

        instancia = this;
        UIManager.instancia.logicaActiva = this;

        // ORDEN CORRECTO: Rabano (0), Zanahoria (1), Remolacha (2)
        UIManager.instancia.SetPrefabs(prefabRabano, prefabZanahoria, prefabRemolacha);
        Sprite[] imagenes = { spriteRabano, spriteZanahoria, spriteRemolacha };
        string[] nombres = { "Rabano", "Zanahoria", "Remolacha" };
        UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);

        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);

        ResetearNivel();
        StartCoroutine(IntroNivel5());
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
            UIManager.instancia.ConfigurarTextosChecklist(
                "Izquierda: sembrar rabano",
                "",
                "Centro: sembrar zanahoria",
                "",
                "Derecha: sembrar remolacha"
            );
        }
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }

    IEnumerator IntroNivel5()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Algoritmo 5.16! Vamos a crear una LISTA DOBLEMENTE LIGADA.");
        yield return new WaitForSeconds(3f);
        andy.Decir("Abre tu mochila para elegir la semilla.");

        UIManager.instancia.MostrarMochilaSolo(true);
        // Esperar a que el jugador abra el panel (como en nivel 3 y 4)
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);

        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        if (fase < nombresNodos.Length)
        {
            andy.Decir("Busca la semilla de " + nombresNodos[fase] + " y siémbrala.");
            UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
            pasoConexion = 0;
        }
    }

    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        StartCoroutine(AsignarNuevoNodo());
    }

    IEnumerator AsignarNuevoNodo()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (nm.gameObject.name.Contains("(Clone)") && !listaNodos.Contains(nm) && nm != managerActual)
            {
                managerActual = nm;
                break;
            }
        }

        if (managerActual != null)
        {
            andy.Decir("¡Nodo creado! Según Cairo, primero conectamos el INICIO al DATO.");
            brilloHead.SetEncendido(true); // Palpita el inicio
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        // Esta línea te dirá en la Consola de Unity qué está llegando exactamente
        Debug.Log("Interacción detectada: Tipo=" + tipo + " | Objeto=" + (objetoTocado != null ? objetoTocado.name : "null"));

        if (managerActual == null) return;
        NodoManager managerTocado = objetoTocado != null ? objetoTocado.GetComponentInParent<NodoManager>() : null;

        // Variables de flexibilidad para los nombres de los letreros
        bool esDato = tipo.Contains("Dato") || tipo.Contains("Entrada");
        bool esSiguiente = tipo.Contains("Siguiente") || tipo.Contains("Salida");
        bool esAnterior = tipo.Contains("Anterior") || tipo.Contains("Izquierda");

        if (!cargandoAgua)
        {
            // --- SECCIÓN: RECOGER AGUA ---

            // PASO 0: Recoger del letrero INICIO (Head)
            if (tipo == "Head" && pasoConexion == 0)
            {
                brilloHead.SetEncendido(false);
                cargandoAgua = true;
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
                andy.Decir("Lleva el agua al letrero DATO de la nueva planta.");
            }
            // PASO 1: Recoger del letrero SIGUIENTE (LIGADER) del nuevo nodo
            else if (esSiguiente && pasoConexion == 1 && managerTocado == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Siguiente", false);
                cargandoAgua = true;

                if (fase == 0)
                {
                    brilloNull.SetEncendido(true);
                    andy.Decir("Como es el primer nodo, su liga SIGUIENTE apunta a NULL.");
                }
                else
                {
                    // Apuntar al que era el antiguo "P" (Inicio actual de la lista)
                    EncenderBrilloEnNodo(listaNodos[0].gameObject, "Dato", true);
                    andy.Decir("Conecta el SIGUIENTE de la nueva planta al DATO de la que ya estaba.");
                }
            }
            // PASO 2: Recoger del letrero ANTERIOR (LIGAIZQ) del nodo que ya estaba en la lista
            else if (esAnterior && pasoConexion == 2 && managerTocado == listaNodos[0])
            {
                EncenderBrilloEnNodo(listaNodos[0].gameObject, "Anterior", false);
                cargandoAgua = true;
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
                andy.Decir("Para terminar la Doble Liga, conecta el ANTERIOR de la vieja planta al DATO de la nueva.");
            }
        }
        else
        {
            // --- SECCIÓN: CONECTAR AGUA ---

            // PASO 0: Conectar al DATO del nuevo nodo
            if (esDato && pasoConexion == 0 && managerTocado == managerActual)
            {
                cargandoAgua = false;
                managerActual.ActivarHuerto();
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);

                andy.Decir("¡Bien! Ahora establezcamos la liga derecha (SIGUIENTE).");
                EncenderBrilloEnNodo(managerActual.gameObject, "Siguiente", true);
                pasoConexion = 1;
            }
            // PASO 1: Conectar la liga derecha (SIGUIENTE -> Destino)
            else if (pasoConexion == 1)
            {
                bool exito = false;
                if (fase == 0 && tipo == "Null") exito = true;
                else if (fase > 0 && esDato && managerTocado == listaNodos[0]) exito = true;

                if (exito)
                {
                    cargandoAgua = false;
                    brilloNull.SetEncendido(false);
                    if (fase > 0) EncenderBrilloEnNodo(listaNodos[0].gameObject, "Dato", false);

                    // Si no es el primer nodo (fase > 0), aplicamos el concepto de Lista Doble
                    if (fase > 0)
                    {
                        andy.Decir("Ahora la liga izquierda: Toca el letrero ANTERIOR de la planta " + nombresNodos[fase - 1]);
                        EncenderBrilloEnNodo(listaNodos[0].gameObject, "Anterior", true);
                        pasoConexion = 2;
                    }
                    else
                    {
                        FinalizarNodoCompleto();
                    }
                }
            }
            // PASO 2: Conectar la liga izquierda (ANTERIOR viejo -> DATO nuevo)
            else if (esDato && pasoConexion == 2 && managerTocado == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
                FinalizarNodoCompleto();
            }
        }
    }

    void FinalizarPasoDoble()
    {
        cargandoAgua = false;
        brilloNull.SetEncendido(false);
        if (fase > 0) EncenderBrilloEnNodo(listaNodos[0].gameObject, "Dato", false);

        if (fase > 0)
        {
            andy.Decir("¡Lista Doble! Ahora el nodo anterior debe apuntar hacia atrás. Toca ANTERIOR de la " + nombresNodos[fase - 1]);
            EncenderBrilloEnNodo(listaNodos[0].gameObject, "Anterior", true);
            pasoConexion = 2;
        }
        else FinalizarNodoCompleto();
    }

    void FinalizarNodoCompleto()
    {
        cargandoAgua = false;
        SumarPuntos(20);
        managerActual.DrenarAgua();
        ApagarBrillosGlobales();

        UIManager.instancia.MarcarTareaCompletada(fase * 2 == 0 ? 0 : fase * 2); // Ajuste simple para tus slots
        listaNodos.Insert(0, managerActual);

        ActualizarLineaFijaDoble();
        fase++;
        managerActual = null;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Excelente! Has creado una Estructura Doblemente Ligada.");
    }

    void ActualizarLineaFijaDoble()
    {
        List<Vector3> camino = new List<Vector3>();
        if (listaNodos.Count == 0) return;

        camino.Add(puntoSalidaHead.position);
        // Camino hacia adelante (LIGADER / Siguiente)
        foreach (var n in listaNodos)
        {
            camino.Add(n.puntoEntrada.position);
            camino.Add(n.puntoSalida.position);
        }
        camino.Add(puntoEntradaNull.position);

        // Camino hacia atrás (LIGAIZQ / Anterior)
        for (int i = listaNodos.Count - 1; i >= 0; i--)
        {
            // Usamos posiciones ligeramente desplazadas o los mismos puntos para representar la doble liga
            camino.Add(listaNodos[i].puntoSalida.position);
            camino.Add(listaNodos[i].puntoEntrada.position);
        }
        camino.Add(puntoSalidaHead.position);

        lineaFija.positionCount = camino.Count;
        lineaFija.SetPositions(camino.ToArray());
    }

    void Update()
    {
        if (lupi == null || lineaAgua == null) return;
        List<Vector3> pts = new List<Vector3>();

        if (cargandoAgua)
        {
            Vector3 origen = puntoSalidaHead.position;

            // Verificamos que managerActual y sus puntos NO sean nulos antes de usarlos
            if (pasoConexion == 1 && managerActual != null && managerActual.puntoSiguiente != null)
                origen = managerActual.puntoSiguiente.position;

            if (pasoConexion == 2 && listaNodos.Count > 1 && listaNodos[1].puntoAnterior != null)
                origen = listaNodos[1].puntoAnterior.position;

            pts.Add(origen);
            pts.Add(lupi.position);
        }

        lineaAgua.positionCount = pts.Count;
        lineaAgua.SetPositions(pts.ToArray());
    }

    IEnumerator EsperarSiguiente()
    {
        yield return new WaitForSeconds(2f);
        ProximoPaso();
    }

    void EncenderBrilloEnNodo(GameObject n, string p, bool e)
    {
        foreach (var b in n.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.gameObject.name.ToUpper().Contains(p.ToUpper())) b.SetEncendido(e);
    }

    void ApagarBrillosGlobales()
    {
        if (brilloHead) brilloHead.SetEncendido(false);
        if (brilloNull) brilloNull.SetEncendido(false);
        foreach (var b in Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None)) b.SetEncendido(false);
    }

    void SumarPuntos(int c) { UIManager.puntosGlobales += c; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }

    void LimpiarNodosEscena()
    {
        foreach (var n in Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (n.name.Contains("(Clone)")) Destroy(n.gameObject);
    }
}