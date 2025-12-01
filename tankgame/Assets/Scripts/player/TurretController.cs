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
        // === ROTACIÓN DE LA TORRETA (horizontal, global) ===
        Vector3 dir = target.position - turret.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                look,
                turretRotationSpeed * Time.deltaTime
            );
        }

        /// === ROTACIÓN DEL CAÑÓN (vertical, velocidad fija) ===
        Vector3 dirCannon = target.position - cannon.position;

        // Cálculo del ángulo vertical
        float hDist = new Vector3(dirCannon.x, 0, dirCannon.z).magnitude;
        float targetAngle = Mathf.Atan2(dirCannon.y, hDist) * Mathf.Rad2Deg;

        // INVERTIR dirección si el cañón se mueve al revés
        targetAngle = -targetAngle;

        // Limitar ángulos permitidos
        targetAngle = Mathf.Clamp(targetAngle, minCannonAngle, maxCannonAngle);

        // Obtener rotación actual en X
        float currentX = cannon.localEulerAngles.x;
        if (currentX > 180f) currentX -= 360f;

        // Calcular nueva rotación en X (velocidad fija)
        float newX = Mathf.MoveTowards(currentX, targetAngle, cannonRotationSpeed * Time.deltaTime);

        // Aplicar solo rotación en X
        cannon.localEulerAngles = new Vector3(newX, 0, 0);

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