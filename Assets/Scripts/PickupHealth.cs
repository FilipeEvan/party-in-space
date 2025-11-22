using UnityEngine;

// Anexe este script ao GameObject do tipo "Pickup_Health".
// Requisitos: collider (Trigger ou colisão normal) e, em pelo menos um dos dois (jogador ou pickup), um Rigidbody/Rigidbody2D.
public class PickupHealth : MonoBehaviour
{
    [Header("Healing")]
    public int healAmount = 1;
    public bool consumeOnlyIfHealed = true; // se true, não some quando a vida já está cheia

    [Header("Motion")]
    public float rotateSpeed = 60f; // graus por segundo
    public float yMin = 1.57f;
    public float yMax = 1.698f;
    [Tooltip("Período completo de subida+descida, em segundos (mais alto = mais suave/lento)")]
    public float floatPeriod = 2.8f;

    float phaseOffset;
    float midY, amplitude, omega;

    void Awake()
    {
        if (yMax < yMin)
        {
            var t = yMax; yMax = yMin; yMin = t;
        }
        phaseOffset = Random.value * Mathf.PI * 2f;
        midY = (yMin + yMax) * 0.5f;
        amplitude = Mathf.Max(0f, (yMax - yMin) * 0.5f);
        omega = floatPeriod <= 0.01f ? 0f : (2f * Mathf.PI / floatPeriod);
    }

    void Update()
    {
        // Rotação sobre o eixo Y (mundo)
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);

        // Flutuação vertical suave (seno) entre yMin e yMax
        if (omega <= 0f)
        {
            omega = 2f * Mathf.PI / 2.8f;
        }
        var pos = transform.position;
        pos.y = midY + amplitude * Mathf.Sin(Time.time * omega + phaseOffset);
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        TryHeal(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryHeal(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHeal(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHeal(collision.collider);
    }

    void TryHeal(Component hit)
    {
        var health = hit.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        bool healed = health.Heal(healAmount);
        if (healed || !consumeOnlyIfHealed)
        {
            gameObject.SetActive(false);
        }
    }
}
