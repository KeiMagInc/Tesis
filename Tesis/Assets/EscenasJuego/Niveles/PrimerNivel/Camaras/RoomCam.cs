using UnityEngine;
using Unity.Cinemachine;

public class RoomCam : MonoBehaviour
{
    [Header("Cámara de esta habitación")]
    public CinemachineCamera virtualCamera;
    [Header("Lógica del Nivel")]
    public GameObject objetoLogica;
    [Header("Texto del Letrero")]
    [TextArea(2, 3)]
    public string textoDeNivel;

    [Header("Sonido Ambiental")]
    public AudioClip sonidoAmbienteZona;

    private AudioSource miAudioSource; // Usaremos solo este

    private void Awake()
    {
        // Detecta el AudioSource que pusiste en el mismo objeto
        miAudioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 10;

            if (objetoLogica != null) objetoLogica.SetActive(true);

            GestorInterfazNivel gestor = Object.FindFirstObjectByType<GestorInterfazNivel>();
            if (gestor != null)
            {
                gestor.MostrarNombre(textoDeNivel);
            }

            // REPRODUCIR SONIDO
            if (miAudioSource != null && sonidoAmbienteZona != null)
            {
                miAudioSource.clip = sonidoAmbienteZona;
                miAudioSource.loop = true;
                miAudioSource.Play();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 0;
            if (objetoLogica != null) objetoLogica.SetActive(false);

            // DETENER SONIDO
            if (miAudioSource != null)
            {
                miAudioSource.Stop();
            }
        }
    }
}