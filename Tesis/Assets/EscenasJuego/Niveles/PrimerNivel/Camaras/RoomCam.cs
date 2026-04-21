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