using UnityEngine;
using TMPro;
using System.Collections;
namespace Mundo2
{
    public class AndyController : MonoBehaviour
    {
        [Header("Audio y Efectos de Texto")]
        public AudioSource fuenteVoz;
        public float velocidadTexto = 0.03f;
        private Coroutine rutinaEscribir;
        [Header("Configuración de Seguimiento")]
        public Transform objetivo;
        public Vector3 offset = new Vector3(0f, 1.3f, 0f); 
        public float suavidad = 7f;
        [Header("Interfaz de Diálogo")]
        public GameObject panelDialogo;
        public TextMeshProUGUI textoMensaje;
        [Header("Ajustes de Animación")]
        public float distanciaParaVolar = 0.2f;
        private Animator anim;
        private SpriteRenderer spriteAndy;
        private SpriteRenderer spriteLupi;
        private float escalaOriginalX;
        void Awake()
        {
            anim = GetComponent<Animator>();
            spriteAndy = GetComponent<SpriteRenderer>();
            if (objetivo != null) spriteLupi = objetivo.GetComponent<SpriteRenderer>();
            escalaOriginalX = Mathf.Abs(transform.localScale.x);
        }
        void Update()
        {
            if (objetivo != null)
            {
                Vector3 posicionDeseada = objetivo.position + offset;
                posicionDeseada.y += Mathf.Sin(Time.time * 2f) * 0.1f;
                transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavidad * Time.deltaTime);
                ActualizarGiro();
                if (anim != null && anim.runtimeAnimatorController != null)
                {
                    float distanciaAlObjetivo = Vector3.Distance(transform.position, posicionDeseada);
                    anim.SetBool("volando", distanciaAlObjetivo > distanciaParaVolar);
                }
            }
        }
        void ActualizarGiro()
        {
            bool mirandoIzquierda = false;

            if (objetivo.localScale.x < 0)
                mirandoIzquierda = true;
            else if (spriteLupi != null && spriteLupi.flipX)
                mirandoIzquierda = true;
            float nuevoGiro = mirandoIzquierda ? -escalaOriginalX : escalaOriginalX;
            transform.localScale = new Vector3(nuevoGiro, transform.localScale.y, transform.localScale.z);
        }
        public void Decir(string mensaje, AudioClip clipVoz = null)
        {
            if (panelDialogo != null && textoMensaje != null)
            {
                panelDialogo.SetActive(true);
                if (rutinaEscribir != null) StopCoroutine(rutinaEscribir);
                if (fuenteVoz != null)
                {
                    if (clipVoz != null)
                    {
                        fuenteVoz.clip = clipVoz;
                        fuenteVoz.Play(); 
                    }
                }
                rutinaEscribir = StartCoroutine(EscribirTexto(mensaje));
            }
        }
        private IEnumerator EscribirTexto(string mensaje)
        {
            textoMensaje.text = "";
            foreach (char letra in mensaje.ToCharArray())
            {
                textoMensaje.text += letra;
                yield return new WaitForSeconds(velocidadTexto);
            }
        }
    }
}