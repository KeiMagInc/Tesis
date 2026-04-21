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

    private enum ModoOperacion { Insertar, Eliminar }
    private ModoOperacion modoActual = ModoOperacion.Insertar;
    // Usaremos estos para saber qué estamos borrando
    private int indiceAEliminar = 4; // Empezaremos eliminando el Rábano (el último)

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

    void OnDisable()
    {
        if (UIManager.instancia != null && UIManager.instancia.logicaActiva == (ILogicaNivel)this)
        {
            UIManager.instancia.logicaActiva = null;
        }
        // LIMPIEZA AL SALIR:
        ResetearNivel();
    }

    public void ResetearNivel()
    {
        modoActual = ModoOperacion.Insertar;
        indiceAEliminar = 4;
        fase = 0;
        subPaso = 0;
        cargandoAgua = false;

        listaNodos.Clear();
        nodoActual = null;
        puntosConfirmados.Clear();

        // 1. Limpieza total de LineRenderers (Visual)
        if (lineaAgua != null)
        {
            lineaAgua.positionCount = 0;
            lineaAgua.SetPositions(new Vector3[0]);
        }
        if (lineaFija != null)
        {
            lineaFija.positionCount = 0;
            lineaFija.SetPositions(new Vector3[0]);
        }

        // 2. Re-inicializar punto de origen
        if (puntoRio != null) puntosConfirmados.Add(puntoRio.position);

        // 3. Destruir plantas (con Tag "Planta")
        GameObject[] plantas = GameObject.FindGameObjectsWithTag("Planta");
        foreach (var p in plantas) Destroy(p);

        // 4. Resetear zonas de plantado
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();

        // 5. Reset UI
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
                // El agua se toma del Liga del nodo anterior
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Liga", true);
                andy.Decir("Ahora conecta el LIGA de la planta anterior a la ENTRADA de la nueva.");
            }
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado)
    {
        if (modoActual == ModoOperacion.Insertar)
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
                    EncenderBrilloHijo(nodoActual.gameObject, "Info", true);
                }
                else if (fase > 0 && tipo == "SalidaHuerto" && managerTocado == listaNodos[fase - 1])
                {
                    cargandoAgua = true;
                    EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Liga", false);
                    EncenderBrilloHijo(nodoActual.gameObject, "Info", true);

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
                    EncenderBrilloHijo(nodoActual.gameObject, "Liga", false);

                    GameObject destino = (fase == 0) ? nodoActual.gameObject : listaNodos[0].gameObject;
                    EncenderBrilloHijo(destino, "Info", true);
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

                        // Solo agregamos la entrada para que la línea se quede en el "Info"
                        puntosConfirmados.Add(nodoActual.puntoEntrada.position);
                        DibujarLineaFija();
                        EncenderBrilloHijo(nodoActual.gameObject, "Info", false);
                        subPaso = 2;
                        EncenderBrilloHijo(nodoActual.gameObject, "Liga", true);
                    }
                    // --- BUSCA ESTA PARTE EN EL SUBPASO 2 DE CONECTAR AGUA ---
                    else if (subPaso == 2)
                    {
                        bool esCorrecto = (fase == 0 && managerTocado == nodoActual) || (fase > 0 && managerTocado == listaNodos[0]);
                        if (esCorrecto)
                        {
                            cargandoAgua = false;

                            // --- CAMBIO AQUÍ: Fijamos los dos tramos finales de golpe ---
                            puntosConfirmados.Add(nodoActual.puntoSalida.position);      // Tramo interno (Info -> Liga)
                            puntosConfirmados.Add(managerTocado.puntoEntrada.position);  // Tramo de cierre (Liga -> Inicio)
                            DibujarLineaFija();

                            EncenderBrilloHijo(managerTocado.gameObject, "Info", false);
                            FinalizarCicloFase();
                        }
                    }
                }
            }
        }
        else
        {
            LogicaEliminar(tipo, objetoTocado);
        }
        
    }

    void FinalizarCicloFase()
    {
        SumarPuntos(15);
        if (nodoActual != null) nodoActual.DrenarAgua();
        listaNodos.Add(nodoActual);
        UIManager.instancia.MarcarTareaCompletada(fase);
        ApagarBrillos();

        if (fase < nombresNodos.Length - 1)
        {
            puntosConfirmados.RemoveAt(puntosConfirmados.Count - 1);
            fase++;
            nodoActual = null;
            StartCoroutine(EsperarSiguiente());
        }
        else
        {
            // ¡TERMINAMOS DE SEMBRAR! Pasamos a eliminación
            StartCoroutine(PrepararEliminacion());
        }
    }

    IEnumerator PrepararEliminacion()
    {
        andy.Decir("¡Increíble! Ahora aprenderemos el Algoritmo 8.10.2: ELIMINAR nodos.");
        yield return new WaitForSeconds(3f);

        modoActual = ModoOperacion.Eliminar;
        fase = 0; // Reiniciamos fase para el checklist de eliminación


        UIManager.instancia.ConfigurarTextosChecklist(
                "",
                "Eliminar rábano",
                "",
                "Eliminar calabaza",
                ""
            );

        ProximoPasoEliminar();
    }

    void ProximoPasoEliminar()
    {
        if (indiceAEliminar == 4) // Caso 1: Eliminar el último (Rábano)
        {
            andy.Decir("Para eliminar el RÁBANO, conecta la ZANAHORIA directamente a la CALABAZA.");
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
        }
        else // Caso 2: Eliminar el primero (Calabaza - El "LC" del libro)
        {
            andy.Decir("Ahora eliminaremos el inicio. Conecta la ZANAHORIA a la PAPA.");
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
        }
    }

    void LogicaEliminar(string tipo, GameObject objetoTocado)
    {
        NodoManager managerTocado = objetoTocado.GetComponentInParent<NodoManager>();

        if (!cargandoAgua)
        {
            // Recogemos agua del Liga del nodo ANTERIOR al que queremos borrar
            if (tipo == "SalidaHuerto" && managerTocado == listaNodos[3])
            {
                cargandoAgua = true;
                EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", false);

                // Brillo en el destino según el libro
                int destino = (indiceAEliminar == 4) ? 0 : 1;
                EncenderBrilloHijo(listaNodos[destino].gameObject, "Info", true);
            }
        }
        else
        {
            if (tipo == "EntradaHuerto")
            {
                int destino = (indiceAEliminar == 4) ? 0 : 1;
                if (managerTocado == listaNodos[destino])
                {
                    cargandoAgua = false;
                    EncenderBrilloHijo(listaNodos[destino].gameObject, "Info", false);

                    // Ejecutar eliminación visual
                    int objetivo = (indiceAEliminar == 4) ? 4 : 0;
                    listaNodos[objetivo].IniciarSecuenciaEliminacion();

                    SumarPuntos(25);
                    UIManager.instancia.MarcarTareaCompletada(fase);

                    // ACTUALIZAMOS LAS LÍNEAS ANTES DE CAMBIAR EL ÍNDICE
                    ActualizarLineaFijaPostEliminacion();

                    if (indiceAEliminar == 4)
                    {
                        indiceAEliminar = 0;
                        fase++;
                        StartCoroutine(EsperarSiguienteEliminar());
                    }
                    else
                    {
                        andy.Decir("¡Lista circular actualizada con éxito! El inicio ahora es la Papa.");
                    }
                }
            }
        }
    }

    void ActualizarLineaFijaPostEliminacion()
    {
        List<Vector3> pts = new List<Vector3>();

        // 1. Siempre empezamos desde el Río (Acceso de la lista)
        pts.Add(puntoRio.position);

        // CASO 1: Acabamos de eliminar el RÁBANO (indiceAEliminar todavía vale 4)
        if (indiceAEliminar == 4)
        {
            // El INICIO se MANTIENE en la CALABAZA (0)
            pts.Add(listaNodos[0].puntoEntrada.position);

            // Dibujamos el camino por los nodos que quedan vivos: 0 -> 1 -> 2 -> 3
            for (int i = 0; i <= 3; i++)
            {
                pts.Add(listaNodos[i].puntoEntrada.position);
                pts.Add(listaNodos[i].puntoSalida.position);
            }
            // CIERRE CIRCULAR: La Zanahoria (3) apunta de regreso a la Calabaza (0)
            pts.Add(listaNodos[0].puntoEntrada.position);
        }
        // CASO 2: Acabamos de eliminar la CALABAZA (indiceAEliminar ya vale 0)
        else if (indiceAEliminar == 0)
        {
            // El INICIO (Río) se MUEVE ahora a la PAPA (1)
            pts.Add(listaNodos[1].puntoEntrada.position);

            // Dibujamos el camino restante: 1 -> 2 -> 3
            for (int i = 1; i <= 3; i++)
            {
                pts.Add(listaNodos[i].puntoEntrada.position);
                pts.Add(listaNodos[i].puntoSalida.position);
            }
            // CIERRE CIRCULAR: La Zanahoria (3) ahora apunta a la Papa (1)
            pts.Add(listaNodos[1].puntoEntrada.position);
        }

        // 4. Aplicamos los puntos a la línea fija
        lineaFija.positionCount = pts.Count;
        lineaFija.SetPositions(pts.ToArray());

        // Limpieza para evitar cables sueltos visuales
        puntosConfirmados.Clear();
    }

    IEnumerator EsperarSiguienteEliminar() { yield return new WaitForSeconds(2f); ProximoPasoEliminar(); }

    void DibujarLineaFija()
    {
        lineaFija.positionCount = puntosConfirmados.Count;
        lineaFija.SetPositions(puntosConfirmados.ToArray());
    }

    void Update()
    {
        if (cargandoAgua && lupi != null)
        {
            lineaAgua.positionCount = 2;
            Vector3 origen = Vector3.zero;

            if (modoActual == ModoOperacion.Insertar)
                origen = (subPaso == 1) ? puntosConfirmados[puntosConfirmados.Count - 1] : nodoActual.puntoSalida.position;
            else
                origen = listaNodos[3].puntoSalida.position; // En este nivel siempre borramos usando el nodo 3 como puente

            lineaAgua.SetPositions(new Vector3[] { origen, lupi.position });
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