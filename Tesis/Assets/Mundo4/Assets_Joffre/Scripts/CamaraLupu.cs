using UnityEngine;

public class CamaraLupu : MonoBehaviour
{
    public Transform objetivo; // Aquí arrastraremos a Lupu

    // El "desfase" es la distancia que mantiene la cámara (sobre todo en Z para no atravesar el mundo)
    public Vector3 desfase = new Vector3(0, 0, -10);

    // Usamos LateUpdate para que la cámara se mueva DESPUÉS de que Lupu haya terminado de moverse
    // Esto evita temblores molestos.
    void LateUpdate()
    {
        if (objetivo != null)
        {
            // Solo copiamos la posición de Lupu + la distancia de seguridad
            transform.position = objetivo.position + desfase;
        }
    }
}