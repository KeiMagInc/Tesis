using UnityEngine;
using Mundo2;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LogicaNivel2 : MonoBehaviour
{
    public AndyController andy;
    public Transform lupi;
    public UIManager uiManager;

    [Header("Interfaz de Puntos")]
    public TextMeshProUGUI textoPuntos;
    private int puntosTotales = 0;

    [Header("Agua")]
    public LineRenderer lineaAgua;
    public Transform puntoHead;
    public Transform puntoNull;

    private List<Transform> caminoFijo = new List<Transform>();
    private bool estaCargandoAgua = false;
    private int faseActual = 0;

    void Start()
    {
        lineaAgua.positionCount = 0;
        lineaAgua.sortingOrder = 25;
        ActualizarTextoPuntos();
        StartCoroutine(SecuenciaNarrativa());
    }

    IEnumerator SecuenciaNarrativa()
    {
        yield return new WaitForSeconds(1.5f);
        andy.Decir("¡Lupi! Mira el checklist. Ese es el orden de siembra.");
        yield return StartCoroutine(uiManager.MostrarChecklist());

        yield return new WaitForSeconds(1f);
        uiManager.MostrarIconoMochila(); // Activa el botón de la mochila
        andy.Decir("He puesto las semillas en tu MOCHILA. Ábrela y siembra en el orden indicado.");
    }

    void Update()
    {
        // Lógica de LineRenderer (Agua) se mantiene igual...
        if (lineaAgua.positionCount == 0 && !estaCargandoAgua) return;

        int totalPuntos = caminoFijo.Count + (estaCargandoAgua ? 1 : 0);
        lineaAgua.positionCount = totalPuntos;

        for (int i = 0; i < caminoFijo.Count; i++)
            lineaAgua.SetPosition(i, caminoFijo[i].position);

        if (estaCargandoAgua)
            lineaAgua.SetPosition(totalPuntos - 1, lupi.position);
    }

    // El resto de tus métodos (AccionEnLetrero, GanarPuntos, etc) se mantienen...
    public void AccionEnLetrero(string tipo, GameObject objetoTocado = null) { /* Tu código original */ }
    void GanarPuntos(int cantidad) { puntosTotales += cantidad; ActualizarTextoPuntos(); }
    void ActualizarTextoPuntos() { if (textoPuntos != null) textoPuntos.text = puntosTotales.ToString(); }
    void VictoriaFinal() => andy.Decir("¡Increíble! Has creado una lista de 3 huertos conectada a NULL.");
}