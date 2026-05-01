using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sprite;
    private bool nivelCompletado = false;
    private bool yaSalioHaciaAdelante = false; // Bloqueo para que no se mueva al regresar

    void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        // 1. Empieza oculto
        if (sprite != null) sprite.enabled = false;
    }

    // Se llama desde la lógica cuando ganas el nivel
    public void AparecerYActivar()
    {
        nivelCompletado = true;
        yaSalioHaciaAdelante = false; // Reset por si reinicias nivel

        // 2. Aparece y se mueve (Celebración de victoria)
        if (sprite != null) sprite.enabled = true;
        if (anim != null) anim.SetBool("activado", true);

        Debug.Log("Checkpoint: Aparece y ondea porque el nivel terminó.");
    }

    // ELIMINAMOS el OnTriggerEnter: 
    // Al quitarlo, si Lupi regresa de otra zona, la bandera NO se activará.

    private void OnTriggerExit2D(Collider2D other)
    {
        // 3. Cuando Lupi sale del sector hacia el siguiente nivel
        if (other.CompareTag("Player") && nivelCompletado && !yaSalioHaciaAdelante)
        {
            if (anim != null) anim.SetBool("activado", false);
            yaSalioHaciaAdelante = true; // Bloqueamos el movimiento para siempre

            Debug.Log("Checkpoint: Lupi dejó la zona. Bandera quieta permanentemente.");
        }
    }
}