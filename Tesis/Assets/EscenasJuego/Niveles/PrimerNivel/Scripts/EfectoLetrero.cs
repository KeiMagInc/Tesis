using UnityEngine;

public class EfectoLetrero : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float amplitud = 0.1f;

    private Vector3 escalaOriginal;
    private bool encendido = false;

    void Start()
    {
        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        if (encendido)
        {
            // Hace que el objeto palpite usando una onda Seno
            float escala = 1f + Mathf.Sin(Time.time * velocidad) * amplitud;
            transform.localScale = escalaOriginal * escala;
        }
        else
        {
            // Vuelve al tamaño normal si está apagado
            transform.localScale = Vector3.Lerp(transform.localScale, escalaOriginal, Time.deltaTime * 10f);
        }
    }

    public void SetEncendido(bool estado)
    {
        encendido = estado;
    }
}