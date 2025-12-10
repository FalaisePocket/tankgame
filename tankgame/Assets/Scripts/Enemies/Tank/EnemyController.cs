using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public float maxHealth = 100f;
    public float currentHealth=100f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float turretRotationSpeed = 30f;
    public float cannonRotationSpeed = 20f;
    public float minCannonAngle = -10f;
    public float maxCannonAngle = 45f;
    public bool trackingEnabled = true;
    public float fuerzaDisparo = 20f;
    [Header("Tasks")]
    public float shootCooldown = 0.25f;
    public float shootTimer = 0f;
    public bool canShoot = true;



    [Header("Inputs")]
    private float moveInput;
    private float rotateInput;
    private float turretRotateInput;
    private float cannonElevateInput;
    private bool shootInput;

    private Vector3 target;

    [Header("Modules")]

    public Rigidbody rb;
    public Transform cannon;
    public Transform turret;
    public Transform chasis;
    public Transform leftTrack;
    public Transform rightTrack;
    public Transform puntoDisparo;
    [SerializeField] public GameObject balaPrefab;
    private AudioSource audioSource;


    public AgentTankAI agent; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        EnemyIndicatorManager.instance.RegisterEnemy(transform);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Se ejecuta cuando el enemigo muere o es destruido
        if (EnemyIndicatorManager.instance != null)
        {
            EnemyIndicatorManager.instance.UnregisterEnemy(transform);
        }
        EnemyIndicatorManager.instance.UnregisterEnemy(transform);
        Debug.Log($"{gameObject.name} ha muerto!");
        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    



    // Update is called once per frame
    void Update()
    {
        if (!canShoot)
        {
            shootTimer += Time.deltaTime;
            
            if (shootTimer >= shootCooldown)
                canShoot = true;
            
        }
        rotateInput = agent.rotate();
        moveInput = agent.forward();
        target = agent.aimAt();

        if (agent.shoot())
            Shoot();

        if (!trackingEnabled || turret == null || cannon == null)
            return;

        AimAtTarget();
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(0, 0, moveInput * moveSpeed * Time.fixedDeltaTime);
        move = transform.TransformDirection(move); // Convierte de local a world space
        rb.MovePosition(rb.position + move);

        // Rotaci�n
        float rotation = rotateInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }



    private void AimAtTarget()
    {
        // === ROTACIN DE LA TORRETA (Horizontal - Eje Y) ===
        Vector3 directionToTarget = target - turret.position;
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
        Vector3 directionFromCannon = target - cannon.position;

        // Calcular el ngulo en el plano vertical
        float horizontalDistance = new Vector3(directionFromCannon.x, 0, directionFromCannon.z).magnitude;
        float targetAngle = Mathf.Atan2(directionFromCannon.y, horizontalDistance) * Mathf.Rad2Deg;

        // Limitar el ngulo entre min y max
        targetAngle = Mathf.Clamp(targetAngle, minCannonAngle, maxCannonAngle);

        // Obtener el ngulo actual del can
        float currentXAngle = cannon.transform.localEulerAngles.x;
        if (currentXAngle > 180f) currentXAngle -= 360f;

        // Interpolar suavemente hacia el ngulo objetivo (invertido)
        float newXAngle = Mathf.MoveTowards(
            currentXAngle,
            -targetAngle,  // Negativo para invertir la rotacin
            cannonRotationSpeed * Time.deltaTime
        );

        // Aplicar solo rotacin en X (local)
        cannon.transform.localEulerAngles = new Vector3(newXAngle, 0, 0);
    }

    private void Shoot()
    {
        if (!canShoot) return;
        audioSource.Play();
        // Crear instancia de la bala
        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);

        // Obtener el Rigidbody de la bala
        Rigidbody rb = bala.GetComponent<Rigidbody>();

        
        // Aplicar fuerza a la bala
        rb.AddForce(puntoDisparo.forward * fuerzaDisparo, ForceMode.Impulse);
        canShoot = false;
        shootTimer = 0f;
    }
}
