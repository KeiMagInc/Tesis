using Mundo2;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private string nombreLogico1 = "Trigo";
    private string nombreLogico2 = "Papa";
    private string nombreLogico3 = "Calabaza";

    private Vector3 escalaOriginalTrigo;
    private Vector3 escalaOriginalPapa;
    private Vector3 escalaOriginalCalabaza;

    public static UIManager instancia;
    public ILogicaNivel logicaActiva;
    public static int puntosGlobales = 0;

    [Header("Referencias de UI")]
    public CanvasGroup groupChecklist;
    public CanvasGroup groupIconoMochila;
    public GameObject panelParcelas;
    public AndyController andy;

    [Header("Botones")]
    public Button btnTrigo; public Button btnPapa; public Button btnCalabaza;
    private GameObject prefabTrigoActual;
    private GameObject prefabPapaActual;
    private GameObject prefabCalabazaActual;

    [Header("Checklist")]
    public TextMeshProUGUI[] itemsChecklist;

    [Header("Configuración Lupi")]
    public Transform playerLupi;
    public float distanciaParaEncajar = 5.0f;

    private string semillaActiva = "";
    private Color colorVerdeMilitar;
    private Vector3 escalaMochilaOriginal;
    private Vector3 escalaChecklistOriginal;

    void Awake()
    {
        instancia = this;
        ColorUtility.TryParseHtmlString("#028A0F", out colorVerdeMilitar);
        escalaMochilaOriginal = groupIconoMochila.transform.localScale;
        escalaChecklistOriginal = groupChecklist.transform.localScale;

        if (btnTrigo) escalaOriginalTrigo = btnTrigo.transform.localScale;
        if (btnPapa) escalaOriginalPapa = btnPapa.transform.localScale;
        if (btnCalabaza) escalaOriginalCalabaza = btnCalabaza.transform.localScale;
    }

    // Vuelve a añadir esta función al UIManager
    public void MostrarInterfaz(bool mostrar)
    {
        MostrarMochilaSolo(mostrar);
        MostrarChecklistSolo(mostrar);
        if (!mostrar && panelParcelas != null) panelParcelas.SetActive(false);
    }

    public void ConfigurarBotonesUI(Sprite img1, string nom1, Sprite img2, string nom2, Sprite img3, string nom3)
    {
        btnTrigo.GetComponent<Image>().sprite = img1;
        btnPapa.GetComponent<Image>().sprite = img2;
        btnCalabaza.GetComponent<Image>().sprite = img3;

        nombreLogico1 = nom1;
        nombreLogico2 = nom2;
        nombreLogico3 = nom3;
    }

    // Métodos de siembra (Solo estos deben existir, borra los del final del archivo)
    public void SembrarTrigo() => IntentarSembrar(nombreLogico1);
    public void SembrarPapa() => IntentarSembrar(nombreLogico2);
    public void SembrarCalabaza() => IntentarSembrar(nombreLogico3);

    public void SetPrefabs(GameObject trigo, GameObject papa, GameObject calabaza)
    {
        prefabTrigoActual = trigo; prefabPapaActual = papa; prefabCalabazaActual = calabaza;
    }

    public void MostrarMochilaSolo(bool mostrar)
    {
        groupIconoMochila.alpha = mostrar ? 1 : 0;
        groupIconoMochila.interactable = mostrar;
        groupIconoMochila.blocksRaycasts = mostrar;
        if (mostrar) StartCoroutine(EfectoPop(groupIconoMochila.transform, escalaMochilaOriginal));
    }

    public void MostrarChecklistSolo(bool mostrar)
    {
        groupChecklist.alpha = mostrar ? 1 : 0;
        groupChecklist.interactable = mostrar;
        groupChecklist.blocksRaycasts = mostrar;
        if (mostrar) StartCoroutine(EfectoPop(groupChecklist.transform, escalaChecklistOriginal));
    }

    IEnumerator EfectoPop(Transform objeto, Vector3 escalaBase)
    {
        objeto.localScale = Vector3.zero;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float s = (t < 0.8f) ? Mathf.Lerp(0, 1.1f, t / 0.8f) : Mathf.Lerp(1.1f, 1f, (t - 0.8f) / 0.2f);
            objeto.localScale = escalaBase * s;
            yield return null;
        }
        objeto.localScale = escalaBase;
    }

    public void ConfigurarTextosChecklist(string t0, string t1, string t2)
    {
        itemsChecklist[0].text = t0; itemsChecklist[1].text = t1; itemsChecklist[2].text = t2;
        foreach (var item in itemsChecklist) { item.color = Color.black; item.text = item.text.Replace(" [OK]", ""); }
    }

    public void MarcarTareaCompletada(int indice)
    {
        if (indice < 0 || indice >= itemsChecklist.Length) return;
        if (!itemsChecklist[indice].text.Contains("[OK]")) { itemsChecklist[indice].text += " [OK]"; itemsChecklist[indice].color = colorVerdeMilitar; }
    }

    public void ResetBotones()
    {
        btnTrigo.interactable = true; btnPapa.interactable = true; btnCalabaza.interactable = true;
        panelParcelas.SetActive(false); semillaActiva = "";
    }

    public void SetSemillaPalpitar(string tipo) => semillaActiva = tipo;

    void Update() { AplicarPalpitoSemilla(); }

    private void AplicarPalpitoSemilla()
    {
        if (string.IsNullOrEmpty(semillaActiva)) return;
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;

        if (btnTrigo) btnTrigo.transform.localScale = semillaActiva.Equals(nombreLogico1, System.StringComparison.OrdinalIgnoreCase) ? escalaOriginalTrigo * pulse : escalaOriginalTrigo;
        if (btnPapa) btnPapa.transform.localScale = semillaActiva.Equals(nombreLogico2, System.StringComparison.OrdinalIgnoreCase) ? escalaOriginalPapa * pulse : escalaOriginalPapa;
        if (btnCalabaza) btnCalabaza.transform.localScale = semillaActiva.Equals(nombreLogico3, System.StringComparison.OrdinalIgnoreCase) ? escalaOriginalCalabaza * pulse : escalaOriginalCalabaza;
    }

    public void IntentarSembrar(string tipo)
    {
        if (tipo != semillaActiva) { andy.Decir("¡Usa la " + semillaActiva + " primero!"); return; }

        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        ZonaPlantado masCercana = null;
        float dMin = float.MaxValue;

        foreach (var z in zonas)
        {
            if (z.tipoDeSemillaPermitida.ToLower().Contains(tipo.ToLower()) && !z.estaOcupada)
            {
                float d = Vector2.Distance(playerLupi.position, z.transform.position);
                if (d < dMin) { dMin = d; masCercana = z; }
            }
        }

        if (masCercana != null && dMin <= distanciaParaEncajar)
        {
            GameObject prefabAErguir = (tipo == nombreLogico1) ? prefabTrigoActual : (tipo == nombreLogico2) ? prefabPapaActual : prefabCalabazaActual;
            Instantiate(prefabAErguir, masCercana.transform.position, Quaternion.identity);
            masCercana.estaOcupada = true;
            masCercana.DesactivarColision();

            // Desactivar el botón correcto comparando con los nombres lógicos actuales
            if (tipo == nombreLogico1) btnTrigo.interactable = false;
            else if (tipo == nombreLogico2) btnPapa.interactable = false;
            else if (tipo == nombreLogico3) btnCalabaza.interactable = false;

            if (logicaActiva != null) logicaActiva.AvanceSiembraExitosa();
        }
        else andy.Decir("Acércate más a la parcela.");
    }

    public void AbrirCerrarMenuParcelas() => panelParcelas.SetActive(!panelParcelas.activeSelf);
}