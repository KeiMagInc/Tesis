using UnityEngine;

public class EfectoLetrero : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float amplitud = 0.1f;

    private Vector3 escalaOriginal;
    private bool encendido = false;

    void Awake()
    {
        // Usamos Awake para capturar la escala antes de que cualquier lógica OnEnable la altere
        escalaOriginal = transform.localScale;
    }

    void OnDisable()
    {
        // SEGURIDAD: Al desactivar el objeto o el nivel, 
        // forzamos el estado apagado y restauramos la escala.
        encendido = false;
        transform.localScale = escalaOriginal;
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
            // Vuelve al tamaño normal suavemente si está apagado
            if (transform.localScale != escalaOriginal)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, escalaOriginal, Time.deltaTime * 10f);

                // Si ya está muy cerca de la escala original, la fijamos para dejar de procesar el Lerp
                if (Vector3.Distance(transform.localScale, escalaOriginal) < 0.001f)
                    transform.localScale = escalaOriginal;
            }
        }
    }

    public void SetEncendido(bool estado)
    {
        encendido = estado;
        // Si lo apagamos manualmente, nos aseguramos de que empiece a volver a su tamaño
    }
}