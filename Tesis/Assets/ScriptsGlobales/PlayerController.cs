using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Velocidad")]
    [Header("Estados Globales")]
    public bool controlesBloqueados = false; 
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    public float acceleration = 40f; 
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isRunningInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool prioritizeX = false;
    private DialogoManager dialogoManager;
    void Start()
    {
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
        bool hayBloqueoExterno = (dialogoManager != null && dialogoManager.hayDialogoActivo);
        if (hayBloqueoExterno || controlesBloqueados)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetBool("isRunning", false);
            return;
        }
        Vector2 finalMove = Vector2.zero;
        if (prioritizeX)
        {
            if (moveInput.x != 0) finalMove.x = moveInput.x > 0 ? 1 : -1;
            else if (moveInput.y != 0) finalMove.y = moveInput.y > 0 ? 1 : -1;
        }
        else
        {
            if (moveInput.y != 0) finalMove.y = moveInput.y > 0 ? 1 : -1;
            else if (moveInput.x != 0) finalMove.x = moveInput.x > 0 ? 1 : -1;
        }
        float targetSpeed = isRunningInput ? runSpeed : walkSpeed;
        Vector2 targetVelocity = finalMove * targetSpeed;

        if (finalMove == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        float currentSpeedMagnitude = rb.linearVelocity.magnitude;
        if (currentSpeedMagnitude > 0.01f) 
        {
            if (finalMove.x < 0) spriteRenderer.flipX = true;
            else if (finalMove.x > 0) spriteRenderer.flipX = false;
            if (finalMove != Vector2.zero)
            {
                animator.SetFloat("moveX", Mathf.Abs(finalMove.x));
                animator.SetFloat("moveY", finalMove.y);
                animator.SetFloat("lastMoveX", Mathf.Abs(finalMove.x));
                animator.SetFloat("lastMoveY", finalMove.y);
            }
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