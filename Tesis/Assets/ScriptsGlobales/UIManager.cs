using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public CanvasGroup groupChecklist;
    public CanvasGroup groupIconoMochila;
    public GameObject panelParcelas; // Este es el que contiene los productos
    public AndyController andy;

    [Header("Botones y Prefabs")]
    public Button btnTrigo; public Button btnPapas; public Button btnZanahorias;
    public GameObject prefabTrigo; public GameObject prefabPapas; public GameObject prefabZanahorias;

    [Header("Checklist")]
    public TextMeshProUGUI[] itemsChecklist;

    [Header("Configuración Lupi")]
    public Transform playerLupi;
    public float distanciaParaEncajar = 3.5f;

    [Header("Ajustes de Palpitar")]
    public float velocidadPalpitar = 5f;
    public float amplitudPalpitar = 0.15f;

    private string[] ordenCorrecto = { "Trigo", "Papas", "Zanahorias" };
    public int indiceProgreso = 0;
    private bool modoLibre = false;

    private List<GameObject> huertosInstanciados = new List<GameObject>();

    private Color colorNegro = Color.black;
    private Color colorVerdeMilitar;

    void Awake()
    {
        ColorUtility.TryParseHtmlString("#028A0F", out colorVerdeMilitar);

        ConfigurarUI(0, false);
        if (panelParcelas != null) panelParcelas.SetActive(false);

        foreach (var item in itemsChecklist)
        {
            item.color = colorNegro;
        }
    }

    void Update()
    {
        if (panelParcelas != null && panelParcelas.activeSelf) AplicarEfectoPalpitar();
    }

    private void AplicarEfectoPalpitar()
    {
        float calculoEscala = 1f + Mathf.Sin(Time.time * velocidadPalpitar) * amplitudPalpitar;
        Vector3 escalaNueva = new Vector3(calculoEscala, calculoEscala, 1f);

        btnTrigo.transform.localScale = Vector3.one;
        btnPapas.transform.localScale = Vector3.one;
        btnZanahorias.transform.localScale = Vector3.one;

        if (!modoLibre)
        {
            if (indiceProgreso == 0 && btnTrigo.interactable) btnTrigo.transform.localScale = escalaNueva;
            else if (indiceProgreso == 1 && btnPapas.interactable) btnPapas.transform.localScale = escalaNueva;
            else if (indiceProgreso == 2 && btnZanahorias.interactable) btnZanahorias.transform.localScale = escalaNueva;
        }
        else
        {
            if (btnTrigo.interactable) btnTrigo.transform.localScale = escalaNueva;
            if (btnPapas.interactable) btnPapas.transform.localScale = escalaNueva;
            if (btnZanahorias.interactable) btnZanahorias.transform.localScale = escalaNueva;
        }
    }

    private void IntentarEncajarPieza(string tipoBuscado, GameObject prefab, Button boton)
    {
        if (!modoLibre && ordenCorrecto[indiceProgreso] != tipoBuscado)
        {
            andy.Decir("¡Aún no, Lupi! Primero insertemos el " + ordenCorrecto[indiceProgreso]);
            return;
        }

        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        ZonaPlantado zonaDestino = null;

        foreach (ZonaPlantado z in zonas)
            if (z.tipoDeSemillaPermitida == tipoBuscado && !z.estaOcupada) { zonaDestino = z; break; }

        if (zonaDestino == null) return;

        if (Vector3.Distance(playerLupi.position, zonaDestino.transform.position) <= distanciaParaEncajar)
        {
            GameObject nuevoHuerto = Instantiate(prefab, zonaDestino.transform.position, Quaternion.identity);
            huertosInstanciados.Add(nuevoHuerto);

            zonaDestino.estaOcupada = true;
            boton.interactable = false;
            boton.transform.localScale = Vector3.one;

            if (!modoLibre)
            {
                AvanzarTutorial();
            }
            else
            {
                int idx = System.Array.IndexOf(ordenCorrecto, tipoBuscado);
                MarcarTareaCompletada(idx);
                andy.Decir("¡Bien! Has sembrado " + tipoBuscado + " en modo libre.");
            }
        }
        else andy.Decir("Estás muy lejos del sector de " + tipoBuscado);
    }

    private void AvanzarTutorial()
    {
        MarcarTareaCompletada(indiceProgreso);
        indiceProgreso++;
        if (indiceProgreso == 1) andy.Decir("¡Muy bien! Ahora inserta las Papas en el segundo pantano.");
        else if (indiceProgreso == 2) andy.Decir("¡Eso es! Termina el tutorial con la Zanahoria.");
        else if (indiceProgreso == 3) StartCoroutine(ActivarModoLibre());
    }

    IEnumerator ActivarModoLibre()
    {
        andy.Decir("¡Perfecto! Ya entiendes la creación secuencial.");
        yield return new WaitForSeconds(3f);

        andy.Decir("Ahora borraré los cultivos. Pruébalo de nuevo, pero ¡en el orden que tú quieras!");

        LimpiarSembríos();

        yield return new WaitForSeconds(1.5f);
        modoLibre = true;

        btnTrigo.interactable = true;
        btnPapas.interactable = true;
        btnZanahorias.interactable = true;
    }

    private void LimpiarSembríos()
    {
        foreach (GameObject go in huertosInstanciados)
        {
            if (go != null) Destroy(go);
        }
        huertosInstanciados.Clear();

        ZonaPlantado[] zonas = Object.FindObjectsByType<ZonaPlantado>(FindObjectsSortMode.None);
        foreach (var z in zonas) z.estaOcupada = false;

        foreach (var item in itemsChecklist)
        {
            item.color = colorNegro;
            item.text = item.text.Replace("[OK] ", "");
        }
    }

    public void MarcarTareaCompletada(int indice)
    {
        if (indice >= 0 && indice < itemsChecklist.Length)
        {
            itemsChecklist[indice].color = colorVerdeMilitar;
            if (!itemsChecklist[indice].text.StartsWith("[OK]"))
                itemsChecklist[indice].text = "[OK] " + itemsChecklist[indice].text;
        }
    }

    public void SembrarTrigo() => IntentarEncajarPieza("Trigo", prefabTrigo, btnTrigo);
    public void SembrarPapas() => IntentarEncajarPieza("Papas", prefabPapas, btnPapas);
    public void SembrarZanahorias() => IntentarEncajarPieza("Zanahorias", prefabZanahorias, btnZanahorias);

    private void ConfigurarUI(float alpha, bool interactuable)
    {
        if (groupChecklist != null) { groupChecklist.alpha = alpha; groupChecklist.interactable = interactuable; groupChecklist.blocksRaycasts = interactuable; }
        if (groupIconoMochila != null) { groupIconoMochila.alpha = alpha; groupIconoMochila.interactable = interactuable; groupIconoMochila.blocksRaycasts = interactuable; }
    }

    public IEnumerator FadeNivel2UI(float targetAlpha, float duration)
    {
        float startAlpha = groupChecklist.alpha;
        float time = 0;
        while (time < duration) { time += Time.deltaTime; ConfigurarUI(Mathf.Lerp(startAlpha, targetAlpha, time / duration), false); yield return null; }
        ConfigurarUI(targetAlpha, targetAlpha > 0);
    }

    public void AbrirCerrarMenuParcelas() => panelParcelas.SetActive(!panelParcelas.activeSelf);

    // CORRECCIÓN AQUÍ: Desactivamos el panelParcelas al ocultar la interfaz
    public void OcultarInterfazNivel2Snappy()
    {
        ConfigurarUI(0, false);
        if (panelParcelas != null) panelParcelas.SetActive(false);
    }

    public void MostrarInterfazNivel2Snappy() => ConfigurarUI(1, true);
}