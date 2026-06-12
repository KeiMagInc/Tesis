using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HistorietaManager : MonoBehaviour
{
    [Header("Referencias a Botones")]
    public Button buttonSaltar;
    public Button buttonRegresar;
    public Button buttonSalir;
    [Header("Configuración de Escenas")]
    public string escenaSiguiente = "PrimerNivel";
    public string escenaAnterior = "MenuInicio";
    [Header("Efecto de Botones (Call to Action)")]
    public RectTransform[] botonesParaAnimar;
    public float intensidadPalpito = 0.05f;
    public float velocidadPalpito = 5f;
    private Vector3[] escalasBase;
    [Header("Sonidos")]
    public AudioClip sonidoClick;
    void Start()
    {
        if (botonesParaAnimar != null)
        {
            escalasBase = new Vector3[botonesParaAnimar.Length];
            for (int i = 0; i < botonesParaAnimar.Length; i++)
            {
                if (botonesParaAnimar[i] != null)
                    escalasBase[i] = botonesParaAnimar[i].localScale;
            }
        }
        if (buttonSaltar != null) buttonSaltar.onClick.AddListener(CargarEscenaSiguiente);
        if (buttonRegresar != null) buttonRegresar.onClick.AddListener(CargarEscenaAnterior);
        if (buttonSalir != null) buttonSalir.onClick.AddListener(SalirDelJuego);
    }
    void Update()
    {
        AplicarEfectoPalpito();
    }
    private void AplicarEfectoPalpito()
    {
        float factor = 1f + Mathf.Sin(Time.time * velocidadPalpito) * intensidadPalpito;
        for (int i = 0; i < botonesParaAnimar.Length; i++)
        {
            if (botonesParaAnimar[i] != null)
                botonesParaAnimar[i].localScale = escalasBase[i] * factor;
        }
    }
    private void ReproducirSonidoBoton()
    {
        if (sonidoClick != null)
        {
            GameObject sonidoTemp = new GameObject("SonidoClick_UI");
            DontDestroyOnLoad(sonidoTemp);
            AudioSource fuenteTemp = sonidoTemp.AddComponent<AudioSource>();
            fuenteTemp.clip = sonidoClick;
            fuenteTemp.Play();
            Destroy(sonidoTemp, sonidoClick.length);
        }
    }
    public void CargarEscenaSiguiente()
    {
        ReproducirSonidoBoton();
        if (!string.IsNullOrEmpty(escenaSiguiente))
        {
            PlayerPrefs.SetInt("EsPartidaNueva", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(escenaSiguiente);
        }
    }
    public void CargarEscenaAnterior()
    {
        ReproducirSonidoBoton();
        if (!string.IsNullOrEmpty(escenaAnterior))
            SceneManager.LoadScene(escenaAnterior);
    }
    public void SalirDelJuego()
    {
        ReproducirSonidoBoton();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Saliendo del juego...");
    }
}