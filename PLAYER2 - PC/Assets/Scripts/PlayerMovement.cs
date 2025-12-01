using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Animación")]
    public Animator animator;   // arrastra aquí tu Humancontroller (Animator del personaje)

    private Rigidbody rb;
    private Vector3 input;
    private Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Opcional pero recomendable: evitar que se incline al chocar
        rb.freezeRotation = true;
    }

    void Update()
    {
        // ---- INPUT (WASD) ----
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);   // que no corra más en diagonal

        // Dirección de movimiento en el plano XZ
        moveDirection = input;

        // ---- ANIMACIÓN ----
        if (animator != null)
        {
            // Asumiendo un parámetro float "Speed" en el Animator
            float speed = moveDirection.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    void FixedUpdate()
    {
        // ---- MOVIMIENTO CON RIGIDBODY ----
        Vector3 newPos = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // ---- ROTACIÓN HACIA LA DIRECCIÓN DE MOVIMIENTO ----
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            Quaternion smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedRot);
        }
    }
}
