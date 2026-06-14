using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ControladorInsignia : MonoBehaviour
{
    [Header("Ajustes Visuales")]
    [Range(0, 1)] public float transparenciaNoGanada = 0.2f;
    [Range(0, 1)] public float escalaFinalSlot = 0.7f;
    [Range(1f, 5f)]
    [Tooltip("Escala de la insignia cuando se muestra en el centro de la pantalla.")]
    public float escalaCentroGanada = 5.0f;
    [Header("Tiempos")]
    public float tiempoAparicion = 0.5f;
    public float tiempoEspera = 1.5f;
    public float tiempoViaje = 0.8f;
    [Header("Sonido")]
    [Tooltip("Opcional: Si se deja vacío, intentará usar la fuente de audio de Andy en el UIManager.")]
    public AudioSource fuenteAudio;
    [Tooltip("Arrastra aquí el clip de sonido 'insignia'.")]
    public AudioClip sonidoInsignia;
    private RectTransform rectTransform;
    private Image imagen;
    private Vector2 posicionOriginalEnSlot;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        imagen = GetComponent<Image>();
        posicionOriginalEnSlot = rectTransform.anchoredPosition;
        if (imagen != null)
        {
            Color c = imagen.color;
            c.a = transparenciaNoGanada;
            imagen.color = c;
        }
        rectTransform.localScale = Vector3.one * escalaFinalSlot;
    }
    public void MostrarInsignia(Sprite spriteInsignia)
    {
        if (imagen != null) imagen.sprite = spriteInsignia;
        StopAllCoroutines();
        StartCoroutine(AnimarInsigniaGanada());
    }
    IEnumerator AnimarInsigniaGanada()
    {
        if (imagen != null)
        {
            Color c = imagen.color;
            c.a = 1f;
            imagen.color = c;
        }
        Transform parentOriginal = rectTransform.parent;
        int siblingIndexOriginal = rectTransform.GetSiblingIndex();
        Vector3 posicionSlotMundo = rectTransform.position;
        Vector3 escalaSlotLocal = Vector3.one * escalaFinalSlot;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rectTransform.SetParent(canvas.transform, true);
            rectTransform.position = canvas.transform.position;
        }
        rectTransform.localScale = Vector3.zero;
        AudioSource audioSourceToUse = fuenteAudio;
        if (audioSourceToUse == null && UIManager.instancia != null)
            audioSourceToUse = UIManager.instancia.fuenteVozAndy;
        if (audioSourceToUse != null && sonidoInsignia != null)
            audioSourceToUse.PlayOneShot(sonidoInsignia);
        float tiempo = 0;
        Vector3 escalaGrande = Vector3.one * escalaCentroGanada;
        while (tiempo < tiempoAparicion)
        {
            tiempo += Time.deltaTime;
            float curva = tiempo / tiempoAparicion;
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, escalaGrande, curva);
            yield return null;
        }
        rectTransform.localScale = escalaGrande;
        yield return new WaitForSeconds(tiempoEspera);
        tiempo = 0;
        Vector3 posCentroMundo = rectTransform.position;
        while (tiempo < tiempoViaje)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoViaje;
            t = t * t * (3f - 2f * t);
            rectTransform.position = Vector3.Lerp(posCentroMundo, posicionSlotMundo, t);
            rectTransform.localScale = Vector3.Lerp(escalaGrande, escalaSlotLocal, t);
            yield return null;
        }
        rectTransform.SetParent(parentOriginal, true);
        rectTransform.SetSiblingIndex(siblingIndexOriginal);
        rectTransform.position = posicionSlotMundo;
        rectTransform.localScale = escalaSlotLocal;
        Debug.Log("<color=green>Insignia Guardada en el tablero!</color>");
    }
    public void ResetearInsignia()
    {
        StopAllCoroutines();
        if (imagen != null)
        {
            Color c = imagen.color;
            c.a = transparenciaNoGanada;
            imagen.color = c;
        }
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = posicionOriginalEnSlot;
            rectTransform.localScale = Vector3.one * escalaFinalSlot;
        }
        Debug.Log("Insignia: Reseteada al estado de 'No ganada'.");
    }
}