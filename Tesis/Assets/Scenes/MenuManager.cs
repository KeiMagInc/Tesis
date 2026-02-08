using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Paneles
    public GameObject panelMenu;
    public GameObject panelCreditos;

    void Start()
    {
        panelMenu.SetActive(true);
        panelCreditos.SetActive(false);
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
        SceneManager.LoadScene("Listas");
    }
}