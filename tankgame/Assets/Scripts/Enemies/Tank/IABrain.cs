using UnityEngine;
using UnityEngine.AI;

public class AgentTankAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform enemy;          // Objetivo a atacar
    public Transform tankBody;       // Cuerpo del tanque
    public Transform turret;         // Torreta
    public Transform cannon;         // Cañón

    [Header("Configuración de Combate")]
    public float idealDistance = 25f;
    public float shootAngleTolerance = 5f;

    [Header("Sistema de Visión")]
    public float visionRange = 50f;              // Rango máximo de visión
    public float visionCheckInterval = 0.2f;     // Cada cuánto buscar enemigos (en segundos)
    public LayerMask enemyLayer;                 // Layer de los enemigos
    public LayerMask obstacleLayer;              // Layer de obstáculos (paredes, etc)
    public Transform visionPoint;                // Punto desde donde se lanza el raycast (posición de la torreta)

    [Header("Sistema de Memoria")]
    public float memoryDuration = 5f;            // Cuánto tiempo recuerda la última posición (segundos)
    public float investigateRadius = 5f;         // A qué distancia considera que llegó al punto de investigación
    public Transform initialInvestigationPoint;  // Punto inicial donde buscar enemigos
    public bool useInitialPoint = true;          // ¿Usar punto inicial al comenzar?

    [Header("Navegación con Obstáculos")]
    public bool useNavMesh = true;                // Usar NavMesh para pathfinding
    public NavMeshAgent navAgent;                 // Referencia al NavMeshAgent
    public float navMeshUpdateInterval = 0.5f;    // Cada cuánto actualizar el path
    public float waypointReachedDistance = 2f;    // Distancia para considerar waypoint alcanzado
    
    [Header("Detección de Obstáculos (Legacy)")]
    public float obstacleDetectionDistance = 10f; // Distancia para detectar obstáculos
    public float avoidanceForce = 1f;             // Fuerza de evasión (0-1)
    public LayerMask navigationObstacleLayer;     // Capas de obstáculos para navegación
    public float raycastAngleStep = 15f;          // Ángulo entre raycasts de detección

    private Vector3 currentTarget;
    private bool canSeeEnemy = false;
    private float nextVisionCheck = 0f;
    
    // Variables de memoria
    private Transform lastSeenEnemy = null;
    private Vector3 lastKnownPosition;
    private float lastSeenTime = 0f;
    private bool hasMemory = false;
    private bool isInvestigating = false;

    // Variables de navegación
    private Vector3 avoidanceDirection = Vector3.zero;
    private bool isAvoiding = false;
    private float nextNavMeshUpdate = 0f;
    private Vector3 currentWaypoint;
    private bool hasValidPath = false;
    private int currentWaypointIndex = 0;

    void Start()
    {
        // Si no se asigna visionPoint, usar la posición del turret
        if (visionPoint == null)
            visionPoint = turret;

        // ADVERTENCIA: Si los layers no están configurados correctamente
        if (enemyLayer.value == 0)
        {
            Debug.LogError($"[{gameObject.name}] Enemy Layer no configurado! Ve a Inspector y selecciona el layer 'Enemy'");
        }

        // Buscar NavMeshAgent si no está asignado
        if (useNavMesh && navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        if (useNavMesh && navAgent != null)
        {
            // CRÍTICO: Configurar NavMeshAgent para que SOLO calcule paths
            navAgent.updatePosition = false;          // NO mover el transform
            navAgent.updateRotation = false;          // NO rotar el transform
            navAgent.velocity = Vector3.zero;         // Sin velocidad
            navAgent.isStopped = true;                // Detenido (solo calcula paths)
            
            // Forzar valores en 0 por código
            navAgent.speed = 0f;
            navAgent.angularSpeed = 0f;
            navAgent.acceleration = 0f;
            navAgent.autoBraking = false;
            navAgent.autoTraverseOffMeshLink = false; // IMPORTANTE: Desactivar saltos
            navAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
            
            Debug.Log($"[{gameObject.name}] NavMeshAgent configurado - updatePosition: {navAgent.updatePosition}, updateRotation: {navAgent.updateRotation}, isStopped: {navAgent.isStopped}");
        }
        else if (useNavMesh)
        {
            Debug.LogWarning($"[{gameObject.name}] useNavMesh activado pero no hay NavMeshAgent. Añade uno o desactiva useNavMesh.");
            useNavMesh = false;
        }

        // Configurar punto inicial de investigación
        if (useInitialPoint && initialInvestigationPoint != null)
        {
            lastKnownPosition = initialInvestigationPoint.position;
            lastSeenTime = Time.time;
            hasMemory = true;
            isInvestigating = true;
            Debug.Log($"[{gameObject.name}] Punto inicial de investigación configurado en {lastKnownPosition}");
        }
    }

    void Update()
    {
        // Actualizar sistema de visión periódicamente
        if (Time.time >= nextVisionCheck)
        {
            CheckVision();
            nextVisionCheck = Time.time + visionCheckInterval;
        }

        // Actualizar pathfinding si estamos investigando
        if (ShouldInvestigate())
        {
            if (useNavMesh)
            {
                UpdateNavMeshPath();
            }
            else
            {
                DetectObstacles();
            }
        }
    }

    void LateUpdate()
    {
        // Sincronizar NavMeshAgent con la posición real del tanque
        // Esto se hace en LateUpdate DESPUÉS de que EnemyController haya movido el tanque
        if (useNavMesh && navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            // Solo actualizar la posición interna del NavMeshAgent
            // NO queremos que el NavMeshAgent mueva el GameObject
            navAgent.Warp(tankBody.position);
        }
    }

    // ---------------------------
    //   SISTEMA DE VISIÓN
    // ---------------------------

    private void CheckVision()
    {
        // Primero verificar si tenemos un enemigo asignado y si podemos verlo
        if (enemy != null)
        {
            canSeeEnemy = CanSeeTarget(enemy);
            
            if (canSeeEnemy)
            {
                // Vemos al enemigo, actualizar memoria
                UpdateMemory(enemy, enemy.position);
                isInvestigating = false;
                return; // Salir, todo está bien
            }
            else
            {
                // NO vemos al enemigo actual
                // La memoria ya debería estar guardada de la última vez que lo vimos
                // NO limpiamos el enemy aquí
            }
        }
        
        // Si llegamos aquí, o no tenemos enemigo o no lo vemos
        // Intentar encontrar un nuevo enemigo visible
        Transform newEnemy = FindNearestVisibleEnemyWithoutClearing();
        if (newEnemy != null)
        {
            enemy = newEnemy;
            canSeeEnemy = true;
            UpdateMemory(newEnemy, newEnemy.position);
            isInvestigating = false;
        }
        else
        {
            // No hay enemigos visibles
            canSeeEnemy = false;
        }

        // Limpiar memoria si ha expirado
        if (hasMemory && Time.time - lastSeenTime > memoryDuration)
        {
            ClearMemory();
        }
    }

    private void FindNearestVisibleEnemy()
    {
        // Buscar todos los colliders de enemigos en el rango
        Collider[] enemiesInRange = Physics.OverlapSphere(
            visionPoint.position, 
            visionRange, 
            enemyLayer
        );

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            // No detectarse a sí mismo
            if (enemyCollider.transform == tankBody || enemyCollider.transform.IsChildOf(tankBody))
                continue;

            Transform potentialEnemy = enemyCollider.transform;
            
            // Verificar si hay línea de visión clara
            if (CanSeeTarget(potentialEnemy))
            {
                float distance = Vector3.Distance(visionPoint.position, potentialEnemy.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = potentialEnemy;
                }
            }
        }

        if (closestEnemy != null)
        {
            enemy = closestEnemy;
            canSeeEnemy = true;
            UpdateMemory(closestEnemy, closestEnemy.position);
            isInvestigating = false;
        }
    }

    private Transform FindNearestVisibleEnemyWithoutClearing()
    {
        // Buscar todos los colliders de enemigos en el rango
        Collider[] enemiesInRange = Physics.OverlapSphere(
            visionPoint.position, 
            visionRange, 
            enemyLayer
        );

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            // No detectarse a sí mismo
            if (enemyCollider.transform == tankBody || enemyCollider.transform.IsChildOf(tankBody))
                continue;

            Transform potentialEnemy = enemyCollider.transform;
            
            // Verificar si hay línea de visión clara
            if (CanSeeTarget(potentialEnemy))
            {
                float distance = Vector3.Distance(visionPoint.position, potentialEnemy.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = potentialEnemy;
                }
            }
        }

        return closestEnemy;
    }

    private bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 directionToTarget = target.position - visionPoint.position;
        float distanceToTarget = directionToTarget.magnitude;

        // Verificar si está dentro del rango
        if (distanceToTarget > visionRange)
            return false;

        // Lanzar raycast para verificar si hay obstáculos
        RaycastHit hit;
        if (Physics.Raycast(
            visionPoint.position, 
            directionToTarget.normalized, 
            out hit, 
            distanceToTarget,
            enemyLayer | obstacleLayer))
        {
            // Si el raycast golpea al objetivo, podemos verlo
            if (hit.transform == target || hit.transform.IsChildOf(target))
            {
                return true;
            }
        }

        return false;
    }

    // ---------------------------
    //   SISTEMA DE MEMORIA
    // ---------------------------

    private void UpdateMemory(Transform enemy, Vector3 position)
    {
        lastSeenEnemy = enemy;
        lastKnownPosition = position;
        lastSeenTime = Time.time;
        hasMemory = true;
        
        Debug.Log($"[{gameObject.name}] Memoria actualizada: Enemigo en {position}, Age: {Time.time - lastSeenTime:F1}s");
    }

    private void ClearMemory()
    {
        lastSeenEnemy = null;
        hasMemory = false;
        isInvestigating = false;
        enemy = null; // Limpiar también el enemigo
    }

    private bool ShouldInvestigate()
    {
        // Solo investigar si:
        // 1. Tenemos memoria de un enemigo
        // 2. No podemos verlo actualmente
        // 3. La memoria no ha expirado
        bool shouldInvestigate = hasMemory && 
                                !canSeeEnemy && 
                                (Time.time - lastSeenTime <= memoryDuration);
        
        // Activar el flag si cumple condiciones
        if (shouldInvestigate)
        {
            if (!isInvestigating)
            {
                isInvestigating = true;
                Debug.Log($"[{gameObject.name}] Iniciando investigación de última posición conocida");
            }
        }
        else
        {
            if (isInvestigating)
            {
                isInvestigating = false;
                Debug.Log($"[{gameObject.name}] Terminando investigación");
            }
        }
        
        return shouldInvestigate;
    }

    private bool HasReachedInvestigationPoint()
    {
        if (!hasMemory) return false;
        
        float distance = Vector3.Distance(tankBody.position, lastKnownPosition);
        return distance <= investigateRadius;
    }

    // Para depuración: visualizar el rango de visión
    private void OnDrawGizmosSelected()
    {
        if (visionPoint == null) return;

        // Dibujar rango de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(visionPoint.position, visionRange);

        // Dibujar línea hacia el enemigo si lo puede ver
        if (enemy != null && canSeeEnemy)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(visionPoint.position, enemy.position);
        }

        // Dibujar memoria: última posición conocida
        if (hasMemory && !canSeeEnemy)
        {
            // Verificar si el destino está en el NavMesh
            NavMeshHit hit;
            bool isOnNavMesh = NavMesh.SamplePosition(lastKnownPosition, out hit, 10f, NavMesh.AllAreas);
            
            Gizmos.color = isOnNavMesh ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(lastKnownPosition, investigateRadius);
            Gizmos.DrawLine(tankBody.position, lastKnownPosition);
            
            // Dibujar una cruz en la última posición conocida
            Gizmos.DrawLine(lastKnownPosition + Vector3.up * 2, lastKnownPosition - Vector3.up * 2);
            Gizmos.DrawLine(lastKnownPosition + Vector3.left * 2, lastKnownPosition + Vector3.right * 2);
            
            // Si está fuera del NavMesh, mostrar el punto más cercano válido
            if (!isOnNavMesh && useNavMesh)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lastKnownPosition, 0.5f);
                
                // Buscar punto válido más cercano
                if (NavMesh.SamplePosition(lastKnownPosition, out hit, 50f, NavMesh.AllAreas))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(hit.position, investigateRadius);
                    Gizmos.DrawLine(lastKnownPosition, hit.position);
                }
            }
        }

        // Dibujar punto inicial de investigación
        if (initialInvestigationPoint != null && useInitialPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(initialInvestigationPoint.position, 2f);
            Gizmos.DrawIcon(initialInvestigationPoint.position, "sv_icon_dot0_pix16_gizmo", true);
        }

        // Dibujar path de NavMesh
        if (useNavMesh && navAgent != null && navAgent.hasPath && isInvestigating)
        {
            Vector3[] corners = navAgent.path.corners;
            
            for (int i = 0; i < corners.Length - 1; i++)
            {
                // Línea del path
                Gizmos.color = Color.green;
                Gizmos.DrawLine(corners[i], corners[i + 1]);
                
                // Waypoints
                Gizmos.color = i == currentWaypointIndex ? Color.yellow : Color.green;
                Gizmos.DrawWireSphere(corners[i], 1f);
            }
            
            // Último waypoint
            if (corners.Length > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(corners[corners.Length - 1], 1f);
            }

            // Waypoint actual
            if (hasValidPath)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentWaypoint, waypointReachedDistance);
                Gizmos.DrawLine(tankBody.position, currentWaypoint);
            }
        }

        // Dibujar raycasts de detección de obstáculos (solo si no usa NavMesh)
        if (!useNavMesh && tankBody != null && isInvestigating)
        {
            // Raycast frontal
            Gizmos.color = Physics.Raycast(tankBody.position, tankBody.forward, obstacleDetectionDistance, navigationObstacleLayer) 
                ? Color.red : Color.green;
            Gizmos.DrawRay(tankBody.position, tankBody.forward * obstacleDetectionDistance);

            // Dibujar dirección de evasión si está activa
            if (isAvoiding && avoidanceDirection != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(tankBody.position, avoidanceDirection * obstacleDetectionDistance);
            }
        }
    }

    // ---------------------------
    //   NAVEGACIÓN CON NAVMESH
    // ---------------------------

    private void UpdateNavMeshPath()
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) return;

        // Actualizar el path periódicamente
        if (Time.time >= nextNavMeshUpdate)
        {
            // Verificar que el destino esté en el NavMesh
            NavMeshHit hit;
            Vector3 targetPosition = lastKnownPosition;
            
            // Si el destino no está en el NavMesh, encontrar el punto más cercano que sí esté
            if (!NavMesh.SamplePosition(lastKnownPosition, out hit, 10f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"[{gameObject.name}] Destino {lastKnownPosition} está fuera del NavMesh. Usando pathfinding legacy.");
                hasValidPath = false;
                return;
            }
            
            targetPosition = hit.position;

            // Calcular path hacia el objetivo
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(tankBody.position, targetPosition, NavMesh.AllAreas, path))
            {
                navAgent.SetPath(path);
                nextNavMeshUpdate = Time.time + navMeshUpdateInterval;

                // Verificar si tenemos un path válido
                if (path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1)
                {
                    hasValidPath = true;
                    currentWaypointIndex = 1; // Empezar desde el primer waypoint (0 es nuestra posición)
                    currentWaypoint = path.corners[currentWaypointIndex];
                }
                else
                {
                    hasValidPath = false;
                    if (path.status == NavMeshPathStatus.PathPartial)
                    {
                        Debug.LogWarning($"[{gameObject.name}] Path parcial hacia {targetPosition}. El destino podría estar bloqueado.");
                    }
                }
            }
            else
            {
                hasValidPath = false;
                Debug.LogWarning($"[{gameObject.name}] No se pudo calcular path hacia {targetPosition}");
            }
        }

        // Actualizar waypoint actual si llegamos al anterior
        if (hasValidPath && navAgent.hasPath && navAgent.path.corners.Length > 0)
        {
            float distanceToWaypoint = Vector3.Distance(tankBody.position, currentWaypoint);
            
            if (distanceToWaypoint < waypointReachedDistance)
            {
                // Avanzar al siguiente waypoint
                currentWaypointIndex++;
                
                if (currentWaypointIndex < navAgent.path.corners.Length)
                {
                    currentWaypoint = navAgent.path.corners[currentWaypointIndex];
                }
                else
                {
                    // Llegamos al final del path
                    hasValidPath = false;
                }
            }
        }
    }

    private Vector3 GetNavigationTarget()
    {
        if (useNavMesh && hasValidPath)
        {
            return currentWaypoint;
        }
        else
        {
            return lastKnownPosition;
        }
    }

    // ---------------------------
    //   DETECCIÓN DE OBSTÁCULOS (LEGACY)
    // ---------------------------

    private void DetectObstacles()
    {
        isAvoiding = false;
        avoidanceDirection = Vector3.zero;

        // Dirección hacia el objetivo
        Vector3 targetDirection = (lastKnownPosition - tankBody.position).normalized;

        // Raycast frontal
        if (Physics.Raycast(tankBody.position, tankBody.forward, obstacleDetectionDistance, navigationObstacleLayer))
        {
            isAvoiding = true;
            
            // Probar direcciones alternativas
            float bestAngle = 0f;
            float maxDistance = 0f;

            // Probar ángulos a izquierda y derecha
            for (float angle = raycastAngleStep; angle <= 90f; angle += raycastAngleStep)
            {
                // Probar a la derecha
                Vector3 rightDir = Quaternion.Euler(0, angle, 0) * tankBody.forward;
                if (!Physics.Raycast(tankBody.position, rightDir, out RaycastHit hitRight, obstacleDetectionDistance, navigationObstacleLayer))
                {
                    // Verificar si esta dirección nos acerca al objetivo
                    float alignment = Vector3.Dot(rightDir, targetDirection);
                    if (alignment > maxDistance)
                    {
                        maxDistance = alignment;
                        bestAngle = angle;
                        avoidanceDirection = rightDir;
                    }
                }

                // Probar a la izquierda
                Vector3 leftDir = Quaternion.Euler(0, -angle, 0) * tankBody.forward;
                if (!Physics.Raycast(tankBody.position, leftDir, out RaycastHit hitLeft, obstacleDetectionDistance, navigationObstacleLayer))
                {
                    float alignment = Vector3.Dot(leftDir, targetDirection);
                    if (alignment > maxDistance)
                    {
                        maxDistance = alignment;
                        bestAngle = -angle;
                        avoidanceDirection = leftDir;
                    }
                }
            }

            // Si no encontramos ruta clara, girar 90 grados
            if (avoidanceDirection == Vector3.zero)
            {
                avoidanceDirection = Quaternion.Euler(0, 90f, 0) * tankBody.forward;
            }
        }
    }

    // ---------------------------
    //      IA PRINCIPAL
    // ---------------------------

    public float rotate()
    {
        // Si estamos investigando, rotar hacia la última posición conocida
        if (ShouldInvestigate())
        {
            // Si ya llegamos al punto de investigación, limpiar memoria
            if (HasReachedInvestigationPoint())
            {
                return 0f;
            }

            Vector3 targetPosition;

            // Usar NavMesh si está disponible
            if (useNavMesh && hasValidPath)
            {
                targetPosition = currentWaypoint;
            }
            else if (isAvoiding && avoidanceDirection != Vector3.zero)
            {
                // Si estamos evitando obstáculos (modo legacy), usar dirección de evasión
                float avoidAngle = Vector3.SignedAngle(tankBody.forward, avoidanceDirection, Vector3.up);
                return Mathf.Clamp(avoidAngle / 45f, -1f, 1f);
            }
            else
            {
                targetPosition = lastKnownPosition;
            }

            // Rotar hacia la posición objetivo
            Vector3 toTarget = (targetPosition - tankBody.position).normalized;
            float angle = Vector3.SignedAngle(tankBody.forward, toTarget, Vector3.up);
            return Mathf.Clamp(angle / 45f, -1f, 1f);
        }

        if (enemy == null || !canSeeEnemy) return 0f;

        Vector3 toEnemy = (enemy.position - tankBody.position).normalized;
        float angleToEnemy = Vector3.SignedAngle(tankBody.forward, toEnemy, Vector3.up);

        return Mathf.Clamp(angleToEnemy / 45f, -1f, 1f);
    }

    public float forward()
    {
        // Si estamos investigando, acercarse a la última posición conocida
        if (ShouldInvestigate())
        {
            float distance = Vector3.Distance(tankBody.position, lastKnownPosition);
            
            // Si ya llegamos, detenerse y limpiar memoria
            if (HasReachedInvestigationPoint())
            {
                Debug.Log($"[{gameObject.name}] Llegó al punto de investigación, limpiando memoria");
                ClearMemory();
                return 0f;
            }
            
            // Moverse hacia la última posición conocida
            return 1f;
        }

        if (enemy == null || !canSeeEnemy) return 0f;

        float distanceToEnemy = Vector3.Distance(tankBody.position, enemy.position);

        if (distanceToEnemy > idealDistance + 3f)
            return 1f;          // avanzar
        else if (distanceToEnemy < idealDistance - 3f)
            return -1f;         // retroceder

        return 0f;
    }

    public Vector3 aimAt()
    {
        // Si estamos investigando, apuntar hacia la última posición conocida
        if (ShouldInvestigate())
        {
            currentTarget = lastKnownPosition + Vector3.up * 1.3f;
            return currentTarget;
        }

        if (enemy == null || !canSeeEnemy) 
            return visionPoint.position + visionPoint.forward * 10f;

        currentTarget = enemy.position + Vector3.up * 1.3f;
        return currentTarget;
    }

    public bool shoot()
    {
        if (enemy == null || !canSeeEnemy) return false;

        // ¿La mira está alineada?
        Vector3 dir = (enemy.position - cannon.position).normalized;
        float angle = Vector3.Angle(cannon.forward, dir);

        return angle < shootAngleTolerance;
    }

    // Método público para verificar si puede ver enemigos
    public bool CanSeeEnemies()
    {
        return canSeeEnemy;
    }

    // Métodos públicos para obtener información del estado
    public bool IsInvestigating()
    {
        return isInvestigating;
    }

    public bool HasEnemyMemory()
    {
        return hasMemory;
    }

    public Vector3 GetLastKnownPosition()
    {
        return lastKnownPosition;
    }

    public float GetMemoryAge()
    {
        if (!hasMemory) return 0f;
        return Time.time - lastSeenTime;
    }
}