using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class NodoManager : MonoBehaviour
{
    [Header("Efecto Kaos")]
    private List<Vector3> escalasOriginalesFuegos = new List<Vector3>();
    private Vector3 escalaOriginalFuego = Vector3.one;
    public GameObject contenedorFuego;
    private List<GameObject> listaFuegos = new List<GameObject>();
    [Header("Conexiones Simples (Niveles 1, 2, 3 y 4)")]
    public Transform puntoEntrada;
    public Transform puntoSalida;
    [Header("Conexiones Dobles (Solo Nivel 5)")]
    public Transform puntoEntradaAnterior;
    public Transform puntoSalidaAnterior;
    public Transform puntoEntradaSiguiente;
    public Transform puntoSalidaSiguiente;
    [Header("Ajustes de Velocidad de Eliminación")]
    [Tooltip("Velocidad de reducción del agua (cuadro azul)")]
    public float velocidadAguaEliminar = 4.0f;
    [Tooltip("Velocidad con la que el nodo completo se encoge al final")]
    public float velocidadEncogidoFinal = 5.0f;
    [Header("Efecto de Agua")]
    public GameObject cuadroAgua;
    public Vector3 escalaMinima = new Vector3(0.1f, 0.1f, 1f);
    public Vector3 escalaMaxima = new Vector3(1.5f, 1.5f, 1f);
    public Vector3 escalaNormal = new Vector3(1f, 1f, 1f);
    public float velocidadEscala = 2.0f;
    [Header("Efecto de Daño / Narrativa")]
    [Tooltip("Color que tomará el cuadro de agua al ser infectado o eliminado por el Kaos")]
    public Color colorAguaEliminar = new Color(0.85f, 0.15f, 0.15f, 130f / 255f);
    private Color colorOriginalAgua;
    private SpriteRenderer spriteRendererAgua;
    [Header("Efecto de Crecimiento/Secado")]
    public float retrasoEntreSembrios = 0.15f;
    public Sprite spriteSeco;
    public Sprite spriteVivo;
    private List<SpriteRenderer> renderersSembrios = new List<SpriteRenderer>();
    private List<Animator> animadoresSembrios = new List<Animator>();
    private bool estaActivado = false;
    private Coroutine rutinaEscala;
    private Coroutine rutinaInfeccion;
    void Awake()
    {
        Transform contenedorSembrios = transform.Find("Sembrios");
        if (contenedorSembrios != null)
        {
            foreach (Transform hijo in contenedorSembrios)
            {
                SpriteRenderer sr = hijo.GetComponent<SpriteRenderer>();
                if (sr != null) renderersSembrios.Add(sr);
                Animator anim = hijo.GetComponent<Animator>();
                if (anim != null)
                {
                    animadoresSembrios.Add(anim);
                    anim.enabled = false;
                }
            }
        }
        if (contenedorFuego != null)
        {
            Vector3 scale = contenedorFuego.transform.localScale;
            if (Mathf.Approximately(scale.z, 0f))
            {
                scale.z = 1f;
                contenedorFuego.transform.localScale = scale;
            }
            escalaOriginalFuego = scale;
            foreach (Transform hijo in contenedorFuego.transform)
            {
                listaFuegos.Add(hijo.gameObject);
                Vector3 escalaHijo = hijo.localScale;
                if (Mathf.Approximately(escalaHijo.z, 0f))
                {
                    escalaHijo.z = 1f;
                    hijo.localScale = escalaHijo;
                }
                escalasOriginalesFuegos.Add(escalaHijo);
                hijo.gameObject.SetActive(false);
            }
        }
        if (cuadroAgua != null)
        {
            spriteRendererAgua = cuadroAgua.GetComponent<SpriteRenderer>();
            if (spriteRendererAgua != null)
                colorOriginalAgua = spriteRendererAgua.color;
        }
        colorAguaEliminar.a = 130f / 255f;
        ResetearNodo();
    }
    public void ResetearNodo()
    {
        estaActivado = false;
        StopAllCoroutines();
        for (int i = 0; i < listaFuegos.Count; i++)
        {
            if (i < escalasOriginalesFuegos.Count)
                listaFuegos[i].transform.localScale = escalasOriginalesFuegos[i];
            listaFuegos[i].SetActive(false);
        }
        if (contenedorFuego != null)
        {
            contenedorFuego.SetActive(false);
            contenedorFuego.transform.localScale = escalaOriginalFuego;
        }
        if (cuadroAgua != null)
        {
            cuadroAgua.transform.localScale = escalaMinima;
            cuadroAgua.SetActive(false);
        }
        if (spriteRendererAgua != null)
            spriteRendererAgua.color = colorOriginalAgua;
        foreach (SpriteRenderer sr in renderersSembrios)
            if (sr != null) sr.sprite = spriteSeco;
        foreach (Animator anim in animadoresSembrios)
            if (anim != null) anim.enabled = false;
        transform.localScale = Vector3.one;
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
    public void IniciarSecuenciaEliminacion()
    {
        estaActivado = false;
        StartCoroutine(RutinaMuerteNodo());
    }
    public void InfectarNodo()
    {
        estaActivado = false;
        if (rutinaInfeccion != null) StopCoroutine(rutinaInfeccion);
        rutinaInfeccion = StartCoroutine(RutinaInfeccionProgresiva());
        StartCoroutine(EncenderFuegosSecuencialmente());
    }
    public void LimpiarNodo()
    {
        StopCoroutine(EncenderFuegosSecuencialmente());
        for (int i = 0; i < listaFuegos.Count; i++)
        {
            if (i < escalasOriginalesFuegos.Count)
                listaFuegos[i].transform.localScale = escalasOriginalesFuegos[i];
            listaFuegos[i].SetActive(false);
        }
        if (contenedorFuego != null)
            contenedorFuego.SetActive(false);
    }
    private IEnumerator EncenderFuegosSecuencialmente()
    {
        if (contenedorFuego != null)
            contenedorFuego.SetActive(true);
        Debug.Log($"[NodoManager] Activando {listaFuegos.Count} fuegos secuencialmente en {gameObject.name}");
        foreach (GameObject fuego in listaFuegos)
        {
            fuego.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }
    }
    private IEnumerator RutinaInfeccionProgresiva()
    {
        if (cuadroAgua != null)
        {
            cuadroAgua.SetActive(true);
            if (rutinaEscala != null) StopCoroutine(rutinaEscala);
            rutinaEscala = StartCoroutine(AnimarEscalaAgua(escalaMaxima));
            if (spriteRendererAgua != null)
            {
                Color colorInicial = spriteRendererAgua.color;
                float transicion = 0f;
                float duracionTransicion = 1.0f; 
                while (transicion < 1f)
                {
                    transicion += Time.deltaTime / duracionTransicion;
                    spriteRendererAgua.color = Color.Lerp(colorInicial, colorAguaEliminar, transicion);
                    yield return null;
                }
                spriteRendererAgua.color = colorAguaEliminar;
            }
        }
        yield return StartCoroutine(SecuenciaCrecimiento(false));
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
    private IEnumerator RutinaMuerteNodo()
    {
        StartCoroutine(SecuenciaCrecimiento(false));
        if (cuadroAgua != null && spriteRendererAgua != null)
            spriteRendererAgua.color = colorAguaEliminar;
        StopCoroutine(EncenderFuegosSecuencialmente());
        for (int i = listaFuegos.Count - 1; i >= 0; i--)
        {
            if (listaFuegos[i] != null)
            {
                listaFuegos[i].SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }
        }
        if (contenedorFuego != null)
            contenedorFuego.SetActive(false);
        if (cuadroAgua != null)
        {
            if (rutinaEscala != null) StopCoroutine(rutinaEscala);
            velocidadEscala = velocidadAguaEliminar;
            yield return StartCoroutine(AnimarEscalaAgua(Vector3.zero));
            cuadroAgua.SetActive(false);
        }
        float t = 0;
        Vector3 escalaInicial = transform.localScale;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadEncogidoFinal;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }
    IEnumerator SecuenciaCrecimiento(bool vivir)
    {
        Sprite spriteFinal = vivir ? spriteVivo : spriteSeco;
        for (int i = 0; i < renderersSembrios.Count; i++)
        {
            if (renderersSembrios[i] != null)
                renderersSembrios[i].sprite = spriteFinal;
            if (i < animadoresSembrios.Count && animadoresSembrios[i] != null)
                animadoresSembrios[i].enabled = vivir;
            yield return new WaitForSeconds(retrasoEntreSembrios);
        }
    }
}