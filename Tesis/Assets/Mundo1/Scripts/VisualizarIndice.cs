using UnityEngine;
using TMPro;

public class VisualizarIndice : MonoBehaviour
{
    private BalsaSlot balsa;
    private TextMeshPro textoTMP;

    void Start()
    {
        // Buscamos el script de la balsa y el componente de texto en el hijo
        balsa = GetComponent<BalsaSlot>();
        textoTMP = GetComponentInChildren<TextMeshPro>();

        if (balsa != null && textoTMP != null)
        {
            // Escribimos el número del índice en el texto
            textoTMP.text = balsa.indice.ToString();
        }
    }
}