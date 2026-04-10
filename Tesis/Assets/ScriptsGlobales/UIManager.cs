using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;
    public ILogicaNivel logicaActiva;
    public static int puntosGlobales = 0;

    [Header("Referencias de UI")]
    public CanvasGroup groupChecklist;
    public CanvasGroup groupIconoMochila;
    public GameObject panelParcelas;
    public AndyController andy;

    [Header("Mochila (Arrastra los 6 botones aquí)")]
    public Button[] botonesSemillas;

    [Header("Checklist")]
    public TextMeshProUGUI[] itemsChecklist;

    [Header("Configuración Lupi")]
    public Transform playerLupi;
    public float distanciaParaEncajar = 5.0f;

    private string[] nombresLogicosActuales = new string[6];
    private GameObject[] prefabsActuales = new GameObject[6];
    private Vector3[] escalasOriginales;
    private string semillaActiva = "";
    private Color colorVerdeMilitar;

    void Awake()
    {
        instancia = this;
        ColorUtility.TryParseHtmlString("#028A0F", out colorVerdeMilitar);

        // Inicializamos los arreglos de datos para evitar errores de índice
        nombresLogicosActuales = new string[botonesSemillas.Length];
        prefabsActuales = new GameObject[botonesSemillas.Length];

        escalasOriginales = new Vector3[botonesSemillas.Length];
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] != null)
                escalasOriginales[i] = botonesSemillas[i].transform.localScale;
        }
    }

    public void MostrarInterfaz(bool mostrar)
    {
        MostrarMochilaSolo(mostrar);
        MostrarChecklistSolo(mostrar);
    }

    public void SetPrefabs(GameObject p1, GameObject p2, GameObject p3)
    {
        // Limpiar para evitar basura de niveles anteriores
        System.Array.Clear(prefabsActuales, 0, prefabsActuales.Length);
        prefabsActuales[0] = p1; prefabsActuales[1] = p2; prefabsActuales[2] = p3;
    }

    public void ConfigurarBotonesUI(Sprite s1, string n1, Sprite s2, string n2, Sprite s3, string n3)
    {
        Sprite[] imgs = { s1, s2, s3 };
        string[] noms = { n1, n2, n3 };
        ConfigurarMochila(imgs, noms, prefabsActuales);
    }

    // FUNCIÓN MEJORADA: Ahora es a prueba de errores de índice
    public void ConfigurarMochila(Sprite[] imagenes, string[] nombres, GameObject[] prefabs)
    {
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] == null) continue;

            // SEGURIDAD: Solo entramos si el índice 'i' existe en TODOS los arreglos recibidos
            if (i < imagenes.Length && i < nombres.Length && i < prefabs.Length)
            {
                if (imagenes[i] != null)
                {
                    botonesSemillas[i].gameObject.SetActive(true);
                    botonesSemillas[i].GetComponent<Image>().sprite = imagenes[i];
                    nombresLogicosActuales[i] = nombres[i];
                    prefabsActuales[i] = prefabs[i];
                    botonesSemillas[i].interactable = true;
                }
                else botonesSemillas[i].gameObject.SetActive(false);
            }
            else
            {
                // Si el nivel no tiene tantas semillas, apagamos el botón sobrante
                botonesSemillas[i].gameObject.SetActive(false);
            }
        }
    }

    public void BotonPresionado(int indice)
    {
        if (indice >= 0 && indice < nombresLogicosActuales.Length)
        {
            if (!string.IsNullOrEmpty(nombresLogicosActuales[indice]))
                IntentarSembrar(nombresLogicosActuales[indice], indice);
        }
    }

    public void IntentarSembrar(string tipo, int indice)
    {
        if (string.IsNullOrEmpty(tipo) || tipo.ToLower() != semillaActiva.ToLower())
        {
            andy.Decir("¡Usa la " + semillaActiva + " primero!");
            return;
        }

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
            if (prefabsActuales[indice] != null)
            {
                Instantiate(prefabsActuales[indice], masCercana.transform.position, Quaternion.identity);
                masCercana.estaOcupada = true;
                masCercana.DesactivarColision();
                botonesSemillas[indice].interactable = false;
                if (logicaActiva != null) logicaActiva.AvanceSiembraExitosa();
            }
        }
        else andy.Decir("Acércate más a la parcela.");
    }

    public void ConfigurarTextosChecklist(params string[] textos)
    {
        foreach (var item in itemsChecklist) if (item != null) item.gameObject.SetActive(false);
        for (int i = 0; i < textos.Length && i < itemsChecklist.Length; i++)
        {
            if (itemsChecklist[i] != null)
            {
                itemsChecklist[i].gameObject.SetActive(true);
                itemsChecklist[i].text = textos[i];
                itemsChecklist[i].color = Color.black;
            }
        }
    }

    public void MarcarTareaCompletada(int indice)
    {
        if (indice >= 0 && indice < itemsChecklist.Length && itemsChecklist[indice] != null)
        {
            if (!itemsChecklist[indice].text.Contains("[OK]"))
            {
                itemsChecklist[indice].text += " [OK]";
                itemsChecklist[indice].color = colorVerdeMilitar;
            }
        }
    }

    public void SetSemillaPalpitar(string tipo) => semillaActiva = tipo;

    void Update()
    {
        if (string.IsNullOrEmpty(semillaActiva)) return;
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] != null && botonesSemillas[i].gameObject.activeSelf &&
                nombresLogicosActuales[i] != null && nombresLogicosActuales[i].Equals(semillaActiva, System.StringComparison.OrdinalIgnoreCase))
                botonesSemillas[i].transform.localScale = escalasOriginales[i] * pulse;
            else if (botonesSemillas[i] != null)
                botonesSemillas[i].transform.localScale = escalasOriginales[i];
        }
    }

    public void ResetBotones() { foreach (var b in botonesSemillas) if (b != null) b.interactable = true; if (panelParcelas) panelParcelas.SetActive(false); semillaActiva = ""; }
    public void MostrarMochilaSolo(bool m) { if (groupIconoMochila) { groupIconoMochila.alpha = m ? 1 : 0; groupIconoMochila.interactable = m; groupIconoMochila.blocksRaycasts = m; } }
    public void MostrarChecklistSolo(bool m) { if (groupChecklist) { groupChecklist.alpha = m ? 1 : 0; groupChecklist.interactable = m; groupChecklist.blocksRaycasts = m; } }
    public void AbrirCerrarMenuParcelas() { if (panelParcelas) panelParcelas.SetActive(!panelParcelas.activeSelf); }
}