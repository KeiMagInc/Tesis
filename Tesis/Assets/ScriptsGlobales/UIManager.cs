using Mundo2;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Añade estas variables privadas para guardar las escalas originales de los botones
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

    // Variables para guardar lo que configuraste en el Inspector
    private Vector3 escalaMochilaOriginal;
    private Vector3 escalaChecklistOriginal;

    void Awake()
    {
        instancia = this;
        ColorUtility.TryParseHtmlString("#028A0F", out colorVerdeMilitar);

        escalaMochilaOriginal = groupIconoMochila.transform.localScale;
        escalaChecklistOriginal = groupChecklist.transform.localScale;

        // GUARDAMOS LAS ESCALAS ORIGINALES DE LOS BOTONES
        if (btnTrigo) escalaOriginalTrigo = btnTrigo.transform.localScale;
        if (btnPapa) escalaOriginalPapa = btnPapa.transform.localScale;
        if (btnCalabaza) escalaOriginalCalabaza = btnCalabaza.transform.localScale;
    }

    public void SetPrefabs(GameObject trigo, GameObject papa, GameObject calabaza)
    {
        prefabTrigoActual = trigo;
        prefabPapaActual = papa;
        prefabCalabazaActual = calabaza;
    }

    // APARECE SOLO LA MOCHILA
    public void MostrarMochilaSolo(bool mostrar)
    {
        if (mostrar)
        {
            groupIconoMochila.alpha = 1;
            groupIconoMochila.interactable = true;
            groupIconoMochila.blocksRaycasts = true;
            StartCoroutine(EfectoPop(groupIconoMochila.transform, escalaMochilaOriginal));
        }
        else
        {
            groupIconoMochila.alpha = 0;
            groupIconoMochila.interactable = false;
            groupIconoMochila.blocksRaycasts = false;
        }
    }

    // APARECE SOLO EL CHECKLIST
    public void MostrarChecklistSolo(bool mostrar)
    {
        if (mostrar)
        {
            groupChecklist.alpha = 1;
            groupChecklist.interactable = true;
            groupChecklist.blocksRaycasts = true;
            StartCoroutine(EfectoPop(groupChecklist.transform, escalaChecklistOriginal));
        }
        else
        {
            groupChecklist.alpha = 0;
            groupChecklist.interactable = false;
            groupChecklist.blocksRaycasts = false;
        }
    }

    // Para el Nivel 1 (Apaga todo)
    public void MostrarInterfaz(bool mostrar)
    {
        MostrarMochilaSolo(mostrar);
        MostrarChecklistSolo(mostrar);
        if (!mostrar && panelParcelas != null) panelParcelas.SetActive(false);
    }

    IEnumerator EfectoPop(Transform objeto, Vector3 escalaBase)
    {
        objeto.localScale = Vector3.zero;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            // Curva de rebote multiplicada por la escala del inspector
            float s = (t < 0.8f) ? Mathf.Lerp(0, 1.1f, t / 0.8f) : Mathf.Lerp(1.1f, 1f, (t - 0.8f) / 0.2f);
            objeto.localScale = escalaBase * s;
            yield return null;
        }
        objeto.localScale = escalaBase;
    }

    public void ConfigurarTextosChecklist(string t0, string t1, string t2)
    {
        itemsChecklist[0].text = t0; itemsChecklist[1].text = t1; itemsChecklist[2].text = t2;
        foreach (var item in itemsChecklist)
        {
            item.color = Color.black;
            item.text = item.text.Replace(" [OK]", "");
        }
    }

    public void MarcarTareaCompletada(int indice)
    {
        if (indice < 0 || indice >= itemsChecklist.Length) return;
        if (!itemsChecklist[indice].text.Contains("[OK]"))
        {
            itemsChecklist[indice].text += " [OK]";
            itemsChecklist[indice].color = colorVerdeMilitar;
        }
    }

    public void ResetBotones()
    {
        btnTrigo.interactable = true; btnPapa.interactable = true; btnCalabaza.interactable = true;
        panelParcelas.SetActive(false);
        semillaActiva = "";
    }

    public void SetSemillaPalpitar(string tipo) => semillaActiva = tipo;
    void Update() { AplicarPalpitoSemilla(); }

    private void AplicarPalpitoSemilla()
    {
        if (string.IsNullOrEmpty(semillaActiva)) return;

        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;

        // Usamos Equals con OrdinalIgnoreCase para evitar errores de dedo con las mayúsculas
        if (btnTrigo != null && btnTrigo.interactable)
            btnTrigo.transform.localScale = semillaActiva.Equals("Trigo", System.StringComparison.OrdinalIgnoreCase)
                ? escalaOriginalTrigo * pulse : escalaOriginalTrigo;

        if (btnPapa != null && btnPapa.interactable)
            btnPapa.transform.localScale = semillaActiva.Equals("Papa", System.StringComparison.OrdinalIgnoreCase)
                ? escalaOriginalPapa * pulse : escalaOriginalPapa;

        if (btnCalabaza != null && btnCalabaza.interactable)
            btnCalabaza.transform.localScale = semillaActiva.Equals("Calabaza", System.StringComparison.OrdinalIgnoreCase)
                ? escalaOriginalCalabaza * pulse : escalaOriginalCalabaza;
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
            GameObject prefabAErguir = (tipo == "Trigo") ? prefabTrigoActual : (tipo == "Papa") ? prefabPapaActual : prefabCalabazaActual;
            Instantiate(prefabAErguir, masCercana.transform.position, Quaternion.identity);
            masCercana.estaOcupada = true;
            masCercana.DesactivarColision();
            if (tipo == "Trigo") btnTrigo.interactable = false; else if (tipo == "Papa") btnPapa.interactable = false; else btnCalabaza.interactable = false;
            if (logicaActiva != null) logicaActiva.AvanceSiembraExitosa();
        }
        else andy.Decir("Acércate más a la parcela.");
    }

    public void AbrirCerrarMenuParcelas() => panelParcelas.SetActive(!panelParcelas.activeSelf);
    public void SembrarTrigo() => IntentarSembrar("Trigo");
    public void SembrarPapa() => IntentarSembrar("Papa");
    public void SembrarCalabaza() => IntentarSembrar("Calabaza");
}