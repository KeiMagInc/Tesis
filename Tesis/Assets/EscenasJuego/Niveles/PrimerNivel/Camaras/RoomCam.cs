using UnityEngine;
using Unity.Cinemachine;

public class RoomCam : MonoBehaviour
{
    [Header("Cámara de esta habitación")]
    public CinemachineCamera virtualCamera;

    [Header("Lógica del Nivel")]
    public GameObject objetoLogica; // Arrastra aquí el objeto "Logica" de esta zona

    [Header("Texto del Letrero")]
    [TextArea(2, 3)]
    public string textoDeNivel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCamera.Priority = 10;

            // ACTIVAMOS la lógica de este nivel
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

            // DESACTIVAMOS la lógica al salir para limpiar efectos
            if (objetoLogica != null) objetoLogica.SetActive(false);
        }
    }
}