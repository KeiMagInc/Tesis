using UnityEngine;
using Unity.Cinemachine;

public class RoomCam : MonoBehaviour
{
    [Header("Cámara de esta habitación")]
    public CinemachineCamera virtualCamera;

    [Header("Texto del Letrero")]
    [TextArea(2, 3)] // Esto hará que en el Inspector aparezca un cuadro grande para escribir
    public string textoDeNivel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 10;

            GestorInterfazNivel gestor = Object.FindFirstObjectByType<GestorInterfazNivel>();
            if (gestor != null)
            {
                // Enviamos el texto tal cual lo escribas en el Inspector
                gestor.MostrarNombre(textoDeNivel);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 0;
        }
    }
}