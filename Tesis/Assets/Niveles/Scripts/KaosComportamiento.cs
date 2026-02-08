using UnityEngine;

public class KaosComportamiento : MonoBehaviour
{
    public Transform[] puntosPatrulla;
    public float velocidad = 3f;
    private int indiceSiguiente = 0;

    // --- NUEVA LÓGICA DE ENCOGIMIENTO ---
    [Header("Evolución del Villano")]
    public float factorReduccion = 0.7f; // Se encoge al 70% de su tamaño actual
    public float tamanoMinimo = 0.2f;    // No queremos que desaparezca del todo aún

    public void ReducirTamano()
    {
        if (transform.localScale.x > tamanoMinimo)
        {
            transform.localScale *= factorReduccion;
            Debug.Log("<color=purple>EL ENEMIGO SE DEBILITA.</color>");
        }
    }

    void Update()
    {
        // Si el nivel terminó, el Kaos se queda congelado o desaparece
        if (SierraGameManager.instancia != null && SierraGameManager.instancia.nivelCompletado)
        {
            // Opcional: hacerlo un poquito transparente para que parezca que se desvanece
            return;
        }

        if (puntosPatrulla.Length > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, puntosPatrulla[indiceSiguiente].position, velocidad * Time.deltaTime);
            if (Vector2.Distance(transform.position, puntosPatrulla[indiceSiguiente].position) < 0.2f)
            {
                indiceSiguiente = (indiceSiguiente + 1) % puntosPatrulla.Length;
            }
        }
    }
}