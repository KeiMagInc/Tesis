using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sprite;

    [Header("Efecto de Brillo")]
    public Light2D luzResplandor;

    private bool nivelCompletado = false;
    private bool yaSalioHaciaAdelante = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        // Empezamos con todo oculto/apagado
        if (sprite != null) sprite.enabled = false;
        if (luzResplandor != null) luzResplandor.enabled = false;
    }

    public void AparecerYActivar()
    {
        if (nivelCompletado) return; // Seguridad para no activar dos veces

        nivelCompletado = true;
        yaSalioHaciaAdelante = false;

        // 1. Mostramos el sprite y activamos la animación de la bandera
        if (sprite != null) sprite.enabled = true;
        if (anim != null) anim.SetBool("activado", true);

        // 2. Encendemos la luz permanentemente
        if (luzResplandor != null)
        {
            luzResplandor.enabled = true;
        }

        Debug.Log("Checkpoint: Activado y brillando permanentemente.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cuando Lupi sale de la zona, detenemos la animación (bandera quieta)
        // Pero la LUZ se mantiene prendida porque el nivel ya está hecho.
        if (other.CompareTag("Player") && nivelCompletado && !yaSalioHaciaAdelante)
        {
            if (anim != null) anim.SetBool("activado", false);
            yaSalioHaciaAdelante = true;
        }
    }
}