using UnityEngine;
using Unity.Cinemachine;
public class RoomCam : MonoBehaviour
{
    [Header("Cámara de esta habitación")]
    public CinemachineCamera virtualCamera;
    [Header("Lógica del Nivel")]
    public GameObject objetoLogica;
    [TextArea(2, 3)] public string textoDeNivel;
    public AudioClip sonidoAmbienteZona;
    private AudioSource miAudioSource;
    private void Awake() => miAudioSource = GetComponent<AudioSource>();
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 10;
            if (objetoLogica != null) objetoLogica.SetActive(true);
            if (UIManager.instancia != null && sonidoAmbienteZona != null)
            {
                if (UIManager.instancia.fuenteMusicaFondo.clip != sonidoAmbienteZona)
                {
                    UIManager.instancia.fuenteMusicaFondo.clip = sonidoAmbienteZona;
                    UIManager.instancia.fuenteMusicaFondo.Play();
                }
                else if (!UIManager.instancia.fuenteMusicaFondo.isPlaying)
                {
                    UIManager.instancia.fuenteMusicaFondo.Play();
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 0;
            if (objetoLogica != null) objetoLogica.SetActive(false);
        }
    }
}