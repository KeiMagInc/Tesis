using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject panelControles;
    private CanvasGroup canvasGroup;
    
    [Header("Ajustes de Animación")]
    public float duracion = 0.3f; 
    private Coroutine animacionActual;

    // Aquí guardaremos el (6, 6, 6) automáticamente
    private Vector3 escalaOriginal;

    void Awake()
    {
        // 1. Guardamos la escala que tú pusiste en el Inspector antes de ponerla en cero
        escalaOriginal = panelControles.transform.localScale;

        canvasGroup = panelControles.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panelControles.AddComponent<CanvasGroup>();
        
        // 2. Ahora sí, lo ocultamos para el inicio
        panelControles.SetActive(false);
        canvasGroup.alpha = 0;
        panelControles.transform.localScale = Vector3.zero;
    }

    public void ToggleControles()
    {
        if (animacionActual != null) StopCoroutine(animacionActual);

        bool estaAbierto = panelControles.activeSelf;

        if (!estaAbierto)
        {
            panelControles.SetActive(true);
            // Animamos de 0 a la escala original (6,6,6)
            animacionActual = StartCoroutine(AnimarPanel(0, 1, Vector3.zero, escalaOriginal));
        }
        else
        {
            // Animamos de la escala original (6,6,6) a 0
            animacionActual = StartCoroutine(AnimarPanel(1, 0, escalaOriginal, Vector3.zero, true));
        }
    }

    IEnumerator AnimarPanel(float alphaInicio, float alphaFin, Vector3 escalaInicio, Vector3 escalaFin, bool cerrarAlTerminar = false)
    {
        float tiempo = 0;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            float suave = Mathf.SmoothStep(0, 1, progreso);

            canvasGroup.alpha = Mathf.Lerp(alphaInicio, alphaFin, suave);
            panelControles.transform.localScale = Vector3.Lerp(escalaInicio, escalaFin, suave);
            
            yield return null;
        }

        canvasGroup.alpha = alphaFin;
        panelControles.transform.localScale = escalaFin;

        if (cerrarAlTerminar) panelControles.SetActive(false);
        
        animacionActual = null;
    }
}