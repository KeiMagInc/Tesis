using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    private bool esModoRepaso = false;
    private bool nivelCompletado = false;
    [Header("Pantalla Derrota")]
    public GameObject panelDerrota;
    public TextMeshProUGUI textoAciertosDerrota;
    public TextMeshProUGUI textoFallosDerrota;
    [Header("Audios Animales")]
    public AudioClip sonidoCodorniz;
    public AudioClip sonidoGallina;
    public AudioClip sonidoCerdo;
    public AudioClip sonidoOveja;
    public AudioClip sonidoVaca;
    private float tiempoUltimaAccion = 0f;
    [Header("Efectos Burbuja")]
    public GameObject prefabBurbuja;
    [Header("Información del Nivel UI")]
    public string nombreDelNivel = "Listas Circulares";
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
    public AudioClip sonidoFinDelJuego;
    [Header("Configuración de Tiempo")]
    public int puntosMaximos = 10;
    public int puntosMinimos = 0;
    public float tiempoLimite = 120f;
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
    public AudioClip audioErrorKaos1;
    public AudioClip audioErrorKaos2;
    public AudioClip audioErrorKaos3;
    [Header("Insignias")]
    public ControladorInsignia controladorInsignia;
    public Sprite insigniaDeEsteNivel;
    public Checkpoint checkpointFinal;
    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoSeleccionar;
    public AudioClip sonidoAlerta;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoInsignia;
    public AudioClip sonidoCompletado;
    public AudioClip sonidoCuy;
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
        UIManager.instancia.DesactivarTodoPostNivel();
        UIManager.instancia.logicaActiva = this;
        esModoRepaso = KaosController.nivelesTerminados.Contains("ListasCirculares");
        nivelCompletado = false;
        if (!esModoRepaso)
        {
            puntosAlIniciarNivel = UIManager.puntosGlobales;
            UIManager.puntosTemporales = 0;
            UIManager.instancia.SetPrefabs(prefabCodorniz, prefabGallina, prefabCerdo, prefabOveja, prefabVaca);
            UIManager.instancia.SetSounds(sonidoCodorniz, sonidoGallina, sonidoCerdo, sonidoOveja, sonidoVaca);
            Sprite[] imagenes = { spriteCodorniz, spriteGallina, spriteCerdo, spriteOveja, spriteVaca };
            string[] nombres = { "Codorniz", "Gallina", "Cerdo", "Oveja", "Vaca" };
            UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);
            KaosController kaos = Object.FindFirstObjectByType<KaosController>(FindObjectsInactive.Include);
            if (kaos != null)
            {
                kaos.gameObject.SetActive(true);
                kaos.ResetearEstadoNivel("ListasCirculares");
            }
        }
        else
        {
            Debug.Log("Modo Repaso Nivel 4: Puntaje protegido.");
            KaosController kaos = Object.FindFirstObjectByType<KaosController>(FindObjectsInactive.Include);
            if (kaos != null) kaos.gameObject.SetActive(false);
        }
        ResetearNivel();
        ActualizarCabeceraNivel4();
        UIManager.instancia.SetMochilaHabilitada(true);
        ActualizarPuntos();
        StartCoroutine(SecuenciaIntro());
    }
    void OnDisable()
    {
        if (UIManager.instancia != null && UIManager.instancia.logicaActiva == (ILogicaNivel)this)
            UIManager.instancia.logicaActiva = null;
        if (!esModoRepaso && !nivelCompletado)
        {
            UIManager.puntosGlobales = puntosAlIniciarNivel;
            UIManager.puntosTemporales = 0;
        }
        ResetearNivel();
    }
    public void DesconectarEnlacePorKaos()
    {
        if (esModoRepaso) return;
        bool debePenalizar = false;
        if (modoActual == ModoOperacion.Insertar && subPaso > 0) debePenalizar = true;
        if (modoActual == ModoOperacion.Eliminar && cargandoAgua) debePenalizar = true;
        if (!debePenalizar) return;
        if (fase >= nombresNodos.Length && modoActual == ModoOperacion.Insertar) return;
        fallosContador++;
        int puntosARestar = 5;
        if (UIManager.puntosTemporales >= puntosARestar)
        {
            UIManager.puntosTemporales -= puntosARestar;
        }
        else
        {
            puntosARestar -= UIManager.puntosTemporales;
            UIManager.puntosTemporales = 0;
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - puntosARestar);
        }
        ActualizarPuntos();
        if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
        rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError) masterSFX.PlayOneShot(sonidoError);
        if (modoActual == ModoOperacion.Insertar)
        {
            cargandoAgua = false;
            lineaAgua.positionCount = 0;
            ApagarBrillos();
            if (subPaso == 2)
            {
                subPaso = 1;
                if (nodoActual != null)
                {
                    nodoActual.ResetearNodo();
                    EncenderBrilloHijo(nodoActual.gameObject, "Info", false);
                    EncenderBrilloHijo(nodoActual.gameObject, "Liga", false);
                }
                if (puntosConfirmados.Count > (fase == 0 ? 1 : fase * 2))
                {
                    puntosConfirmados.RemoveAt(puntosConfirmados.Count - 1);
                    DibujarLineaFija();
                }
                if (andy != null)
                    andy.Decir("¡Cuidado Lupi! Kaos desconectó tu manguera. Reconecta desde el origen.", audioErrorKaos1);
            }
            else if (subPaso == 1)
            {
                if (andy != null)
                    andy.Decir("¡Oh no! Kaos ha interrumpido la conexión del bebedero. Vuelve al origen.", audioErrorKaos1);
            }
            if (fase == 0)
            {
                brilloRio.SetEncendido(true);
                if (andy != null && brilloRio != null) andy.CambiarObjetivo(brilloRio.transform);
            }
            else
            {
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Liga", true);
            }
        }
        else if (modoActual == ModoOperacion.Eliminar)
        {
            cargandoAgua = false;
            ApagarBrillos();
            ProximoPasoEliminar();
            if (andy != null) andy.Decir("¡Emergencia! Kaos saboteó la eliminación. Intenta reasignar el ciclo otra vez.", audioErrorKaos3);
        }
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
        Vector3 escalaMax = escalaOriginalPuntos * 1.5f;
        float tiempo = 0f;
        float duracionPop = 0.08f;
        while (tiempo < duracionPop)
        {
            textoPuntos.transform.localScale = Vector3.Lerp(escalaOriginalPuntos, escalaMax, tiempo / duracionPop);
            tiempo += Time.deltaTime;
            yield return null;
        }
        textoPuntos.transform.localScale = escalaMax;
        tiempo = 0f;
        float duracionRetorno = 0.18f;
        Color colorInicialEfecto = textoPuntos.color;
        while (tiempo < duracionRetorno)
        {
            float t = tiempo / duracionRetorno;
            textoPuntos.transform.localScale = Vector3.Lerp(escalaMax, escalaOriginalPuntos, t);
            textoPuntos.color = Color.Lerp(colorInicialEfecto, colorOriginalPuntos, t);
            tiempo += Time.deltaTime;
            yield return null;
        }
        textoPuntos.transform.localScale = escalaOriginalPuntos;
        textoPuntos.color = colorOriginalPuntos;
    }
    IEnumerator MostrarResumenFinal()
    {
        if (audioExitoTotal != null)
        {
            yield return new WaitForSeconds(audioExitoTotal.length + 0.8f);
        }
        else
        {
            yield return new WaitForSeconds(3.5f);
        }
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (fuenteAudio != null && sonidoFinDelJuego != null)
                fuenteAudio.PlayOneShot(sonidoFinDelJuego);
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
        UIManager.DescartarPuntos();
        StopAllCoroutines();
        if (!esModoRepaso)
            UIManager.puntosGlobales = puntosAlIniciarNivel;
        ActualizarPuntos();
        if (KaosController.instancia != null && !esModoRepaso)
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
        UIManager.instancia.SetMochilaHabilitada(true);
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
    void ActualizarPuntos()
    {
        if (textoPuntos)
            textoPuntos.text = (UIManager.puntosGlobales + UIManager.puntosTemporales).ToString();
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
        if (UIManager.instancia != null && UIManager.instancia.logicaActiva == (ILogicaNivel)this)
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
        UIManager.instancia.MostrarMochilaSolo(true);
        andy.Decir("Abre tu mochila. Cada animal que llegue será un NODO Q dentro de este ciclo de vida.", audioPrepararNodo);
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
            if (UIManager.instancia != null)
                UIManager.instancia.MarcarTareaEnProgreso(fase);
        }
    }
    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        AsignarNodoInmediato();
    }
    void AsignarNodoInmediato()
    {
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
                if (andy != null && brilloRio != null) andy.CambiarObjetivo(brilloRio.transform);
                andy.Decir("Al ser el primer animal, el puntero de acceso P debe inicializarse apuntando a sí mismo: P^.LIGA = P", audioPrimerNodoCircular);
            }
            else
            {
                EncenderBrilloHijo(listaNodos[fase - 1].gameObject, "Liga", true);
                andy.Decir("Para insertar a Q, actualizaremos la LIGA del animal anterior para que apunte a la dirección de memoria de este nuevo integrante.", audioInsertarIntermedio);
            }
        }
    }
    IEnumerator EsperarYAsignarNodo()
    {
        yield return new WaitForSeconds(0.1f);
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
                if (andy != null && brilloRio != null) andy.CambiarObjetivo(brilloRio.transform);
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
        if (Time.time - tiempoUltimaAccion < 0.05f) return;
        tiempoUltimaAccion = Time.time;
        if (fuenteAudio != null && sonidoSeleccionar != null)
            fuenteAudio.PlayOneShot(sonidoSeleccionar);
        if (modoActual == ModoOperacion.Insertar)
        {
            if (nodoActual == null) AsignarNodoInmediato();
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
                if (tipo == "EntradaHuerto" || tipo == "Info")
                {
                    if (subPaso == 1 && managerTocado == nodoActual)
                    {
                        int puntos = CalcularPuntosDinamicos();
                        cargandoAgua = false;
                        nodoActual.ActivarHuerto();
                        SumarPuntos(puntos);
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
        if (fuenteAudio != null && sonidoAlerta != null)
            fuenteAudio.PlayOneShot(sonidoAlerta);
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
        UIManager.instancia.SetMochilaHabilitada(false);
        if (listaNodos != null && listaNodos.Count > 4 && listaNodos[4] != null)
        {
            Debug.Log($"[LogicaNivel4] Infectando último nodo (Vaca) con éxito: {listaNodos[4].gameObject.name}");
            listaNodos[4].InfectarNodo();
        }
        else
        {
            Debug.LogError($"[LogicaNivel4] ¡Error! No se pudo infectar el último nodo (Vaca). ¿La lista está incompleta? Tamaño de listaNodos: {(listaNodos != null ? listaNodos.Count.ToString() : "NULA")}");
        }
        ProximoPasoEliminar();
    }
    void ProximoPasoEliminar()
    {
        tiempoInicioEstado = Time.time;
        if (fuenteAudio != null && sonidoAlerta != null)
            fuenteAudio.PlayOneShot(sonidoAlerta);
        if (indiceAEliminar == 4)
        {
            andy.Decir("La vaca se ha retirado. Usa el puntero auxiliar T para que la LIGA de la oveja apunte de regreso a las codornices (P)", audioEliminarFinal);
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
            if (UIManager.instancia != null)
                UIManager.instancia.MarcarTareaEnProgreso(1);
        }
        else
        {
            andy.Decir("¡Emergencia! El primer animal (P) ha sido infectado. Debemos reasignar el acceso de la lista al siguiente animal antes de borrarlo.", audioEliminarInicio);
            EncenderBrilloHijo(listaNodos[3].gameObject, "Liga", true);
            if (UIManager.instancia != null)
                UIManager.instancia.MarcarTareaEnProgreso(3);
            if (listaNodos != null && listaNodos.Count > 0 && listaNodos[0] != null)
            {
                Debug.Log($"[LogicaNivel4] Infectando primer nodo (Codorniz) con éxito: {listaNodos[0].gameObject.name}");
                listaNodos[0].InfectarNodo();
            }
            else
            {
                Debug.LogError($"[LogicaNivel4] ¡Error! No se pudo infectar el primer nodo (Codorniz). Tamaño de listaNodos: {(listaNodos != null ? listaNodos.Count.ToString() : "NULA")}");
            }
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
                        nivelCompletado = true;
                        if (andy != null && lupi != null) andy.CambiarObjetivo(lupi);
                        UIManager.instancia.DesactivarTodoPostNivel();
                        if (!esModoRepaso)
                            UIManager.ConfirmarPuntos();
                        ActualizarPuntos();
                        CongelarLupi(true);
                        StartCoroutine(SecuenciaFinNivel());
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
    IEnumerator SecuenciaFinNivel()
    {
        if (UIManager.instancia != null && UIManager.instancia.fuenteVozAndy != null)
        {
            while (UIManager.instancia.fuenteVozAndy.isPlaying)
                yield return null; 
        }
        if (barreraSiguiente != null && checkpointFinal != null && controladorInsignia != null)
        {
            barreraSiguiente.Abrir();
            checkpointFinal.AparecerYActivar();
            controladorInsignia.MostrarInsignia(insigniaDeEsteNivel); 
            if (fuenteAudio != null && sonidoInsignia != null)
                fuenteAudio.PlayOneShot(sonidoInsignia); 
            if (!esModoRepaso && KaosController.instancia != null)
                KaosController.instancia.RecibirDanoYDesaparecer("ListasCirculares");
        }
        float tiempoEsperaMedalla = (sonidoInsignia != null) ? sonidoInsignia.length : 2.0f;
        yield return new WaitForSeconds(tiempoEsperaMedalla);
        if (andy != null)
            andy.Decir("¡Victoria Supervisor de Flujo Circular! Has gestionado los punteros P, Q y T perfectamente. ¡La memoria de Tahuantindata está a salvo!", audioExitoTotal);
        StartCoroutine(MostrarResumenFinal());
    }
    void ReproducirNivelCompleto()
    {
        if (fuenteAudio != null && sonidoCompletado != null)
            fuenteAudio.PlayOneShot(sonidoCompletado);
    }
    void ActualizarLineaFijaPostEliminacion()
    {
        List<Vector3> pts = new List<Vector3>();
        pts.Add(puntoRio.position);
        if (indiceAEliminar == 4)
        {
            for (int i = 0; i <= 3; i++)
            {
                pts.Add(listaNodos[i].puntoEntrada.position);
                pts.Add(listaNodos[i].puntoSalida.position);
            }
            pts.Add(listaNodos[0].puntoEntrada.position);
        }
        else if (indiceAEliminar == 0)
        {
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
        if (panelFinal != null && panelFinal.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                BotonSiguiente();
        }
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
        {
            if (b.name.ToUpper().Contains(parte.ToUpper()))
            {
                b.SetEncendido(activar);
                if (activar && andy != null)
                    andy.CambiarObjetivo(b.transform);
            }
        }
    }
    void ApagarBrillos()
    {
        if (brilloRio) brilloRio.SetEncendido(false);
        foreach (var b in Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None)) b.SetEncendido(false);
        if (andy != null && lupi != null) andy.CambiarObjetivo(lupi);
    }
    public void MostrarDerrota()
    {
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
            if (textoAciertos) textoAciertos.text = aciertosContador.ToString();
            if (textoFallos) textoFallos.text = fallosContador.ToString();
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
    }
    void ReproducirError()
    {
        fallosContador++;
        if (UIManager.instancia != null)
            UIManager.instancia.RevisarDerrotaPorPorcentaje(aciertosContador, fallosContador);
        if (Time.timeScale == 0f)
        {
            CongelarLupi(true);
            return;
        }
        AudioSource masterSFX = UIManager.instancia.fuenteVozAndy;
        if (masterSFX && sonidoError)
            masterSFX.PlayOneShot(sonidoError);
        if (!esModoRepaso)
        {
            UIManager.puntosGlobales = Mathf.Max(0, UIManager.puntosGlobales - 5);
            ActualizarPuntos();
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(false));
            if (KaosController.instancia != null)
                KaosController.instancia.ReaccionarAError();
        }
    }
    void SumarPuntos(int cant, bool silencioso = false)
    {
        aciertosContador++;
        if (!esModoRepaso)
            UIManager.puntosTemporales += cant;
        ActualizarPuntos();
        if (textoPuntos != null)
        {
            if (rutinaEfectoPuntos != null) StopCoroutine(rutinaEfectoPuntos);
            rutinaEfectoPuntos = StartCoroutine(AnimacionPuntos(true));
        }
        if (prefabBurbuja != null)
        {
            Vector3 posicionAparicion = Vector3.zero;
            bool objetivoEncontrado = false;
            if (nodoActual != null)
            {
                posicionAparicion = nodoActual.transform.position;
                objetivoEncontrado = true;
            }
            else if (modoActual == ModoOperacion.Eliminar && listaNodos.Count > 0)
            {
                int index = (indiceAEliminar == 4) ? 4 : 0;
                if (index < listaNodos.Count && listaNodos[index] != null)
                {
                    posicionAparicion = listaNodos[index].transform.position;
                    objetivoEncontrado = true;
                }
            }
            if (objetivoEncontrado)
            {
                posicionAparicion.z = -1f;
                GameObject nuevaBurbuja = Instantiate(prefabBurbuja, posicionAparicion, Quaternion.identity);
                if (nuevaBurbuja.TryGetComponent<EfectoBurbuja>(out var efecto))
                {
                    efecto.Configurar(esModoRepaso ? 0 : cant);
                    Debug.Log($"[Nivel 4] Burbuja +{(esModoRepaso ? 0 : cant)} creada en {posicionAparicion}");
                }
            }
        }
        if (!silencioso && UIManager.instancia.fuenteVozAndy && sonidoAcierto)
        {
            if (sonidoAcierto) UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoAcierto);
            if (sonidoCuy) UIManager.instancia.fuenteVozAndy.PlayOneShot(sonidoCuy);
        }
    }
    IEnumerator EsperarSiguiente() { 
        yield return new WaitForSeconds(0.3f); 
        ProximoPasoSiembra(); 
    }
}