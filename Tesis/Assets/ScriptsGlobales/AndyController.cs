using UnityEngine;
using TMPro; // Para el texto
using System.Collections;

namespace Mundo2
{
    public class AndyController : MonoBehaviour
    {
        [Header("Audio y Efectos de Texto")]
        public AudioSource fuenteVoz; // Añade un AudioSource a Andy y arrástralo aquí
        public float velocidadTexto = 0.03f; // Tiempo que tarda en aparecer cada letra
        private Coroutine rutinaEscribir;
        [Header("Configuración de Seguimiento")]
        public Transform objetivo; // Lupi
        public Vector3 offset = new Vector3(-1f, 1f, 0f);
        public float suavidad = 5f;

        [Header("Interfaz de Diálogo (Rectángulo Abajo)")]
        public GameObject panelDialogo; // Arrastra el Panel_Dialogo_Andy
        public TextMeshProUGUI textoMensaje; // Arrastra el Texto_Mensaje
        public float tiempoVisible = 4f;

        private Coroutine rutinaOcultar;

        void Update()
        {
            if (objetivo != null)
            {
                // Seguimiento suave que ya tenías
                Vector3 posicionDeseada = objetivo.position + offset;

                // Efecto extra: pequeño flote para que parezca que vuela
                posicionDeseada.y += Mathf.Sin(Time.time * 2f) * 0.1f;

                transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavidad * Time.deltaTime);

                // Girar a Andy según hacia dónde mira Lupi
                if (objetivo.localScale.x > 0) transform.localScale = new Vector3(1, 1, 1);
                else transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        // Esta es la función que llamaremos para proyectar los mensajes en el rectángulo
        // Dentro de AndyController.cs
        public void Decir(string mensaje, AudioClip clipVoz = null)
        {
            if (panelDialogo != null && textoMensaje != null)
            {
                panelDialogo.SetActive(true);

                // 1. Detenemos cualquier texto o audio anterior
                if (rutinaEscribir != null) StopCoroutine(rutinaEscribir);
                if (fuenteVoz != null) fuenteVoz.Stop();

                // 2. Reproducimos el nuevo audio de IA (si se envió uno)
                if (clipVoz != null && fuenteVoz != null)
                {
                    fuenteVoz.clip = clipVoz;
                    fuenteVoz.Play();
                }

                // 3. Iniciamos el efecto letra por letra
                rutinaEscribir = StartCoroutine(EscribirTexto(mensaje));
            }
            Debug.Log("<color=yellow>ANDY DICE: </color>" + mensaje);
        }

        private IEnumerator EscribirTexto(string mensaje)
        {
            textoMensaje.text = ""; // Vaciamos el texto inicial
            foreach (char letra in mensaje.ToCharArray())
            {
                textoMensaje.text += letra; // Añadimos una letra
                yield return new WaitForSeconds(velocidadTexto); // Esperamos una fracción de segundo
            }
        }
    }
}