using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel4 : MonoBehaviour, ILogicaNivel
{
    [Header("Configuración de Semillas")]
    public Sprite[] misSprites;
    public GameObject[] misPrefabs;

    // Nombres exactos como están en las Zonas de Plantado (evita tildes si el inspector no las tiene)
    private string[] nombresNodos = { "Calabaza", "Papa", "Trigo", "Zanahoria", "Rabano" };

    private string[] textosChecklist = {
        "1. Insertar Calabaza (Inicio del ciclo)",
        "2. Insertar Papa",
        "3. Insertar Trigo",
        "4. Insertar Zanahoria",
        "5. Insertar Rabano"
    };

    [Header("Referencias de Escena")]
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;
    public Transform puntoInicioRio; // El punto de acceso inicial

    private Transform lcActual;
    private NodoManager nuevoNodo;
    private List<NodoManager> nodosEnLista = new List<NodoManager>();

    private int fase = 0;
    private int subPasoAlgoritmo = 0;
    private bool cargandoAgua = false;
    private Vector3 temporalPaso1Destino = Vector3.zero;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;
        ResetearNivel();
        StartCoroutine(Intro());
    }

    public void ResetearNivel()
    {
        fase = 0; subPasoAlgoritmo = 0;
        lcActual = puntoInicioRio;
        nuevoNodo = null;
        cargandoAgua = false;
        nodosEnLista.Clear();
        temporalPaso1Destino = Vector3.zero;

        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;

        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        UIManager.instancia.ResetBotones();
        UIManager.instancia.SetSemillaPalpitar("");

        UIManager.instancia.ConfigurarMochila(misSprites, nombresNodos, misPrefabs);
        UIManager.instancia.ConfigurarTextosChecklist(textosChecklist);

        ApagarBrillosEscena();
        ActualizarVisualCirculo();
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(1f);
        andy.Decir("¡Lupi! El río es el origen. Vamos a insertar cultivos usando el algoritmo de Joyanes.");
        yield return new WaitForSeconds(3.5f);
        UIManager.instancia.MostrarMochilaSolo(true);
        yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        ApagarBrillosEscena();
        if (fase < nombresNodos.Length)
        {
            UIManager.instancia.SetSemillaPalpitar(nombresNodos[fase]);
            andy.Decir("Instrucción: " + textosChecklist[fase]);
        }
        else andy.Decir("¡Ciclo circular completado! El agua regresa al río eternamente.");
        subPasoAlgoritmo = 0;
        temporalPaso1Destino = Vector3.zero;
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado)
    {
        if (nuevoNodo == null) return;
        NodoManager managerTocado = objetoTocado.GetComponentInParent<NodoManager>();

        // PASO 1: nuevo.sig = acceso.sig
        if (subPasoAlgoritmo == 0)
        {
            if (!cargandoAgua && (tipo == "LC" || tipo == "Head" || (tipo == "SalidaHuerto" && managerTocado != nuevoNodo)))
            {
                cargandoAgua = true;
                ApagarBrillosEscena();
                EncenderBrilloHijo(nuevoNodo.gameObject, "Puntero", true);
                andy.Decir("Lleva el agua al PUNTERO de la planta.");
            }
            else if (cargandoAgua && tipo == "SalidaHuerto" && managerTocado == nuevoNodo)
            {
                cargandoAgua = false;
                subPasoAlgoritmo = 1;
                // El nuevo nodo ahora apunta a donde el acceso apuntaba antes
                temporalPaso1Destino = (fase == 0) ? puntoInicioRio.position : nodosEnLista[0].puntoEntrada.position;

                ApagarBrillosEscena();
                ActivarBrilloAccesoActual(true);
                ActualizarVisualCirculo();
                andy.Decir("¡Bien! Paso 2: Primero.sig = nuevo. Recoge agua del acceso.");
            }
        }
        // PASO 2: acceso.sig = nuevo
        else if (subPasoAlgoritmo == 1)
        {
            if (!cargandoAgua && (tipo == "LC" || tipo == "Head" || (tipo == "SalidaHuerto" && managerTocado != nuevoNodo)))
            {
                cargandoAgua = true;
                ApagarBrillosEscena();
                EncenderBrilloHijo(nuevoNodo.gameObject, "Dato", true);
                andy.Decir("Conecta el flujo a la ENTRADA (Dato) de la planta.");
            }
            else if (cargandoAgua && tipo == "EntradaHuerto" && managerTocado == nuevoNodo)
            {
                FinalizarInsercion();
            }
        }
    }

    void FinalizarInsercion()
    {
        cargandoAgua = false;
        nuevoNodo.ActivarHuerto();
        SumarPuntos(20);
        nodosEnLista.Add(nuevoNodo);
        lcActual = nuevoNodo.puntoSalida;

        UIManager.instancia.MarcarTareaCompletada(fase);
        ActualizarVisualCirculo();
        fase++;
        nuevoNodo = null;
        if (fase < nombresNodos.Length) StartCoroutine(EsperarSiguiente());
        else ApagarBrillosEscena();
    }

    void ActualizarVisualCirculo()
    {
        List<Vector3> pts = new List<Vector3>();

        // 1. Siempre inicia en el Río
        pts.Add(puntoInicioRio.position);

        if (nodosEnLista.Count > 0)
        {
            // El río conecta al primer nodo
            pts.Add(nodosEnLista[0].puntoEntrada.position);

            for (int i = 0; i < nodosEnLista.Count; i++)
            {
                // De la entrada a la salida de la misma planta (pero sin línea visual si quieres el corte)
                // Para que se vea el corte, añadimos los puntos pero el LineRenderer los unirá.
                // Si quieres cortes físicos reales, necesitarías múltiples LineRenderers.
                // Aquí simulamos el flujo lógico:
                pts.Add(nodosEnLista[i].puntoSalida.position);

                if (i + 1 < nodosEnLista.Count)
                    pts.Add(nodosEnLista[i + 1].puntoEntrada.position);
                else
                    pts.Add(puntoInicioRio.position); // Cierre al Río
            }
        }

        // Línea temporal del Paso 1
        if (nuevoNodo != null && temporalPaso1Destino != Vector3.zero)
        {
            pts.Add(nuevoNodo.puntoSalida.position);
            pts.Add(temporalPaso1Destino);
        }

        lineaFija.positionCount = pts.Count;
        lineaFija.SetPositions(pts.ToArray());
    }

    void Update()
    {
        if (lineaAgua == null || lupi == null) return;
        if (cargandoAgua && lcActual != null)
        {
            lineaAgua.positionCount = 2;
            lineaAgua.SetPositions(new Vector3[] { lcActual.position, lupi.position });
        }
        else lineaAgua.positionCount = 0;
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
            if (nm.name.Contains("(Clone)") && !nodosEnLista.Contains(nm))
            {
                nuevoNodo = nm; break;
            }
        }
        if (nuevoNodo != null)
        {
            andy.Decir("¡Huerto listo! Toma agua del PUNTO DE ACCESO.");
            ActivarBrilloAccesoActual(true);
        }
    }

    void ActivarBrilloAccesoActual(bool activar)
    {
        if (fase == 0)
        {
            EfectoLetrero b = puntoInicioRio.GetComponentInChildren<EfectoLetrero>();
            if (b == null && puntoInicioRio.parent != null) b = puntoInicioRio.parent.GetComponentInChildren<EfectoLetrero>();
            if (b != null) b.SetEncendido(activar);
        }
        else EncenderBrilloHijo(nodosEnLista[fase - 1].gameObject, "Puntero", activar);
    }

    void EncenderBrilloHijo(GameObject n, string parte, bool activar)
    {
        if (n == null) return;
        foreach (var b in n.GetComponentsInChildren<EfectoLetrero>(true))
            if (b.name.ToUpper().Contains(parte.ToUpper())) b.SetEncendido(activar);
    }

    void ApagarBrillosEscena()
    {
        if (puntoInicioRio != null)
        {
            EfectoLetrero b = puntoInicioRio.GetComponentInChildren<EfectoLetrero>();
            if (b == null && puntoInicioRio.parent != null) b = puntoInicioRio.parent.GetComponentInChildren<EfectoLetrero>();
            if (b != null) b.SetEncendido(false);
        }
        foreach (var b in Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None)) b.SetEncendido(false);
    }

    void SumarPuntos(int cant) { UIManager.puntosGlobales += cant; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}