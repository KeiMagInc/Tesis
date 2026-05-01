using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoCompletado;
    [Header("Progreso")]
    public BarreraProgreso barreraSiguiente;
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
    private List<NodoManager> listaNodos = new List<NodoManager>();
    private NodoManager nodoActual;
    private bool cargandoAgua = false;
    private int fase = 0;
    private int subPaso = 0;
    private List<Vector3> puntosConfirmados = new List<Vector3>();
    private string[] nombresNodos = { "Calabaza", "Papa", "Trigo", "Zanahoria", "Rabano" };
    private enum ModoOperacion { Insertar, Eliminar }
    private ModoOperacion modoActual = ModoOperacion.Insertar;
    private int indiceAEliminar = 4; 

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabCalabaza, prefabPapa, prefabTrigo, prefabRabano, prefabZanahoria);
        Sprite[] misSprites = { spriteCalabaza, spritePapa, spriteTrigo, spriteRabano, spriteZanahoria };
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
        if (puntoRio != null) puntosConfirmados.Add(puntoRio.position);
        GameObject[] plantas = GameObject.FindGameObjectsWithTag("Planta");
        foreach (var p in plantas) Destroy(p);
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
            subPaso = 0; 
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
            subPaso = 1; 
            if (fase == 0)
            {
                brilloRio.SetEncendido(true);
                andy.Decir("Recoge agua del RÍO para el primer nodo.");
            }
            else
            {
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
                    if (!puntosConfirmados.Contains(managerTocado.puntoSalida.position))
                    {
                        puntosConfirmados.Add(managerTocado.puntoSalida.position);
                        DibujarLineaFija();
                    }
                }
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
                if (tipo == "EntradaHuerto")
                {
                    if (subPaso == 1 && managerTocado == nodoActual)
                    {
                        cargandoAgua = false;
                        nodoActual.ActivarHuerto();
                        SumarPuntos(10);
                        puntosConfirmados.Add(nodoActual.puntoEntrada.position);
                        DibujarLineaFija();
                        EncenderBrilloHijo(nodoActual.gameObject, "Info", false);
                        subPaso = 2;
                        EncenderBrilloHijo(nodoActual.gameObject, "Liga", true);
                    }
                    else if (subPaso == 2)
                    {
                        bool esCorrecto = (fase == 0 && managerTocado == nodoActual) || (fase > 0 && managerTocado == listaNodos[0]);
                        if (esCorrecto)
                        {
                            cargandoAgua = false;
                            puntosConfirmados.Add(nodoActual.puntoSalida.position);     
                            puntosConfirmados.Add(managerTocado.puntoEntrada.position);  
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
            StartCoroutine(PrepararEliminacion());
        }
    }

    IEnumerator PrepararEliminacion()
    {
        andy.Decir("¡Increíble! Ahora aprenderemos el Algoritmo 8.10.2: ELIMINAR nodos.");
        yield return new WaitForSeconds(3f);
        modoActual = ModoOperacion.Eliminar;
        fase = 0; 
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
        if (indiceAEliminar == 4) 
        {
            andy.Decir("Para eliminar el RÁBANO, conecta la ZANAHORIA directamente a la CALABAZA.");
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
        }
        else 
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
            if (tipo == "SalidaHuerto" && managerTocado == listaNodos[3])
            {
                cargandoAgua = true;
                EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", false);
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
                    int objetivo = (indiceAEliminar == 4) ? 4 : 0;
                    listaNodos[objetivo].IniciarSecuenciaEliminacion();
                    SumarPuntos(25);
                    UIManager.instancia.MarcarTareaCompletada((fase * 2) + 1);
                    ActualizarLineaFijaPostEliminacion();
                    if (indiceAEliminar == 4)
                    {
                        indiceAEliminar = 0;
                        fase++;
                        StartCoroutine(EsperarSiguienteEliminar());
                    }
                    else
                    {                        
                        if (barreraSiguiente != null)
                        {
                            barreraSiguiente.Abrir();
                        }
                        andy.Decir("¡Lista circular actualizada con éxito! El inicio ahora es la Papa.");
                        ReproducirNivelCompleto();
                    }
                }
            }
        }
    }

    void ReproducirNivelCompleto()
    {
        if (fuenteAudio && sonidoCompletado)
            for (int i = 0; i < 2; i++) fuenteAudio.PlayOneShot(sonidoCompletado);
    }

    void ActualizarLineaFijaPostEliminacion()
    {
        List<Vector3> pts = new List<Vector3>();
        pts.Add(puntoRio.position);
        if (indiceAEliminar == 4)
        {
            pts.Add(listaNodos[0].puntoEntrada.position);
            for (int i = 0; i <= 3; i++)
            {
                pts.Add(listaNodos[i].puntoEntrada.position);
                pts.Add(listaNodos[i].puntoSalida.position);
            }
            pts.Add(listaNodos[0].puntoEntrada.position);
        }
        else if (indiceAEliminar == 0)
        {
            pts.Add(listaNodos[1].puntoEntrada.position);
            for (int i = 1; i <= 3; i++)
            {
                pts.Add(listaNodos[i].puntoEntrada.position);
                pts.Add(listaNodos[i].puntoSalida.position);
            }
            pts.Add(listaNodos[1].puntoEntrada.position);
        }
        lineaFija.positionCount = pts.Count;
        lineaFija.SetPositions(pts.ToArray());
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
                origen = listaNodos[3].puntoSalida.position;

            lineaAgua.SetPositions(new Vector3[] { origen, lupi.position });
        }
        else lineaAgua.positionCount = 0;
    }

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