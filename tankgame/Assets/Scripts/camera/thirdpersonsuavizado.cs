using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraTerceraPersonaSuavizado : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo;

    [Header("Aim Target")]
    public Transform target;
    public Vector3 tagertOffSet = new Vector3(0, -2, 5);



    [Header("Configuración de Posición")]
    public Vector3 offset = new Vector3(0, 2, -5);

    [Header("Configuración de Rotación")]
    public float sensibilidadMouse = 2f;

    [Header("Límites de Rotación")]
    public float limiteVerticalMin = -30f;
    public float limiteVerticalMax = 60f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Vector2 mouseDelta;
    private Rigidbody rbObjetivo;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (objetivo != null)
        {
            rbObjetivo = objetivo.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        // Solo capturar input del mouse aquí
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }
    }

    void FixedUpdate()
    {
        if (objetivo == null) return;

        // CLAVE: Actualizar la rotación de la cámara en FixedUpdate también
        rotacionY += mouseDelta.x * sensibilidadMouse * 0.02f;
        rotacionX -= mouseDelta.y * sensibilidadMouse * 0.02f;
        rotacionX = Mathf.Clamp(rotacionX, limiteVerticalMin, limiteVerticalMax);

        // Resetear mouseDelta para el siguiente frame
        mouseDelta = Vector2.zero;

        // Usar la posición del Rigidbody
        Vector3 posicionObjetivo = rbObjetivo != null ? rbObjetivo.position : objetivo.position;
        Quaternion rotacionObjetivo = rbObjetivo != null ? rbObjetivo.rotation : objetivo.rotation;

        Quaternion rotacion = Quaternion.Euler(rotacionX, rotacionY, 0);
        Vector3 posicionDeseada = posicionObjetivo + rotacion * offset;

        

        transform.position = posicionDeseada;
        transform.LookAt(posicionObjetivo + Vector3.up * 1.5f);



        Quaternion rotacionTarget = Quaternion.Euler(rotacionX, rotacionY, 0); 
        Vector3 posicionDeseadaTarget = posicionObjetivo + rotacionTarget * tagertOffSet;


        target.transform.position = posicionDeseadaTarget;
        target.transform.LookAt(posicionDeseadaTarget + Vector3.down * 1.5f);


    }
}