using Mundo2;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public CanvasGroup groupChecklist;
    public CanvasGroup groupIconoMochila;
    public GameObject panelParcelas;
    public AndyController andy;

    [Header("Botones y Prefabs")]
    public Button btnTrigo; public Button btnPapa; public Button btnCalabaza;
    public GameObject prefabTrigo; public GameObject prefabPapa; public GameObject prefabCalabaza;

    [Header("Checklist")]
    public TextMeshProUGUI[] itemsChecklist;

    [Header("Configuración Lupi")]
    public Transform playerLupi;
    public float distanciaParaEncajar = 3.5f;

    private string semillaActiva = "";
    private Color colorVerdeMilitar; 

    void Awake()
    {
        ColorUtility.TryParseHtmlString("#028A0F", out colorVerdeMilitar);

        ConfigurarUI(0, false);
        if (panelParcelas != null) panelParcelas.SetActive(false);
    }

    void Update()
    {
        AplicarPalpitoSemilla();
    }

    // --- CORRECCIÓN AQUÍ: MAYÚSCULA AL INICIO Y [OK] AL FINAL ---
    public void MarcarTareaCompletada(int indice)
    {
        if (indice >= 0 && indice < itemsChecklist.Length)
        {
            string textoBase = itemsChecklist[indice].text;

            // Verificamos que no tenga ya el [OK]
            if (!textoBase.ToLower().Contains("[OK]"))
            {
                // 1. Convertimos todo a minúsculas primero
                string todoMinuscula = textoBase.ToLower();
                
                // 2. Tomamos la primera letra, la hacemos Mayúscula y sumamos el resto
                string textoFormateado = char.ToUpper(todoMinuscula[0]) + todoMinuscula.Substring(1);

                // 3. Añadimos el [OK] y el color verde militar
                itemsChecklist[indice].text = textoFormateado + " [OK]";
                itemsChecklist[indice].color = colorVerdeMilitar;
            }
        }
    }

    public IEnumerator AparecerSuave(CanvasGroup grupo)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            grupo.alpha = t;
            yield return null;
        }
        grupo.alpha = 1;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private void AplicarPalpitoSemilla()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;
        btnTrigo.transform.localScale = (semillaActiva == "Trigo" && btnTrigo.interactable) ? new Vector3(pulse, pulse, 1) : Vector3.one;
        btnPapa.transform.localScale = (semillaActiva == "Papa" && btnPapa.interactable) ? new Vector3(pulse, pulse, 1) : Vector3.one;
        btnCalabaza.transform.localScale = (semillaActiva == "Calabaza" && btnCalabaza.interactable) ? new Vector3(pulse, pulse, 1) : Vector3.one;
    }

    public void AbrirCerrarMenuParcelas()
    {
        if (panelParcelas != null) panelParcelas.SetActive(!panelParcelas.activeSelf);
    }

    public void SetSemillaPalpitar(string tipo) => semillaActiva = tipo;

    public void IntentarSembrar(string tipo)
    {
        if (tipo != semillaActiva) { andy.Decir("¡Usa el " + semillaActiva + " primero!"); return; }

        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas)
        {
            if (z.tipoDeSemillaPermitida == tipo && !z.estaOcupada)
            {
                if (Vector3.Distance(playerLupi.position, z.transform.position) <= distanciaParaEncajar)
                {
                    Instantiate(ObtenerPrefab(tipo), z.transform.position, Quaternion.identity);
                    z.estaOcupada = true;
                    z.DesactivarColision();
                    DesactivarBoton(tipo);
                    LogicaNivel2.instancia.AvanceSiembraExitosa();
                }
                else andy.Decir("Acércate a la parcela de " + tipo);
                return;
            }
        }
    }

    private GameObject ObtenerPrefab(string t) => t == "Trigo" ? prefabTrigo : t == "Papa" ? prefabPapa : prefabCalabaza;

    private void DesactivarBoton(string t)
    {
        if (t == "Trigo") { btnTrigo.interactable = false; btnTrigo.transform.localScale = Vector3.one; }
        else if (t == "Papa") { btnPapa.interactable = false; btnPapa.transform.localScale = Vector3.one; }
        else { btnCalabaza.interactable = false; btnCalabaza.transform.localScale = Vector3.one; }
    }

    public void ConfigurarUI(float alpha, bool interact)
    {
        if (groupChecklist) { groupChecklist.alpha = alpha; groupChecklist.interactable = interact; groupChecklist.blocksRaycasts = interact; }
        if (groupIconoMochila) { groupIconoMochila.alpha = alpha; groupIconoMochila.interactable = interact; groupIconoMochila.blocksRaycasts = interact; }
    }

    public void MostrarInterfazNivel2Snappy() => ConfigurarUI(1, true);
    public void OcultarInterfazNivel2Snappy()
    {
        ConfigurarUI(0, false);
        if (panelParcelas != null) panelParcelas.SetActive(false);
    }

    public void SembrarTrigo() => IntentarSembrar("Trigo");
    public void SembrarPapa() => IntentarSembrar("Papa");
    public void SembrarCalabaza() => IntentarSembrar("Calabaza");
}