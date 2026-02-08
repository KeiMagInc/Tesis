using UnityEngine;
using TMPro;
using System.Collections;

public class DialogoManager : MonoBehaviour
{
    public TextMeshProUGUI textoUI;
    public CanvasGroup panelDialogo;
    public float velocidadEscritura = 0.03f; // Un poco más rápido se siente mejor

    private bool escribiendo = false;
    private string mensajeActual;
    private Coroutine corrutinaEscritura;

    public bool hayDialogoActivo => panelDialogo.alpha > 0;

    public void ManejarDialogo(string mensaje)
    {
        // Caso 1: El panel está cerrado -> Abrir y empezar a escribir
        if (panelDialogo.alpha == 0)
        {
            AbrirPanel(mensaje);
        }
        // Caso 2: Se está escribiendo -> Completar el texto de golpe
        else if (escribiendo)
        {
            CompletarTexto();
        }
        // Caso 3: El texto ya está completo -> Cerrar
        else
        {
            CerrarDialogo();
        }
    }

    private void AbrirPanel(string mensaje)
    {
        mensajeActual = mensaje;
        panelDialogo.alpha = 1;
        panelDialogo.blocksRaycasts = true; // Evita clicks accidentales a través del panel
        
        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        corrutinaEscritura = StartCoroutine(EscribirMensaje(mensaje));
    }

    IEnumerator EscribirMensaje(string mensaje)
    {
        escribiendo = true;
        textoUI.text = mensaje;
        textoUI.maxVisibleCharacters = 0; // Ocultamos todos los caracteres

        // Vamos mostrando caracteres uno a uno
        for (int i = 0; i <= mensaje.Length; i++)
        {
            textoUI.maxVisibleCharacters = i;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        escribiendo = false;
    }

    private void CompletarTexto()
    {
        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        textoUI.maxVisibleCharacters = mensajeActual.Length;
        escribiendo = false;
    }

    public void CerrarDialogo()
    {
        panelDialogo.alpha = 0;
        panelDialogo.blocksRaycasts = false;
        textoUI.text = "";
        
        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        
        // Avisar al MisionManager de forma segura
        MisionManager mm = Object.FindFirstObjectByType<MisionManager>();
        if (mm != null) mm.NotificarDialogoCerrado();
    }
}