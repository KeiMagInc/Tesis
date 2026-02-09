using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Button
using UnityEngine.SceneManagement; // Necesario para SceneManager
using System.Collections; // Necesario para Coroutines (transiciones)
using TMPro; // Necesario si usas TextMeshPro para el texto del botón Saltar

public class HistorietaManager : MonoBehaviour
{
    [Header("Configuración de Páginas")]
    [Tooltip("Arrastra aquí los Sprites de cada página de tu historieta en orden.")]
    public Sprite[] paginasHistorieta; // Array para guardar los sprites de cada página

    [Header("Referencias UI")]
    [Tooltip("Arrastra aquí el componente Image que mostrará las páginas.")]
    public Image panelHistorieta; // Referencia al panel Image que mostrará las páginas

    [Tooltip("Arrastra aquí el Button para ir a la página anterior.")]
    public Button buttonAnterior;

    [Tooltip("Arrastra aquí el Button para ir a la página siguiente.")]
    public Button buttonSiguiente;

    [Tooltip("Arrastra aquí el Button para saltar la historieta.")]
    public Button buttonSaltar;

    [Header("Configuración de Transición")]
    [Tooltip("Duración de la transición entre páginas.")]
    public float duracionTransicion = 0.5f; // Duración en segundos de la transición

    [Header("Destino Salto")]
    [Tooltip("Nombre de la escena a cargar cuando se presione el botón 'Saltar'.")]
    public string nombreEscenaMapa = "MapaPrincipal"; // Nombre de la escena del mapa

    private int paginaActual = 0; // Índice de la página actual
    private bool estaEnTransicion = false; // Para evitar clicks durante la transición

    void Start()
    {
        // Asegurarse de que tenemos páginas para mostrar
        if (paginasHistorieta == null || paginasHistorieta.Length == 0)
        {
            Debug.LogError("No hay páginas de historieta asignadas. Asigna Sprites al array 'paginasHistorieta'.");
            return;
        }

        // Mostrar la primera página al inicio
        MostrarPagina(0);

        // Configurar los listeners para los botones
        // Se añaden los listeners programáticamente para mayor control.
        if (buttonAnterior != null) buttonAnterior.onClick.AddListener(PaginaAnterior);
        if (buttonSiguiente != null) buttonSiguiente.onClick.AddListener(PaginaSiguiente);
        if (buttonSaltar != null) buttonSaltar.onClick.AddListener(SaltarHistorieta);
    }

    /// <summary>
    /// Muestra la página de la historieta en el índice especificado con una transición.
    /// </summary>
    /// <param name="index">El índice de la página a mostrar.</param>
    void MostrarPagina(int index)
    {
        // Asegurarse de que el índice está dentro de los límites
        if (index < 0 || index >= paginasHistorieta.Length)
        {
            Debug.LogWarning("Intento de mostrar una página fuera de los límites: " + index);
            return;
        }

        paginaActual = index;
        // Inicia la corrutina para la transición
        StartCoroutine(TransicionPagina(paginasHistorieta[paginaActual]));
        ActualizarEstadoBotones(); // Actualiza la visibilidad de los botones
    }

    /// <summary>
    /// Corrutina para manejar la transición de la página.
    /// </summary>
    IEnumerator TransicionPagina(Sprite nuevaPagina)
    {
        estaEnTransicion = true;

        // Si la imagen ya tiene una opacidad inicial, guárdala
        Color colorInicialTarget = panelHistorieta.color;
        // La animación siempre empezará desde la opacidad total de la imagen actual
        Color colorActualOpacidadCompleta = new Color(colorInicialTarget.r, colorInicialTarget.g, colorInicialTarget.b, 1f);


        // Efecto de fade out (hacer transparente la imagen actual)
        for (float t = 0; t < duracionTransicion; t += Time.deltaTime)
        {
            panelHistorieta.color = Color.Lerp(colorActualOpacidadCompleta, Color.clear, t / duracionTransicion);
            yield return null; // Espera al siguiente frame
        }
        panelHistorieta.color = Color.clear; // Asegurarse de que esté completamente transparente

        // Cambiar la imagen una vez que es transparente
        panelHistorieta.sprite = nuevaPagina;

        // Efecto de fade in (hacer visible la nueva imagen)
        for (float t = 0; t < duracionTransicion; t += Time.deltaTime)
        {
            panelHistorieta.color = Color.Lerp(Color.clear, colorActualOpacidadCompleta, t / duracionTransicion);
            yield return null; // Espera al siguiente frame
        }
        panelHistorieta.color = colorActualOpacidadCompleta; // Asegurarse de que esté completamente visible

        estaEnTransicion = false;
    }

    /// <summary>
    /// Avanza a la siguiente página de la historieta.
    /// </summary>
    public void PaginaSiguiente()
    {
        if (!estaEnTransicion && paginaActual < paginasHistorieta.Length - 1)
        {
            MostrarPagina(paginaActual + 1);
        }
    }

    /// <summary>
    /// Retrocede a la página anterior de la historieta.
    /// </summary>
    public void PaginaAnterior()
    {
        if (!estaEnTransicion && paginaActual > 0)
        {
            MostrarPagina(paginaActual - 1);
        }
    }

    /// <summary>
    /// Actualiza el estado (visibilidad) de los botones Anterior, Siguiente y Saltar.
    /// </summary>
    void ActualizarEstadoBotones()
    {
        // Botón Anterior
        if (buttonAnterior != null)
        {
            buttonAnterior.gameObject.SetActive(paginaActual > 0);
        }

        // Botón Siguiente
        if (buttonSiguiente != null)
        {
            buttonSiguiente.gameObject.SetActive(paginaActual < paginasHistorieta.Length - 1);
        }

        // Botón Saltar (opcional: cambia texto a "Jugar" en la última página)
        if (buttonSaltar != null)
        {
            if (paginaActual == paginasHistorieta.Length - 1)
            {
                // Si es la última página, el botón "Saltar" se puede convertir en "¡Jugar!"
                // Intentar cambiar el texto para TextMeshPro
                TMP_Text tmpTextComponent = buttonSaltar.GetComponentInChildren<TMP_Text>();
                if (tmpTextComponent != null)
                {
                    tmpTextComponent.text = "¡Jugar!";
                }
                else
                {
                    // Si no es TextMeshPro, intentar con Text estándar
                    Text textComponent = buttonSaltar.GetComponentInChildren<Text>();
                    if (textComponent != null) textComponent.text = "¡Jugar!";
                }
            }
            else
            {
                // Si no es la última página, el texto es "Saltar"
                TMP_Text tmpTextComponent = buttonSaltar.GetComponentInChildren<TMP_Text>();
                if (tmpTextComponent != null)
                {
                    tmpTextComponent.text = "Saltar";
                }
                else
                {
                    Text textComponent = buttonSaltar.GetComponentInChildren<Text>();
                    if (textComponent != null) textComponent.text = "Saltar";
                }
            }
        }
    }

    /// <summary>
    /// Salta la historieta y carga la escena del mapa.
    /// </summary>
    public void SaltarHistorieta()
    {
        if (!string.IsNullOrEmpty(nombreEscenaMapa))
        {
            SceneManager.LoadScene(nombreEscenaMapa);
        }
        else
        {
            Debug.LogError("No se ha especificado el nombre de la escena del mapa para saltar.");
        }
    }
}