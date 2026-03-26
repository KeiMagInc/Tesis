using UnityEngine;
using Unity.Cinemachine; // Librería nueva de Unity 6

public class RoomCam : MonoBehaviour
{
    [Header("Cámara de esta habitación")]
    public CinemachineCamera virtualCamera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Al entrar en esta habitación, subimos la prioridad de ESTA cámara
            virtualCamera.Priority = 10;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Al salir, bajamos la prioridad
            virtualCamera.Priority = 0;
        }
    }
}