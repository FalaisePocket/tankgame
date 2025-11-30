using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraTerceraPersonaSuavizado : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo;
    [SerializeField] private LayerMask layerMask = ~0;

    [Header("Aim Target")]
    public Transform target;
    public Vector3 tagertOffSet = new Vector3(0, -2, 5);

    [Header("Configuración de Posición")]
    public Vector3 offset = new Vector3(0, 2, -5);

    [Header("Configuración de Rotación")]
    public float sensibilidadMouse = 2f;

    [Header("Límites de Rotación")]
    public float limiteVerticalMin = -30f;
    public float limiteVerticalMax = 60f;

    [Header("Configuración de Colisión")]
    public LayerMask capaColision = ~0; // Todas las capas por defecto
    public float radioColision = 0.3f; // Radio del SphereCast
    public float distanciaMinima = 0.5f; // Distancia mínima al objetivo
    public float suavizadoColision = 10f; // Velocidad de ajuste al colisionar

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Vector2 mouseDelta;
    private Rigidbody rbObjetivo;
    private float distanciaActual; // Distancia actual de la cámara
    private Vector3 direccionSuavizada;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (objetivo != null)
        {
            rbObjetivo = objetivo.GetComponent<Rigidbody>();
        }

        // Inicializar la distancia actual
        distanciaActual = offset.magnitude;
        direccionSuavizada = transform.forward;

    }

    void Update()
    {
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        // --- ROTACIÓN ---
        rotacionY += mouseDelta.x * sensibilidadMouse * 0.02f;
        rotacionX -= mouseDelta.y * sensibilidadMouse * 0.02f;
        rotacionX = Mathf.Clamp(rotacionX, limiteVerticalMin, limiteVerticalMax);
        mouseDelta = Vector2.zero;

        Vector3 posicionObjetivo = rbObjetivo != null ? rbObjetivo.position : objetivo.position;
        Quaternion rotacion = Quaternion.Euler(rotacionX, rotacionY, 0);
        
        Vector3 direccion = rotacion * offset.normalized;
        float distanciaDeseada = offset.magnitude;
        float distanciaFinal = DetectarColision(posicionObjetivo, direccion, distanciaDeseada);

        distanciaActual = Mathf.Lerp(distanciaActual, distanciaFinal, Time.deltaTime * suavizadoColision);

        Vector3 posicionFinal = posicionObjetivo + direccion * distanciaActual;
        transform.position = posicionFinal;
        transform.LookAt(posicionObjetivo + Vector3.up * 1.5f);

        // --- TARGET ---
        ActualizarTarget(posicionObjetivo, rotacion);
    }

    float DetectarColision(Vector3 origen, Vector3 direccion, float distanciaMax)
    {
        RaycastHit hit;

        // Punto de inicio ligeramente alejado del objetivo para evitar colisionar con él
        Vector3 puntoInicio = origen + Vector3.up * 0.5f;

        // SphereCast para detectar colisiones
        if (Physics.SphereCast(puntoInicio, radioColision, direccion, out hit, distanciaMax, capaColision))
        {
            // Si hay colisión, ajustar distancia
            float distanciaColision = hit.distance - radioColision;
            return Mathf.Max(distanciaColision, distanciaMinima);
        }

        // También hacer un raycast al suelo para evitar atravesarlo
        Vector3 posicionCamara = origen + direccion * distanciaMax;
        if (Physics.Raycast(posicionCamara, Vector3.down, out hit, 2f, capaColision))
        {
            if (hit.distance < 0.5f)
            {
                // La cámara está muy cerca del suelo, elevarla
                float ajuste = (0.5f - hit.distance) / distanciaMax;
                return distanciaMax * (1f - ajuste);
            }
        }

        return distanciaMax;
    }

    void ActualizarTarget(Vector3 posicionObjetivo, Quaternion rotacion)
    {
        /*
        if (target == null) return;

        // 1. Dirección de la cámara SUAVIZADA
        Vector3 direccionBruta = rotacion * Vector3.forward;
        direccionSuavizada = Vector3.Lerp(direccionSuavizada, direccionBruta, Time.deltaTime * 12f);

        // 2. Origen ESTABLE (la cámara, no el personaje)
        Vector3 origen = transform.position;

        // 3. Cálculo base (offset fijo, estable)
        Vector3 destinoBase = origen + direccionSuavizada * 30f;

        // 4. Corrección SOLO si colisiona
        RaycastHit hit;
        if (Physics.Raycast(origen, direccionSuavizada, out hit, 30f, layerMask))
        {
            destinoBase = hit.point;
        }

        // 5. Movimiento suave del target
        target.position = Vector3.Lerp(
            target.position,
            destinoBase,
            Time.deltaTime * 20f
        );*/


        
        if (target == null) return;

        Quaternion rotacionTarget = Quaternion.Euler(rotacionX, rotacionY, 0);
        Vector3 posicionDeseadaTarget = posicionObjetivo + rotacionTarget * tagertOffSet;

        target.transform.position = posicionDeseadaTarget;
        target.transform.LookAt(posicionDeseadaTarget + Vector3.down * 1.5f);
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        if (objetivo == null || !Application.isPlaying) return;

        Vector3 posicionObjetivo = rbObjetivo != null ? rbObjetivo.position : objetivo.position;
        Quaternion rotacion = Quaternion.Euler(rotacionX, rotacionY, 0);
        Vector3 direccion = rotacion * offset.normalized;

        // Dibujar línea de la cámara
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(posicionObjetivo, transform.position);

        // Dibujar esfera de colisión
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioColision);
    }
}