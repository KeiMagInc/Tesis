using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    [Header("Información del Nivel UI")]
    public string nombreDelNivel = "Listas Ciculares";
    private Color colorOriginalPuntos;
    private Vector3 escalaOriginalPuntos;
    private Coroutine rutinaEfectoPuntos;
    [Header("Posicionamiento")]
    public Transform puntoInicioNivel;
    [Header("Pantalla Final")]
    public GameObject panelFinal;
    public TextMeshProUGUI textoPuntajeFinal;
    public TextMeshProUGUI textoAciertos;
    public TextMeshProUGUI textoFallos;
    private int aciertosContador = 0;
    private int fallosContador = 0;
    private int puntosAlIniciarNivel;
    [Header("Configuración de Tiempo")]
    public int puntosMaximos = 10;
    public int puntosMinimos = 1;
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
    public Sprite spriteCodorniz;
    public Sprite spriteGallina;
    public Sprite spriteCerdo;
    public Sprite spriteOveja;
    public Sprite spriteVaca;
    [Header("Prefabs de los Nodos")]
    public GameObject prefabCodorniz;
    public GameObject prefabGallina;
    public GameObject prefabCerdo;
    public GameObject prefabOveja;
    public GameObject prefabVaca;
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
    private string[] nombresNodos = { "Codorniz", "Gallina", "Cerdo", "Oveja", "Vaca" };
    private enum ModoOperacion { Insertar, Eliminar }
    private ModoOperacion modoActual = ModoOperacion.Insertar;
    private int indiceAEliminar = 4;
    void Awake()
    {
        instancia = this;
        if (textoPuntos != null)
        {
            colorOriginalPuntos = textoPuntos.color;
            escalaOriginalPuntos = textoPuntos.transform.localScale;
        }
    }
    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        puntosAlIniciarNivel = UIManager.puntosGlobales;
        UIManager.instancia.logicaActiva = this;
        UIManager.instancia.SetPrefabs(prefabCodorniz, prefabGallina, prefabCerdo, prefabOveja, prefabVaca);
        Sprite[] misSprites = { spriteCodorniz, spriteGallina, spriteCerdo, spriteOveja, spriteVaca };
        string[] misNombres = { "Codorniz", "Gallina", "Cerdo", "Oveja", "Vaca" };
        UIManager.instancia.ConfigurarBotonesUI(misSprites, misNombres);
        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        ResetearNivel();
        ActualizarCabeceraNivel4();
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
    void ActualizarCabeceraNivel4()
    {
        if (UIManager.instancia == null) return;
        string operacionTexto = "";
        if (modoActual == ModoOperacion.Insertar)
        {
            operacionTexto = "Inserción en Lista Circular";
        }
        else
        {
            if (indiceAEliminar == 4)
                operacionTexto = "Eliminación por el final de la lista";
            else
                operacionTexto = "Eliminación por el inicio de la lista";
        }
        UIManager.instancia.ConfigurarCabeceraNivel(nombreDelNivel, operacionTexto);
    }
    IEnumerator AnimacionPuntos(bool esAumento)
    {
        textoPuntos.color = esAumento ? Color.green : Color.red;
        float tiempoPaso = 0.07f;
        Vector3 escalaFlash = escalaOriginalPuntos * 1.3f;
        for (int i = 0; i < 3; i++)
        {
            textoPuntos.transform.localScale = escalaFlash;
            yield return new WaitForSeconds(tiempoPaso);
            textoPuntos.transform.localScale = escalaOriginalPuntos;
            yield return new WaitForSeconds(tiempoPaso);
        }
        textoPuntos.transform.localScale = escalaOriginalPuntos;
        textoPuntos.color = colorOriginalPuntos;
    }
    IEnumerator MostrarResumenFinal()
    {
        yield return new WaitForSeconds(3.5f);
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (textoPuntajeFinal) textoPuntajeFinal.text = UIManager.puntosGlobales.ToString();
            if (textoAciertos) textoAciertos.text = aciertosContador.ToString();
            if (textoFallos) textoFallos.text = fallosContador.ToString();
            Debug.Log("Panel Final activado y Lupi congelado.");
        }
        else
        {
            Debug.LogError("¡No has asignado el Panel Final en el Inspector!");
        }
    }
    void CongelarLupi(bool congelar)
    {
        if (lupi != null)
        {
            var controlMovimiento = lupi.GetComponent<PlayerController>();
            if (controlMovimiento != null) controlMovimiento.enabled = !congelar;

            Rigidbody2D rb = lupi.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
    public void BotonReintentar()
    {
        StopAllCoroutines();
        UIManager.puntosGlobales = puntosAlIniciarNivel;
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        if (KaosController.instancia != null)
            KaosController.instancia.ResetearEstadoNivel("ListasCirculares");
        if (checkpointFinal != null)
            checkpointFinal.ResetearCheckpoint();
        if (barreraSiguiente != null)
            barreraSiguiente.Cerrar();
        if (controladorInsignia != null)
            controladorInsignia.ResetearInsignia();
        if (panelFinal != null) panelFinal.SetActive(false);
        CongelarLupi(false);
        ResetearNivel();
        ActualizarCabeceraNivel4();
        if (lupi != null && puntoInicioNivel != null)
            lupi.position = puntoInicioNivel.position;
        StartCoroutine(SecuenciaIntro());
    }
    public void BotonSiguiente()
    {
        if (panelFinal != null) panelFinal.SetActive(false);
        if (lupi != null)
        {
            var controlMovimiento = lupi.GetComponent<PlayerController>();
            if (controlMovimiento != null) controlMovimiento.enabled = true;
        }
        Debug.Log("Lupi descongelado, puede avanzar al siguiente nivel en la misma escena.");
    }
    void ActualizarPuntos() { if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    int CalcularPuntosDinamicos()
    {
        float tiempoTranscurrido = Time.time - tiempoInicioEstado;
        float t = Mathf.Clamp01(tiempoTranscurrido / tiempoLimite);
        int puntos = Mathf.RoundToInt(Mathf.Lerp(puntosMaximos, puntosMinimos, t));
        return puntos;
    }
    public void ResetearNivel()
    {
        aciertosContador = 0;
        fallosContador = 0;
        if (panelFinal) panelFinal.SetActive(false);
        modoActual = ModoOperacion.Insertar;
        indiceAEliminar = 4;
        fase = 0;
        subPaso = 0;
        cargandoAgua = false;
        listaNodos.Clear();
        nodoActual = null;
        puntosConfirmados.Clear();
        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            UIManager.instancia.ConfigurarTextosChecklist(
                "new Nodo(\"Codorniz\");",
                "new Nodo(\"Gallina\");",
                "new Nodo(\"Cerdo\");",
                "new Nodo(\"Oveja\");",
                "new Nodo(\"Vaca\");"
            );
        }
        if (lineaAgua != null) lineaAgua.positionCount = 0;
        if (lineaFija != null) lineaFija.positionCount = 0;
        if (puntoRio != null) puntosConfirmados.Add(puntoRio.position);
        GameObject[] plantas = GameObject.FindGameObjectsWithTag("Planta");
        foreach (var p in plantas) Destroy(p);
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();
        ApagarBrillos();
    }
    IEnumerator SecuenciaIntro()
    {
        yield return new WaitForSeconds(0.5f);
        andy.Decir("¡Lupi! Para que el bebedero de Tahuantindata nunca se agote, usaremos Listas Circulares. El agua fluirá entre los animales en un retorno perpetuo.", audioIntroCircular);
        yield return new WaitForSeconds(audioIntroCircular.length + 0.5f);
        andy.Decir("Abre tu mochila. Cada animal que llegue será un NODO Q dentro de este ciclo de vida.", audioPrepararNodo);
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
            andy.Decir("Lleva al animal a su lugar en el ciclo. Su presencia definirá el campo Q^.INFO", audioSembrar);
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
                andy.Decir("Al ser el primer animal, el puntero de acceso P debe inicializarse apuntando a sí mismo: P^.LIGA = P", audioPrimerNodoCircular);
            }
            else
            {
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Liga", true);
                andy.Decir("Para insertar a Q, actualizaremos la LIGA del animal anterior para que apunte a la dirección de memoria de este nuevo integrante.", audioInsertarIntermedio);
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
                    andy.Decir("¡Atención! Para que la lista sea circular, la LIGA del último animal debe apuntar siempre al inicio, donde se encuentra P.", audioCerrarCiclo);
                }
                else
                {
                    ReproducirError();
                    if (fase == 0) andy.Decir("¡Cuidado Lupi! Para iniciar el ciclo de bebederos, debemos asignar el puntero de acceso P desde la fuente de agua original.", audioErrorNoRio);
                    else if (subPaso == 0 || subPaso == 1) andy.Decir("¡Error de enlace! El algoritmo dicta que la conexión debe nacer de la LIGA del animal anterior para integrar al nuevo NODO Q.", audioErrorNoLigaAnterior);
                    else andy.Decir("¡No pierdas el ciclo! Para cerrar la estructura circular, activa la LIGA del último animal para que retorne al inicio donde está P.", audioErrorNoLigaAnteriorCerrar);
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
        int puntos = CalcularPuntosDinamicos();
        SumarPuntos(puntos);
        tiempoInicioEstado = Time.time;
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
        andy.Decir("¡Lupifantástico! Has creado un ciclo perfecto. El puntero P ahora nos permite recorrer todos los animales sin encontrar un final.", audioExitoCiclo);
        if (audioExitoCiclo != null)
            yield return new WaitForSeconds(audioExitoCiclo.length + 0.5f);
        else
            yield return new WaitForSeconds(3f); 
        andy.Decir("¡Impresionante! El Kaos ha infectado a los animales. Debemos realizar una ELIMINACIÓN, liberando la memoria del nodo sin romper el flujo.", audioIntroEliminar);
        if (audioIntroEliminar != null)
            yield return new WaitForSeconds(audioIntroEliminar.length + 0.5f);
        else
            yield return new WaitForSeconds(3f);
        modoActual = ModoOperacion.Eliminar;
        indiceAEliminar = 4;
        ActualizarCabeceraNivel4();
        fase = 0;
        UIManager.instancia.ConfigurarTextosChecklist(
                "",
                "delete(Vaca)",
                "",
                "delete(Codorniz)",
                ""
            );
        ProximoPasoEliminar();
    }
    void ProximoPasoEliminar()
    {
        tiempoInicioEstado = Time.time;
        if (indiceAEliminar == 4)
        {
            andy.Decir("La vaca se ha retirado. Usa el puntero auxiliar T para que la LIGA de la oveja apunte de regreso a las cordornices (P)", audioEliminarFinal);
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
        }
        else
        {
            andy.Decir("¡Emergencia! El primer animal (P) ha sido infectado. Debemos reasignar el acceso de la lista al siguiente animal antes de borrarlo.", audioEliminarInicio);
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
                andy.Decir("¡Error de eliminación! Para remover un animal sin romper el ciclo, debemos reasignar la LIGA del animal anterior (T) hacia el sucesor del nodo infectado.", audioErrorEliminacion);
            }
        }
        else
        {
            if (tipo == "EntradaHuerto")
            {
                int destino = (indiceAEliminar == 4) ? 0 : 1;
                if (managerTocado == listaNodos[destino])
                {
                    int puntos = CalcularPuntosDinamicos();
                    cargandoAgua = false;
                    EncenderBrilloHijo(listaNodos[destino].gameObject, "Info", false);
                    int objetivo = (indiceAEliminar == 4) ? 4 : 0;
                    listaNodos[objetivo].IniciarSecuenciaEliminacion();
                    SumarPuntos(puntos);
                    UIManager.instancia.MarcarTareaCompletada((fase * 2) + 1);
                    ActualizarLineaFijaPostEliminacion();
                    if (indiceAEliminar == 4)
                    {
                        indiceAEliminar = 0;
                        ActualizarCabeceraNivel4();
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
                        CongelarLupi(true);
                        ReproducirNivelCompleto();
                        andy.Decir("¡Victoria Supervisor de Flujo Circular! Has gestionado los punteros P, Q y T perfectamente. ¡La memoria de Tahuantindata está a salvo!", audioExitoTotal);                        
                        StartCoroutine(MostrarResumenFinal());
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
        aciertosContador++;
        UIManager.puntosGlobales += cant; 
        if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString();
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        if (fuenteAudio && sonidoAcierto) fuenteAudio.PlayOneShot(sonidoAcierto);
    }
    void ReproducirError()
    {
        fallosContador++;
        if (fuenteAudio && sonidoError) fuenteAudio.PlayOneShot(sonidoError);
        if (!KaosController.nivelesTerminados.Contains("ListasCirculares"))
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPasoSiembra(); }
}