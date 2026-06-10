using UnityEngine;
public class GameInitializer : MonoBehaviour
{
    public Transform playerLupi;
    public Transform puntoInicioNivel1;
    void Start()
    {
        if (playerLupi != null && puntoInicioNivel1 != null)
            playerLupi.position = puntoInicioNivel1.position;
    }
}