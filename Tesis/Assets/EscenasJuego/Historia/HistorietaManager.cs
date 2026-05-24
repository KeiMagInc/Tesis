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
    void Start()
    {
        if (botonesParaAnimar != null)
        {
            escalasBase = new Vector3[botonesParaAnimar.Length];
            for (int i = 0; i < botonesParaAnimar.Length; i++)
            {
                if (botonesParaAnimar[i] != null)
                {
                    escalasBase[i] = botonesParaAnimar[i].localScale;
                }
            }
        }
        if (buttonSaltar != null) buttonSaltar.onClick.AddListener(CargarEscenaSiguiente);
        if (buttonRegresar != null) buttonRegresar.onClick.AddListener(CargarEscenaAnterior);
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
            {
                botonesParaAnimar[i].localScale = escalasBase[i] * factor;
            }
        }
    }
    public void CargarEscenaSiguiente()
    {
        if (!string.IsNullOrEmpty(escenaSiguiente)) SceneManager.LoadScene(escenaSiguiente);
    }
    public void CargarEscenaAnterior()
    {
        if (!string.IsNullOrEmpty(escenaAnterior)) SceneManager.LoadScene(escenaAnterior);
    }
    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Saliendo del juego...");
    }
}