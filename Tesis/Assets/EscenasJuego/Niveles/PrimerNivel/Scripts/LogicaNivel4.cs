using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    public static LogicaNivel4 instancia;

    [Header("Sprites UI")]
    public Sprite spriteCalabaza;
    public Sprite spritePapa;
    public Sprite spriteTrigo;
    public Sprite spriteZanahoria;
    public Sprite spriteRabano;

    [Header("Prefabs de los Nodos")]
    public GameObject prefabCalabaza;
    public GameObject prefabPapa;
    public GameObject prefabTrigo;
    public GameObject prefabZanahoria;
    public GameObject prefabRabano;

    [Header("Referencias de Escena")]
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;

    [Header("Puntos de Control")]
    public Transform puntoRio;
    public EfectoLetrero brilloRio;

    // Variables de estado
    private List<NodoManager> listaNodos = new List<NodoManager>();
    private NodoManager nodoActual;
    private bool cargandoAgua = false;
    private int fase = 0;
    private int subPaso = 0;
    private List<Vector3> puntosConfirmados = new List<Vector3>();
    private string[] nombresNodos = { "Calabaza", "Papa", "Trigo", "Zanahoria", "Rabano" };

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;

        // 1. Intercambiamos el orden de los PREFABS (Trigo primero, luego Papa)
        UIManager.instancia.SetPrefabs(prefabCalabaza, prefabPapa, prefabTrigo, prefabRabano, prefabZanahoria);

        // 2. Intercambiamos el orden de los SPRITES
        Sprite[] misSprites = { spriteCalabaza, spritePapa, spriteTrigo, spriteRabano, spriteZanahoria };

        // 3. Intercambiamos el orden de los NOMBRES
        string[] misNombres = { "Calabaza", "Papa", "Trigo", "Rabano", "Zanahoria" };

        UIManager.instancia.ConfigurarBotonesUI(misSprites, misNombres);

        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);

        ResetearNivel();
        StartCoroutine(SecuenciaIntro());
    }

    public void ResetearNivel()
    {
        fase = 0;
        subPaso = 0;
        cargandoAgua = false;
        listaNodos.Clear();
        nodoActual = null;

        // 1. Limpiar la lista de puntos y resetear las líneas
        puntosConfirmados.Clear();
        if (puntoRio != null) puntosConfirmados.Add(puntoRio.position);

        if (lineaAgua != null) lineaAgua.positionCount = 0;
        if (lineaFija != null)
        {
            lineaFija.positionCount = 0;
            lineaFija.SetPositions(new Vector3[0]); // Fuerza la limpieza de vértices antiguos
        }

        // 2. BUSCAR Y DESTRUIR LAS PLANTAS SEMBRADAS (Clones)
        // Asegúrate de que tus prefabs de plantas tengan el Tag "Planta"
        foreach (var p in GameObject.FindGameObjectsWithTag("Planta"))
        {
            Destroy(p);
        }

        // 3. Resetear las zonas de plantado para que permitan volver a sembrar
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();

        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist(
                "Sembrar calabaza",
                "Sembrar papa",
                "Sembrar trigo",
                "Sembrar zanahoria",
                "Sembrar rábano"
            );
        }
        ApagarBrillos();
    }

    IEnumerator SecuenciaIntro()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Algoritmo 8.10! Vamos a construir una LISTA CIRCULAR de 5 cultivos.");
        yield return new WaitForSeconds(2.5f);

        andy.Decir("Abre tu mochila y comencemos por la derecha.");
        UIManager.instancia.MostrarMochilaSolo(true);

        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);

        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPasoSiembra();
    }

    void ProximoPasoSiembra()
    {
        if (fase < nombresNodos.Length)
        {
            UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
            andy.Decir("Siembra " + nombresNodos[fase] + " en la posición indicada.");
            subPaso = 0; // Esperando siembra
        }
        else
        {
            andy.Decir("¡Increíble! Has completado el ciclo circular de los 5 cultivos.");
        }
    }

    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        StartCoroutine(EsperarYAsignarNodo());
    }

    IEnumerator EsperarYAsignarNodo()
    {
        yield return new WaitForSeconds(0.2f);
        // Buscar el nodo que se acaba de clonar
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (nm.name.Contains("(Clone)") && !listaNodos.Contains(nm))
            {
                nodoActual = nm;
                break;
            }
        }

        if (nodoActual != null)
        {
            subPaso = 1; // Recoger agua
            if (fase == 0)
            {
                brilloRio.SetEncendido(true);
                andy.Decir("Recoge agua del RÍO para el primer nodo.");
            }
            else
            {
                // El agua se toma del puntero del nodo anterior
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Puntero", true);
                andy.Decir("Ahora conecta el PUNTERO de la planta anterior a la ENTRADA de la nueva.");
            }
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado)
    {
        if (nodoActual == null) return;
        NodoManager managerTocado = objetoTocado.GetComponentInParent<NodoManager>();

        if (!cargandoAgua)
        {
            // RECOGER AGUA
            if (fase == 0 && (tipo == "LC" || tipo == "Head"))
            {
                cargandoAgua = true;
                brilloRio.SetEncendido(false);
                EncenderBrilloHijo(nodoActual.gameObject, "Dato", true);
            }
            else if (fase > 0 && tipo == "SalidaHuerto" && managerTocado == listaNodos[fase - 1])
            {
                cargandoAgua = true;
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Puntero", false);
                EncenderBrilloHijo(nodoActual.gameObject, "Dato", true);

                // Añadimos el punto de salida del anterior para que la línea se fije ahí al recoger
                if (!puntosConfirmados.Contains(managerTocado.puntoSalida.position))
                {
                    puntosConfirmados.Add(managerTocado.puntoSalida.position);
                    DibujarLineaFija();
                }
            }
            // --- BUSCA ESTO Y DÉJALO ASÍ (Borra el bloque de puntosConfirmados.Add) ---
            else if (subPaso == 2 && tipo == "SalidaHuerto" && managerTocado == nodoActual)
            {
                cargandoAgua = true;
                EncenderBrilloHijo(nodoActual.gameObject, "Puntero", false);

                GameObject destino = (fase == 0) ? nodoActual.gameObject : listaNodos[0].gameObject;
                EncenderBrilloHijo(destino, "Dato", true);
                andy.Decir("¡Agua recogida! Llévala a la ENTRADA para cerrar el círculo.");
            }
        }
        else
        {
            // CONECTAR AGUA
            if (tipo == "EntradaHuerto")
            {
                if (subPaso == 1 && managerTocado == nodoActual)
                {
                    cargandoAgua = false;
                    nodoActual.ActivarHuerto();
                    SumarPuntos(10);

                    // Solo agregamos la entrada para que la línea se quede en el "Dato"
                    puntosConfirmados.Add(nodoActual.puntoEntrada.position);
                    DibujarLineaFija();
                    EncenderBrilloHijo(nodoActual.gameObject, "Dato", false);
                    subPaso = 2;
                    EncenderBrilloHijo(nodoActual.gameObject, "Puntero", true);
                }
                // --- BUSCA ESTA PARTE EN EL SUBPASO 2 DE CONECTAR AGUA ---
                else if (subPaso == 2)
                {
                    bool esCorrecto = (fase == 0 && managerTocado == nodoActual) || (fase > 0 && managerTocado == listaNodos[0]);
                    if (esCorrecto)
                    {
                        cargandoAgua = false;

                        // --- CAMBIO AQUÍ: Fijamos los dos tramos finales de golpe ---
                        puntosConfirmados.Add(nodoActual.puntoSalida.position);      // Tramo interno (Dato -> Puntero)
                        puntosConfirmados.Add(managerTocado.puntoEntrada.position);  // Tramo de cierre (Puntero -> Inicio)
                        DibujarLineaFija();

                        EncenderBrilloHijo(managerTocado.gameObject, "Dato", false);
                        FinalizarCicloFase();
                    }
                }
            }
        }
    }

    void FinalizarCicloFase()
    {
        SumarPuntos(15);

        // --- AÑADE ESTA LÍNEA AQUÍ ---
        if (nodoActual != null) nodoActual.DrenarAgua();
        // ----------------------------

        listaNodos.Add(nodoActual);
        UIManager.instancia.MarcarTareaCompletada(fase);
        ApagarBrillos();

        // Si no es el último nivel, el siguiente tramo empezará desde la salida de este nodo
        if (fase < nombresNodos.Length - 1)
        {
            puntosConfirmados.RemoveAt(puntosConfirmados.Count - 1); // Quitamos el punto de cierre temporal
        }

        fase++;
        nodoActual = null;
        StartCoroutine(EsperarSiguiente());
    }

    void DibujarLineaFija()
    {
        lineaFija.positionCount = puntosConfirmados.Count;
        lineaFija.SetPositions(puntosConfirmados.ToArray());
    }

    void Update()
    {
        if (cargandoAgua && lupi != null && puntosConfirmados.Count > 0)
        {
            lineaAgua.positionCount = 2;

            // --- CAMBIO AQUÍ: El origen cambia según el subPaso ---
            Vector3 origenManguera = (subPaso == 1) ?
                puntosConfirmados[puntosConfirmados.Count - 1] : // Sale del Río o planta anterior
                nodoActual.puntoSalida.position;                // Sale del Puntero actual

            lineaAgua.SetPositions(new Vector3[] { origenManguera, lupi.position });
        }
        else lineaAgua.positionCount = 0;
    }

    // --- UTILIDADES ---
    void EncenderBrilloHijo(GameObject n, string parte, bool activar)
    {
        if (n == null) return;
        foreach (var b in n.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.name.ToUpper().Contains(parte.ToUpper())) b.SetEncendido(activar);
    }

    void ApagarBrillos()
    {
        if (brilloRio) brilloRio.SetEncendido(false);
        foreach (var b in Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None)) b.SetEncendido(false);
    }

    void SumarPuntos(int cant) { UIManager.puntosGlobales += cant; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPasoSiembra(); }
}