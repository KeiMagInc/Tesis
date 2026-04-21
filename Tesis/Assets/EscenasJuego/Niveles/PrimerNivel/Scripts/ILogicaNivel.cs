using UnityEngine;

public interface ILogicaNivel
{
    void AvanceSiembraExitosa();
    void AccionEnLetrero(string tipo, GameObject objetoTocado = null);
    void ResetearNivel();
}