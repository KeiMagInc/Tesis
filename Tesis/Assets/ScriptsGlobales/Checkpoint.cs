using UnityEngine;
public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sprite;
    private bool nivelCompletado = false;
    private bool yaSalioHaciaAdelante = false; 
    void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;
    }
    public void AparecerYActivar()
    {
        nivelCompletado = true;
        yaSalioHaciaAdelante = false; 
        if (sprite != null) sprite.enabled = true;
        if (anim != null) anim.SetBool("activado", true);

        Debug.Log("Checkpoint: Aparece y ondea porque el nivel terminó.");
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && nivelCompletado && !yaSalioHaciaAdelante)
        {
            if (anim != null) anim.SetBool("activado", false);
            yaSalioHaciaAdelante = true; 
            Debug.Log("Checkpoint: Lupi dejó la zona. Bandera quieta permanentemente.");
        }
    }
}