using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LupiInteraccion : MonoBehaviour
{
    [Header("Estado de la Lista")]
    public NodoTambo nodoOrigen;
    public NodoTambo nodoCercano;
    public NodoTambo backupSiguiente; // Para restaurar si falla la lógica
    public bool cargandoPuntero = false;

    [Header("Efectos")]
    public Light2D luzGlow;

    void Start()
    {
        if (luzGlow != null) luzGlow.enabled = false;
    }

    void Update()
    {
        // Bloqueo 1: Por tutorial inicial
        if (SierraGameManager.instancia.enTutorialInicial) return;

        // Bloqueo 2: Por nivel ya completado
        if (SierraGameManager.instancia.nivelCompletado) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // FASE DE RECOGIDA
            if (!cargandoPuntero && nodoCercano != null)
            {
                // Si el nodo ya está bien puesto, Andy avisa y no permitimos moverlo
                if (nodoCercano.bloqueado)
                {
                    SierraGameManager.instancia.AndyDice("Lupi, ese Tambo ya está en su lugar correcto. ¡No lo muevas!");
                    return;
                }

                nodoOrigen = nodoCercano;
                backupSiguiente = nodoOrigen.siguienteNodo;
                nodoOrigen.siguienteNodo = null; // Desenganche visual

                cargandoPuntero = true;
                if (luzGlow != null) luzGlow.enabled = true;
                Debug.Log("Puntero recogido de: " + nodoOrigen.name);
            }
            // FASE DE CONEXIÓN
            else if (cargandoPuntero && nodoCercano != null && nodoCercano != nodoOrigen)
            {
                bool esCorrecto = SierraGameManager.instancia.ValidarPaso(nodoOrigen, nodoCercano);

                if (esCorrecto)
                {
                    nodoOrigen.siguienteNodo = nodoCercano;
                }
                else
                {
                    // Si falló, restauramos el hilo de luz a su posición anterior
                    nodoOrigen.siguienteNodo = backupSiguiente;
                }

                FinalizarInteraccion();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Nodo")) nodoCercano = col.GetComponent<NodoTambo>();
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Nodo")) nodoCercano = null;
    }

    void FinalizarInteraccion()
    {
        cargandoPuntero = false;
        nodoOrigen = null;
        backupSiguiente = null;
        if (luzGlow != null) luzGlow.enabled = false;
    }

    public void RecibirDanioKaos()
    {
        if (cargandoPuntero)
        {
            nodoOrigen.siguienteNodo = backupSiguiente;
            FinalizarInteraccion();
            SierraGameManager.instancia.AndyDice("¡Oh no! El Kaos te tocó y perdiste la conexión.");
        }
    }
}