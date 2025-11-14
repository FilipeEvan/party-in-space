using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 2;
    public float horizontalSpeed = 3;
    public float horizontalSpeedMultiplier = 3f; // aumentei a velocidade lateral
    public float rightLimit = -5.5f;
    public float leftLimit = 5.5f;
    public float jumpForce = 10f;

    Rigidbody rb;
    Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInChildren<Rigidbody>();
            if (rb == null)
            {
                rb = GetComponentInParent<Rigidbody>();
            }
        }

        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = GetComponentInChildren<Rigidbody2D>();
            if (rb2d == null)
            {
                rb2d = GetComponentInParent<Rigidbody2D>();
            }
        }

        if (rb2d != null)
        {
            rb2d.freezeRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.Self);
        float sideSpeed = horizontalSpeed * horizontalSpeedMultiplier;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (this.gameObject.transform.position.z < leftLimit)
            {
                transform.Translate(Vector3.left * Time.deltaTime * sideSpeed);
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.gameObject.transform.position.z > rightLimit)
            {
                transform.Translate(Vector3.left * Time.deltaTime * sideSpeed * -1);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && IsGrounded())
        {
            if (rb != null)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            else if (rb2d != null)
            {
                rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    bool IsGrounded()
    {
        // Checagem para 3D
        if (rb != null)
        {
            // Distância um pouco maior para tolerar tamanhos de collider diferentes
            return Physics.Raycast(transform.position, Vector3.down, 2f);
        }

        // Checagem para 2D
        if (rb2d != null)
        {
            return Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
        }

        // Se não tiver rigidbody, considera sempre "no chão" para não travar o pulo
        return true;
    }
}
