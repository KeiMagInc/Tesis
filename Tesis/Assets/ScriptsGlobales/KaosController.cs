using Mundo2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class KaosController : MonoBehaviour
{
    private float puntosUltimoFrame;
    public float distanciaMinimaAlJugador = 5.0f; 
    [Header("UI de Emotes")]
    public SpriteRenderer imagenReaccion;
    public List<Sprite> emotesPositivos;
    public List<Sprite> emotesNegativos;
    public float escalaFijaEmote = 15.0f;
    [System.Serializable]
    public class ConfiguracionZona
    {
        public string nombreDeLaZona;
        public Collider2D triggerCamara;
        public Transform puntoA;
        public Transform puntoB;
    }
    public static KaosController instancia;
    public static List<string> nivelesTerminados = new List<string>();
    private bool recibiendoDano = false;
    private Material materialOriginal;
    [Header("Material Silueta")]
    public Material materialSilueta;
    [Header("Efectos Visuales")]
    private SpriteRenderer sr;
    private bool estaAnimando = false;
    [Header("Configuración de Zonas")]
    public List<ConfiguracionZona> listaZonas;
    [Header("Ajustes de Movimiento")]
    public float velocidad = 2f;
    public float distanciaDeFrenado = 0.2f;
    [Header("Evolución (Tamaño)")]
    public float escalaInicial = 2.5f;
    public float escalaMinima = 0.2f;
    public int puntosObjetivoPara20Porciento = 300;
    private float reduccionPorPunto;
    private Transform puntoA_Actual;
    private Transform puntoB_Actual;
    private Transform destinoActual;
    private Collider2D triggerActual;
    private GameObject lupi;
    private float escalaActualBase;
    void Awake()
    {
        instancia = this;
        lupi = GameObject.FindGameObjectWithTag("Player");
        sr = GetComponent<SpriteRenderer>();
        materialOriginal = sr.material;
        reduccionPorPunto = (escalaInicial * 0.20f) / puntosObjetivoPara20Porciento;
        escalaActualBase = escalaInicial;
        if (imagenReaccion != null) imagenReaccion.gameObject.SetActive(false);
    }
    void Update()
    {
        DetectarZonaPorLista();
        if (triggerActual != null) SeguirJugador();
        float puntosActuales = UIManager.puntosGlobales + UIManager.puntosTemporales;
        if (puntosActuales > puntosUltimoFrame)
        {
            ActualizarTamanoBase(true);
        }
        else if (puntosActuales < puntosUltimoFrame)
        {
            ActualizarTamanoBase(false);
        }
        puntosUltimoFrame = puntosActuales;
        if (imagenReaccion != null && imagenReaccion.gameObject.activeSelf)
        {
            float sX = escalaFijaEmote / Mathf.Abs(transform.localScale.x);
            float sY = escalaFijaEmote / transform.localScale.y;
            imagenReaccion.transform.localScale = new Vector3(sX, sY, 1);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.timeScale == 0) return;
        if (collision.CompareTag("Player"))
        {
            if (recibiendoDano || estaAnimando || !sr.enabled) return;
            StartCoroutine(EfectoAtaqueExitosoKaos());
            if (LogicaNivel1.instancia != null && LogicaNivel1.instancia.gameObject.activeInHierarchy) LogicaNivel1.instancia.DesconectarEnlacePorKaos();
            if (LogicaNivel2.instancia != null && LogicaNivel2.instancia.gameObject.activeInHierarchy) LogicaNivel2.instancia.DesconectarEnlacePorKaos();
            if (LogicaNivel3.instancia != null && LogicaNivel3.instancia.gameObject.activeInHierarchy) LogicaNivel3.instancia.DesconectarEnlacePorKaos();
            if (LogicaNivel4.instancia != null && LogicaNivel4.instancia.gameObject.activeInHierarchy) LogicaNivel4.instancia.DesconectarEnlacePorKaos();
            if (LogicaNivel5.instancia != null && LogicaNivel5.instancia.gameObject.activeInHierarchy) LogicaNivel5.instancia.DesconectarEnlacePorKaos();
        }
    }
    IEnumerator EfectoAtaqueExitosoKaos()
    {
        estaAnimando = true;
        MostrarEmoteAleatorio(emotesPositivos);
        if (sr != null)
        {
            sr.material = materialSilueta != null ? materialSilueta : materialOriginal;
            sr.color = new Color(0f, 0.6f, 1f, 1f);
        }
        float escalaTemporal = escalaActualBase;
        escalaActualBase = escalaTemporal * 1.2f;
        AplicarEscalaVisual();
        yield return new WaitForSeconds(0.15f);
        escalaActualBase = escalaTemporal;
        AplicarEscalaVisual();
        yield return new WaitForSeconds(0.5f);
        if (sr != null)
        {
            sr.material = materialOriginal;
            sr.color = Color.white;
        }
        OcultarEmote();
        estaAnimando = false;
    }
    private void MostrarEmoteAleatorio(List<Sprite> lista)
    {
        if (imagenReaccion != null && lista.Count > 0)
        {
            int indice = Random.Range(0, lista.Count);
            imagenReaccion.sprite = lista[indice];
            imagenReaccion.gameObject.SetActive(true);
        }
    }
    private void OcultarEmote()
    {
        if (imagenReaccion != null) imagenReaccion.gameObject.SetActive(false);
    }
    public void RecibirDanoYDesaparecer(string nombreNivel)
    {
        if (!recibiendoDano)
        {
            if (!nivelesTerminados.Contains(nombreNivel))
                nivelesTerminados.Add(nombreNivel);
            StartCoroutine(AnimacionMuerteKaos());
        }
    }
    public void ReaccionarAError()
    {
        if (recibiendoDano || estaAnimando || !sr.enabled) return;
        StartCoroutine(EfectoCastigoVisual());
    }
    IEnumerator EfectoCastigoVisual()
    {
        estaAnimando = true;
        MostrarEmoteAleatorio(emotesPositivos);
        if (sr != null && materialSilueta != null)
        {
            sr.material = materialSilueta;
            sr.color = new Color(0f, 0.5f, 1f, 1f);
        }
        float escalaTemporal = escalaActualBase;
        for (int i = 0; i < 2; i++)
        {
            escalaActualBase = escalaTemporal * 1.15f;
            AplicarEscalaVisual();
            yield return new WaitForSeconds(0.07f);
            escalaActualBase = escalaTemporal;
            AplicarEscalaVisual();
            yield return new WaitForSeconds(0.07f);
        }
        sr.material = materialOriginal;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        OcultarEmote();
        estaAnimando = false;
        ActualizarTamanoBase(false);
    }
    IEnumerator EfectoTransformacionMario(float escalaFinal)
    {
        estaAnimando = true;
        MostrarEmoteAleatorio(emotesNegativos);
        if (sr != null)
        {
            sr.material = materialSilueta != null ? materialSilueta : materialOriginal;
            sr.color = Color.red;
        }
        float escalaAnterior = escalaActualBase;
        for (int i = 0; i < 3; i++)
        {
            escalaActualBase = escalaAnterior * 1.3f;
            AplicarEscalaVisual();
            yield return new WaitForSeconds(0.08f);
            escalaActualBase = escalaAnterior;
            AplicarEscalaVisual();
            yield return new WaitForSeconds(0.08f);
        }
        escalaActualBase = escalaFinal;
        AplicarEscalaVisual();
        if (sr != null)
        {
            sr.material = materialOriginal;
            sr.color = Color.white;
        }
        yield return new WaitForSeconds(0.8f);
        OcultarEmote();
        estaAnimando = false;
    }
    System.Collections.IEnumerator AnimacionMuerteKaos()
    {
        recibiendoDano = true;
        estaAnimando = true;
        MostrarEmoteAleatorio(emotesNegativos);
        if (sr != null && materialSilueta != null) sr.material = materialSilueta;
        float tiempo = 0;
        while (tiempo < 1.0f)
        {
            sr.color = (sr.color == Color.red) ? Color.clear : Color.red;
            yield return new WaitForSeconds(0.1f);
            tiempo += 0.1f;
        }
        sr.enabled = false;
        OcultarEmote();
        recibiendoDano = false;
        estaAnimando = false;
    }
    void DetectarZonaPorLista()
    {
        if (lupi == null || recibiendoDano) return;
        foreach (var zona in listaZonas)
        {
            if (zona.triggerCamara != null && zona.triggerCamara.OverlapPoint(lupi.transform.position))
            {
                if (nivelesTerminados.Contains(zona.nombreDeLaZona)) { sr.enabled = false; return; }
                sr.enabled = true;
                if (triggerActual != zona.triggerCamara) { triggerActual = zona.triggerCamara; AsignarNuevaZona(zona); }
                return;
            }
        }
    }
    void AsignarNuevaZona(ConfiguracionZona nuevaZona)
    {
        puntoA_Actual = nuevaZona.puntoA; puntoB_Actual = nuevaZona.puntoB; destinoActual = puntoA_Actual;
        if (puntoA_Actual != null) transform.position = puntoA_Actual.position;
    }
    void SeguirJugador()
    {
        if (lupi == null || recibiendoDano) return;
        float distanciaActual = Vector2.Distance(transform.position, lupi.transform.position);
        if (distanciaActual > distanciaMinimaAlJugador)
            transform.position = Vector2.MoveTowards(transform.position, lupi.transform.position, velocidad * Time.deltaTime);
        AplicarEscalaVisual();
    }
    void ActualizarTamanoBase(bool huboGanancia)
    {
        float puntosTotales = UIManager.puntosGlobales + UIManager.puntosTemporales;
        float reduccionTotal = puntosTotales * reduccionPorPunto;
        float nuevaEscalaBase = Mathf.Max(escalaMinima, escalaInicial - reduccionTotal);
        if (huboGanancia && !estaAnimando && !recibiendoDano)
        {
            StartCoroutine(EfectoTransformacionMario(nuevaEscalaBase));
        }
        else if (!estaAnimando)
        {
            escalaActualBase = nuevaEscalaBase;
            AplicarEscalaVisual();
        }
    }
    void AplicarEscalaVisual()
    {
        if (lupi == null) return;
        float mirandoA = (lupi.transform.position.x < transform.position.x) ? -escalaActualBase : escalaActualBase;
        transform.localScale = new Vector3(mirandoA, escalaActualBase, 1);
    }
    public void ResetearEstadoNivel(string nombreNivel)
    {
        if (nivelesTerminados.Contains(nombreNivel)) nivelesTerminados.Remove(nombreNivel);
        recibiendoDano = false; estaAnimando = false; StopAllCoroutines();
        if (sr != null) { sr.enabled = true; sr.material = materialOriginal; sr.color = Color.white; }
        triggerActual = null; OcultarEmote();
    }
}