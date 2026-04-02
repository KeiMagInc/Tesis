using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject iconoMochila;
    public GameObject panelParcelas;
    public GameObject panelChecklist;
    public AndyController andy;

    [Header("Botones")]
    public Button btnTrigo; public Button btnPapas; public Button btnZanahorias;
    [Header("Prefabs de Siembra")]
    public GameObject prefabTrigo; public GameObject prefabPapas; public GameObject prefabZanahorias;

    [Header("Checklist")]
    public TextMeshProUGUI[] itemsChecklist;

    [Header("Configuración de Lupi")]
    public Transform playerLupi;
    public float distanciaParaEncajar = 3.5f;

    private string[] ordenCorrecto = { "Trigo", "Papas", "Zanahoria" };
    private int indiceProgreso = 0;
    private CanvasGroup canvasChecklist;

    void Awake()
    {
        canvasChecklist = panelChecklist.GetComponent<CanvasGroup>();
        iconoMochila.SetActive(false);
        panelParcelas.SetActive(false);
    }

    public IEnumerator MostrarChecklist()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            if (canvasChecklist != null) canvasChecklist.alpha = t;
            yield return null;
        }
    }

    public void MostrarIconoMochila() => iconoMochila.SetActive(true);
    public void AbrirCerrarMenuParcelas() => panelParcelas.SetActive(!panelParcelas.activeSelf);

    public void SembrarTrigo() => IntentarEncajarPieza("Trigo", prefabTrigo, btnTrigo);
    public void SembrarPapas() => IntentarEncajarPieza("Papas", prefabPapas, btnPapas);
    public void SembrarZanahorias() => IntentarEncajarPieza("Zanahoria", prefabZanahorias, btnZanahorias);

    private void IntentarEncajarPieza(string tipoBuscado, GameObject prefab, Button boton)
    {
        if (ordenCorrecto[indiceProgreso] != tipoBuscado)
        {
            andy.Decir("¡Ese no es el orden! Revisa el checklist. Toca sembrar " + ordenCorrecto[indiceProgreso]);
            return;
        }

        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        ZonaPlantado zonaCorrecta = null;

        foreach (ZonaPlantado z in zonas)
        {
            if (z.tipoDeSemillaPermitida == tipoBuscado) { zonaCorrecta = z; break; }
        }

        if (zonaCorrecta == null || zonaCorrecta.estaOcupada) return;

        float dist = Vector3.Distance(playerLupi.position, zonaCorrecta.transform.position);

        if (dist <= distanciaParaEncajar)
        {
            // ENCAJE PERFECTO: Se instancia exactamente en la posición de la zona
            Instantiate(prefab, zonaCorrecta.transform.position, Quaternion.identity);
            zonaCorrecta.estaOcupada = true;
            boton.interactable = false;

            // Marcar en checklist (esto llama a tu lógica de progreso)
            MarcarTareaCompletada(indiceProgreso);
            indiceProgreso++;

            if (indiceProgreso < ordenCorrecto.Length)
                andy.Decir("¡Muy bien! Ahora busca el sector de " + ordenCorrecto[indiceProgreso]);
            else
                andy.Decir("¡Excelente! Has terminado la siembra. Ahora podemos conectar el agua.");

            // LA MOCHILA NO SE CIERRA SOLA (Requerimiento 2)
        }
        else
        {
            andy.Decir("Estás muy lejos del sector de " + tipoBuscado);
        }
    }

    public void MarcarTareaCompletada(int indice)
    {
        if (indice < itemsChecklist.Length)
        {
            // Creamos una variable para guardar el color
            Color miColor;

            // Convertimos el código Hexadecimal a un Color de Unity
            if (ColorUtility.TryParseHtmlString("#0f5199", out miColor))
            {
                itemsChecklist[indice].color = miColor;
            }
        }
    }
}