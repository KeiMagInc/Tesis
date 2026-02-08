using UnityEngine;

public class LupuInteraccion : MonoBehaviour
{
    [Header("Configuración")]
    public float radioDeteccion = 1.0f; // Qué tan cerca debe estar para tocarla
    public LayerMask capaBurbujas;      // Para que solo detecte burbujas y no paredes

    [Header("Estado Actual")]
    public EsferaLogica esferaSeleccionada; // La burbuja que estamos tocando ahora
    private EsferaLogica esferaGuardada;    // La primera burbuja que seleccionamos (para el intercambio)

    void Update()
    {
        // Si el diálogo está activo, Lupu se queda "congelado" y no detecta nada
        if (SistemaDialogo.Instance.dialogoActivo || SortGameManager.Instance.estaIntercambiando) return;

        DetectarEsferaCercana();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            ProcesarSeleccion();
        }
    }

    void DetectarEsferaCercana()
    {
        // Creamos un círculo invisible alrededor de Lupu para ver qué toca
        Collider2D colision = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaBurbujas);

        if (colision != null)
        {
            // ¡Encontramos una burbuja!
            EsferaLogica esferaEncontrada = colision.GetComponent<EsferaLogica>();

            if (esferaEncontrada != esferaSeleccionada)
            {
                // Si es una nueva, desmarcamos la anterior (si había)
                if (esferaSeleccionada != null && esferaSeleccionada != esferaGuardada)
                    esferaSeleccionada.Resaltar(false);

                // Marcamos la nueva
                esferaSeleccionada = esferaEncontrada;
                esferaSeleccionada.Resaltar(true);
            }
        }
        else
        {
            // No hay nada cerca, limpiamos selección (menos la guardada)
            if (esferaSeleccionada != null && esferaSeleccionada != esferaGuardada)
            {
                esferaSeleccionada.Resaltar(false);
                esferaSeleccionada = null;
            }
        }
    }

    void ProcesarSeleccion()
    {
        if (esferaSeleccionada == null) return;

        if (esferaGuardada == null)
        {
            // PRIMER CLIC: Guardar
            esferaGuardada = esferaSeleccionada;
            esferaGuardada.Resaltar(true); // O cambiar a color verde
        }
        else
        {
            // SEGUNDO CLIC: Ya tenemos dos esferas.
            if (esferaSeleccionada != esferaGuardada)
            {
                // --- AQUÍ ESTÁ EL CAMBIO IMPORTANTE ---
                // En lugar de hacer la lógica aquí, llamamos al Manager
                SortGameManager.Instance.IntentarMovimiento(esferaGuardada, esferaSeleccionada);

                // Reiniciamos la selección visual
                esferaGuardada.Resaltar(false);
                esferaSeleccionada.Resaltar(false);
                esferaGuardada = null;
                esferaSeleccionada = null;
            }
            else
            {
                // Cancelar si clicas la misma
                esferaGuardada.Resaltar(false);
                esferaGuardada = null;
            }
        }
    }

    void IntercambiarEsferas(EsferaLogica a, EsferaLogica b)
    {
        Debug.Log("Intercambiando " + a.valor + " con " + b.valor);

        // 1. Intercambio LÓGICO (Valores)
        int tempValor = a.valor;
        a.AsignarValor(b.valor);
        b.AsignarValor(tempValor);

        // 2. Intercambio VISUAL (Animación o Posición)
        // Por ahora solo cambiamos los números, luego podemos hacer que se muevan físicamente
    }

    // Esto dibuja el círculo rojo en el editor para que veas el rango
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}