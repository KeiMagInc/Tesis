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
    private int pasoConexion = 0;
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
        lineaAgua.positionCount = 0; lineaFija.positionCount = 0;
        listaNodos.Clear(); managerActual = null;

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
        andy.Decir("¡Vamos a crear una LISTA DOBLEMENTE LIGADA insertando nodos al final!");
        yield return new WaitForSeconds(3.5f);
        andy.Decir("Abre tu mochila para elegir la semilla.");
        UIManager.instancia.MostrarMochilaSolo(true);
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
                managerActual = nm; break;
            }
        }

        if (managerActual != null)
        {
            managerActual.ActivarHuerto(); // Activamos visualmente
            if (fase == 0)
            {
                andy.Decir("Lleva el inicio al PUNTERO ANTERIOR del rábano.");
                brilloHead.SetEncendido(true);
            }
            else
            {
                string viejo = nombresNodos[fase - 1];
                andy.Decir($"Conecta la SALIDA SIGUIENTE de {viejo} a la ENTRADA ANTERIOR de {nombresNodos[fase]}.");
                EncenderBrilloEnNodo(listaNodos[fase - 1].gameObject, "Siguiente", true);
            }
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        Debug.Log("Interacción detectada: " + tipo);
        if (managerActual == null) return;

        NodoManager managerTocado = objetoTocado != null ? objetoTocado.GetComponentInParent<NodoManager>() : null;
        NodoManager nodoAnterior = listaNodos.Count > 0 ? listaNodos[listaNodos.Count - 1] : null;

        if (!cargandoAgua)
        {
            // === RECOGER AGUA ===
            if (fase == 0)
            {
                if (tipo == "Head" && pasoConexion == 0)
                {
                    brilloHead.SetEncendido(false); cargandoAgua = true;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Anterior", true);
                }
                else if (tipo == "SalidaSiguiente" && pasoConexion == 1 && managerTocado == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Siguiente", false);
                    cargandoAgua = true; brilloNull.SetEncendido(true);
                    andy.Decir("Ahora conecta la SALIDA SIGUIENTE del rábano hacia NULL.");
                }
            }
            else // Fase 1 (Zanahoria) y Fase 2 (Remolacha)
            {
                if (tipo == "SalidaSiguiente" && pasoConexion == 0 && managerTocado == nodoAnterior)
                {
                    EncenderBrilloEnNodo(nodoAnterior.gameObject, "Siguiente", false);
                    cargandoAgua = true;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Anterior", true);
                }
                else if (tipo == "SalidaAnterior" && pasoConexion == 1 && managerTocado == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Anterior", false);
                    cargandoAgua = true;
                    EncenderBrilloEnNodo(nodoAnterior.gameObject, "Siguiente", true);
                    andy.Decir($"¡Enlace doble! Conecta la SALIDA ANTERIOR de {nombresNodos[fase]} a la ENTRADA SIGUIENTE de {nombresNodos[fase - 1]}.");
                }
                else if (tipo == "SalidaSiguiente" && pasoConexion == 2 && managerTocado == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Siguiente", false);
                    cargandoAgua = true; brilloNull.SetEncendido(true);
                    andy.Decir($"Finalmente, conecta la SALIDA SIGUIENTE de {nombresNodos[fase]} a NULL.");
                }
            }
        }
        else
        {
            // === CONECTAR AGUA ===
            if (fase == 0)
            {
                if (tipo == "EntradaAnterior" && pasoConexion == 0 && managerTocado == managerActual)
                {
                    cargandoAgua = false; pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Anterior", false);
                    EncenderBrilloEnNodo(managerActual.gameObject, "Siguiente", true);
                    andy.Decir("¡Bien! Ve a la SALIDA SIGUIENTE del rábano.");
                }
                else if (tipo == "Null" && pasoConexion == 1)
                {
                    FinalizarNodoCompleto();
                }
            }
            else // Fase 1 y 2
            {
                if (tipo == "EntradaAnterior" && pasoConexion == 0 && managerTocado == managerActual)
                {
                    cargandoAgua = false; pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Anterior", false);
                    EncenderBrilloEnNodo(managerActual.gameObject, "Anterior", true); // Brillo en la salida anterior
                    andy.Decir($"Ve a la SALIDA ANTERIOR de {nombresNodos[fase]}.");
                }
                else if (tipo == "EntradaSiguiente" && pasoConexion == 1 && managerTocado == nodoAnterior)
                {
                    cargandoAgua = false; pasoConexion = 2;
                    EncenderBrilloEnNodo(nodoAnterior.gameObject, "Siguiente", false);
                    EncenderBrilloEnNodo(managerActual.gameObject, "Siguiente", true);
                    andy.Decir($"Por último, ve a la SALIDA SIGUIENTE de {nombresNodos[fase]}.");
                }
                else if (tipo == "Null" && pasoConexion == 2)
                {
                    FinalizarNodoCompleto();
                }
            }
        }
    }

    void FinalizarNodoCompleto()
    {
        cargandoAgua = false;
        SumarPuntos(20);
        managerActual.DrenarAgua();
        ApagarBrillosGlobales();

        UIManager.instancia.MarcarTareaCompletada(fase * 2 == 0 ? 0 : fase * 2);
        listaNodos.Add(managerActual); // Inserción AL FINAL de la lista

        ActualizarLineaFijaDoble();
        fase++;
        managerActual = null;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else andy.Decir("¡Excelente! Has creado una Estructura Doblemente Ligada insertando al final.");
    }

    void ActualizarLineaFijaDoble()
    {
        if (listaNodos.Count == 0) return;
        List<Vector3> camino = new List<Vector3>();

        // Dibujamos el recorrido principal de ida y vuelta para simular la doble liga
        camino.Add(puntoSalidaHead.position);

        // Ida
        for (int i = 0; i < listaNodos.Count; i++)
        {
            camino.Add(listaNodos[i].puntoEntradaAnterior.position);
            camino.Add(listaNodos[i].puntoSalidaSiguiente.position);
        }
        camino.Add(puntoEntradaNull.position);

        // Vuelta
        for (int i = listaNodos.Count - 1; i > 0; i--)
        {
            camino.Add(listaNodos[i].puntoSalidaAnterior.position);
            camino.Add(listaNodos[i - 1].puntoEntradaSiguiente.position);
        }

        lineaFija.positionCount = camino.Count;
        lineaFija.SetPositions(camino.ToArray());
    }

    void Update()
    {
        if (lupi == null || lineaAgua == null) return;
        List<Vector3> pts = new List<Vector3>();

        if (cargandoAgua && managerActual != null)
        {
            Vector3 origen = puntoSalidaHead.position;

            if (fase == 0)
            {
                if (pasoConexion == 1) origen = managerActual.puntoSalidaSiguiente.position;
            }
            else
            {
                NodoManager anterior = listaNodos[listaNodos.Count - 1];
                if (pasoConexion == 0) origen = anterior.puntoSalidaSiguiente.position;
                if (pasoConexion == 1) origen = managerActual.puntoSalidaAnterior.position;
                if (pasoConexion == 2) origen = managerActual.puntoSalidaSiguiente.position;
            }

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

    void EncenderBrilloEnNodo(GameObject n, string palabra, bool encender)
    {
        foreach (var b in n.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.gameObject.name.ToUpper().Contains(palabra.ToUpper())) b.SetEncendido(encender);
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