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
        if (sprite != null) sprite.enabled = false;
        if (luzResplandor != null) luzResplandor.enabled = false;
    }
    public void ResetearCheckpoint()
    {
        nivelCompletado = false;
        yaSalioHaciaAdelante = false;
        if (sprite != null) sprite.enabled = false;
        if (luzResplandor != null) luzResplandor.enabled = false;
        if (anim != null) anim.SetBool("activado", false); 
        Debug.Log("Checkpoint: Reseteado y oculto.");
    }
    public void AparecerYActivar()
    {
        if (nivelCompletado) return; 
        nivelCompletado = true;
        yaSalioHaciaAdelante = false;
        if (sprite != null) sprite.enabled = true;
        if (anim != null) anim.SetBool("activado", true);
        if (luzResplandor != null)
        {
            luzResplandor.enabled = true;
        }
        Debug.Log("Checkpoint: Activado y brillando permanentemente.");
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && nivelCompletado && !yaSalioHaciaAdelante)
        {
            if (anim != null) anim.SetBool("activado", false);
            yaSalioHaciaAdelante = true;
        }
    }
}