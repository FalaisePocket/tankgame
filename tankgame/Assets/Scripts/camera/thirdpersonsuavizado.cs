using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraTerceraPersonaSuavizado : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo;
    [SerializeField] private LayerMask layerMask = ~0;

    [Header("Aim Target")]
    public Transform target;
    public Vector3 targetOffSet = new Vector3(0, 0, 0);

    [Header("Configuraci�n de Posici�n")]
    public Vector3 offset = new Vector3(0, 2, -5);

    [Header("Configuraci�n de Rotaci�n")]
    public float sensibilidadMouse = 2f;

    [Header("L�mites de Rotaci�n")]
    public float limiteVerticalMin = -30f;
    public float limiteVerticalMax = 60f;

    [Header("Configuraci�n de Colisi�n")]
    public LayerMask capaColision = ~0; // Todas las capas por defecto
    public float radioColision = 0.3f; // Radio del SphereCast
    public float distanciaMinima = 0.5f; // Distancia m�nima al objetivo
    public float suavizadoColision = 10f; // Velocidad de ajuste al colisionar

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Vector2 mouseDelta;
    private Rigidbody rbObjetivo;
    private float distanciaActual; // Distancia actual de la c�mara
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
        if (objetivo == null)
        {
        // Evitar que la cámara siga intentando moverse si el objetivo murió
        return;
        }
        // --- ROTACI�N ---
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

        // Punto de inicio ligeramente alejado del objetivo para evitar colisionar con �l
        Vector3 puntoInicio = origen + Vector3.up * 0.5f;

        // SphereCast para detectar colisiones
        if (Physics.SphereCast(puntoInicio, radioColision, direccion, out hit, distanciaMax, capaColision))
        {
            // Si hay colisi�n, ajustar distancia
            float distanciaColision = hit.distance - radioColision;
            return Mathf.Max(distanciaColision, distanciaMinima);
        }

        // Tambi�n hacer un raycast al suelo para evitar atravesarlo
        Vector3 posicionCamara = origen + direccion * distanciaMax;
        if (Physics.Raycast(posicionCamara, Vector3.down, out hit, 2f, capaColision))
        {
            if (hit.distance < 0.5f)
            {
                // La c�mara est� muy cerca del suelo, elevarla
                float ajuste = (0.5f - hit.distance) / distanciaMax;
                return distanciaMax * (1f - ajuste);
            }
        }

        return distanciaMax;
    }

void ActualizarTarget(Vector3 posicionObjetivo, Quaternion rotacion)
{
    if (target == null) return;

    // Origen del rayo desde la posición de la cámara
    Vector3 origen = transform.position;

    // Ajustar la dirección para apuntar más arriba
    // Puedes modificar el valor 0.1f para controlar cuánto sube (valores más altos = más arriba)
    Vector3 direccion = transform.forward + transform.up * targetOffSet.y;
    direccion.Normalize(); // Normalizar para mantener la dirección correcta

    RaycastHit hit;
    if (Physics.Raycast(origen, direccion, out hit, 1000f, layerMask))
        target.position = hit.point;
    else
        target.position = origen + direccion * 1000f;
}



    // Visualizaci�n en el editor
    void OnDrawGizmos()
    {
        if (objetivo == null || !Application.isPlaying) return;

        Vector3 posicionObjetivo = rbObjetivo != null ? rbObjetivo.position : objetivo.position;
        Quaternion rotacion = Quaternion.Euler(rotacionX, rotacionY, 0);
        Vector3 direccion = rotacion * offset.normalized;

        // Dibujar l�nea de la c�mara
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(posicionObjetivo, transform.position);

        // Dibujar esfera de colisi�n
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioColision);
    }
}