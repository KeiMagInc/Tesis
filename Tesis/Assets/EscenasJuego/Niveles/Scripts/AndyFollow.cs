using UnityEngine;
using TMPro; // Para el texto
using System.Collections;

namespace Mundo2
{
    public class AndyFollow : MonoBehaviour
    {
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
        public void Decir(string mensaje)
        {
            if (panelDialogo != null && textoMensaje != null)
            {
                // Si ya estaba hablando, reiniciamos el tiempo
                if (rutinaOcultar != null) StopCoroutine(rutinaOcultar);

                panelDialogo.SetActive(true);
                textoMensaje.text = mensaje;

                rutinaOcultar = StartCoroutine(OcultarDialogo());
            }

            // Mantenemos el log por si acaso
            Debug.Log("<color=yellow>ANDY DICE: </color>" + mensaje);
        }

        IEnumerator OcultarDialogo()
        {
            yield return new WaitForSeconds(tiempoVisible);
            panelDialogo.SetActive(false);
        }
    }
}