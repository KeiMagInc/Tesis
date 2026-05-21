using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
public class UIManager : MonoBehaviour
{
    [Header("Mochila (Nuevas Referencias)")]
    public TextMeshProUGUI[] textosNombresSemillas; 
    public TextMeshProUGUI[] textosNumerosSemillas; 
    [Header("Información del Nivel")] 
    public TextMeshProUGUI textoNombreNivel;
    public TextMeshProUGUI textoNombreOperacion;
    [Header("Menú de Pausa")]
    public GameObject panelPausa;
    private bool estaPausado = false;
    [Header("Audios Diálogos Andy (Errores)")]
    public AudioClip audioUsaSemillaPrimero;
    public AudioClip audioAcercateMas;
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
        nombresLogicosActuales = new string[botonesSemillas.Length];
        prefabsActuales = new GameObject[botonesSemillas.Length];
        escalasOriginales = new Vector3[botonesSemillas.Length];
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] != null)
            {
                Transform tIcono = botonesSemillas[i].transform.Find("Icono");
                if (tIcono != null)
                    escalasOriginales[i] = tIcono.localScale;
                else
                    escalasOriginales[i] = Vector3.one; 
            }
        }
        if (panelParcelas != null) panelParcelas.SetActive(false);
        MostrarInterfaz(false);
    }
    public void ConfigurarCabeceraNivel(string nombreNivel, string operacion)
    {
        if (textoNombreNivel != null)
            textoNombreNivel.text = nombreNivel; 

        if (textoNombreOperacion != null)
            textoNombreOperacion.text = operacion; 
    }
    public void MostrarInterfaz(bool mostrar)
    {
        MostrarMochilaSolo(mostrar);
        MostrarChecklistSolo(mostrar);
    }
    public void SetPrefabs(params GameObject[] prefabs)
    {
        System.Array.Clear(prefabsActuales, 0, prefabsActuales.Length);
        for (int i = 0; i < prefabs.Length && i < prefabsActuales.Length; i++)
        {
            prefabsActuales[i] = prefabs[i];
        }
    }
    public void ConfigurarBotonesUI(Sprite[] imgs, string[] noms)
    {
        ConfigurarMochila(imgs, noms, prefabsActuales);
    }
    public void ConfigurarMochila(Sprite[] imagenes, string[] nombres, GameObject[] prefabs)
    {
        if (nombresLogicosActuales == null) nombresLogicosActuales = new string[6];
        if (prefabsActuales == null) prefabsActuales = new GameObject[6];
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] == null) continue;
            bool slotActivo = i < imagenes.Length && i < nombres.Length && i < prefabs.Length;
            if (slotActivo && imagenes[i] != null)
            {
                botonesSemillas[i].gameObject.SetActive(true);
                Transform tIcono = botonesSemillas[i].transform.Find("Icono");
                if (tIcono != null && tIcono.TryGetComponent(out Image imgComponent))
                {
                    imgComponent.sprite = imagenes[i];
                    imgComponent.color = Color.white;
                }
                nombresLogicosActuales[i] = nombres[i];
                prefabsActuales[i] = prefabs[i];
                botonesSemillas[i].interactable = true;
                if (textosNombresSemillas != null && i < textosNombresSemillas.Length && textosNombresSemillas[i] != null)
                    textosNombresSemillas[i].text = nombres[i];
                if (textosNumerosSemillas != null && i < textosNumerosSemillas.Length && textosNumerosSemillas[i] != null)
                    textosNumerosSemillas[i].text = (i + 1).ToString();
            }
            else
            {
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
            andy.Decir("¡Lupi! Usa el NODO que te indica la mochila.", audioUsaSemillaPrimero);
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
                Transform tIcono = botonesSemillas[indice].transform.Find("Icono");
                if (tIcono != null && tIcono.TryGetComponent(out Image imgIcono))
                {
                    imgIcono.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                if (logicaActiva != null) logicaActiva.AvanceSiembraExitosa();
            }
        }
        else 
            andy.Decir("Acércate más a la parcela que corresponde al NODO.", audioAcercateMas);
    }
    public void ConfigurarTextosChecklist(params string[] textos)
    {
        for (int i = 0; i < itemsChecklist.Length; i++)
        {
            if (itemsChecklist[i] != null)
            {
                itemsChecklist[i].text = "";
                itemsChecklist[i].color = Color.black;
                itemsChecklist[i].gameObject.SetActive(false); 
            }
        }
        for (int i = 0; i < textos.Length && i < itemsChecklist.Length; i++)
        {
            if (itemsChecklist[i] != null && !string.IsNullOrEmpty(textos[i]))
            {
                itemsChecklist[i].gameObject.SetActive(true);
                itemsChecklist[i].text = textos[i];
                itemsChecklist[i].text = itemsChecklist[i].text.Replace(" [OK]", "");
                Debug.Log($"Checklist: Encendiendo Slot {i} con texto: {textos[i]}");
            }
        }
    }
    public void DesactivarTodoPostNivel()
    {
        MostrarMochilaSolo(false);
        MostrarChecklistSolo(false);
        if (panelParcelas != null) panelParcelas.SetActive(false);
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
    public void AlternarPausa()
    {
        estaPausado = !estaPausado;
        if (panelPausa) panelPausa.SetActive(estaPausado);
        Time.timeScale = estaPausado ? 0f : 1f;
        AudioListener.pause = estaPausado;
    }
    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Debug.Log("Regresando al menú de inicio...");
        SceneManager.LoadScene("MenuInicio");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            AlternarPausa();
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.M))
            if (!estaPausado)
                AbrirCerrarMenuParcelas();
        if (estaPausado) return;
        if (panelParcelas.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) BotonPresionado(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) BotonPresionado(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) BotonPresionado(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) BotonPresionado(3);
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) BotonPresionado(4);
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) BotonPresionado(5);
        }
        if (string.IsNullOrEmpty(semillaActiva)) return;
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.12f;
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] != null)
            {
                Transform iconoTransform = botonesSemillas[i].transform.Find("Icono");
                if (iconoTransform != null)
                {
                    if (botonesSemillas[i].gameObject.activeSelf &&
                        botonesSemillas[i].interactable &&
                        nombresLogicosActuales[i] != null &&
                        nombresLogicosActuales[i].Equals(semillaActiva, System.StringComparison.OrdinalIgnoreCase))
                    {
                        iconoTransform.localScale = escalasOriginales[i] * pulse;
                    }
                    else
                    {
                        iconoTransform.localScale = escalasOriginales[i];
                    }
                }
            }
        }
    }
    public void ResetBotones()
    {
        for (int i = 0; i < botonesSemillas.Length; i++)
        {
            if (botonesSemillas[i] != null)
            {
                botonesSemillas[i].interactable = true;
                Transform tIcono = botonesSemillas[i].transform.Find("Icono");
                if (tIcono != null && tIcono.TryGetComponent(out Image imgIcono))
                {
                    imgIcono.color = Color.white;
                    tIcono.localScale = escalasOriginales[i];
                }
            }
        }
        if (panelParcelas) panelParcelas.SetActive(false);
        semillaActiva = "";
    }
    public void MostrarMochilaSolo(bool m)
    {
        if (groupIconoMochila)
        {
            groupIconoMochila.gameObject.SetActive(m);
            groupIconoMochila.alpha = m ? 1 : 0;
        }
    }
    public void MostrarChecklistSolo(bool m)
    {
        if (groupChecklist != null)
        {
            groupChecklist.gameObject.SetActive(m);
            groupChecklist.alpha = m ? 1 : 0;
            groupChecklist.interactable = m;
            groupChecklist.blocksRaycasts = m;
        }
    }
    public void AbrirCerrarMenuParcelas()
    {
        if (panelParcelas != null)
            panelParcelas.SetActive(!panelParcelas.activeSelf);
    }
}