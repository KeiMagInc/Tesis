using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    [Header("Configuración de Tiempo")]
    public int puntosMaximos = 10;
    public int puntosMinimos = 0;
    public float tiempoLimite = 60f;
    private float tiempoInicioEstado;
    [Header("Audios de Error")]
    public AudioClip audioErrorNoRio;
    public AudioClip audioErrorNoLigaAnterior;
    public AudioClip audioErrorNoInfoNueva;
    public AudioClip audioErrorNoCerrarCiclo;
    public AudioClip audioErrorEliminacion;
    public AudioClip audioErrorNoInfoNuevaNodo;
    public AudioClip audioErrorEliminacionFinal;
    public AudioClip audioErrorNoLigaAnteriorCerrar;
    [Header("Audios Diálogos Andy")]
    public AudioClip audioIntroCircular;
    public AudioClip audioPrimerNodoCircular;
    public AudioClip audioInsertarIntermedio;
    public AudioClip audioCerrarCiclo;
    public AudioClip audioIntroEliminar;
    public AudioClip audioEliminarFinal;
    public AudioClip audioEliminarInicio;
    public AudioClip audioPrepararNodo;
    public AudioClip audioSembrar;
    public AudioClip audioExitoCiclo;
    public AudioClip audioExitoTotal;
    [Header("Insignias")]
    public ControladorInsignia controladorInsignia;
    public Sprite insigniaDeEsteNivel;
    public Checkpoint checkpointFinal;
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
    int CalcularPuntosDinamicos()
    {
        float tiempoTranscurrido = Time.time - tiempoInicioEstado;
        float t = Mathf.Clamp01(tiempoTranscurrido / tiempoLimite);
        int puntos = Mathf.RoundToInt(Mathf.Lerp(puntosMaximos, puntosMinimos, t));
        return puntos;
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
                "new Nodo(\"Calabaza\");",
                "new Nodo(\"Papa\");",
                "new Nodo(\"Trigo\");",
                "new Nodo(\"Zanahoria\");",
                "new Nodo(\"Rábano\");"
            );
        }
        ApagarBrillos();
    }
    IEnumerator SecuenciaIntro()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Lupi! Para que el riego de Tahuantindata sea eterno, aplicaremos las Listas Circulares que una estructura donde la vida fluye sin principio ni fin.", audioIntroCircular);
        yield return new WaitForSeconds(audioIntroCircular.length + 0.5f);
        andy.Decir("Abre tu mochila. Cada semilla será un nuevo Nodo en este ciclo sagrado.", audioPrepararNodo);
        UIManager.instancia.MostrarMochilaSolo(true);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPasoSiembra();
    }
    void ProximoPasoSiembra()
    {
        tiempoInicioEstado = Time.time;
        if (fase < nombresNodos.Length)
        {
            UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
            andy.Decir("Siembra la semilla en la parcela. Cada semilla es un NODO del ciclo de Tahuantindata.", audioSembrar);
            subPaso = 0; 
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
                andy.Decir("Como es el primer NODO, el puntero de acceso LIGA debe inicializarse apuntando a sí mismo.", audioPrimerNodoCircular);
            }
            else
            {
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Liga", true);
                andy.Decir("Para insertar elementos, debemos actualizar el campo LIGA del NODO anterior hacia el nuevo componente de la Lista.", audioInsertarIntermedio);
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
                    andy.Decir("¡Escencial mi querido Lupi! Para mantener la circularidad, el enlace del último NODO debe apuntar siempre al primero de la estructura.", audioCerrarCiclo);
                }
                else
                {
                    ReproducirError();
                    if (fase == 0) andy.Decir("¡Cuidado Lupi! Para iniciar la Lista Circular, debemos recoger el flujo sagrado directamente del INICIO.", audioErrorNoRio);
                    else if (subPaso == 0 || subPaso == 1) andy.Decir("El algoritmo indica que el puntero debe nacer de la LIGA del NODO anterior para mantener la secuencia.", audioErrorNoLigaAnterior);
                    else andy.Decir("Para cerrar el círculo, activa la LIGA de la última parcela sembrada.", audioErrorNoLigaAnteriorCerrar);
                }
            }
            else 
            {
                if (tipo == "EntradaHuerto")
                {
                    if (subPaso == 1 && managerTocado == nodoActual)
                    {
                        int puntos = CalcularPuntosDinamicos();
                        cargandoAgua = false;
                        nodoActual.ActivarHuerto();
                        SumarPuntos(puntos);
                        tiempoInicioEstado = Time.time;
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
                        else
                        {
                            ReproducirError();
                            andy.Decir("¡Error de circularidad! El último enlace debe retornar al primer NODO para que el flujo sea perpetuo.", audioErrorNoCerrarCiclo);
                        }
                    }
                    else
                    {
                        ReproducirError();
                        andy.Decir("Ese no es el destino correcto. El agua debe entrar por el campo INFO del nuevo NODO.", audioErrorNoInfoNuevaNodo);
                    }
                }
                else
                {
                    ReproducirError();
                    andy.Decir("¡No pierdas el flujo! Debes llevar el puntero al campo INFO para conectar la estructura.", audioErrorNoInfoNueva);
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
        SumarPuntos(10);
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
        andy.Decir("¡Lupifantástico! La armonía ha vuelto. Has completado una estructura circular donde la vida y el agua fluyen en un retorno perpetuo.", audioExitoCiclo);
        if (audioExitoCiclo != null)
            yield return new WaitForSeconds(audioExitoCiclo.length + 0.5f);
        else
            yield return new WaitForSeconds(3f); 
        andy.Decir("¡Impresionante! Pero el Kaos ha infectado los NODOS, es momento de eliminarlos liberando memoria sin romper la armonía del flujo.", audioIntroEliminar);
        if (audioIntroEliminar != null)
            yield return new WaitForSeconds(audioIntroEliminar.length + 0.5f);
        else
            yield return new WaitForSeconds(3f);
        modoActual = ModoOperacion.Eliminar;
        fase = 0;
        UIManager.instancia.ConfigurarTextosChecklist(
                "",
                "delete(Rábano)",
                "",
                "delete(Calabaza)",
                ""
            );
        ProximoPasoEliminar();
    }
    void ProximoPasoEliminar()
    {
        tiempoInicioEstado = Time.time;
        if (indiceAEliminar == 4)
        {
            andy.Decir("El rábano se ha marchitado. Reasigna el enlace de la zanahoria hacia la calabaza para que el sistema libere la memoria ocupada.", audioEliminarFinal);
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
        }
        else
        {
            andy.Decir("¡Alerta Lupi! Ahora Kaos ha infectado la calabaza. Debemos realizar una Eliminación para proteger el resto de la estructura.", audioEliminarInicio);
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
            else
            {
                ReproducirError();
                andy.Decir("Para eliminar un NODO sin romper el ciclo, debemos reasignar el enlace de la parcela anterior.", audioErrorEliminacion);
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
                    SumarPuntos(10);
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
                        if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null && KaosController.instancia != null)
                        {
                            barreraSiguiente.Abrir();
                            checkpointFinal.AparecerYActivar();
                            controladorInsignia.MostrarInsignia(insigniaDeEsteNivel);
                            KaosController.instancia.RecibirDanoYDesaparecer("ListasCirculares");
                        }
                        andy.Decir("¡Victoria Supervisor de Flujo Circular! Eliminamos el NODO, liberamos la memoria y actualizamos el puntero LIGA.", audioExitoTotal);
                        ReproducirNivelCompleto();
                    }
                }
                else
                {
                    ReproducirError();
                    andy.Decir("¡Lupi cuidado! Si conectas al puntero equivocado, el ciclo de Tahuantindata se perderá en el vacío.", audioErrorEliminacionFinal);
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
    void SumarPuntos(int cant)
    {
        if (KaosController.nivelesTerminados.Contains("ListasCirculares")) return;
        UIManager.puntosGlobales += cant; 
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        if (fuenteAudio && sonidoAcierto) fuenteAudio.PlayOneShot(sonidoAcierto);
    }
    void ReproducirError()
    {
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError);
        if (!KaosController.nivelesTerminados.Contains("ListasCirculares"))
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPasoSiembra(); }
}