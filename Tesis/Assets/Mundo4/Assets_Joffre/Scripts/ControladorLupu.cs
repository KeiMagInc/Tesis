using UnityEngine;

public class ControladorLupu : MonoBehaviour
{
    public float velocidad = 5f; // Puedes cambiar esto en Unity para ir más rápido
    private Rigidbody2D rb;
    private Vector2 movimiento;

    void Start()
    {
        // Buscamos el componente físico automáticamente
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. ESCUCHAR TECLADO
        // Input.GetAxisRaw devuelve -1, 0 o 1. Hace que el movimiento sea "seco" (estilo Pokemon)
        // Si quieres que resbale un poco, quita el "Raw".
        movimiento.x = Input.GetAxisRaw("Horizontal"); // A-D o Flechas Izq-Der
        movimiento.y = Input.GetAxisRaw("Vertical");   // W-S o Flechas Arr-Aba

        // 2. CORREGIR DIAGONALES
        // Sin esto, caminar en diagonal es más rápido que recto. Esto lo iguala.
        movimiento = movimiento.normalized;
    }

    void FixedUpdate()
    {
        // 3. MOVER EL CUERPO
        // Usamos MovePosition para respetar las colisiones (si choca, se para)
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}