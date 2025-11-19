using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{


    [Header("Tank Stats")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float rotationSpeed = 100f;
    public float fuerzaDisparo = 20f;
    [Header("Tank Bindings")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    




    private float moveInput;
    private float rotateInput;
    private Rigidbody rb;

    private PlayerInput playerInput;
    private InputAction shootAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Obtener la acción por nombre
        shootAction = playerInput.actions["Shoot"];
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        // Suscribirse al evento
        shootAction.performed += OnShoot;
    }

    void OnDisable()
    {
        // Desuscribirse del evento
        shootAction.performed -= OnShoot;
    }

    // IMPORTANTE: Debe ser público y tener exactamente este formato
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
        // Crear instancia de la bala
        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);

        // Obtener el Rigidbody de la bala
        Rigidbody rb = bala.GetComponent<Rigidbody>();

        
        // Aplicar fuerza a la bala
        rb.AddForce(puntoDisparo.forward * fuerzaDisparo, ForceMode.Impulse);
        

    }

    void FixedUpdate()
    {
        // Movimiento solo en el eje Z local del transform
        Vector3 move = new Vector3(0, 0, moveInput * moveSpeed * Time.fixedDeltaTime);
        move = transform.TransformDirection(move); // Convierte de local a world space
        rb.MovePosition(rb.position + move);

        // Rotación
        float rotation = rotateInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}