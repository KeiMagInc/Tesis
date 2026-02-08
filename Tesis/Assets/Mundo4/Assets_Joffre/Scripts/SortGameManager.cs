using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SortGameManager : MonoBehaviour
{
    public static SortGameManager Instance;

    [Header("Configuración del Nivel")]
    public List<EsferaLogica> esferasEnJuego;
    public int puntaje = 0;

    private int aciertos = 0;
    private int fallos = 0;

    // --- VARIABLE PARA BLOQUEAR ACCIONES MIENTRAS SE MUEVEN ---
    public bool estaIntercambiando = false;

    [Header("UI General")]
    public GameObject panelResultados;
    public TextMeshProUGUI textoPuntaje;
    public TextMeshProUGUI textoAciertos;
    public TextMeshProUGUI textoFallos;
    public GameObject panelContinuara;
    public CanvasGroup fadeCanvasGroup;

    [Header("UI HUD In-Game")]
    public TextMeshProUGUI textoPuntosHUD;
    public TextMeshProUGUI textoPseudocodigo;

    // --- REFERENCIAS PARA LA VARIABLE AUXILIAR ---
    [Header("Visualización Variable Auxiliar")]
    public GameObject panelVarAux;        // El objeto padre que creaste en el paso 1
    public TextMeshProUGUI textoVarAux;   // El texto del número dentro de ese panel

    [Header("Estado del Algoritmo")]
    private int currentI = 0;
    private int currentJ = 0;
    private bool algoritmoTerminado = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        esferasEnJuego.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        if (panelResultados != null) panelResultados.SetActive(false);
        if (panelContinuara != null) panelContinuara.SetActive(false);
        if (panelVarAux != null) panelVarAux.SetActive(false);

        ActualizarTextoHUD();

        ActualizarPseudocodigo("Iniciando algoritmo...", Color.white);

        // --- LANZAR TUTORIAL SECUENCIAL ---
        string[] tutorial = {
            "¡Bienvenido al entrenamiento de Ordenamiento Burbuja!",
            "Este algoritmo funciona comparando pares de números 'ADYACENTES' y ordenándolos.",
            "Tu misión: Revisa cada par. Si el número de la IZQUIERDA es mayor que el de la derecha, ¡intercámbialos!",
            "Si ya están en orden, se quedarán como están y pasaremos al siguiente par.",
            "¡Empecemos! Selecciona el primer par."
        };
        // Enviamos la lista al sistema de diálogo
        SistemaDialogo.Instance.MostrarSecuencia(tutorial);
    }

    public void IntentarMovimiento(EsferaLogica esferaA, EsferaLogica esferaB)
    {
        // Agregamos "estaIntercambiando" al bloqueo
        if (algoritmoTerminado || SistemaDialogo.Instance.dialogoActivo || estaIntercambiando) return;

        int indexA = esferasEnJuego.IndexOf(esferaA);
        int indexB = esferasEnJuego.IndexOf(esferaB);
        if (indexA > indexB) { int temp = indexA; indexA = indexB; indexB = temp; }

        // 1. VALIDACIÓN ADYACENCIA
        if (Mathf.Abs(indexA - indexB) != 1)
        {
            RegistrarFallo("¡Error de Adyacencia!\n" +
                           "En el Ordenamiento Burbuja SOLO puedes comparar parejas que estén juntas (vecinos).\n" +
                           $"Has seleccionado la posición {indexA} y la {indexB}, que están separadas.");
            return;
        }

        // 2. VALIDACIÓN ORDEN
        if (indexA != currentI || indexB != currentI + 1)
        {
            ActualizarPseudocodigo("Incorrecto.", Color.red);
            int valorEsperadoIzq = esferasEnJuego[currentI].valor;
            int valorEsperadoDer = esferasEnJuego[currentI + 1].valor;
            RegistrarFallo($"¡Paso Incorrecto!\n" +
                           $"El algoritmo va en orden. Ahora mismo toca revisar el par de la posición {currentI} y {currentI + 1}.\n" +
                           $"Deberías estar comparando los números [{valorEsperadoIzq}] y [{valorEsperadoDer}].");
            return;
        }

        // --- ACIERTO ---
        RegistrarAcierto();

        // Validar si necesita intercambio (Bubble Sort: Izquierda > Derecha)
        if (esferasEnJuego[indexA].valor > esferasEnJuego[indexB].valor)
        {
            ActualizarPseudocodigo($"Correcto. {esferasEnJuego[indexA].valor} > {esferasEnJuego[indexB].valor}. Guardamos en Variable Temporal.", Color.green);
            // Iniciamos la Corrutina visual
            StartCoroutine(SecuenciaSwapVisual(indexA, indexB));
        }
        else
        {
            ActualizarPseudocodigo($"Correcto. {esferasEnJuego[indexA].valor} < {esferasEnJuego[indexB].valor}. No se requiere cambio.", Color.green);
            // Si no hay intercambio, solo avanzamos
            AvanzarPasoBubbleSort();
        }
    }

    // Función auxiliar para cambiar el texto de arriba
    void ActualizarPseudocodigo(string texto, Color color)
    {
        if (textoPseudocodigo != null)
        {
            textoPseudocodigo.text = texto;
            textoPseudocodigo.color = color;
        }
    }

    // --- LA SECUENCIA VISUAL CON VARIABLE AUXILIAR ---
    IEnumerator SecuenciaSwapVisual(int indexA, int indexB)
    {
        estaIntercambiando = true; // Bloqueamos a Lupu

        EsferaLogica esferaIzq = esferasEnJuego[indexA];
        EsferaLogica esferaDer = esferasEnJuego[indexB];

        // PASO 1: Aparece la Variable Auxiliar (Copiamos valor de la izquierda)
        // Código teórico: temp = arr[j];
        if (panelVarAux != null)
        {
            panelVarAux.SetActive(true);
            textoVarAux.text = esferaIzq.valor.ToString();
        }
        yield return new WaitForSeconds(1.0f); // Esperamos 1 seg para que el jugador vea la variable

        // PASO 2: Mover físicamente las esferas (Simulamos arr[j] = arr[j+1] y arr[j+1] = temp)
        // Intercambio de posiciones
        Vector3 posA = esferaIzq.transform.position;
        Vector3 posB = esferaDer.transform.position;

        float tiempo = 0;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * 2; // Velocidad de movimiento
            esferaIzq.transform.position = Vector3.Lerp(posA, posB, tiempo);
            esferaDer.transform.position = Vector3.Lerp(posB, posA, tiempo);
            yield return null;
        }

        // Actualizar lista lógica
        esferasEnJuego[indexA] = esferaDer;
        esferasEnJuego[indexB] = esferaIzq;

        yield return new WaitForSeconds(0.5f);

        // PASO 3: Desaparece la Variable Auxiliar (Ya se usó)
        if (panelVarAux != null)
        {
            panelVarAux.SetActive(false);
        }

        estaIntercambiando = false; // Desbloqueamos a Lupu
        AvanzarPasoBubbleSort();
    }

    void RegistrarAcierto() { aciertos++; ModificarPuntaje(5); }
    void RegistrarFallo(string motivo) { fallos++; ModificarPuntaje(-5); SistemaDialogo.Instance.MostrarMensaje(motivo); }
    void ModificarPuntaje(int cantidad) { puntaje += cantidad; if (puntaje < 0) puntaje = 0; ActualizarTextoHUD(); }
    void ActualizarTextoHUD() { if (textoPuntosHUD != null) textoPuntosHUD.text = puntaje.ToString(); }

    void AvanzarPasoBubbleSort()
    {
        currentI++;
        if (currentI >= esferasEnJuego.Count - 1 - currentJ)
        {
            currentI = 0;
            currentJ++;
            if (currentJ >= esferasEnJuego.Count - 1)
            {
                algoritmoTerminado = true;
                StartCoroutine(CharlaEficienciaFinal());
            }
        }
    }

    IEnumerator CharlaEficienciaFinal()
    {
        yield return new WaitForSeconds(0.5f);
        ActualizarPseudocodigo("¡Arreglo Ordenado!", Color.yellow);

        // Calculamos ineficiencia (Comparaciones teóricas vs fallos)
        int comparacionesTotales = (esferasEnJuego.Count * (esferasEnJuego.Count - 1)) / 2; // N*(N-1)/2

        string[] charlaFinal = {
            "¡Excelente! Has ordenado todo el arreglo.",
            "Hablemos de eficiencia... El método de Burbuja es famoso por ser lento (O(n²)).",
            $"Para ordenar estas 5 esferas, hemos tenido que hacer {comparacionesTotales} comparaciones obligatorias.",
            $"Además, cometiste {fallos} errores. Cada error es tiempo de procesamiento desperdiciado en la vida real.",
            "En arreglos gigantes, este método tardaría una eternidad. ¡Pero es genial para aprender!",
            "Veamos tus resultados finales."
        };

        // Pasamos la lista y le decimos que al terminar (Callback) muestre el HUD
        SistemaDialogo.Instance.MostrarSecuencia(charlaFinal, MostrarResultados);
    }

    void MostrarResultados() { panelResultados.SetActive(true); textoPuntaje.text = puntaje.ToString("00000000"); textoAciertos.text = aciertos.ToString(); textoFallos.text = fallos.ToString(); }
    public void BotonRejugar() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void BotonSiguiente() { panelResultados.SetActive(false); StartCoroutine(AnimacionContinuara()); }
    IEnumerator AnimacionContinuara() { panelContinuara.SetActive(true); float alpha = 0; while (alpha < 1) { alpha += Time.deltaTime * 0.5f; if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = alpha; yield return null; } }
}