using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControladorInsignia : MonoBehaviour
{
    [Header("Ajustes Visuales")]
    [Range(0, 1)] public float transparenciaNoGanada = 0.2f;
    [Range(0, 1)] public float escalaFinalSlot = 0.7f; // Ajusta esto para el tamaño final

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

        // Al empezar, la ponemos transparente Y pequeña
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
        // 1. Cobrar Vida
        if (imagen != null)
        {
            Color c = imagen.color;
            c.a = 1f;
            imagen.color = c;
        }

        // 2. Saltar al Centro (Efecto Pop-up Grande)
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.zero;

        float tiempo = 0;
        while (tiempo < tiempoAparicion)
        {
            tiempo += Time.deltaTime;
            float curva = tiempo / tiempoAparicion;
            // Crece hasta tamaño normal (1.3 para impacto)
            rectTransform.localScale = Vector3.one * Mathf.Lerp(0, 1.3f, curva);
            yield return null;
        }
        rectTransform.localScale = Vector3.one * 1.3f;

        yield return new WaitForSeconds(tiempoEspera);

        // 3. Volar de regreso y ENCOGERSE
        tiempo = 0;
        Vector2 posCentro = rectTransform.anchoredPosition;
        Vector3 escalaGrande = rectTransform.localScale;
        Vector3 escalaPequeña = Vector3.one * escalaFinalSlot;

        while (tiempo < tiempoViaje)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoViaje;
            t = t * t * (3f - 2f * t); // Suavizado

            rectTransform.anchoredPosition = Vector2.Lerp(posCentro, posicionOriginalEnSlot, t);
            // Aquí ocurre el encogimiento
            rectTransform.localScale = Vector3.Lerp(escalaGrande, escalaPequeña, t);
            yield return null;
        }

        rectTransform.anchoredPosition = posicionOriginalEnSlot;
        rectTransform.localScale = escalaPequeña;
        Debug.Log("<color=green>Insignia Guardada!</color>");
    }
}