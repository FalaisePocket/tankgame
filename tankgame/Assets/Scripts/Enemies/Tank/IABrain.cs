using UnityEngine;

public class AgentTankAI : MonoBehaviour
{
    public Transform enemy;          // Objetivo a atacar
    public Transform tankBody;       // Cuerpo del tanque
    public Transform turret;         // Torreta
    public Transform cannon;         // Cañón

    public float idealDistance = 25f;
    public float shootAngleTolerance = 5f;

    private Vector3 targetPoint;
    private float rotateInput;
    private float moveInput;

    private Vector3 currentTarget;

    // ---------------------------
    //      IA PRINCIPAL
    // ---------------------------

    public float rotate()
    {
        if (enemy == null) return 0f;

        // Pensamiento:
        // “Si mi enemigo está a mi izquierda, giro izquierda. Si está a la derecha, derecha.”

        Vector3 toEnemy = (enemy.position - tankBody.position).normalized;
        float angle = Vector3.SignedAngle(tankBody.forward, toEnemy, Vector3.up);

        return Mathf.Clamp(angle / 45f, -1f, 1f);
    }

    public float forward()
    {
        if (enemy == null) return 0f;

        float distance = Vector3.Distance(tankBody.position, enemy.position);

        // Pensamiento:
        // “Si estoy lejos, avanzo. Si estoy muy cerca, retrocedo.”

        if (distance > idealDistance + 3f)
            return 1f;          // avanzar
        else if (distance < idealDistance - 3f)
            return -1f;         // retroceder
        // quedarme

        return 0f;
    }

    public Vector3 aimAt()
    {
        if (enemy == null) return Vector3.zero;

        // Pensamiento:
        // “Apuntar al centro del tanque enemigo, pero puedo luego mejorar con lead predictivo.”

        currentTarget = enemy.position + Vector3.up * 1.3f;
        return currentTarget;
    }

    // ---------------------------
    //      APUNTADO
    // ---------------------------

    public void rotateTurret()
    {
        
    }

    public void elevateCannon()
    {
        
    }

    // ---------------------------
    //       DISPARO
    // ---------------------------

    public bool shoot()
    {
        if (enemy == null) return false;

        // ¿La mira está alineada?
        Vector3 dir = (enemy.position - cannon.position).normalized;
        float angle = Vector3.Angle(cannon.forward, dir);

        return angle < shootAngleTolerance;
    }
}
