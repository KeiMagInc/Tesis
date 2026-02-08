using UnityEngine;

public class AndyFollow : MonoBehaviour
{
    [Header("Configuración")]
    public Transform objetivo; // Lupe
    public float distanciaMinima = 1.2f;
    public float tiempoDeSuavizado = 0.2f; // Cuanto menor sea, más rápido alcanza a Lupe

    private Vector3 velocidadActual = Vector3.zero; // Referencia interna para SmoothDamp
    private SpriteRenderer spriteRenderer;
    // Añade esto arriba en AndyFollow
    public bool debeSeguir = true;
    public Transform puntoFijoMuelle; // Crea un objeto vacío en el muelle y arrástralo aquí
    public float tiempoSuavizadoMuelle = 1.5f; // Mayor valor = Más lento
    public float velocidadMaximaMuelle = 5f; // Límite de velocidad para el vuelo

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Usamos LateUpdate para que Andy se mueva DESPUÉS de que Lupe haya terminado su movimiento
    // Esto es clave para eliminar el temblor
    void LateUpdate()
    {
        if (objetivo == null) return;


        if (debeSeguir)
        {
            // 1. Calcular la distancia actual
            float distanciaActual = Vector2.Distance(transform.position, objetivo.position);

            // 2. Si Andy está lejos, calculamos la posición de destino
            if (distanciaActual > distanciaMinima)
            {
                // Calculamos un punto que esté un poco "detrás" de Lupe para que no se encimen
                Vector3 direccion = (transform.position - objetivo.position).normalized;
                Vector3 puntoDestino = objetivo.position + (direccion * distanciaMinima);

                // 3. SmoothDamp hace la magia: mueve a Andy de forma ultra suave
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    puntoDestino,
                    ref velocidadActual,
                    tiempoDeSuavizado
                );

                // 4. Orientación del Sprite
                ActualizarOrientacion(objetivo.position);
            }
            
        }
        else if (puntoFijoMuelle != null)
        {
            // Andy se mueve suavemente a su puesto en el muelle
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                puntoFijoMuelle.position, 
                ref velocidadActual, 
                tiempoSuavizadoMuelle,
                velocidadMaximaMuelle // Agregamos este parámetro para que no "dispare"
            );
            spriteRenderer.flipX = (objetivo.position.x < transform.position.x); // Mirar a Lupe
        }
    }

    void ActualizarOrientacion(Vector3 destino)
    {
        // Solo girar si el movimiento lateral es notable para evitar parpadeos
        if (Mathf.Abs(destino.x - transform.position.x) > 0.05f)
        {
            spriteRenderer.flipX = (destino.x < transform.position.x);
        }
    }
}