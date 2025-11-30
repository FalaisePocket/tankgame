using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform de la torreta (gira horizontalmente)")]
    public Transform turret;

    [Tooltip("Transform del can (gira verticalmente)")]
    public Transform cannon;

    [Tooltip("Transform del objetivo a seguir")]
    public Transform target;

    [Header("Configuracin")]
    [Tooltip("Velocidad de rotacin de la torreta (grados/segundo)")]
    public float turretRotationSpeed = 30f;

    [Tooltip("Velocidad de rotacin del can (grados/segundo)")]
    public float cannonRotationSpeed = 20f;

    [Tooltip("angulo minimo de elevacin del can")]
    public float minCannonAngle = -10f;

    [Tooltip("angulo maximo de elevacin del can")]
    public float maxCannonAngle = 45f;

    [Tooltip("Si esta activo, la torreta seguira al objetivo")]
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
        // === ROTACIN DE LA TORRETA (Horizontal - Eje Y) ===
        Vector3 directionToTarget = target.position - turret.position;
        directionToTarget.y = 0; // Ignorar diferencia de altura para rotacin horizontal

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                targetRotation,
                turretRotationSpeed * Time.deltaTime
            );
        }

        // === ROTACIN DEL CAN (Vertical - Eje X) ===
        Vector3 directionFromCannon = target.position - cannon.position;

        // Calcular el ngulo en el plano vertical
        float horizontalDistance = new Vector3(directionFromCannon.x, 0, directionFromCannon.z).magnitude;
        float targetAngle = Mathf.Atan2(directionFromCannon.y, horizontalDistance) * Mathf.Rad2Deg;

        // Limitar el ngulo entre min y max
        targetAngle = Mathf.Clamp(targetAngle, minCannonAngle, maxCannonAngle);

        // Obtener el ngulo actual del can
        float currentXAngle = cannon.localEulerAngles.x;
        if (currentXAngle > 180f) currentXAngle -= 360f;

        // Interpolar suavemente hacia el ngulo objetivo (invertido)
        float newXAngle = Mathf.MoveTowards(
            currentXAngle,
            -targetAngle,  // Negativo para invertir la rotacin
            cannonRotationSpeed * Time.deltaTime
        );

        // Aplicar solo rotacin en X (local)
        cannon.localEulerAngles = new Vector3(newXAngle, 0, 0);
    }

    // Mtodo para activar/desactivar el seguimiento
    public void SetTracking(bool enabled)
    {
        trackingEnabled = enabled;
    }

    // Mtodo para cambiar el objetivo
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Dibujar lneas de ayuda en el editor
    private void OnDrawGizmos()
    {
        if (target == null || turret == null || cannon == null)
            return;

        // Lnea de la torreta al objetivo
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(turret.position, target.position);

        // Direccin del can
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cannon.position, cannon.forward * 5f);
    }
}