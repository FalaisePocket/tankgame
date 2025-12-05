using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{


    [Header("Tank Stats")]
    [SerializeField] public float currentHealth = 100f;
    
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float rotationSpeed = 100f;
    public float fuerzaDisparo = 20f;
    [Header("Tank Bindings")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;

    public GameObject cannon;
    private ParticleSystem smokeCannon;
    public GameObject hull;
    




    private float moveInput;
    private float rotateInput;
    private Rigidbody rb;

    private PlayerInput playerInput;
    private InputAction shootAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Obtener la acci�n por nombre
        shootAction = playerInput.actions["Shoot"];
        hull.GetComponent<AudioSource>().volume = 0.05f;
        hull.GetComponent<AudioSource>().Play();
        
        
        smokeCannon = cannon.GetComponent<ParticleSystem>();

    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.freezeRotation = true;

    }

    void OnEnable()
    {
        // Suscribirse al evento
        shootAction.performed += OnShoot;
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
        Debug.Log($"{gameObject.name} ha muerto!");
        Destroy(gameObject);
    }

    void OnDisable()
    {
        // Desuscribirse del evento
        shootAction.performed -= OnShoot;
    }

    // IMPORTANTE: Debe ser p�blico y tener exactamente este formato
    public void OnRotateTank(InputAction.CallbackContext context)
    {
        rotateInput = context.ReadValue<float>();
        
    }

    public void OnMoveTankForward(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
        
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        cannon.GetComponent<AudioSource>().Play();
        // Crear instancia de la bala
        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);

        // Obtener el Rigidbody de la bala
        Rigidbody rbShoot = bala.GetComponent<Rigidbody>();

        
        // Aplicar fuerza a la bala
        rbShoot.AddForce(puntoDisparo.forward * fuerzaDisparo, ForceMode.Impulse);
        rb.AddForce(5*(-puntoDisparo.forward * fuerzaDisparo), ForceMode.Impulse);


        smokeCannon.Play();
        

    }

    void FixedUpdate()
    {
        // Movimiento solo en el eje Z local del transform
        Vector3 move = new Vector3(0, 0, moveInput * moveSpeed * Time.fixedDeltaTime);
        move = transform.TransformDirection(move); // Convierte de local a world space
        rb.MovePosition(rb.position + move);

        // Rotaci�n
        float rotation = rotateInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
        if( moveInput != 0 || rotateInput != 0)
        {
            hull.GetComponent<AudioSource>().volume = 0.1f;
        }
        else
        {
            hull.GetComponent<AudioSource>().volume = 0.05f;
        }
        
        
    }
}