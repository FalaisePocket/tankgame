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
        /*
        // === ROTACIÓN DE LA TORRETA (horizontal, global) ===
        Vector3 dir = target.position - turret.position;
        dir.y = 0; // eliminamos inclinación vertical

        if (dir.sqrMagnitude > 0.001f)
        {
            // Calcular dirección objetivo en espacio GLOBAL
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            float targetYAngle = targetRotation.eulerAngles.y;
            
            // Convertir a espacio LOCAL del parent (tanque)
            Transform parent = turret.parent;
            if (parent != null)
            {
                // Calcular diferencia entre rotación objetivo y rotación del tanque
                float parentYAngle = parent.eulerAngles.y;
                float localTargetY = targetYAngle - parentYAngle;
                
                // Normalizar ángulo
                if (localTargetY > 180f) localTargetY -= 360f;
                if (localTargetY < -180f) localTargetY += 360f;
                
                // Obtener rotación local actual
                float currentLocalY = turret.localEulerAngles.y;
                if (currentLocalY > 180f) currentLocalY -= 360f;
                
                // Mover suavemente hacia el ángulo objetivo
                float newLocalY = Mathf.MoveTowardsAngle(
                    currentLocalY,
                    localTargetY,
                    turretRotationSpeed * Time.deltaTime
                );
                
                // Aplicar SOLO rotación LOCAL en Y
                // Esto mantiene X y Z relativos al tanque
                turret.localRotation = Quaternion.Euler(0, newLocalY, 0);
            }
        }
        */
// ===== ROTACIÓN DE LA TORRETA (CORREGIDA: usa espacio LOCAL del padre) =====
Transform parent = turret.parent;
if (parent == null) return;

Vector3 dirWorld = target.position - turret.position;

// proyectar sobre el plano "horizontal" del padre (usar parent.up)
Vector3 dirProjected = Vector3.ProjectOnPlane(dirWorld, parent.up);

if (dirProjected.sqrMagnitude > 0.001f)
{
    // Convertir la dirección proyectada al espacio LOCAL del padre
    // (esto da la dirección relativa al forward/up del tanque)
    Vector3 dirLocal = parent.InverseTransformDirection(dirProjected);

    // Calcular el ángulo yaw objetivo en el espacio local del padre:
    // atan2(x, z) -> 0 cuando apunta hacia forward (z+), positivo hacia la derecha (x+)
    float targetY = Mathf.Atan2(dirLocal.x, dirLocal.z) * Mathf.Rad2Deg;

    // Normalizar ángulo actual local
    float currentY = turret.localEulerAngles.y;
    if (currentY > 180f) currentY -= 360f;

    // Interpolar suavemente
    float newY = Mathf.MoveTowardsAngle(currentY, targetY, turretRotationSpeed * Time.deltaTime);

    // Aplicar SOLO Y en local (mantener X/Z = 0 para evitar drift)
    turret.localRotation = Quaternion.Euler(0f, newY, 0f);
}




        rotateCannon();

    }

    private void rotateCannon()
    {/*
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
        //cannon.localEulerAngles = new Vector3(newX, 0, 0);
        Vector3 finalRot = cannon.localEulerAngles;
        finalRot.x = newX;
        finalRot.y = 0;       // evita drift de rotación
        finalRot.z = 0;
        cannon.localEulerAngles = finalRot;*/
        /*
        Vector3 dir = target.position - cannon.position;

        // h es la distancia horizontal (XZ)
        float h = new Vector2(dir.x, dir.z).magnitude;

        // ángulo vertical correcto
        float angle = Mathf.Atan2(dir.y, h); 
        
        angle = -angle;  // Resultado en radianes

        float finalRotation = Mathf.MoveTowardsAngle(
            cannon.localEulerAngles.x,
            angle * Mathf.Rad2Deg,             // convertir a grados
            cannonRotationSpeed * Time.deltaTime
        );

        cannon.localEulerAngles = new Vector3(finalRotation, 0f, 0f);
        */
        /*
        // Dirección hacia el objetivo
    Vector3 dir = target.position - cannon.position;

    // Distancia horizontal en XZ (hipotenusa del plano horizontal)
    float horizontal = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);

    // Ángulo vertical (Atan2 usa Y vs horizontal)
    float angle = Mathf.Atan2(dir.y, horizontal) * Mathf.Rad2Deg;

    // Mantener tu inversión necesaria
    angle = -angle;

    // Limitar ángulos permitidos (si lo necesitas)
    angle = Mathf.Clamp(angle, minCannonAngle, maxCannonAngle);

    // Rotación actual del cañón en X (normalizada a -180..180)
    float currentX = cannon.localEulerAngles.x;
    if (currentX > 180f) currentX -= 360f;

    // Nueva rotación en X con velocidad fija
    float newX = Mathf.MoveTowards(
        currentX,
        angle,
        cannonRotationSpeed * Time.deltaTime
    );

    // Aplicar solo X sin tocar Y/Z
    cannon.localEulerAngles = new Vector3(newX, 0f, 0f);*/

    // 1. Direccion hacia el objetivo en espacio GLOBAL
    Vector3 dirWorld = target.position - cannon.position;

    // 2. Convertir direccion al espacio LOCAL del cañón
    Vector3 dirLocal = cannon.parent.InverseTransformDirection(dirWorld);

    // 3. Calcular angulo vertical usando su propio eje local
    float angle = Mathf.Atan2(dirLocal.y, dirLocal.z) * Mathf.Rad2Deg;

    // 4. Invertir porque tu cañón lo necesita
    angle = -angle;

    // 5. Clamp
    angle = Mathf.Clamp(angle, minCannonAngle, maxCannonAngle);

    // 6. Rotación actual normalizada
    float currentX = cannon.localEulerAngles.x;
    if (currentX > 180f) currentX -= 360f;

    float newX = Mathf.MoveTowards(currentX, angle, cannonRotationSpeed * Time.deltaTime);

    cannon.localEulerAngles = new Vector3(newX, 0f, 0f);




        

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