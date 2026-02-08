using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    public string nombreItem;
    public bool estaEnBalsa = false;
    private Collider2D miCollider;
     void Awake()
    {
        miCollider = GetComponent<Collider2D>();
    }
    
    // Método para mover la caja suavemente cuando haya Shifting
    public void MoverA(Vector3 destino)
    {
         if(miCollider != null) miCollider.enabled = false;
        StopAllCoroutines();
        StartCoroutine(MoverSuave(destino));
    }

    private System.Collections.IEnumerator MoverSuave(Vector3 destino)
    {
        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, 10f * Time.deltaTime);
            yield return null;
        }
        transform.position = destino;
        if(miCollider != null) miCollider.enabled = true;
    }
}