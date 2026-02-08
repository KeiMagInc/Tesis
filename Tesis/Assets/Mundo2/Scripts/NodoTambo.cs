using UnityEngine;

public class NodoTambo : MonoBehaviour
{
    public NodoTambo siguienteNodo;
    private LineRenderer rayoEnergia;
    public bool bloqueado = false;
    void Start()
    {
        rayoEnergia = GetComponent<LineRenderer>();
    }

    void Update()
    {
        LupiInteraccion lupi = FindObjectOfType<LupiInteraccion>();

        // 1. Si Lupi lleva este nodo, el rayo lo sigue a él (Prioridad)
        if (lupi != null && lupi.nodoOrigen == this && lupi.cargandoPuntero)
        {
            rayoEnergia.positionCount = 2;
            rayoEnergia.SetPosition(0, transform.position);
            // El rayo va hacia Lupi con un pequeño ajuste de altura (-1 en Z)
            rayoEnergia.SetPosition(1, new Vector3(lupi.transform.position.x, lupi.transform.position.y, -1f));
        }
        // 2. Si no lo lleva Lupi, pero tiene un nodo siguiente, dibuja la conexión
        else if (siguienteNodo != null)
        {
            rayoEnergia.positionCount = 2;
            rayoEnergia.SetPosition(0, transform.position);
            rayoEnergia.SetPosition(1, new Vector3(siguienteNodo.transform.position.x, siguienteNodo.transform.position.y, -1f));
        }
        // 3. Si no hay nada, apaga el rayo
        else
        {
            rayoEnergia.positionCount = 0;
        }

        if (bloqueado)
        {
            rayoEnergia.startColor = Color.green;
            rayoEnergia.endColor = Color.green;
        }
    }
}