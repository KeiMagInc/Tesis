using UnityEngine;
using TMPro;
using System.Collections;

public class GestorInterfazNivel : MonoBehaviour
{
    public TextMeshProUGUI textoNivel;
    public CanvasGroup panelGrupo;

    private void Start()
    {
        panelGrupo.alpha = 0; // Empieza invisible hasta que entres a la primera zona
    }

    public void MostrarNombre(string nombre)
    {
        StopAllCoroutines();
        StartCoroutine(SecuenciaMostrar(nombre));
    }

    IEnumerator SecuenciaMostrar(string nombre)
    {
        textoNivel.text = nombre;

        // Solo hacemos el efecto de "Aparecer" (Fade In)
        while (panelGrupo.alpha < 1)
        {
            panelGrupo.alpha += Time.deltaTime * 2;
            yield return null;
        }

        // Nos aseguramos que sea 1 y el código termina aquí.
        // Al no haber más instrucciones, el letrero se quedará visible para siempre
        // o hasta que cambies de zona y el texto se actualice.
        panelGrupo.alpha = 1;
    }
}