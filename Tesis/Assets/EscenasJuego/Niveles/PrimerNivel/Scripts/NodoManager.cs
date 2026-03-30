using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodoManager : MonoBehaviour
{
    public Transform puntoEntrada;
    public Transform puntoSalida;

    [Header("Efecto de Agua")]
    public GameObject cuadroAgua;
    public Vector3 escalaMinima = new Vector3(0.1f, 0.1f, 1f);
    public Vector3 escalaMaxima = new Vector3(1.5f, 1.5f, 1f); // Se desborda un poco
    public Vector3 escalaNormal = new Vector3(1f, 1f, 1f);    // Tamaño del sembrío
    public float velocidadEscala = 0.5f;

    [Header("Efecto de Crecimiento")]
    public float retrasoEntreSembrios = 0.15f;
    public Sprite spriteSeco;
    public Sprite spriteVivo;

    private List<SpriteRenderer> renderersSembrios = new List<SpriteRenderer>();
    private bool estaActivado = false;
    private Coroutine rutinaAgua;

    void Awake()
    {
        if (cuadroAgua != null)
        {
            cuadroAgua.SetActive(false);
            cuadroAgua.transform.localScale = escalaMinima;
        }

        Transform contenedor = transform.Find("Sembrios");
        if (contenedor != null)
        {
            foreach (Transform hijo in contenedor)
            {
                SpriteRenderer sr = hijo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    renderersSembrios.Add(sr);
                    sr.sprite = spriteSeco;
                }
            }
        }
    }

    public void ActivarHuerto()
    {
        if (!estaActivado)
        {
            estaActivado = true;
            if (cuadroAgua != null)
            {
                cuadroAgua.SetActive(true);
                if (rutinaAgua != null) StopCoroutine(rutinaAgua);
                rutinaAgua = StartCoroutine(AnimarEscalaAgua(escalaMaxima));
            }
            StartCoroutine(SecuenciaCrecimientoConLag());
        }
    }

    public void DrenarAgua()
    {
        // Llamado cuando se conecta al NULL
        if (cuadroAgua != null)
        {
            if (rutinaAgua != null) StopCoroutine(rutinaAgua);
            rutinaAgua = StartCoroutine(AnimarEscalaAgua(escalaNormal));
        }
    }

    IEnumerator AnimarEscalaAgua(Vector3 escalaObjetivo)
    {
        while (Vector3.Distance(cuadroAgua.transform.localScale, escalaObjetivo) > 0.01f)
        {
            cuadroAgua.transform.localScale = Vector3.Lerp(cuadroAgua.transform.localScale, escalaObjetivo, Time.deltaTime * velocidadEscala);
            yield return null;
        }
        cuadroAgua.transform.localScale = escalaObjetivo;
    }

    IEnumerator SecuenciaCrecimientoConLag()
    {
        foreach (SpriteRenderer sr in renderersSembrios)
        {
            sr.sprite = spriteVivo;
            yield return new WaitForSeconds(retrasoEntreSembrios);
        }
    }
}