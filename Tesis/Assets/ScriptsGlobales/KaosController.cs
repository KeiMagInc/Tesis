using UnityEngine;
using System.Collections.Generic;
using Mundo2;

public class KaosController : MonoBehaviour
{
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
    public Color colorSiluetaRoja = new Color(0.5f, 0f, 0f, 1f);
    private SpriteRenderer sr;
    private bool estaAnimando = false;
    private float escalaUltimoFrame; 
    [Header("Configuración de Zonas")]
    public List<ConfiguracionZona> listaZonas;
    [Header("Ajustes de Movimiento")]
    public float velocidad = 2f;
    public float distanciaDeFrenado = 0.2f; 
    [Header("Evolución (Tamaño)")]
    public float escalaInicial = 2.5f;
    public float escalaMinima = 0.3f;
    public float reduccionPorPunto = 0.01f;
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
        escalaActualBase = escalaInicial;
        escalaUltimoFrame = escalaInicial;
    }
    void Update()
    {
        ActualizarTamanoBase();
        DetectarZonaPorLista();
        if (puntoA_Actual != null && puntoB_Actual != null)
        {
            Patrullar();
        }
    }
    void DetectarZonaPorLista()
    {
        if (lupi == null || recibiendoDano) return;
        foreach (var zona in listaZonas)
        {
            if (zona.triggerCamara != null && zona.triggerCamara.OverlapPoint(lupi.transform.position))
            {
                if (nivelesTerminados.Contains(zona.nombreDeLaZona))
                {
                    sr.enabled = false; 
                    return;
                }
                sr.enabled = true; 
                if (triggerActual != zona.triggerCamara)
                {
                    triggerActual = zona.triggerCamara;
                    AsignarNuevaZona(zona);
                }
                return;
            }
        }
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
    System.Collections.IEnumerator AnimacionMuerteKaos()
    {
        recibiendoDano = true;
        estaAnimando = true;
        if (sr != null && materialSilueta != null) sr.material = materialSilueta;
        float tiempo = 0;
        while (tiempo < 1.0f) 
        {
            sr.color = (sr.color == Color.red) ? Color.clear : Color.red;
            yield return new WaitForSeconds(0.1f);
            tiempo += 0.1f;
        }
        sr.enabled = false; 
        recibiendoDano = false;
        estaAnimando = false;
    }
    void AsignarNuevaZona(ConfiguracionZona nuevaZona)
    {
        puntoA_Actual = nuevaZona.puntoA;
        puntoB_Actual = nuevaZona.puntoB;
        destinoActual = puntoA_Actual;
        if (puntoA_Actual != null)
        {
            transform.position = puntoA_Actual.position;
            Debug.Log("<color=cyan>Kaos:</color> Nueva zona: " + nuevaZona.nombreDeLaZona);
        }
    }
    void Patrullar()
    {
        Vector2 posActual = transform.position;
        Vector2 posDestino = destinoActual.position;
        transform.position = Vector2.MoveTowards(posActual, posDestino, velocidad * Time.deltaTime);
        if (Vector2.Distance(transform.position, destinoActual.position) < distanciaDeFrenado)
        {
            destinoActual = (destinoActual == puntoA_Actual) ? puntoB_Actual : puntoA_Actual;
        }
        float diffX = destinoActual.position.x - transform.position.x;
        if (Mathf.Abs(diffX) > 0.05f)
        {
            float mirandoA = diffX > 0 ? escalaActualBase : -escalaActualBase;
            transform.localScale = new Vector3(mirandoA, escalaActualBase, 1);
        }
    }
    void ActualizarTamanoBase()
    {
        if (estaAnimando) return;
        float reduccionTotal = UIManager.puntosGlobales * reduccionPorPunto;
        float nuevaEscalaBase = Mathf.Max(escalaMinima, escalaInicial - reduccionTotal);
        if (nuevaEscalaBase < escalaActualBase)
        {
            StartCoroutine(EfectoTransformacionMario(nuevaEscalaBase));
        }
        else
        {
            escalaActualBase = nuevaEscalaBase;
            AplicarEscalaVisual();
        }
    }
    void AplicarEscalaVisual()
    {
        float signoX = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(escalaActualBase * signoX, escalaActualBase, 1);
    }
    System.Collections.IEnumerator EfectoTransformacionMario(float escalaFinal)
    {
        estaAnimando = true;
        if (sr != null && materialSilueta != null)
        {
            sr.material = materialSilueta;
            sr.color = Color.red;
        }
        float escalaTemporal = escalaActualBase;
        float tiempoPaso = 0.07f;
        for (int i = 0; i < 3; i++)
        {
            escalaActualBase = escalaTemporal * 1.2f;
            AplicarEscalaVisual();
            yield return new WaitForSeconds(tiempoPaso);
            escalaActualBase = escalaFinal;
            AplicarEscalaVisual();
            yield return new WaitForSeconds(tiempoPaso);
        }
        escalaActualBase = escalaFinal;
        escalaUltimoFrame = escalaFinal;
        if (sr != null)
        {
            sr.material = materialOriginal;
            sr.color = Color.white;
        }
        estaAnimando = false;
    }
    private void OnDrawGizmos()
    {
        if (puntoA_Actual != null && puntoB_Actual != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(puntoA_Actual.position, puntoB_Actual.position);
            Gizmos.DrawWireSphere(destinoActual.position, 0.3f);
        }
    }
}