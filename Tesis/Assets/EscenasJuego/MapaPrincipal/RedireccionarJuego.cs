using UnityEngine;
using UnityEngine.SceneManagement;
public class RedireccionarJuego : MonoBehaviour
{
    [Header("Configuración de Carga de Escenas")]
    [Tooltip("Introduce el nombre EXACTO de la escena a la que este botón debe redirigir.")]
    [SerializeField]
    private string nombreDeLaEscenaADirigir;
    [Header("Música de Fondo (Opcional)")]
    [Tooltip("Arrastra aquí el AudioSource si quieres que este objeto ponga música al cargar el mapa.")]
    public AudioSource musicaFondo;
    void Start()
    {
        if (musicaFondo != null)
        {
            if (!musicaFondo.isPlaying)
            {
                musicaFondo.loop = true;
                musicaFondo.Play();
            }
        }
    }
    public void CargarEscenaEspecifica()
    {
        if (string.IsNullOrEmpty(nombreDeLaEscenaADirigir))
        {
            Debug.LogError("Error: No se ha especificado un nombre de escena en el Inspector para este botón.");
            return;
        }
        SceneManager.LoadScene(nombreDeLaEscenaADirigir);
        Debug.Log("Redirigiendo a la escena: " + nombreDeLaEscenaADirigir);
    }
    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Saliendo del juego...");
    }
}