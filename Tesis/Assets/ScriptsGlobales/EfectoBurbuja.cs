using UnityEngine;
using TMPro;
public class EfectoBurbuja : MonoBehaviour
{
    public TextMeshProUGUI textoPuntos;
    public float tiempoDeVida = 3f;
    public float velocidadSubida = 0.5f;
    public void Configurar(int puntos)
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = "+" + puntos;
            RectTransform rect = textoPuntos.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
        }
        Destroy(gameObject, tiempoDeVida);
    }
    void Update()
    {
        transform.position += Vector3.up * velocidadSubida * Time.deltaTime;
    }
}