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
    public float distanciaParaEncajar = 5.0f;

    private string semillaActiva = "";
    private Color colorVerdeMilitar;

    void Awake()
    {
        ColorUtility.TryParseHtmlString("#028A0F", out colorVerdeMilitar);
        ConfigurarUI(0, false);
        if (panelParcelas != null) panelParcelas.SetActive(false);
    }

    void Update() { AplicarPalpitoSemilla(); }

    public void MarcarTareaCompletada(int indice)
    {
        if (indice >= 0 && indice < itemsChecklist.Length)
        {
            string textoBase = itemsChecklist[indice].text.Trim().ToLower().Replace(" [ok]", "");
            if (string.IsNullOrEmpty(textoBase)) return;
            string textoFormateado = char.ToUpper(textoBase[0]) + textoBase.Substring(1) + " [ok]";
            itemsChecklist[indice].text = textoFormateado;
            itemsChecklist[indice].color = colorVerdeMilitar;
        }
    }

    public void IntentarSembrar(string tipo)
    {
        if (tipo != semillaActiva) { andy.Decir("¡Usa el " + semillaActiva + " primero!"); return; }

        // Buscamos todas las zonas
        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        ZonaPlantado zonaMasCercana = null;
        float menorDistancia = float.MaxValue;

        foreach (var z in zonas)
        {
            // Solo consideramos zonas del tipo correcto que no estén ocupadas
            if (z.tipoDeSemillaPermitida == tipo && !z.estaOcupada)
            {
                // Calculamos distancia solo en X e Y (ignora Z)
                float dist = Vector2.Distance(playerLupi.position, z.transform.position);
                if (dist < menorDistancia)
                {
                    menorDistancia = dist;
                    zonaMasCercana = z;
                }
            }
        }

        // Si encontramos la zona y está dentro del rango
        if (zonaMasCercana != null)
        {
            if (menorDistancia <= distanciaParaEncajar)
            {
                Instantiate(ObtenerPrefab(tipo), zonaMasCercana.transform.position, Quaternion.identity);
                zonaMasCercana.estaOcupada = true;
                zonaMasCercana.DesactivarColision();
                DesactivarBoton(tipo);
                if (LogicaNivel2.instancia != null) LogicaNivel2.instancia.AvanceSiembraExitosa();
            }
            else
            {
                andy.Decir("Acércate más a la parcela de " + tipo);
            }
        }
    }

    private void AplicarPalpitoSemilla()
    {
        if (btnTrigo == null || btnPapa == null || btnCalabaza == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;
        btnTrigo.transform.localScale = (semillaActiva == "Trigo" && btnTrigo.interactable) ? new Vector3(pulse, pulse, 1) : Vector3.one;
        btnPapa.transform.localScale = (semillaActiva == "Papa" && btnPapa.interactable) ? new Vector3(pulse, pulse, 1) : Vector3.one;
        btnCalabaza.transform.localScale = (semillaActiva == "Calabaza" && btnCalabaza.interactable) ? new Vector3(pulse, pulse, 1) : Vector3.one;
    }

    public void AbrirCerrarMenuParcelas() => panelParcelas.SetActive(!panelParcelas.activeSelf);
    public void SetSemillaPalpitar(string tipo) => semillaActiva = tipo;
    private GameObject ObtenerPrefab(string t) => t == "Trigo" ? prefabTrigo : t == "Papa" ? prefabPapa : prefabCalabaza;
    private void DesactivarBoton(string t) { if (t == "Trigo") btnTrigo.interactable = false; else if (t == "Papa") btnPapa.interactable = false; else btnCalabaza.interactable = false; }
    public void ConfigurarUI(float alpha, bool interact) { groupChecklist.alpha = alpha; groupChecklist.interactable = interact; groupChecklist.blocksRaycasts = interact; groupIconoMochila.alpha = alpha; groupIconoMochila.interactable = interact; groupIconoMochila.blocksRaycasts = interact; }
    public IEnumerator AparecerSuave(CanvasGroup grupo) { float t = 0; while (t < 1) { t += Time.deltaTime * 2f; grupo.alpha = t; yield return null; } grupo.alpha = 1; grupo.interactable = true; grupo.blocksRaycasts = true; }
    public void OcultarInterfazNivel2Snappy() { ConfigurarUI(0, false); if (panelParcelas != null) panelParcelas.SetActive(false); }
    public void SembrarTrigo() => IntentarSembrar("Trigo");
    public void SembrarPapa() => IntentarSembrar("Papa");
    public void SembrarCalabaza() => IntentarSembrar("Calabaza");
}