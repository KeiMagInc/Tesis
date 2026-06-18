using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class AnimadorLineaAgua : MonoBehaviour
{
    [Header("Velocidad de Flujo")]
    [Tooltip("Velocidad en el eje X para simular el avance del agua.")]
    public float velocidadX = -1f;
    public float velocidadY = 0f;
    private LineRenderer lineRenderer;
    private Material materialDeLinea;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
            materialDeLinea = lineRenderer.material;
    }
    void Update()
    {
        if (materialDeLinea == null) return;
        float offsetX = Time.time * velocidadX;
        float offsetY = Time.time * velocidadY;
        materialDeLinea.SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));
        if (materialDeLinea.HasProperty("_BaseMap"))
            materialDeLinea.SetTextureOffset("_BaseMap", new Vector2(offsetX, offsetY));
    }
}