using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    [SerializeField] int damage = 1;

    private void DamageIfPlayer(Component hit)
    {
        var health = hit.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DamageIfPlayer(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DamageIfPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageIfPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DamageIfPlayer(collision.collider);
    }
}
