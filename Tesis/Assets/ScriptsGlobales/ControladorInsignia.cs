using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ControladorInsignia : MonoBehaviour
{
    [Header("Ajustes Visuales")]
    [Range(0, 1)] public float transparenciaNoGanada = 0.2f;
    [Range(0, 1)] public float escalaFinalSlot = 0.7f;
    [Header("Tiempos")]
    public float tiempoAparicion = 0.5f;
    public float tiempoEspera = 1.5f;
    public float tiempoViaje = 0.8f;
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
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.zero;
        float tiempo = 0;
        while (tiempo < tiempoAparicion)
        {
            tiempo += Time.deltaTime;
            float curva = tiempo / tiempoAparicion;
            rectTransform.localScale = Vector3.one * Mathf.Lerp(0, 1.3f, curva);
            yield return null;
        }
        rectTransform.localScale = Vector3.one * 1.3f;
        yield return new WaitForSeconds(tiempoEspera);
        tiempo = 0;
        Vector2 posCentro = rectTransform.anchoredPosition;
        Vector3 escalaGrande = rectTransform.localScale;
        Vector3 escalaPequeña = Vector3.one * escalaFinalSlot;
        while (tiempo < tiempoViaje)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoViaje;
            t = t * t * (3f - 2f * t); 
            rectTransform.anchoredPosition = Vector2.Lerp(posCentro, posicionOriginalEnSlot, t);
            rectTransform.localScale = Vector3.Lerp(escalaGrande, escalaPequeña, t);
            yield return null;
        }
        rectTransform.anchoredPosition = posicionOriginalEnSlot;
        rectTransform.localScale = escalaPequeña;
        Debug.Log("<color=green>Insignia Guardada!</color>");
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