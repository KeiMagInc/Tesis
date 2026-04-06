using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodoManager : MonoBehaviour
{
    public Transform puntoEntrada;
    public Transform puntoSalida;
    [Header("Ajustes de Velocidad de Eliminación")]
    [Tooltip("Velocidad de reducción del agua (cuadro azul)")]
    public float velocidadAguaEliminar = 3.0f;

    [Tooltip("Velocidad con la que el nodo completo se encoge al final")]
    public float velocidadEncogidoFinal = 4.0f;

    [Header("Efecto de Agua")]
    public GameObject cuadroAgua;
    public Vector3 escalaMinima = new Vector3(0.1f, 0.1f, 1f);
    public Vector3 escalaMaxima = new Vector3(1.5f, 1.5f, 1f);
    public Vector3 escalaNormal = new Vector3(1f, 1f, 1f);
    public float velocidadEscala = 2.0f;

    [Header("Efecto de Crecimiento/Secado")]
    public float retrasoEntreSembrios = 0.15f;
    public Sprite spriteSeco;
    public Sprite spriteVivo;

    private List<SpriteRenderer> renderersSembrios = new List<SpriteRenderer>();
    private bool estaActivado = false;
    private Coroutine rutinaEscala;

    void Awake()
    {
        Transform contenedor = transform.Find("Sembrios");
        if (contenedor != null)
        {
            foreach (Transform hijo in contenedor)
            {
                SpriteRenderer sr = hijo.GetComponent<SpriteRenderer>();
                if (sr != null) renderersSembrios.Add(sr);
            }
        }
        ResetearNodo();
    }

    public void ResetearNodo()
    {
        estaActivado = false;
        StopAllCoroutines();
        if (cuadroAgua != null)
        {
            cuadroAgua.transform.localScale = escalaMinima;
            cuadroAgua.SetActive(false);
        }
        foreach (SpriteRenderer sr in renderersSembrios)
        {
            if (sr != null) sr.sprite = spriteSeco;
        }
        transform.localScale = Vector3.one; // Asegurar que sea visible al resetear
    }

    public void ActivarHuerto()
    {
        if (!estaActivado)
        {
            estaActivado = true;
            if (cuadroAgua != null)
            {
                cuadroAgua.SetActive(true);
                if (rutinaEscala != null) StopCoroutine(rutinaEscala);
                rutinaEscala = StartCoroutine(AnimarEscalaAgua(escalaMaxima));
            }
            StartCoroutine(SecuenciaCrecimiento(true));
        }
    }

    public void DrenarAgua()
    {
        if (cuadroAgua != null)
        {
            if (rutinaEscala != null) StopCoroutine(rutinaEscala);
            rutinaEscala = StartCoroutine(AnimarEscalaAgua(escalaNormal));
        }
    }

    // --- NUEVOS MÉTODOS PARA ELIMINACIÓN ---

    public void IniciarSecuenciaEliminacion()
    {
        estaActivado = false;
        StartCoroutine(RutinaMuerteNodo());
    }

    private IEnumerator RutinaMuerteNodo()
    {
        // 1. Reducir agua (Se lanza y el código sigue inmediatamente a la siguiente línea)
        if (cuadroAgua != null)
        {
            if (rutinaEscala != null) StopCoroutine(rutinaEscala);
            // Usamos una velocidad alta para que el agua baje rápido mientras se secan las plantas
            float velocidadOriginal = velocidadEscala;
            velocidadEscala = velocidadAguaEliminar;
            StartCoroutine(AnimarEscalaAgua(Vector3.zero));
        }

        // 2. Secar plantas (CON 'yield return': el código se detiene aquí hasta que la última planta sea 'spriteSeco')
        // El tiempo que tarda depende de 'retrasoEntreSembrios' y la cantidad de plantas.
        yield return StartCoroutine(SecuenciaCrecimiento(false));

        // 3. Una vez secas, reducir el nodo completo hasta desaparecer
        float t = 0;
        Vector3 escalaInicial = transform.localScale;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadEncogidoFinal; // Aquí controlas qué tan rápido desaparece el nodo
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t);
            yield return null;
        }

        // Destrucción definitiva
        Destroy(gameObject);
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

    // He generalizado esta función para que sirva para crecer y secar
    IEnumerator SecuenciaCrecimiento(bool vivir)
    {
        Sprite spriteFinal = vivir ? spriteVivo : spriteSeco;
        foreach (SpriteRenderer sr in renderersSembrios)
        {
            if (sr != null) sr.sprite = spriteFinal;
            yield return new WaitForSeconds(retrasoEntreSembrios);
        }
    }
}