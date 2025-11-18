using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;

    private float moveInput;
    private float rotateInput;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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