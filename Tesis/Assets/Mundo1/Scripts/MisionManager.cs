using UnityEngine;

[System.Serializable]
public class MisionArreglo
{
    public string instruccion;
    public string nombreItemRequerido;
    public int indiceObjetivo;
    public bool permiteAvanceConError; // <--- NUEVO: Checkbox para misiones 1 y 2
    [TextArea] public string explicacionExito;
    [TextArea] public string explicacionError;
}

public class MisionManager : MonoBehaviour
{
    public MisionArreglo[] misiones;
    private int misionActual = 0;
    private bool esperandoQueAndyTermine = false;
    private bool modoLibre = false;
    private string instruccionPermanente;

    private AndyInteraccion andy;
    private PuntajeManager puntaje;

    void Start()
    {
        andy = Object.FindFirstObjectByType<AndyInteraccion>();
        puntaje = Object.FindFirstObjectByType<PuntajeManager>();
    }

    public void IniciarMisiones()
    {
        Object.FindFirstObjectByType<AndyFollow>().debeSeguir = false;
        ActualizarInstruccionAndy();
    }

    public void ActualizarInstruccionAndy()
    {
        if (modoLibre) return;

        if (misionActual < misiones.Length)
        {
            instruccionPermanente = misiones[misionActual].instruccion;
            andy.CambiarMensaje(instruccionPermanente);
            andy.Hablar();
        }
        else
        {
            EntrarEnModoLibre();
        }
        esperandoQueAndyTermine = false;
    }

    public void ValidarAccion(int indiceAccion, string nombreItem)
    {
        if (modoLibre || esperandoQueAndyTermine || misionActual >= misiones.Length) return;

        MisionArreglo m = misiones[misionActual];

        // 1. Verificar si el item es el correcto
        if (nombreItem != m.nombreItemRequerido)
        {
            // Evitamos que Andy se queje si estamos sacando algo para corregir un error
            if (indiceAccion != -1)
            {
                EnviarFeedbackTemporal($"Ese no es el objeto. Necesito la {m.nombreItemRequerido}.");
            }
            return;
        }

        // 2. Validar según el objetivo
        if (indiceAccion == m.indiceObjetivo)
        {
            // ACIERTO TOTAL
            AvanzarMision(false);
        }
        else
        {
            // EL JUGADOR FALLÓ EL ÍNDICE
            if (m.permiteAvanceConError)
            {
                // Misiones 1 y 2: Falló pero avanzamos porque el ArrayManager lo arregló
                puntaje.ModificarPuntos(-50, false);
                AvanzarMision(true);
            }
            else
            {
                // Misiones 3 y 4: NO avanzamos. Andy explica el error y hay que reintentar.
                // IMPORTANTE: Solo restamos puntos si intentó INSERTAR en el lugar mal. 
                // Si solo lo sacó (indiceAccion -1), no castigamos.
                if (indiceAccion != -1)
                {
                    puntaje.ModificarPuntos(-25, false);
                    EnviarFeedbackTemporal(m.explicacionError);
                }
            }
        }
    }

    void EnviarFeedbackTemporal(string mensaje)
    {
        andy.CambiarMensaje(mensaje);
        andy.Hablar();
    }

    void ResetearMensajeAndy()
    {
        andy.CambiarMensaje(instruccionPermanente);
    }

    void AvanzarMision(bool fueConError)
    {
        esperandoQueAndyTermine = true;
        if (!fueConError) puntaje.ModificarPuntos(100, true);

        string feedback = fueConError ? misiones[misionActual].explicacionError : misiones[misionActual].explicacionExito;
        andy.CambiarMensaje(feedback);
        andy.Hablar();

        misionActual++;

    }


    // Añade esto a las variables del MisionManager:
    public bool tutorialTerminado = false;
    public GameObject panelSiNo; // Arrastra el "Panel_Pregunta" aquí
                                 // Si el jugador cierra el diálogo de éxito/error antes del Invoke, forzamos la siguiente instrucción
    public void NotificarDialogoCerrado()
    {
        if (esperandoQueAndyTermine)
        {
            esperandoQueAndyTermine = false;
            ActualizarInstruccionAndy();
        }
    }

    // Modifica EntrarEnModoLibre:
    void EntrarEnModoLibre()
    {
        modoLibre = true;
        tutorialTerminado = true;
        instruccionPermanente = "¡Bien hecho, Lupe! Ya dominas los arreglos. Practica libremente insertando o eliminando cargamentos. ¡Háblame de nuevo cuando estés listo para avanzar de nivel!";
        andy.CambiarMensaje(instruccionPermanente);
        andy.Hablar();
    }

    // Nueva función para cuando el jugador habla con Andy al final
    public void PreguntarCambioNivel()
    {
        andy.CambiarMensaje("¿Quieres terminar el entrenamiento y ver tus resultados?");
        andy.Hablar();
        panelSiNo.SetActive(true); // Aparecen los botones Sí/No
    }
}