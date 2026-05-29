using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
public class UIManager : MonoBehaviour
{
    private bool mochilaHabilitada = true;
    public CanvasGroup groupNombreNivel;
    private PlayerController lupiController;
    [Header("Gestión de Sonido y Audio")]
    public AudioSource fuenteMusicaFondo;
    public AudioSource fuenteVozAndy;
    public AudioClip sonidoPausa;
    public AudioClip sonidoMochila;
    public Image iconoMusica;
    public Image iconoAudio;    
    public Sprite spriteSonidoOn, spriteSonidoOff;
    [Header("Tutorial de Controles")]
    public GameObject panelControles;
    private bool controlesYaOcultos = false;
    [Header("Mochila (Nuevas Referencias)")]
    public TextMeshProUGUI[] textosNombresSemillas;
    public Image[] imagenesNumerosSemillas;
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
    private AudioClip[] sonidosActuales = new AudioClip[6];
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
        lupiController = playerLupi.GetComponent<PlayerController>();
    }
    void Start()
    {
        PlayerPrefs.SetInt("MusicaMute", 0);
        PlayerPrefs.SetInt("AndyMute", 0);
        if (fuenteMusicaFondo != null)
        {
            fuenteMusicaFondo.loop = true;
            if (!fuenteMusicaFondo.isPlaying)
                fuenteMusicaFondo.Play();
        }
        fuenteMusicaFondo.mute = false;
        fuenteVozAndy.mute = false;
        fuenteVozAndy.ignoreListenerPause = true;
        ActualizarIconos();
    }
    public void SetMochilaHabilitada(bool habilitada)
    {
        mochilaHabilitada = habilitada;
        MostrarMochilaSolo(habilitada);
        if (!habilitada && panelParcelas != null && panelParcelas.activeSelf)
        {
            panelParcelas.SetActive(false);
            if (lupiController != null) lupiController.controlesBloqueados = false;
        }
    }
    public void SetSounds(params AudioClip[] sonidos)
    {
        System.Array.Clear(sonidosActuales, 0, sonidosActuales.Length);
        for (int i = 0; i < sonidos.Length && i < sonidosActuales.Length; i++)
        {
            sonidosActuales[i] = sonidos[i];
        }
    }
    public void AlternarMusica()
    {
        fuenteMusicaFondo.mute = !fuenteMusicaFondo.mute;
        PlayerPrefs.SetInt("MusicaMute", fuenteMusicaFondo.mute ? 1 : 0);
        ActualizarIconos();
    }
    public void AlternarAudioAndy()
    {
        fuenteVozAndy.mute = !fuenteVozAndy.mute;
        PlayerPrefs.SetInt("AndyMute", fuenteVozAndy.mute ? 1 : 0);
        ActualizarIconos();
    }
    private void ActualizarIconos()
    {
        if (iconoMusica) iconoMusica.sprite = fuenteMusicaFondo.mute ? spriteSonidoOff : spriteSonidoOn;
        if (iconoAudio) iconoAudio.sprite = fuenteVozAndy.mute ? spriteSonidoOff : spriteSonidoOn;
    }
    public void ConfigurarCabeceraNivel(string nombreNivel, string operacion)
    {
        if (textoNombreNivel != null)
            textoNombreNivel.text = nombreNivel; 

        if (textoNombreOperacion != null)
            textoNombreOperacion.text = operacion;
        if (groupNombreNivel != null)
        {
            groupNombreNivel.alpha = 1;
            groupNombreNivel.gameObject.SetActive(true);
        }
        if (panelControles != null)
        {
            bool esNivelAnatomia = nombreNivel.Contains("Anatomía y Componentes");
            panelControles.SetActive(esNivelAnatomia);
            controlesYaOcultos = !esNivelAnatomia;
            Debug.Log("El nivel actual es: " + nombreNivel);
        }
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
                if (imagenesNumerosSemillas != null && i < imagenesNumerosSemillas.Length && imagenesNumerosSemillas[i] != null)
                    imagenesNumerosSemillas[i].gameObject.SetActive(true);
            }
            else
            {
                botonesSemillas[i].gameObject.SetActive(false);
                if (imagenesNumerosSemillas != null && i < imagenesNumerosSemillas.Length && imagenesNumerosSemillas[i] != null)
                    imagenesNumerosSemillas[i].gameObject.SetActive(false);
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
                if (indice < sonidosActuales.Length && sonidosActuales[indice] != null)
                    fuenteVozAndy.PlayOneShot(sonidosActuales[indice]);
                masCercana.estaOcupada = true;
                masCercana.DesactivarColision();
                if (panelParcelas != null)
                {
                    panelParcelas.SetActive(false);
                    if (lupiController != null) lupiController.controlesBloqueados = false; 
                }
                botonesSemillas[indice].interactable = false;
                Transform tIcono = botonesSemillas[indice].transform.Find("Icono");
                if (tIcono != null && tIcono.TryGetComponent(out Image imgIcono))
                    imgIcono.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                if (logicaActiva != null) logicaActiva.AvanceSiembraExitosa();
            }
        }
        else
        {
            andy.Decir("Acércate más a la parcela que corresponde al NODO.", audioAcercateMas);
        }
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
        if (panelControles != null)
        {
            panelControles.SetActive(false);
            controlesYaOcultos = true;
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
    public void AlternarPausa()
    {
        estaPausado = !estaPausado;
        if (fuenteVozAndy != null && sonidoPausa != null)
            fuenteVozAndy.PlayOneShot(sonidoPausa);
        if (panelPausa) panelPausa.SetActive(estaPausado);
        Time.timeScale = estaPausado ? 0f : 1f;
        AudioListener.pause = estaPausado;
    }
    public void SalirDelJuego()
    {
        PlayerPrefs.SetInt("MusicaMute", 0);
        PlayerPrefs.SetInt("AndyMute", 0);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Debug.Log("Regresando al menú de inicio y reseteando audio por defecto...");
        SceneManager.LoadScene("MenuInicio");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            AlternarPausa();
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.M))
        {
            if (!estaPausado && mochilaHabilitada)
                AbrirCerrarMenuParcelas();
        }
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
        if (panelParcelas)
        {
            panelParcelas.SetActive(false);
            if (lupiController != null) lupiController.controlesBloqueados = false;
        }
        semillaActiva = "";
    }
    public void MostrarMochilaSolo(bool m)
    {
        if (groupIconoMochila)
        {
            bool activar = m && mochilaHabilitada;
            groupIconoMochila.gameObject.SetActive(activar);
            groupIconoMochila.alpha = activar ? 1 : 0;
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
        if (groupIconoMochila == null || groupIconoMochila.alpha == 0 || !mochilaHabilitada) return;
        if (fuenteVozAndy != null && sonidoMochila != null)
            fuenteVozAndy.PlayOneShot(sonidoMochila);
        if (panelParcelas != null)
            panelParcelas.SetActive(!panelParcelas.activeSelf);
        if (lupiController != null)
            lupiController.controlesBloqueados = panelParcelas.activeSelf;
    }
}