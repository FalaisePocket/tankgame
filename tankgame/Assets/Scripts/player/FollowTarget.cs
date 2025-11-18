using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform de la torreta (gira horizontalmente)")]
    public Transform turret;

    [Tooltip("Transform del cañón (gira verticalmente)")]
    public Transform cannon;

    [Tooltip("Transform del objetivo a seguir")]
    public Transform target;

    [Header("Configuración")]
    [Tooltip("Velocidad de rotación de la torreta (grados/segundo)")]
    public float turretRotationSpeed = 30f;

    [Tooltip("Velocidad de rotación del cañón (grados/segundo)")]
    public float cannonRotationSpeed = 20f;

    [Tooltip("Ángulo mínimo de elevación del cañón")]
    public float minCannonAngle = -10f;

    [Tooltip("Ángulo máximo de elevación del cañón")]
    public float maxCannonAngle = 45f;

    [Tooltip("Si está activo, la torreta seguirá al objetivo")]
    public bool trackingEnabled = true;

    private void Start()
    {
        // Validar referencias
        if (turret == null)
            Debug.LogError("Turret no asignado en " + gameObject.name);

        if (cannon == null)
            Debug.LogError("Cannon no asignado en " + gameObject.name);

        if (target == null)
            Debug.LogWarning("Target no asignado en " + gameObject.name);
    }

    private void Update()
    {
        if (!trackingEnabled || target == null || turret == null || cannon == null)
            return;

        AimAtTarget();
    }

    private void AimAtTarget()
    {
        // === ROTACIÓN DE LA TORRETA (Horizontal - Eje Y) ===
        Vector3 directionToTarget = target.position - turret.position;
        directionToTarget.y = 0; // Ignorar diferencia de altura para rotación horizontal

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                targetRotation,
                turretRotationSpeed * Time.deltaTime
            );
        }

        // === ROTACIÓN DEL CAÑÓN (Vertical - Eje X) ===
        Vector3 directionFromCannon = target.position - cannon.position;

        // Calcular el ángulo en el plano vertical
        float horizontalDistance = new Vector3(directionFromCannon.x, 0, directionFromCannon.z).magnitude;
        float targetAngle = Mathf.Atan2(directionFromCannon.y, horizontalDistance) * Mathf.Rad2Deg;

        // Limitar el ángulo entre min y max
        targetAngle = Mathf.Clamp(targetAngle, minCannonAngle, maxCannonAngle);

        // Obtener el ángulo actual del cañón
        float currentXAngle = cannon.localEulerAngles.x;
        if (currentXAngle > 180f) currentXAngle -= 360f;

        // Interpolar suavemente hacia el ángulo objetivo (invertido)
        float newXAngle = Mathf.MoveTowards(
            currentXAngle,
            -targetAngle,  // Negativo para invertir la rotación
            cannonRotationSpeed * Time.deltaTime
        );

        // Aplicar solo rotación en X (local)
        cannon.localEulerAngles = new Vector3(newXAngle, 0, 0);
    }

    // Método para activar/desactivar el seguimiento
    public void SetTracking(bool enabled)
    {
        trackingEnabled = enabled;
    }

    // Método para cambiar el objetivo
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Dibujar líneas de ayuda en el editor
    private void OnDrawGizmos()
    {
        if (target == null || turret == null || cannon == null)
            return;

        // Línea de la torreta al objetivo
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(turret.position, target.position);

        // Dirección del cañón
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cannon.position, cannon.forward * 5f);
    }
}