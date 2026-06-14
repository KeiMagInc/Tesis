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
        [Header("Ajustes de Velocidad Dinámica")]
        [Tooltip("Velocidad de seguimiento (suavidad) cuando está con Lupi")]
        public float suavidadLupi = 7f;
        [Tooltip("Velocidad de desplazamiento (suavidad) al viajar hacia un letrero o entre letreros")]
        public float suavidadLetrero = 2f;
        [Header("Ajustes de Altura Dinámica")]
        [Tooltip("Distancia en Y cuando el objetivo es Lupi (Jugador)")]
        public float offsetYLupi = 2.2f;
        [Tooltip("Distancia en Y cuando el objetivo es un Letrero u otro objeto")]
        public float offsetYLetrero = 0.5f;
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
                bool esLupi = objetivo.CompareTag("Player");
                float offsetYActual = esLupi ? offsetYLupi : offsetYLetrero;
                float suavidadActual = esLupi ? suavidadLupi : suavidadLetrero;
                Vector3 posicionDeseada = objetivo.position + new Vector3(offset.x, offsetYActual, offset.z);
                posicionDeseada.y += Mathf.Sin(Time.time * 2f) * 0.1f;
                transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavidadActual * Time.deltaTime);
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
            if (spriteLupi != null)
            {
                if (objetivo.localScale.x < 0)
                    mirandoIzquierda = true;
                else if (spriteLupi.flipX)
                    mirandoIzquierda = true;
            }
            else if (objetivo != null)
            {
                mirandoIzquierda = objetivo.position.x < transform.position.x;
            }
            float nuevoGiro = mirandoIzquierda ? -escalaOriginalX : escalaOriginalX;
            transform.localScale = new Vector3(nuevoGiro, transform.localScale.y, transform.localScale.z);
        }
        public void CambiarObjetivo(Transform nuevoObjetivo)
        {
            objetivo = nuevoObjetivo;
            if (objetivo != null)
            {
                spriteLupi = objetivo.GetComponent<SpriteRenderer>();
            }
            else
            {
                spriteLupi = null;
            }
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