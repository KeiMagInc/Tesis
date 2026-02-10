using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SierraGameManager : MonoBehaviour
{
    public static SierraGameManager instancia;

    [Header("Estructura de la Lista")]
    public NodoTambo nodoA;
    public NodoTambo nodoB;
    public NodoTambo nodoC;

    [Header("Estado Inicial")]
    public bool enTutorialInicial = true;

    [Header("Interfaz de Juego (HUD arriba)")]
    public TextMeshProUGUI textoPuntajeUI;

    [Header("Interfaz de Resultados (Panel Final)")]
    public GameObject panelPuntajeFinal;
    public TextMeshProUGUI txtValorPuntaje;
    public TextMeshProUGUI txtValorAciertos;
    public TextMeshProUGUI txtValorFallos;

    [Header("Estado del Juego")]
    public int puntos = 0;
    public int aciertos = 0;
    public int fallos = 0;
    public bool nivelCompletado = false;

    [Header("Configuración de Sonido")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;
    public AudioClip sonidoVictoria; // <--- NUEVO: Arrastra aquí 'nivelCompletado'

    void Awake() { instancia = this; }

    void Start()
    {
        ActualizarInterfaz();
        if (panelPuntajeFinal != null) panelPuntajeFinal.SetActive(false);

        // 1. Bloqueamos a Lupi al iniciar
        BloquearLupi(true);

        // 2. Andy da las instrucciones
        Invoke("InstruccionesIniciales", 1.0f);
    }

    void Update()
    {
        if (enTutorialInicial && Input.GetKeyDown(KeyCode.E))
        {
            CancelInvoke("FinalizarTutorial");
            FinalizarTutorial();
        }
    }

    void InstruccionesIniciales()
    {
        AndyDice("¡Hola Lupi! Kaos ha roto el camino lineal.\nEstán conectados los Tambos A - C \nInserta el Tambo B entre A y C como una lista simple.");

        var scriptAndy = Object.FindFirstObjectByType<Mundo2.AndyFollow>();
        float tiempoDeLectura = (scriptAndy != null) ? scriptAndy.tiempoVisible : 6.0f;

        Invoke("FinalizarTutorial", tiempoDeLectura);
    }

    void BloquearLupi(bool estado)
    {
        PlayerController lupi = Object.FindFirstObjectByType<PlayerController>();

        if (lupi != null)
        {
            lupi.controlesBloqueados = estado;
            Debug.Log("Lupi encontrado. Estado de bloqueo: " + estado);
        }
        else
        {
            StartCoroutine(ReintentarBloqueo(estado));
        }
    }

    IEnumerator ReintentarBloqueo(bool estado)
    {
        yield return new WaitForSeconds(0.2f);
        PlayerController lupi = Object.FindFirstObjectByType<PlayerController>();
        if (lupi != null) lupi.controlesBloqueados = estado;
    }

    void FinalizarTutorial()
    {
        enTutorialInicial = false;
        BloquearLupi(false);

        var scriptAndy = Object.FindFirstObjectByType<Mundo2.AndyFollow>();
        if (scriptAndy != null && scriptAndy.panelDialogo != null)
            scriptAndy.panelDialogo.SetActive(false);
    }

    private void Reproducir(AudioClip clip)
    {
        if (fuenteAudio != null && clip != null)
        {
            fuenteAudio.PlayOneShot(clip);
        }
    }

    public bool ValidarPaso(NodoTambo origen, NodoTambo destino)
    {
        if (nivelCompletado) return false;

        if (origen == nodoB && destino == nodoC)
        {
            if (!origen.bloqueado)
            {
                Reproducir(sonidoAcierto);
                SumarPuntos(50);
                aciertos++;
                origen.bloqueado = true;
                AndyDice("¡Excelente Lupi! El Tambo B ahora apunta a C. \n¡Ya casi restauras la ruta!");
                EncogerKaos();
                return true;
            }
        }

        if (origen == nodoA && destino == nodoB)
        {
            if (nodoB.siguienteNodo == nodoC)
            {
                Reproducir(sonidoAcierto);
                SumarPuntos(100);
                aciertos++;
                origen.bloqueado = true;
                nivelCompletado = true;
                AndyDice("¡Lo lograste! La ruta lineal está completa. \n¡Kaos es cada vez más pequeño!");
                EncogerKaos();

                // Programamos la aparición de la pantalla final
                Invoke("MostrarPantallaFinal", 6f);
                return true;
            }
            else
            {
                Reproducir(sonidoError);
                RestarPuntos(50);
                fallos++;
                AndyDice("¡Espera! Si conectas A con B ahora, perderás la referencia de C. \n¡Conecta B a C primero!");
                return false;
            }
        }

        Reproducir(sonidoError);
        fallos++;
        if (origen == nodoC)
        {
            RestarPuntos(50);
            AndyDice("Lupi, el Tambo C es el final del camino. \n¡No tiene a quién apuntar!");
        }
        else
        {
            RestarPuntos(25);
            AndyDice("Ese movimiento no tiene lógica lineal. \n¡Prueba otra combinación!");
        }
        return false;
    }

    public void SumarPuntos(int cantidad) { puntos += cantidad; ActualizarInterfaz(); }
    public void RestarPuntos(int cantidad) { puntos = Mathf.Max(0, puntos - cantidad); ActualizarInterfaz(); }

    void ActualizarInterfaz()
    {
        if (textoPuntajeUI != null) textoPuntajeUI.text = puntos.ToString();
    }

    void MostrarPantallaFinal()
    {
        if (panelPuntajeFinal != null)
        {
            panelPuntajeFinal.SetActive(true);

            // --- NUEVO: REPRODUCIR SONIDO DE VICTORIA ---
            Reproducir(sonidoVictoria);

            if (txtValorPuntaje != null) txtValorPuntaje.text = puntos.ToString();
            if (txtValorAciertos != null) txtValorAciertos.text = aciertos.ToString();
            if (txtValorFallos != null) txtValorFallos.text = fallos.ToString();
        }
    }

    public void IrASiguienteEscena(string nombreEscena) { SceneManager.LoadScene(nombreEscena); }
    public void ReiniciarNivel() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

    void EncogerKaos()
    {
        KaosComportamiento kaos = Object.FindFirstObjectByType<KaosComportamiento>();
        if (kaos != null) kaos.ReducirTamano();
    }

    public void AndyDice(string mensaje)
    {
        Mundo2.AndyFollow andy = Object.FindFirstObjectByType<Mundo2.AndyFollow>();
        if (andy != null) andy.Decir(mensaje);
    }
}