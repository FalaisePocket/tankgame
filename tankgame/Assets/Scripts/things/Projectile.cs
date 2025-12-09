using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 20f;


    void Start()
    {
        // Destruir el proyectil despu�s de un tiempo
        Destroy(gameObject, lifetime);
    }



    void OnCollisionEnter(Collision collision)
    {
        // Intentar obtener el componente Health del objeto impactado
        EnemyController health = collision.gameObject.GetComponent<EnemyController>();


        if (health != null)
        {
            health.TakeDamage(damage);
        }

        TankController playerHealth = collision.gameObject.GetComponent<TankController>();
        playerHealth?.TakeDamage(damage);

        // Destruir el proyectil al impactar
        Destroy(gameObject);
    }

    // Si usas Triggers en lugar de Colliders normales
    void OnTriggerEnter(Collider other)
    {
        EnemyController health = other.GetComponent<EnemyController>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}