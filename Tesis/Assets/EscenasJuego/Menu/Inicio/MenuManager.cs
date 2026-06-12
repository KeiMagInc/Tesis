using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    [Header("Referencias a Paneles")]
    public GameObject panelMenu;
    public GameObject panelCreditos;
    [Header("Música de Fondo")]
    public AudioSource musicaMenu;
    [Header("Efecto de Botones (Feedback Visual)")]
    [Tooltip("Arrastra aquí los botones que quieres que palpiten.")]
    public RectTransform[] botonesParaAnimar;
    [Range(0.01f, 0.2f)] public float intensidadPalpito = 0.05f;
    public float velocidadPalpito = 5f;
    [Header("Sonidos")]
    public AudioClip sonidoClick;
    void Start()
    {
        if (musicaMenu != null)
        {
            musicaMenu.loop = true;
            musicaMenu.Play();
        }
        panelMenu.SetActive(true);
        panelCreditos.SetActive(false);
    }
    void Update()
    {
        AplicarEfectoPalpito();
    }
    private void AplicarEfectoPalpito()
    {
        float calculoEscala = 1f + Mathf.Sin(Time.time * velocidadPalpito) * intensidadPalpito;
        foreach (RectTransform boton in botonesParaAnimar)
        {
            if (boton != null && boton.gameObject.activeInHierarchy)
                boton.localScale = new Vector3(calculoEscala, calculoEscala, 1f);
        }
    }
    private void ReproducirSonidoBoton()
    {
        if (sonidoClick != null)
        {
            GameObject sonidoTemp = new GameObject("SonidoClick_Menu");
            DontDestroyOnLoad(sonidoTemp);
            AudioSource fuenteTemp = sonidoTemp.AddComponent<AudioSource>();
            fuenteTemp.clip = sonidoClick;
            fuenteTemp.Play();
            Destroy(sonidoTemp, sonidoClick.length);
        }
    }
    public void MostrarCreditos()
    {
        ReproducirSonidoBoton();
        panelMenu.SetActive(false);
        panelCreditos.SetActive(true);
    }
    public void VolverAlMenu()
    {
        ReproducirSonidoBoton();
        panelCreditos.SetActive(false);
        panelMenu.SetActive(true);
    }
    public void Jugar()
    {
        ReproducirSonidoBoton();
        SceneManager.LoadScene("HistoriaInicio");
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