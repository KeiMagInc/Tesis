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
            {
                boton.localScale = new Vector3(calculoEscala, calculoEscala, 1f);
            }
        }
    }
    public void MostrarCreditos()
    {
        panelMenu.SetActive(false);
        panelCreditos.SetActive(true);
    }
    public void VolverAlMenu()
    {
        panelCreditos.SetActive(false);
        panelMenu.SetActive(true);
    }
    public void Jugar()
    {
        SceneManager.LoadScene("HistoriaInicio");
    }
}