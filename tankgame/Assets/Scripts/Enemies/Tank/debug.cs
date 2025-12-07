using UnityEngine;

public class TankAIDebugger : MonoBehaviour
{
    public AgentTankAI agent;
    public EnemyController controller;

    void OnGUI()
    {
        if (agent == null || controller == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 450, 500));
        GUILayout.Box("=== TANK AI DEBUG ===");
        
        // Estado de referencias
        GUILayout.Label($"Enemy: {(agent.enemy != null ? agent.enemy.name : "NULL")}");
        
        if (agent.enemy != null)
        {
            float dist = Vector3.Distance(agent.tankBody.position, agent.enemy.position);
            GUILayout.Label($"Distance to Enemy: {dist:F1}m");
        }
        
        GUILayout.Label($"Tank Body: {(agent.tankBody != null ? "OK" : "NULL")}");
        GUILayout.Label($"Turret: {(agent.turret != null ? "OK" : "NULL")}");
        GUILayout.Label($"Cannon: {(agent.cannon != null ? "OK" : "NULL")}");
        GUILayout.Label($"Vision Point: {(agent.visionPoint != null ? "OK" : "NULL")}");
        
        GUILayout.Space(10);
        
        // Estado de visión
        Color oldColor = GUI.color;
        
        GUI.color = agent.CanSeeEnemies() ? Color.green : Color.red;
        GUILayout.Label($"Can See Enemy: {agent.CanSeeEnemies()}");
        
        GUI.color = agent.HasEnemyMemory() ? Color.yellow : Color.gray;
        GUILayout.Label($"Has Memory: {agent.HasEnemyMemory()}");
        
        GUI.color = agent.IsInvestigating() ? Color.cyan : Color.gray;
        GUILayout.Label($"Is Investigating: {agent.IsInvestigating()}");
        
        GUI.color = oldColor;
        
        if (agent.HasEnemyMemory())
        {
            GUILayout.Label($"Memory Age: {agent.GetMemoryAge():F1}s / {agent.memoryDuration:F1}s");
            GUILayout.Label($"Last Known Pos: {agent.GetLastKnownPosition()}");
            
            // Distancia a la última posición conocida
            float distToMemory = Vector3.Distance(agent.tankBody.position, agent.GetLastKnownPosition());
            GUILayout.Label($"Dist to Memory: {distToMemory:F1}m (threshold: {agent.investigateRadius:F1}m)");
        }

        // Info de navegación
        if (agent.IsInvestigating())
        {
            GUILayout.Space(5);
            GUI.color = Color.cyan;
            GUILayout.Label("=== NAVEGACIÓN ===");
            GUI.color = oldColor;
            
            GUILayout.Label($"Use NavMesh: {agent.useNavMesh}");
            
            if (agent.useNavMesh && agent.navAgent != null)
            {
                GUI.color = agent.navAgent.hasPath ? Color.green : Color.red;
                GUILayout.Label($"Has Path: {agent.navAgent.hasPath}");
                GUI.color = oldColor;
                
                if (agent.navAgent.hasPath)
                {
                    GUILayout.Label($"Path Status: {agent.navAgent.pathStatus}");
                    GUILayout.Label($"Waypoints: {agent.navAgent.path.corners.Length}");
                    GUILayout.Label($"Remaining Distance: {agent.navAgent.remainingDistance:F1}m");
                }
            }
            else
            {
                // Detectar si hay obstáculos adelante (modo legacy)
                RaycastHit hit;
                bool hasObstacle = Physics.Raycast(
                    agent.tankBody.position, 
                    agent.tankBody.forward, 
                    out hit,
                    agent.obstacleDetectionDistance, 
                    agent.navigationObstacleLayer
                );
                
                GUI.color = hasObstacle ? Color.red : Color.green;
                GUILayout.Label($"Obstacle Ahead: {hasObstacle} {(hasObstacle ? $"({hit.distance:F1}m)" : "")}");
                GUI.color = oldColor;
            }
        }
        
        GUILayout.Space(10);
        
        // Inputs de la IA
        float rotateVal = agent.rotate();
        float moveVal = agent.forward();
        
        GUI.color = rotateVal != 0 ? Color.green : Color.gray;
        GUILayout.Label($"Rotate Input: {rotateVal:F2}");
        
        GUI.color = moveVal != 0 ? Color.green : Color.gray;
        GUILayout.Label($"Move Input: {moveVal:F2}");
        
        GUI.color = oldColor;
        GUILayout.Label($"Aim Target: {agent.aimAt()}");
        GUILayout.Label($"Should Shoot: {agent.shoot()}");
        
        GUILayout.Space(10);
        
        // Estado del controlador
        GUILayout.Label($"Health: {controller.currentHealth:F0}/{controller.maxHealth:F0}");
        GUILayout.Label($"Tracking Enabled: {controller.trackingEnabled}");
        
        GUILayout.Space(10);
        
        // Layers configurados
        GUILayout.Label($"Enemy Layer: {agent.enemyLayer.value}");
        GUILayout.Label($"Obstacle Layer: {agent.obstacleLayer.value}");
        GUILayout.Label($"Vision Range: {agent.visionRange:F1}");
        
        GUILayout.EndArea();
    }
}