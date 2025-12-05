using UnityEngine;

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

    private Vector3 currentTarget;
    private bool canSeeEnemy = false;
    private float nextVisionCheck = 0f;
    
    // Variables de memoria
    private Transform lastSeenEnemy = null;
    private Vector3 lastKnownPosition;
    private float lastSeenTime = 0f;
    private bool hasMemory = false;
    private bool isInvestigating = false;

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
    }

    void Update()
    {
        // Actualizar sistema de visión periódicamente
        if (Time.time >= nextVisionCheck)
        {
            CheckVision();
            nextVisionCheck = Time.time + visionCheckInterval;
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(lastKnownPosition, investigateRadius);
            Gizmos.DrawLine(tankBody.position, lastKnownPosition);
            
            // Dibujar una cruz en la última posición conocida
            Gizmos.DrawLine(lastKnownPosition + Vector3.up * 2, lastKnownPosition - Vector3.up * 2);
            Gizmos.DrawLine(lastKnownPosition + Vector3.left * 2, lastKnownPosition + Vector3.right * 2);
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

            // Rotar hacia la última posición conocida
            Vector3 toLastPosition = (lastKnownPosition - tankBody.position).normalized;
            float angle = Vector3.SignedAngle(tankBody.forward, toLastPosition, Vector3.up);
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