using UnityEngine;
using UnityEngine.UI;

public class IndicadorPosicion : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetoASegir; // El objeto que se mueve
    public RectTransform indicadorUI; // La imagen/icono en el Canvas
    public Text textoCoordenadasUI; // Opcional: texto para mostrar coordenadas

    [Header("Configuración")]
    public Vector2 offset = Vector2.zero; // Ajuste de posición del indicador
    public bool mostrarCoordenadas = true;

    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = indicadorUI.GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (objetoASegir == null || indicadorUI == null) return;

        // Convertir la posición del objeto 3D a coordenadas de pantalla
        Vector3 posicionPantalla = mainCamera.WorldToScreenPoint(objetoASegir.position);

        // Verificar si el objeto está frente a la cámara
        if (posicionPantalla.z > 0)
        {
            // Convertir a coordenadas del Canvas
            Vector2 posicionCanvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                posicionPantalla,
                canvas.worldCamera,
                out posicionCanvas
            );

            // Aplicar la posición al indicador
            indicadorUI.localPosition = posicionCanvas + offset;
            indicadorUI.gameObject.SetActive(true);

            // Actualizar texto de coordenadas (opcional)
            if (mostrarCoordenadas && textoCoordenadasUI != null)
            {
                textoCoordenadasUI.text = $"X: {objetoASegir.position.x:F1}\n" +
                                          $"Y: {objetoASegir.position.y:F1}\n" +
                                          $"Z: {objetoASegir.position.z:F1}";
            }
        }
        else
        {
            // Ocultar indicador si el objeto está detrás de la cámara
            indicadorUI.gameObject.SetActive(false);
        }
    }
}