using UnityEngine;

public interface ILogicaNivel
{
    // Solo definimos los NOMBRES de los métodos que todos los niveles deben tener
    void AvanceSiembraExitosa();
    void AccionEnLetrero(string tipo, GameObject objetoTocado = null);
    void ResetearNivel();
}