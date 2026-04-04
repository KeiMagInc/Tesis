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
    public GameObject panelParcelas; // Asegúrate de arrastrar el objeto "Parcelas" aquí
    public AndyController andy;

    [Header("Botones y Prefabs")]
    public Button btnTrigo; public Button btnPapa; public Button btnCalabaza;
    public GameObject prefabTrigo; public GameObject prefabPapa; public GameObject prefabCalabaza;

    [Header("Checklist")]
    public TextMeshProUGUI[] itemsChecklist; // Arrastra los 3 textos del checklist aquí (Trigo, Papa, Calabaza)

    [Header("Configuración Lupi")]
    public Transform playerLupi;
    public float distanciaParaEncajar = 3.5f;

    private string semillaActiva = "";

    void Awake()
    {
        ConfigurarUI(0, false);
        if (panelParcelas != null) panelParcelas.SetActive(false);
    }

    void Update()
    {
        AplicarPalpitoSemilla();
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

    // --- FUNCIÓN PARA LA MOCHILA ---
    public void AbrirCerrarMenuParcelas()
    {
        if (panelParcelas != null)
        {
            panelParcelas.SetActive(!panelParcelas.activeSelf);
        }
    }

    // --- LÓGICA DEL CHECKLIST ---
    public void MarcarTareaCompletada(int indice)
    {
        if (indice >= 0 && indice < itemsChecklist.Length)
        {
            string textoOriginal = itemsChecklist[indice].text.ToLower(); // Forzamos minúsculas
            if (!textoOriginal.StartsWith("[ok]"))
            {
                itemsChecklist[indice].text = "[OK] " + textoOriginal;
                itemsChecklist[indice].color = new Color(0.1f, 0.5f, 0.1f); // Un verde bonito
            }
        }
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

                    // 1. Instanciar el vegetal (Trigo, Papa o Calabaza)
                    Instantiate(ObtenerPrefab(tipo), z.transform.position, Quaternion.identity);

                    // 2. Marcar como ocupada
                    z.estaOcupada = true;

                    // 3. ¡NUEVO!: Desactivar el collider de la zona para que Lupi pueda entrar
                    z.DesactivarColision();

                    // 4. UI y Progresión
                    DesactivarBoton(tipo);
                    LogicaNivel2.instancia.AvanceSiembraExitosa();
                }
                else
                {
                    andy.Decir("Acércate a la parcela de " + tipo);
                }
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