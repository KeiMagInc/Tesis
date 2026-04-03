using UnityEngine;
using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LogicaNivel2 : MonoBehaviour
{
    public AndyController andy;
    public Transform lupi;
    public UIManager uiManager;

    [Header("Interfaz")]
    public TextMeshProUGUI textoPuntos;
    private int puntosTotales = 0;

    [Header("Agua")]
    public LineRenderer lineaAgua;
    public Transform puntoHead;
    public Transform puntoNull;

    [Header("Brillos Estáticos")]
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private List<Transform> nodosConectados = new List<Transform>();
    private bool cargandoEnlace = false;
    private EfectoLetrero brilloActivo;

    // Static para que si sales del nivel y vuelves, el juego recuerde que ya viste la intro
    private static bool introVista = false;

    void OnEnable()
    {
        // Configuramos la línea
        lineaAgua.positionCount = 0;
        lineaAgua.sortingOrder = 25;
        ActualizarTextoPuntos();

        if (!introVista)
        {
            StartCoroutine(SecuenciaNarrativa());
        }
        else
        {
            // Si ya vio la intro y regresa, mostramos la UI de golpe (sin esperas)
            uiManager.MostrarInterfazNivel2Snappy();

            // Reactivamos el brillo donde se quedó (por defecto Head si no empezó)
            if (nodosConectados.Count == 0 && brilloHead != null)
            {
                brilloHead.SetEncendido(true);
                brilloActivo = brilloHead;
            }
        }
    }

    void OnDisable()
    {
        // Al salir del nivel (desactivado por RoomCam), ocultamos todo el Nivel 2
        if (uiManager != null)
        {
            uiManager.OcultarInterfazNivel2Snappy();
        }
        ApagarBrilloActual();
    }

    IEnumerator SecuenciaNarrativa()
    {
        // 1. Espera inicial para el cartel de "Nivel 2"
        yield return new WaitForSeconds(1.5f);

        // 2. Andy introduce el objetivo
        andy.Decir("¡Lupi! El Valle necesita recuperar sus huertos.");
        yield return new WaitForSeconds(2.5f);

        andy.Decir("Mira a tu izquierda y revisa tu mochila, ahí están las semillas.");

        // 3. Aparece TODA la interfaz (Checklist y Mochila) al mismo tiempo con Fade
        // Usamos la función de UIManager que hace el fundido de ambos grupos
        yield return StartCoroutine(uiManager.FadeNivel2UI(1f, 1.5f));

        yield return new WaitForSeconds(1f);
        andy.Decir("Siembra el Trigo en el primer pantano para empezar la lista.");

        // 4. Activamos el primer brillo para guiar al jugador
        if (brilloHead != null)
        {
            brilloHead.SetEncendido(true);
            brilloActivo = brilloHead;
        }

        introVista = true;
    }

    void Update()
    {
        DibujarManguera();
    }

    void DibujarManguera()
    {
        if (nodosConectados.Count == 0 && !cargandoEnlace) return;

        int totalPuntos = nodosConectados.Count + (cargandoEnlace ? 1 : 0);
        lineaAgua.positionCount = totalPuntos;

        for (int i = 0; i < nodosConectados.Count; i++)
        {
            lineaAgua.SetPosition(i, nodosConectados[i].position);
        }

        if (cargandoEnlace)
        {
            lineaAgua.SetPosition(totalPuntos - 1, lupi.position);
        }
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        // 1. RECOGER DEL INICIO (HEAD)
        if (tipo == "Head" && !cargandoEnlace && nodosConectados.Count == 0)
        {
            // Verificamos si ya sembró algo en el UIManager
            if (uiManager.indiceProgreso == 0)
            {
                andy.Decir("¡Lupi! Primero debes sembrar al menos un huerto.");
                return;
            }

            cargandoEnlace = true;
            nodosConectados.Add(puntoHead);
            GanarPuntos(10);
            andy.Decir("¡Enlace HEAD recogido! Llévalo al DATO del primer huerto.");

            EncenderBrilloEnObjeto(objetoTocado, false);
            PasarBrilloAHuertoProximo("Dato");
        }

        // 2. CONECTAR A ENTRADA (DATO)
        else if (tipo == "EntradaHuerto" && cargandoEnlace)
        {
            cargandoEnlace = false;
            nodosConectados.Add(objetoTocado.transform.Find("PuntoEntrada"));
            objetoTocado.GetComponentInParent<NodoManager>().ActivarHuerto();
            GanarPuntos(10);
            andy.Decir("Dato guardado. Ahora recoge el PUNTERO de este huerto.");

            EncenderBrilloEnObjeto(objetoTocado, false);
            EncenderBrilloEnObjeto(objetoTocado.transform.parent.gameObject, true, "Puntero");
        }

        // 3. RECOGER DE SALIDA (PUNTERO)
        else if (tipo == "SalidaHuerto" && !cargandoEnlace)
        {
            cargandoEnlace = true;
            nodosConectados.Add(objetoTocado.transform.Find("PuntoSalida"));
            GanarPuntos(10);
            andy.Decir("Puntero activo. Conéctalo al siguiente DATO o al NULL.");

            EncenderBrilloEnObjeto(objetoTocado, false);

            if (nodosConectados.Count >= 6)
                EncenderBrilloNull(true);
            else
                PasarBrilloAHuertoProximo("Dato");
        }

        // 4. CERRAR EN NULL
        else if (tipo == "Null" && cargandoEnlace)
        {
            if (uiManager.indiceProgreso < 3)
            {
                andy.Decir("¡Espera! La lista debe tener los 3 datos antes del NULL.");
                return;
            }
            cargandoEnlace = false;
            nodosConectados.Add(puntoNull);
            GanarPuntos(20);
            EncenderBrilloNull(false);
            VictoriaFinal();
        }
    }

    // --- SISTEMA DE BRILLOS ---

    void ApagarBrilloActual()
    {
        if (brilloActivo != null) brilloActivo.SetEncendido(false);
    }

    void EncenderBrilloEnObjeto(GameObject padre, bool estado, string nombreHijo = "")
    {
        ApagarBrilloActual();
        if (padre == null) return;

        EfectoLetrero[] todos = padre.GetComponentsInChildren<EfectoLetrero>();
        foreach (var b in todos)
        {
            if (nombreHijo == "" || b.gameObject.name.Contains(nombreHijo))
            {
                b.SetEncendido(estado);
                if (estado) brilloActivo = b;
                break;
            }
        }
    }

    void EncenderBrilloNull(bool estado)
    {
        ApagarBrilloActual();
        if (brilloNull != null)
        {
            brilloNull.SetEncendido(estado);
            if (estado) brilloActivo = brilloNull;
        }
    }

    void PasarBrilloAHuertoProximo(string tipoLetrero)
    {
        NodoManager[] huertos = Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None);
        foreach (var h in huertos)
        {
            bool yaTieneAgua = false;
            Transform puntoEntrada = h.transform.Find("PuntoEntrada");
            foreach (var t in nodosConectados)
            {
                if (t == puntoEntrada) { yaTieneAgua = true; break; }
            }

            if (!yaTieneAgua)
            {
                EncenderBrilloEnObjeto(h.gameObject, true, tipoLetrero);
                return;
            }
        }
    }
    public void ResetearConexionesAgua()
    {
        nodosConectados.Clear();
        cargandoEnlace = false;
        lineaAgua.positionCount = 0;
        ApagarBrilloActual();
        if (brilloHead != null) brilloHead.SetEncendido(true);
    }

    void GanarPuntos(int cantidad) { puntosTotales += cantidad; ActualizarTextoPuntos(); }
    void ActualizarTextoPuntos() { if (textoPuntos != null) textoPuntos.text = puntosTotales.ToString(); }
    void VictoriaFinal() => andy.Decir("¡Increíble! Has creado una lista enlazada conectada a NULL.");
}