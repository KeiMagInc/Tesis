using UnityEngine;
using TMPro;
using System.Collections;

public class DialogoManager : MonoBehaviour
{
    public TextMeshProUGUI textoUI;
    public CanvasGroup panelDialogo;
    public float velocidadEscritura = 0.03f;

    private bool escribiendo = false;
    private string mensajeActual;
    private Coroutine corrutinaEscritura;

    // Esta variable es la que Lupi lee para quedarse quieto
    public bool hayDialogoActivo => panelDialogo != null && panelDialogo.alpha > 0;

    public void ManejarDialogo(string mensaje)
    {
        if (panelDialogo.alpha == 0)
        {
            AbrirPanel(mensaje);
        }
        else if (escribiendo)
        {
            CompletarTexto();
        }
        else
        {
            CerrarDialogo();
        }
    }

    private void AbrirPanel(string mensaje)
    {
        mensajeActual = mensaje;
        panelDialogo.alpha = 1;
        panelDialogo.blocksRaycasts = true;

        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        corrutinaEscritura = StartCoroutine(EscribirMensaje(mensaje));
    }

    IEnumerator EscribirMensaje(string mensaje)
    {
        escribiendo = true;
        textoUI.text = mensaje;
        textoUI.maxVisibleCharacters = 0;

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

        // --- LIMPIEZA: Se eliminó la referencia a MisionManager que daba error ---
        Debug.Log("Diálogo cerrado.");
    }
}