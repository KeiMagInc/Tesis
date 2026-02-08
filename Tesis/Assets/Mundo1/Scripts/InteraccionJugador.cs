using UnityEngine;
using UnityEngine.InputSystem;

public class InteraccionJugador : MonoBehaviour
{
    [Header("Configuración")]
    public Transform puntoAgarre;
    public float radioDeteccion = 1.2f;
    public LayerMask capaInteractuable;
    public LayerMask capaBalsa;

    private ItemRecogible itemCargado;
    private ArrayManager arrayManager;
    private DialogoManager dialogoManager;
    private AndyInteraccion andy; // Referencia directa a Andy


    void Awake()
    {
        arrayManager = Object.FindFirstObjectByType<ArrayManager>();
        dialogoManager = Object.FindFirstObjectByType<DialogoManager>();
        andy = Object.FindFirstObjectByType<AndyInteraccion>();
    }

    // Añade esta variable arriba en InteraccionJugador
    public LayerMask capaAndy;
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // SI HAY DIÁLOGO ACTIVO: Hablar con Andy directamente (sin importar la distancia)
        // Esto permite adelantar el texto aunque Andy esté lejos después de un Trigger.
        if (dialogoManager != null && dialogoManager.hayDialogoActivo)
        {
            andy.Hablar();
            return;
        }

        // Si no hay diálogo, lógica normal
        Collider2D colAndyCerca = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaAndy);
        if (colAndyCerca != null)
        {
            MisionManager mm = Object.FindFirstObjectByType<MisionManager>();
            if (mm.tutorialTerminado && !Object.FindFirstObjectByType<DialogoManager>().hayDialogoActivo)
            {
                mm.PreguntarCambioNivel();
            }
            else
            {
                colAndyCerca.GetComponent<AndyInteraccion>().Hablar();
            }
            return;
        }

        if (itemCargado == null)
            IntentarAgarrar();
        else
            IntentarSoltar();
    }

    void IntentarAgarrar()
    {
        // 1. Buscar items en el suelo o balsa
        Collider2D colItem = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaInteractuable);

        if (colItem != null)
        {
            itemCargado = colItem.GetComponent<ItemRecogible>();
            if (itemCargado != null)
            {
                // SI EL ITEM ESTABA EN UNA BALSA, DEBEMOS LIMPIAR EL ARREGLO
                if (itemCargado.estaEnBalsa)
                {
                    // Buscamos qué balsa tiene debajo el item justo ahora
                    Collider2D colBalsa = Physics2D.OverlapCircle(itemCargado.transform.position, 0.5f, capaBalsa);
                    if (colBalsa != null)
                    {
                        BalsaSlot slot = colBalsa.GetComponent<BalsaSlot>();

                        MisionManager mm = Object.FindFirstObjectByType<MisionManager>();
                        mm.ValidarAccion(-1, itemCargado.nombreItem);
                        arrayManager.EliminarDe(slot.indice);
                        Debug.Log("Item extraído del índice: " + slot.indice);
                    }
                }

                // Agarre visual
                itemCargado.transform.SetParent(puntoAgarre);
                itemCargado.transform.localPosition = Vector3.zero;
                itemCargado.GetComponent<Collider2D>().enabled = false;
                itemCargado.estaEnBalsa = false; // IMPORTANTE: Ya no está en la balsa
            }
        }
    }

    void IntentarSoltar()
    {
        // 1. Buscar si hay una balsa debajo de Lupe justo ahora
        Collider2D colBalsa = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaBalsa);

        if (colBalsa != null)
        {
            BalsaSlot slot = colBalsa.GetComponent<BalsaSlot>();
            string nombreItem = itemCargado.nombreItem;
            bool exito = arrayManager.InsertarEn(slot.indice, itemCargado);

            if (exito)
            {
                // AVISAR AL MANAGER DE MISIONES
                Object.FindFirstObjectByType<MisionManager>().ValidarAccion(slot.indice, nombreItem);
                FinalizarSoltado(true);
            }
        }
        else
        {
            // Soltar en el suelo
            FinalizarSoltado(false);
        }
    }

    void FinalizarSoltado(bool esBalsa)
    {
        itemCargado.transform.SetParent(null);

        if (!esBalsa)
        {
            // Si es el suelo, lo activamos, pero le damos un pequeño empujón 
            // hacia adelante para que no aparezca exactamente en los pies de Lupe
            itemCargado.transform.position += transform.up * -0.5f;
            itemCargado.GetComponent<Collider2D>().enabled = true;
        }
        // Si ES balsa, no hacemos nada aquí. 
        // El ArrayManager llamará a MoverA y ese script lo activará al llegar.

        itemCargado = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}