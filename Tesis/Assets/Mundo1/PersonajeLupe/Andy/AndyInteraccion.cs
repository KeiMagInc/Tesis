using UnityEngine;

public class AndyInteraccion : MonoBehaviour
{
    [TextArea(3, 5)]
    public string mensajeActual = "¡Lupe, el Kaos atacó los muelles! ¡Debemos ir a ayudar!";

    public void Hablar()
    {
        // Llamamos al manager con el mensaje que tenga en ese momento
        Object.FindFirstObjectByType<DialogoManager>().ManejarDialogo(mensajeActual);
    }

    // Esta función la llamaremos desde los Triggers para cambiar lo que Andy dice
    public void CambiarMensaje(string nuevoMensaje)
    {
        mensajeActual = nuevoMensaje;
    }
}