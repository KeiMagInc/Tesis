using UnityEngine;
using TMPro; // Necesario para modificar el texto

public class EsferaLogica : MonoBehaviour
{
    public int valor; // El número que vale esta esfera (ej. 5, 10, 1)
    public TextMeshPro textoNumero; // Referencia al texto visual
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        ActualizarVisual();
    }

    // Función para cambiar el número y actualizar el texto
    public void AsignarValor(int nuevoValor)
    {
        valor = nuevoValor;
        ActualizarVisual();
    }

    void ActualizarVisual()
    {
        if (textoNumero != null)
        {
            textoNumero.text = valor.ToString();
        }
    }

    // Función para resaltar la esfera cuando Lupu la toca
    public void Resaltar(bool activar)
    {
        if (activar)
            spriteRenderer.color = Color.yellow; // Se pone amarilla al seleccionarla
        else
            spriteRenderer.color = Color.white;  // Vuelve a normal
    }
}