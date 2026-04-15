using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicaNivel3 : MonoBehaviour, ILogicaNivel
{
    [Header("Sprites UI Originales")]
    public Sprite spriteTrigo;
    public Sprite spritePapa;
    public Sprite spriteCalabaza;

    public static LogicaNivel3 instancia;
    public AndyController andy;
    public TextMeshProUGUI textoPuntos;
    public LineRenderer lineaAgua;
    public LineRenderer lineaFija;
    public Transform lupi;

    private enum ModoOperacion { InsertarInicio, InsertarFinal, EliminarInicio, EliminarFinal }
    private ModoOperacion modoActual = ModoOperacion.InsertarInicio;

    [Header("Prefabs Específicos Nivel 3")]
    public GameObject prefabPapaN3;
    public GameObject prefabTrigoN3;
    public GameObject prefabCalabazaN3;

    [Header("Conexiones y Brillos Fijos")]
    public Transform puntoSalidaHead;
    public Transform puntoEntradaNull;
    public EfectoLetrero brilloHead;
    public EfectoLetrero brilloNull;

    private int fase = 0;
    private int pasoConexion = 0;
    private bool cargandoAgua = false;

    private NodoManager managerActual;
    private NodoManager managerAnterior;
    private List<NodoManager> listaNodos = new List<NodoManager>();

    private List<Vector3> puntosCadenaFija = new List<Vector3>();

    // ORDEN PARA INSERTAR INICIO: Derecha -> Centro -> Izquierda
    private string[] nombresNodosInicio = { "Papa", "Trigo", "Calabaza" };

    // ORDEN PARA INSERTAR FINAL: Izquierda -> Centro -> Derecha
    private string[] nombresNodosFinal = { "Calabaza", "Trigo", "Papa" };

    void Awake() => instancia = this;

    void OnEnable()
    {
        if (UIManager.instancia == null) return;
        UIManager.instancia.logicaActiva = this;

        UIManager.instancia.SetPrefabs(prefabPapaN3, prefabTrigoN3, prefabCalabazaN3);

        // Agrupamos en arreglos
        Sprite[] imagenes = { spritePapa, spriteTrigo, spriteCalabaza };
        string[] nombres = { "Papa", "Trigo", "Calabaza" };

        UIManager.instancia.ConfigurarBotonesUI(imagenes, nombres);

        UIManager.instancia.MostrarMochilaSolo(false);
        UIManager.instancia.MostrarChecklistSolo(false);
        ResetearNivel();
        StartCoroutine(Intro());
    }

    public void ResetearNivel()
    {
        modoActual = ModoOperacion.InsertarInicio;
        fase = 0;
        pasoConexion = 0;
        cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        listaNodos.Clear();
        managerAnterior = null;
        managerActual = null;

        if (UIManager.instancia != null)
        {
            UIManager.instancia.ResetBotones();
            ActualizarTextosChecklistSegunAlgoritmo();
        }

        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }

    void ActualizarTextosChecklistSegunAlgoritmo()
    {
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            UIManager.instancia.ConfigurarTextosChecklist(
                "Derecha: sembrar papa",   // Slot 0
                "",                        // Slot 1
                "Centro: sembrar trigo",   // Slot 2
                "",                        // Slot 3
                "Izquierda: sembrar calabaza" // Slot 4
            );
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            UIManager.instancia.ConfigurarTextosChecklist(
                "Izquierda: sembrar calabaza", // Slot 0 (Cambiado según tu lógica de nivel)
                "",                            // Slot 1
                "Centro: sembrar trigo",       // Slot 2
                "",                            // Slot 3
                "Derecha: sembrar papa"        // Slot 4
            );
        }
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(0.5f);

        if (modoActual == ModoOperacion.InsertarInicio)
            andy.Decir("¡Algoritmo 5.1! Vamos a insertar al INICIO.");
        else if (modoActual == ModoOperacion.InsertarFinal)
            andy.Decir("¡Algoritmo 5.2! Ahora insertaremos al FINAL.");
        else if (modoActual == ModoOperacion.EliminarInicio)
            andy.Decir("¡Algoritmo 5.9! Vamos a eliminar el PRIMER nodo.");

        yield return new WaitForSeconds(2.5f);

        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            andy.Decir("Primero, abre tu mochila para elegir la semilla que vamos a plantar.");
            UIManager.instancia.MostrarMochilaSolo(true);
            yield return new WaitUntil(() => UIManager.instancia.panelParcelas.activeSelf);
        }

        UIManager.instancia.MostrarChecklistSolo(true);
        ProximoPaso();
    }

    void ProximoPaso()
    {
        if (modoActual == ModoOperacion.InsertarInicio || modoActual == ModoOperacion.InsertarFinal)
        {
            string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
            if (fase < nombres.Length)
            {
                andy.Decir("Busca la semilla de " + nombres[fase] + " y siémbrala. \nLuego recoge el agua del letrero que indica.");
                UIManager.instancia.SetSemillaPalpitar(nombres[fase]);
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("Toca el INICIO para soltar la conexión del primer NODO.");
            brilloHead.SetEncendido(true);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            andy.Decir("Toca el PUNTERO del Trigo. Vamos a desconectar el último NODO de la lista.");
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Puntero", true);
        }
        pasoConexion = 0;
    }

    IEnumerator CambiarDeModo()
    {
        if (modoActual == ModoOperacion.InsertarInicio)
        {
            andy.Decir("¡Excelente trabajo! Has dominado la inserción por el frente de la lista.");
            yield return new WaitForSeconds(3f);
            modoActual = ModoOperacion.InsertarFinal;
            LimpiarEscenaParaSiguienteAlgoritmo();

            // CORRECCIÓN: Actualizamos los textos para el nuevo modo
            ActualizarTextosChecklistSegunAlgoritmo();

            StartCoroutine(Intro());
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            andy.Decir("¡Increíble! Ya sabes cómo construir una lista añadiendo elementos al final.");
            yield return new WaitForSeconds(3f);
            modoActual = ModoOperacion.EliminarInicio;
            fase = 0;
            UIManager.instancia.ResetBotones();

            // IMPORTANTE: Enviamos los 5 campos para que "sembrar trigo" y los demás desaparezcan
            UIManager.instancia.ConfigurarTextosChecklist(
                "", // Slot 0
                "Eliminar Calabaza",     // Slot 1
                "",                  // Slot 2 (Limpiar)
                "Eliminar Papa",                  // Slot 3 (Limpiar)
                ""                   // Slot 4 (Limpiar)
            );
            StartCoroutine(Intro());
        }
    }

    // --- LÓGICAS DE ELIMINACIÓN ---

    void LogicaEliminarInicio(string tipo, GameObject objetoTocado)
    {
        if (tipo == "Head" && !cargandoAgua)
        {
            brilloHead.SetEncendido(false);
            cargandoAgua = true;
            andy.Decir("Conecta el INICIO directamente al DATO del Trigo. Así saltaremos la Calabaza.");
            EncenderBrilloEnNodo(listaNodos[1].gameObject, "Dato", true);
        }
        else if (tipo == "EntradaHuerto" && cargandoAgua)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[1])
            {
                cargandoAgua = false;
                EncenderBrilloEnNodo(listaNodos[1].gameObject, "Dato", false);
                andy.Decir("¡Bien! Al quitarle el agua desaparecerá de la lista.");
                StartCoroutine(SecuenciaEliminacionExitosa(0));
            }
        }
    }

    void LogicaEliminarFinal(string tipo, GameObject objetoTocado)
    {
        if (tipo == "SalidaHuerto" && !cargandoAgua)
        {
            if (objetoTocado.GetComponentInParent<NodoManager>() == listaNodos[1])
            {
                EncenderBrilloEnNodo(listaNodos[1].gameObject, "Puntero", false);
                cargandoAgua = true;
                brilloNull.SetEncendido(true);
                andy.Decir("Lleva el PUNTERO del Trigo hacia NULL. Esto dejará a la Papa fuera de la lista.");
            }
        }
        else if (tipo == "Null" && cargandoAgua)
        {
            cargandoAgua = false;
            brilloNull.SetEncendido(false);
            andy.Decir("¡Perfecto! El Trigo ahora es el final y la Papa ha sido eliminada.");
            StartCoroutine(SecuenciaEliminacionExitosa(2));
        }
    }

    IEnumerator SecuenciaEliminacionExitosa(int indiceNodo)
    {
        SumarPuntos(20);
        listaNodos[indiceNodo].IniciarSecuenciaEliminacion();

        yield return new WaitForSeconds(1.5f);
        ActualizarLineaFijaPostEliminacion();
        UIManager.instancia.MarcarTareaCompletada(fase);
        fase++;

        if (modoActual == ModoOperacion.EliminarInicio)
        {
            andy.Decir("¡Adiós calabaza! Último paso...");
            yield return new WaitForSeconds(1.5f);
            modoActual = ModoOperacion.EliminarFinal;
            // No llamamos a Intro completo para no repetir mochila, solo el texto y guía
            ProximoPaso();
        }
        else
        {
            andy.Decir("¡Perfecto! Dominas las listas de Cairo.");
        }
    }

    // --- MÉTODOS DE APOYO (REUTILIZADOS) ---

    void LimpiarEscenaParaSiguienteAlgoritmo()
    {
        fase = 0; pasoConexion = 0; cargandoAgua = false;
        lineaAgua.positionCount = 0;
        lineaFija.positionCount = 0;
        puntosCadenaFija.Clear();
        listaNodos.Clear();
        managerAnterior = null;
        managerActual = null;
        if (UIManager.instancia != null) UIManager.instancia.ResetBotones();
        LimpiarNodosEscena();
        ApagarBrillosGlobales();
    }

    void FinalizarNodo()
    {
        cargandoAgua = false;
        SumarPuntos(10);
        managerActual.DrenarAgua();
        ApagarBrillosGlobales();
        UIManager.instancia.MarcarTareaCompletada(fase);

        if (modoActual == ModoOperacion.InsertarInicio)
        {
            listaNodos.Insert(0, managerActual);
            List<Vector3> nuevaRuta = new List<Vector3>() { managerActual.puntoEntrada.position, managerActual.puntoSalida.position };
            if (fase == 0) nuevaRuta.Add(puntoEntradaNull.position);
            else nuevaRuta.AddRange(puntosCadenaFija);
            puntosCadenaFija = nuevaRuta;
        }
        else
        {
            listaNodos.Add(managerActual);
            if (fase == 0) puntosCadenaFija = new List<Vector3>() { puntoSalidaHead.position, managerActual.puntoEntrada.position, managerActual.puntoSalida.position, puntoEntradaNull.position };
            else
            {
                puntosCadenaFija.RemoveAt(puntosCadenaFija.Count - 1);
                puntosCadenaFija.Add(managerActual.puntoEntrada.position);
                puntosCadenaFija.Add(managerActual.puntoSalida.position);
                puntosCadenaFija.Add(puntoEntradaNull.position);
            }
        }

        lineaFija.positionCount = puntosCadenaFija.Count;
        lineaFija.SetPositions(puntosCadenaFija.ToArray());

        managerAnterior = managerActual; // Guardamos la actual como anterior
        managerActual = null;            // ¡IMPORTANTE! Limpiamos la actual para la siguiente fase
        fase++;

        if (fase < 3) StartCoroutine(EsperarSiguiente());
        else StartCoroutine(CambiarDeModo());
    }

    void Update()
    {
        // Si alguno de los objetos esenciales fue destruido, dejamos de ejecutar el Update
        if (puntoSalidaHead == null || lupi == null || puntoEntradaNull == null) return;

        ActualizarVisualManguera();
    }

    void ActualizarVisualManguera()
    {
        List<Vector3> puntosActivos = new List<Vector3>();

        if (modoActual == ModoOperacion.InsertarInicio)
        {
            puntosActivos.Add(puntoSalidaHead.position);
            if (managerActual != null)
            {
                if (pasoConexion == 0)
                {
                    if (cargandoAgua) puntosActivos.Add(lupi.position);
                    else if (managerAnterior != null) puntosActivos.Add(managerAnterior.puntoEntrada.position);
                }
                else if (pasoConexion == 1) puntosActivos.Add(managerActual.puntoEntrada.position);
                else if (pasoConexion == 2)
                {
                    puntosActivos.Add(managerActual.puntoEntrada.position);
                    puntosActivos.Add(managerActual.puntoSalida.position);
                    if (cargandoAgua) puntosActivos.Add(lupi.position);
                }
            }
            else if (managerAnterior != null) puntosActivos.Add(managerAnterior.puntoEntrada.position);
        }
        else if (modoActual == ModoOperacion.InsertarFinal)
        {
            if (managerActual != null)
            {
                // Verificamos que managerAnterior no sea nulo antes de pedir su posición
                Vector3 origen = (fase == 0 || managerAnterior == null) ? puntoSalidaHead.position : managerAnterior.puntoSalida.position;
                puntosActivos.Add(origen);
                if (pasoConexion == 0) { if (cargandoAgua) puntosActivos.Add(lupi.position); }
                else if (pasoConexion == 1) puntosActivos.Add(managerActual.puntoEntrada.position);
                else if (pasoConexion == 2) { puntosActivos.Add(managerActual.puntoEntrada.position); puntosActivos.Add(managerActual.puntoSalida.position); if (cargandoAgua) puntosActivos.Add(lupi.position); }
            }
        }
        else if (modoActual == ModoOperacion.EliminarInicio)
        {
            puntosActivos.Add(puntoSalidaHead.position);
            if (cargandoAgua) puntosActivos.Add(lupi.position);
        }
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            // Verificamos que el nodo exista antes de acceder a su transform
            if (listaNodos.Count > 1 && listaNodos[1] != null)
                puntosActivos.Add(listaNodos[1].puntoSalida.position);

            if (cargandoAgua) puntosActivos.Add(lupi.position);
        }

        if (lineaAgua != null)
        {
            lineaAgua.positionCount = puntosActivos.Count;
            lineaAgua.SetPositions(puntosActivos.ToArray());
        }
    }

    void ActualizarLineaFijaPostEliminacion()
    {
        puntosCadenaFija.Clear();

        // Si acabamos de eliminar la primera planta (Calabaza)
        if (modoActual == ModoOperacion.EliminarInicio)
        {
            puntosCadenaFija.Add(puntoSalidaHead.position);
            puntosCadenaFija.Add(listaNodos[1].puntoEntrada.position);
            puntosCadenaFija.Add(listaNodos[1].puntoSalida.position);
            puntosCadenaFija.Add(listaNodos[2].puntoEntrada.position);
            puntosCadenaFija.Add(listaNodos[2].puntoSalida.position);
            puntosCadenaFija.Add(puntoEntradaNull.position);
        }
        // Si acabamos de eliminar la última planta (Papa)
        else if (modoActual == ModoOperacion.EliminarFinal)
        {
            puntosCadenaFija.Add(puntoSalidaHead.position);
            puntosCadenaFija.Add(listaNodos[1].puntoEntrada.position); // Trigo
            puntosCadenaFija.Add(listaNodos[1].puntoSalida.position);  // Puntero Trigo
            puntosCadenaFija.Add(puntoEntradaNull.position);          // Hacia el NULL final
        }

        lineaFija.positionCount = puntosCadenaFija.Count;
        lineaFija.SetPositions(puntosCadenaFija.ToArray());
    }

    void LimpiarNodosEscena()
    {
        // Buscamos todos los NodoManager en la escena
        NodoManager[] todosLosNodos = Object.FindObjectsByType<NodoManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var n in todosLosNodos)
        {
            // Si es un clon (Nivel 3), lo destruimos
            if (n.gameObject.name.Contains("(Clone)"))
            {
                Destroy(n.gameObject);
            }
        }

        // Reseteamos el Nivel 1 una sola vez, no dentro del bucle
        if (LogicaNivel1.instancia != null)
        {
            LogicaNivel1.instancia.ResetearNivelSilencioso();
        }

        // Reseteamos las zonas de plantado
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.ResetearZona();
    }

    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null)
    {
        switch (modoActual)
        {
            case ModoOperacion.InsertarInicio: LogicaInsertarInicio(tipo, objetoTocado); break;
            case ModoOperacion.InsertarFinal: LogicaInsertarFinal(tipo, objetoTocado); break;
            case ModoOperacion.EliminarInicio: LogicaEliminarInicio(tipo, objetoTocado); break;
            case ModoOperacion.EliminarFinal: LogicaEliminarFinal(tipo, objetoTocado); break;
        }
    }

    void LogicaInsertarInicio(string tipo, GameObject objetoTocado)
    {
        // Si aún no hemos sembrado, Andy avisa
        if (managerActual == null && (tipo == "Head" || tipo == "EntradaHuerto"))
        {
            andy.Decir("Primero siembra la semilla de " + nombresNodosInicio[fase]);
            return;
        }

        if (cargandoAgua)
        {
            if (pasoConexion == 0) // El agua viene del INICIO
            {
                if (tipo == "SalidaHuerto")
                {
                    andy.Decir("¡No! El INICIO debe conectarse al DATO (izquierda) para entrar al NODO.");
                    return;
                }

                // Verificamos si tocamos el DATO del nodo actual
                if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(10);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", true);
                    andy.Decir("¡Muy bien! Ahora activa su PUNTERO.");
                }
            }
            else if (pasoConexion == 2) // El agua va hacia el siguiente
            {
                if (tipo == "Null" && fase == 0) FinalizarNodo();
                else if (tipo == "EntradaHuerto" && fase > 0 && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior) FinalizarNodo();
            }
        }
        else // RECOGER AGUA
        {
            if (tipo == "Head" && pasoConexion == 0)
            {
                brilloHead.SetEncendido(false);
                cargandoAgua = true;
                EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
                andy.Decir("Lleva el agua al DATO de la planta nueva.");
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1 && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
            {
                EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", false);
                cargandoAgua = true;
                pasoConexion = 2;
                if (fase == 0) brilloNull.SetEncendido(true);
                else EncenderBrilloEnNodo(managerAnterior.gameObject, "Dato", true);
            }
        }
    }

    void LogicaInsertarFinal(string tipo, GameObject objetoTocado)
    {
        if (managerActual == null) return;

        if (cargandoAgua)
        {
            if (pasoConexion == 0) // Viene del Head o del Puntero anterior
            {
                // BLOQUEO: Si intenta entrar por la salida
                if (tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    andy.Decir("¡Error! El flujo de la lista debe entrar por el DATO (izquierda) del nuevo nodo.");
                    return;
                }

                if (tipo == "EntradaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Dato", false);
                    managerActual.ActivarHuerto();
                    SumarPuntos(10);
                    cargandoAgua = false;
                    pasoConexion = 1;
                    EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", true);
                    andy.Decir("¡Bien! Ahora lleva el PUNTERO de esta nueva planta hacia NULL.");
                }
                else
                {
                    andy.Decir("Conecta la manguera al letrero de DATO (izquierdo) de la nueva planta.");
                }
            }
            else if (pasoConexion == 2) // El agua va hacia NULL
            {
                if (tipo == "Null") FinalizarNodo();
                else andy.Decir("Como es el final de la lista, el PUNTERO debe ir a NULL.");
            }
        }
        else // RECOGER AGUA
        {
            if (pasoConexion == 0)
            {
                if (fase == 0 && tipo == "Head")
                {
                    brilloHead.SetEncendido(false);
                    cargandoAgua = true;
                }
                else if (fase > 0 && tipo == "SalidaHuerto" && objetoTocado.GetComponentInParent<NodoManager>() == managerAnterior)
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Puntero", false);
                    cargandoAgua = true;
                }

                if (cargandoAgua) EncenderBrilloEnNodo(managerActual.gameObject, "Dato", true);
            }
            else if (tipo == "SalidaHuerto" && pasoConexion == 1)
            {
                if (objetoTocado.GetComponentInParent<NodoManager>() == managerActual)
                {
                    EncenderBrilloEnNodo(managerActual.gameObject, "Puntero", false);
                    cargandoAgua = true;
                    pasoConexion = 2;
                    brilloNull.SetEncendido(true);
                }
            }
        }
    }

    public void AvanceSiembraExitosa()
    {
        UIManager.instancia.SetSemillaPalpitar("");
        StartCoroutine(EsperarYAsignarNodo());
    }

    IEnumerator EsperarYAsignarNodo()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForFixedUpdate();

        managerActual = BuscarNuevoNodoEnEscena();

        if (managerActual != null)
        {
            if (modoActual == ModoOperacion.InsertarInicio)
            {
                brilloHead.SetEncendido(true);
                andy.Decir("¡NODO listo! Ahora recoge el agua del INICIO para conectarla.");
            }
            else if (modoActual == ModoOperacion.InsertarFinal)
            {
                if (fase == 0)
                {
                    brilloHead.SetEncendido(true);
                    andy.Decir("¡NODO listo! Ahora recoge el agua del INICIO.");
                }
                else
                {
                    EncenderBrilloEnNodo(managerAnterior.gameObject, "Puntero", true);
                    andy.Decir("¡NODO listo! Recoge el agua del PUNTERO del NODO anterior.");
                }
            }
        }
        else
        {
            Debug.LogError("No se encontró el nodo en la escena. Revisa que el nombre del Prefab contenga el nombre de la semilla.");
        }
    }

    NodoManager BuscarNuevoNodoEnEscena()
    {
        string[] nombres = (modoActual == ModoOperacion.InsertarInicio) ? nombresNodosInicio : nombresNodosFinal;
        if (fase >= nombres.Length) return null;

        string buscado = nombres[fase].ToLower();
        foreach (var nm in Object.FindObjectsByType<NodoManager>(FindObjectsSortMode.None))
        {
            if (nm.gameObject.name.ToLower().Contains(buscado) &&
                nm.gameObject.name.Contains("(Clone)") &&
                !listaNodos.Contains(nm))
            {
                return nm;
            }
        }
        return null;
    }
    void EncenderBrilloEnNodo(GameObject nodo, string parte, bool encender) { if (nodo == null) return; foreach (var b in nodo.GetComponentsInChildren<EfectoLetrero>(true)) if (b.gameObject.name.ToUpper().Contains(parte.ToUpper())) b.SetEncendido(encender); }
    void SumarPuntos(int cant) { UIManager.puntosGlobales += cant; if (textoPuntos) textoPuntos.text = UIManager.puntosGlobales.ToString(); }
    void ApagarBrillosGlobales() { if (brilloHead) brilloHead.SetEncendido(false); if (brilloNull) brilloNull.SetEncendido(false); EfectoLetrero[] todos = Object.FindObjectsByType<EfectoLetrero>(FindObjectsSortMode.None); foreach (var b in todos) b.SetEncendido(false); }
    IEnumerator EsperarSiguiente() { yield return new WaitForSeconds(2f); ProximoPaso(); }
}
