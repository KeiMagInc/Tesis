using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para la carga de escenas

public class RedireccionarJuego : MonoBehaviour
{
    [Header("Configuración de Carga de Escenas")]
    [Tooltip("Introduce el nombre EXACTO de la escena a la que este botón debe redirigir.")]
    [SerializeField] // Esto hace que el campo privado sea visible y editable en el Inspector
    private string nombreDeLaEscenaADirigir;

    /// <summary>
    /// Esta función se llama cuando un botón (configurado con este script) es presionado.
    /// Carga la escena cuyo nombre ha sido especificado en el Inspector.
    /// </summary>
    public void CargarEscenaEspecifica()
    {
        // Verifica si se ha especificado un nombre de escena
        if (string.IsNullOrEmpty(nombreDeLaEscenaADirigir))
        {
            Debug.LogError("Error: No se ha especificado un nombre de escena en el Inspector para este botón. Por favor, asigna uno.");
            return; // Sale de la función si no hay nombre de escena
        }

        // Carga la escena por el nombre especificado
        SceneManager.LoadScene(nombreDeLaEscenaADirigir);
        Debug.Log("Redirigiendo a la escena: " + nombreDeLaEscenaADirigir);
    }

    // Puedes añadir otras funciones útiles aquí si las necesitas, por ejemplo, para salir del juego.
    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        // Esto solo funciona en el editor para detener el modo de juego
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Saliendo del juego...");
    }
}