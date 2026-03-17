using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HistorietaManager : MonoBehaviour
{
    [Header("Referencias a Botones")]
    [Tooltip("Arrastra aquí el ButtonSaltar desde la jerarquía.")]
    public Button buttonSaltar; [Tooltip("Arrastra aquí el ButtonRegresar desde la jerarquía.")]
    public Button buttonRegresar; [Header("Configuración de Escenas")]
    [Tooltip("Nombre exacto de la escena a la que vas al saltar/jugar.")]
    public string escenaSiguiente = "PrimerNivel"; [Tooltip("Nombre exacto de la escena a la que regresas (ej. el Menú Principal).")]
    public string escenaAnterior = "MenuInicio";

    void Start()
    {
        // Asignamos las funciones a los botones si están conectados en el Inspector
        if (buttonSaltar != null)
        {
            buttonSaltar.onClick.AddListener(CargarEscenaSiguiente);
        }

        if (buttonRegresar != null)
        {
            buttonRegresar.onClick.AddListener(CargarEscenaAnterior);
        }
    }

    // Función para ir al nivel
    public void CargarEscenaSiguiente()
    {
        if (!string.IsNullOrEmpty(escenaSiguiente))
        {
            SceneManager.LoadScene(escenaSiguiente);
        }
        else
        {
            Debug.LogError("Falta escribir el nombre de la Escena Siguiente en el Inspector.");
        }
    }

    // Función para regresar al menú
    public void CargarEscenaAnterior()
    {
        if (!string.IsNullOrEmpty(escenaAnterior))
        {
            SceneManager.LoadScene(escenaAnterior);
        }
        else
        {
            Debug.LogError("Falta escribir el nombre de la Escena Anterior en el Inspector.");
        }
    }
}