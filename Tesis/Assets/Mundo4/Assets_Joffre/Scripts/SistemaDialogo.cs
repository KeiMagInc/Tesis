using UnityEngine;
using TMPro;
using System.Collections.Generic; // Necesario para Queue
using System; // Necesario para Action

public class SistemaDialogo : MonoBehaviour
{
    public static SistemaDialogo Instance;

    [Header("Referencias UI")]
    public GameObject cajaDialogo;
    public TextMeshProUGUI textoMensaje;

    [Header("Estado")]
    public bool dialogoActivo = false;

    // Cola para guardar los mensajes secuenciales
    private Queue<string> colaMensajes = new Queue<string>();

    // Acción que se ejecuta al terminar toda la conversación (opcional)
    private Action alTerminarDialogo;

    void Awake()
    {
        if (Instance == null) Instance = this;
        OcultarDialogo();
    }

    void Update()
    {
        if (dialogoActivo && Input.GetKeyDown(KeyCode.E))
        {
            MostrarSiguienteMensaje();
        }
    }

    // --- NUEVA FUNCIÓN: Recibe una lista de frases ---
    public void MostrarSecuencia(string[] mensajes, Action alTerminar = null)
    {
        colaMensajes.Clear(); // Limpiamos charlas anteriores
        alTerminarDialogo = alTerminar; // Guardamos qué hacer al finalizar

        foreach (string msj in mensajes)
        {
            colaMensajes.Enqueue(msj);
        }

        cajaDialogo.SetActive(true);
        dialogoActivo = true;
        MostrarSiguienteMensaje();
    }

    // Sobrecarga para un solo mensaje (como el de los errores)
    public void MostrarMensaje(string mensaje)
    {
        string[] arrayUnico = { mensaje };
        MostrarSecuencia(arrayUnico);
    }

    void MostrarSiguienteMensaje()
    {
        if (colaMensajes.Count == 0)
        {
            // Ya no hay más texto, cerramos
            OcultarDialogo();

            // Si había una acción programada (ej. mostrar HUD), la ejecutamos
            if (alTerminarDialogo != null)
            {
                alTerminarDialogo.Invoke();
                alTerminarDialogo = null;
            }
            return;
        }

        string frase = colaMensajes.Dequeue();
        textoMensaje.text = frase + "\n\n<size=60%><color=yellow>(Presiona 'E')</color></size>";
    }

    public void OcultarDialogo()
    {
        cajaDialogo.SetActive(false);
        dialogoActivo = false;
    }
}