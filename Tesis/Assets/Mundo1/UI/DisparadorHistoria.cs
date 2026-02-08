using UnityEngine;

public class DisparadorHistoria : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [TextArea(3, 5)]
    public string mensajeParaAndy; // El mensaje que Andy dirá si le hablas después
    public bool hablarAutomaticamente = true; // ¿Andy habla apenas tocas el trigger?

    [Header("Control de un solo uso")]
    private bool yaSeActivo = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos que sea Lupe quien entra y que no se haya activado antes
        if (other.CompareTag("Player") && !yaSeActivo)
        {
            AndyInteraccion andy = Object.FindFirstObjectByType<AndyInteraccion>();
            
            if (andy != null)
            {
                // 1. Actualizamos el mensaje de Andy para el futuro
                andy.CambiarMensaje(mensajeParaAndy);

                // 2. Si queremos que hable de inmediato (ej: al entrar al bosque)
                if (hablarAutomaticamente)
                {
                    andy.Hablar();
                }
            }

            yaSeActivo = true; // Evita que el dialogo salga cada vez que pases por ahí
            // Opcional: gameObject.SetActive(false); // Si quieres borrar el trigger
        }
    }
}