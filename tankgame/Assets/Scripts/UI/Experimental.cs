using UnityEngine;
using UnityEngine.UI;

public class TankAimingUI : MonoBehaviour
{
    [Header("Referencias de la Cámara")]
    [Tooltip("El script de la cámara que contiene el target")]
    public CamaraTerceraPersonaSuavizado camaraScript;

    [Header("Referencias del Tanque")]
    [Tooltip("Transform de la punta del cañón")]
    public Transform cannonTip;

    [Header("UI - Retículas")]
    [Tooltip("Imagen UI para el punto de mira deseado (donde quieres apuntar)")]
    public RectTransform reticulaObjetivo;

    [Tooltip("Imagen UI para el punto donde apunta el cañón")]
    public RectTransform reticulaCanon;

    [Header("Configuración Visual")]
    [Tooltip("Color de la retícula objetivo")]
    public Color colorObjetivo = Color.green;

    [Tooltip("Color de la retícula del cañón")]
    public Color colorCanon = Color.red;

    [Tooltip("Tamaño de las retículas")]
    public float tamanoReticula = 30f;

    [Tooltip("Distancia máxima de raycast para el apuntado")]
    public float distanciaMaxima = 1000f;

    [Header("Feedback Visual")]
    [Tooltip("Cambiar color cuando están alineadas")]
    public bool usarFeedbackAlineacion = true;

    [Tooltip("Color cuando están alineadas")]
    public Color colorAlineado = Color.yellow;

    [Tooltip("Distancia en píxeles para considerar alineación")]
    public float umbralAlineacion = 50f;

    private Camera camara;
    private Image imagenObjetivo;
    private Image imagenCanon;

    void Start()
    {
        // Obtener la cámara principal
        camara = Camera.main;

        // Validar referencias
        if (camaraScript == null)
            Debug.LogError("No se asignó el script de cámara");

        if (cannonTip == null)
            Debug.LogError("No se asignó la punta del cañón");

        // Configurar retículas
        ConfigurarReticula(reticulaObjetivo, colorObjetivo, out imagenObjetivo);
        ConfigurarReticula(reticulaCanon, colorCanon, out imagenCanon);
    }

    void ConfigurarReticula(RectTransform reticula, Color color, out Image imagen)
    {
        imagen = null;

        if (reticula == null)
        {
            Debug.LogWarning("Retícula no asignada");
            return;
        }

        // Configurar tamaño
        reticula.sizeDelta = new Vector2(tamanoReticula, tamanoReticula);

        // Obtener o agregar componente Image
        imagen = reticula.GetComponent<Image>();
        if (imagen == null)
            imagen = reticula.gameObject.AddComponent<Image>();

        imagen.color = color;

        // Asegurar que esté activo
        reticula.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        if (camara == null || camaraScript == null) return;

        // Actualizar posición de la retícula objetivo
        ActualizarReticulaObjetivo();

        // Actualizar posición de la retícula del cañón
        ActualizarReticulaCanon();

        // Actualizar feedback visual
        if (usarFeedbackAlineacion)
            ActualizarFeedbackAlineacion();
    }

    void ActualizarReticulaObjetivo()
    {
        if (reticulaObjetivo == null || camaraScript.target == null) return;

        // Convertir posición del target a espacio de pantalla
        Vector3 posicionPantalla = camara.WorldToScreenPoint(camaraScript.target.position);

        // Verificar si está frente a la cámara
        if (posicionPantalla.z > 0)
        {
            reticulaObjetivo.position = posicionPantalla;
            reticulaObjetivo.gameObject.SetActive(true);
        }
        else
        {
            reticulaObjetivo.gameObject.SetActive(false);
        }
    }

    void ActualizarReticulaCanon()
    {
        if (reticulaCanon == null || cannonTip == null) return;

        // Obtener la dirección del cañón
        Vector3 direccionCanon = cannonTip.forward;
        Vector3 origenCanon = cannonTip.position;

        // Hacer raycast para encontrar el punto de impacto
        RaycastHit hit;
        Vector3 puntoImpacto;

        if (Physics.Raycast(origenCanon, direccionCanon, out hit, distanciaMaxima))
        {
            puntoImpacto = hit.point;
        }
        else
        {
            // Si no hay colisión, usar un punto lejano
            puntoImpacto = origenCanon + direccionCanon * distanciaMaxima;
        }

        // Convertir a espacio de pantalla
        Vector3 posicionPantalla = camara.WorldToScreenPoint(puntoImpacto);

        // Verificar si está frente a la cámara
        if (posicionPantalla.z > 0)
        {
            reticulaCanon.position = posicionPantalla;
            reticulaCanon.gameObject.SetActive(true);
        }
        else
        {
            reticulaCanon.gameObject.SetActive(false);
        }
    }

    void ActualizarFeedbackAlineacion()
    {
        if (imagenObjetivo == null || imagenCanon == null) return;
        if (reticulaObjetivo == null || reticulaCanon == null) return;

        // Calcular distancia entre retículas
        float distancia = Vector2.Distance(reticulaObjetivo.position, reticulaCanon.position);

        // Si están cerca, cambiar color
        if (distancia < umbralAlineacion)
        {
            imagenObjetivo.color = colorAlineado;
            imagenCanon.color = colorAlineado;
        }
        else
        {
            imagenObjetivo.color = colorObjetivo;
            imagenCanon.color = colorCanon;
        }
    }

    // Método público para obtener si están alineadas
    public bool EstanAlineadas()
    {
        if (reticulaObjetivo == null || reticulaCanon == null) return false;

        float distancia = Vector2.Distance(reticulaObjetivo.position, reticulaCanon.position);
        return distancia < umbralAlineacion;
    }

    // Método público para obtener la distancia entre retículas
    public float GetDistanciaReticulas()
    {
        if (reticulaObjetivo == null || reticulaCanon == null) return float.MaxValue;

        return Vector2.Distance(reticulaObjetivo.position, reticulaCanon.position);
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || cannonTip == null) return;

        // Dibujar línea del cañón
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cannonTip.position, cannonTip.forward * 10f);

        // Dibujar punto de impacto
        RaycastHit hit;
        if (Physics.Raycast(cannonTip.position, cannonTip.forward, out hit, distanciaMaxima))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hit.point, 0.5f);
        }
    }
}