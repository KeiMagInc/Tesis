using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Velocidad")]
    [Header("Estados Globales")]
    public bool controlesBloqueados = false; // Nueva variable universal
    public float walkSpeed = 4f;
    public float runSpeed = 7f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isRunningInput;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool prioritizeX = false;
    private DialogoManager dialogoManager;

    void Start()
    {
        // Buscamos el manager una vez al inicio
        dialogoManager = Object.FindFirstObjectByType<DialogoManager>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMove(InputValue value)
    {
        Vector2 newValue = value.Get<Vector2>();

        if (newValue.x != 0 && moveInput.x == 0) prioritizeX = true;
        else if (newValue.y != 0 && moveInput.y == 0) prioritizeX = false;

        moveInput = newValue;
    }

    void OnSprint(InputValue value)
    {
        isRunningInput = value.isPressed;
    }

    void FixedUpdate()
    {
        // Bloqueamos si el DialogoManager lo dice O si activamos el freno manual
        bool hayBloqueoExterno = (dialogoManager != null && dialogoManager.hayDialogoActivo);
        if (hayBloqueoExterno || controlesBloqueados)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetBool("isRunning", false);
            return;
        }

        // BLOQUEO DE MOVIMIENTO ESTILO POKÉMON
        if (dialogoManager != null && dialogoManager.hayDialogoActivo)
        {
            // Forzamos velocidad cero
            rb.linearVelocity = Vector2.zero;

            // Detenemos animaciones
            animator.SetBool("isMoving", false);
            animator.SetBool("isRunning", false);

            return; // Salimos de la función, ignorando el código de abajo
        }
        Vector2 finalMove = Vector2.zero;

        // --- SOLUCIÓN AL PROBLEMA DE VELOCIDAD ---
        // En lugar de usar moveInput.x directamente (que puede ser 0.7), 
        // comprobamos si es mayor o menor a cero y asignamos 1 o -1 puro.

        if (prioritizeX)
        {
            if (moveInput.x != 0)
                finalMove.x = moveInput.x > 0 ? 1 : -1;
            else if (moveInput.y != 0)
                finalMove.y = moveInput.y > 0 ? 1 : -1;
        }
        else
        {
            if (moveInput.y != 0)
                finalMove.y = moveInput.y > 0 ? 1 : -1;
            else if (moveInput.x != 0)
                finalMove.x = moveInput.x > 0 ? 1 : -1;
        }

        // Aplicar velocidad (ahora finalMove siempre tiene magnitud 1 o 0)
        float currentSpeed = isRunningInput ? runSpeed : walkSpeed;
        rb.linearVelocity = finalMove * currentSpeed;

        if (finalMove != Vector2.zero)
        {
            if (finalMove.x < 0) spriteRenderer.flipX = true;
            else if (finalMove.x > 0) spriteRenderer.flipX = false;

            float absX = Mathf.Abs(finalMove.x);

            animator.SetFloat("moveX", absX);
            animator.SetFloat("moveY", finalMove.y);
            animator.SetFloat("lastMoveX", absX);
            animator.SetFloat("lastMoveY", finalMove.y);

            animator.SetBool("isMoving", true);
            animator.SetBool("isRunning", isRunningInput);
        }
        else
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isRunning", false);
        }
    }
}