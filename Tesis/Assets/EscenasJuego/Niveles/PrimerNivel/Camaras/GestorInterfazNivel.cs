using UnityEngine;
using TMPro;
using System.Collections;
public class GestorInterfazNivel : MonoBehaviour
{
    public TextMeshProUGUI textoNivel;
    public CanvasGroup panelGrupo;
    private void Start()
    {
        panelGrupo.alpha = 0;
    }
    public void MostrarNombre(string nombre)
    {
        StopAllCoroutines();
        StartCoroutine(SecuenciaMostrar(nombre));
    }
    IEnumerator SecuenciaMostrar(string nombre)
    {
        textoNivel.text = nombre;
        while (panelGrupo.alpha < 1)
        {
            panelGrupo.alpha += Time.deltaTime * 2;
            yield return null;
        }
        panelGrupo.alpha = 1;
    }
}