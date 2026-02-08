using UnityEngine;

public class ControladorNodo : MonoBehaviour
{
    // Definimos los tipos posibles
    public enum TipoDeNodo { Raiz, Padre, Hijo }
    
    [Header("Configuración del Nodo")]
    public TipoDeNodo miTipo; // Se elige en el Inspector
    
    private Vector3 posicionInicial;
    private bool estaSiendoArrastrado = false;
    private Camera cam;

    void Start() {
        posicionInicial = transform.position;
        cam = Camera.main;
    }

    // Se ejecuta cuando haces clic en el círculo (necesita el Collider)
    void OnMouseDown() {
        estaSiendoArrastrado = true;
    }

    // Se ejecuta mientras mantienes el clic y mueves el ratón
    void OnMouseDrag() {
        if (estaSiendoArrastrado) {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Mantenerlo en el plano 2D
            transform.position = mousePos;
        }
    }

    // Se ejecuta cuando sueltas el clic
    void OnMouseUp() {
        estaSiendoArrastrado = false;
        // Si no se quedó en una cesta (que lo desactivará), regresa al árbol
        Invoke("RegresarSiNoAceptado", 0.1f);
    }

    void RegresarSiNoAceptado() {
        if (gameObject.activeSelf) {
            transform.position = posicionInicial;
        }
    }
}