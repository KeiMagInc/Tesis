using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class PantallaFinalManager : MonoBehaviour
{
    [Header("Panel de Compañero")]
    public GameObject panelResultados; // El objeto "Panel_Puntaje"
    public TextMeshProUGUI txtPuntaje; // txtValorPuntaje
    public TextMeshProUGUI txtAciertos; // txtValorAciertos
    public TextMeshProUGUI txtFallos; // txtValorFallos
     [Header("Configuración de Escenas")]
    [Tooltip("Escribe el nombre exacto de la siguiente escena tal como aparece en tus carpetas")]
    public string nombreSiguienteEscena;

    public void MostrarResultados()
    {
        PuntajeManager pm = Object.FindFirstObjectByType<PuntajeManager>();
        
        // Asignamos los valores finales a los textos de tu compañero
        txtPuntaje.text = pm.GetPuntos().ToString();
        txtAciertos.text = pm.GetAciertos().ToString();
        txtFallos.text = pm.GetFallos().ToString();

        panelResultados.SetActive(true);
        // Opcional: Congelar el tiempo de juego
        Time.timeScale = 0;
    }

    public void Rejugar()
    {
        // Resetear el tiempo por si pausamos en MostrarResultados
        Time.timeScale = 1;
        
        // Obtiene el nombre de la escena actual y la vuelve a cargar
        string escenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(escenaActual);
    }

    public void Siguiente()
    {
        Time.timeScale = 1;

        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else
        {
            Debug.LogError("¡Olvidadte poner el nombre de la siguiente escena en el Inspector!");
        }
    }
}