using UnityEngine;

public class EnemyFlying : MonoBehaviour
{
    public float yMin = 1f;
    public float yMax = 4.5f;
    public float verticalSpeed = 2f;
    public int damage = 1;

    float phaseOffset;

    void Awake()
    {
        phaseOffset = Random.value * 10f;
        if (yMax < yMin)
        {
            var t = yMax; yMax = yMin; yMin = t;
        }
    }

    void Update()
    {
        var pos = transform.position;
        pos.y = Mathf.PingPong((Time.time + phaseOffset) * verticalSpeed, yMax - yMin) + yMin;
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        var health = collision.collider.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var health = collision.collider.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
    }
}
