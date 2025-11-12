using UnityEngine;

// Attach to the Pickup_Jar prefab. Shares the same hover/rotate
// movement as Pickup_Health but grants temporary invulnerability
// to the player on pickup.
public class PickupJar : MonoBehaviour
{
    [Header("Invulnerability")]
    public float invulnerabilitySeconds = 4f;
    [Header("Blink Visuals")]
    public Color tintColor = new Color(1f, 0.92f, 0.1f, 1f); // strong yellow
    [Range(0f,1f)] public float tintStrength = 0.75f; // intensity of the tint
    public bool useEmissionGlow = true;
    [Min(0f)] public float emissionIntensity = 2.5f;

    [Header("Motion (same as Pickup_Health)")]
    public float rotateSpeed = 60f;
    public float yMin = 1.57f;
    public float yMax = 1.698f;
    [Tooltip("Full period (up+down) in seconds. Higher = slower/smoother.")]
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
        // Rotate around global Y
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);

        // Hover vertically like Pickup_Health
        if (omega <= 0f) omega = 2f * Mathf.PI / 2.8f;
        var pos = transform.position;
        pos.y = midY + amplitude * Mathf.Sin(Time.time * omega + phaseOffset);
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other) => TryApply(other);
    void OnCollisionEnter(Collision other) => TryApply(other.collider);
    void OnTriggerEnter2D(Collider2D other) => TryApply(other);
    void OnCollisionEnter2D(Collision2D other) => TryApply(other.collider);

    void TryApply(Component hit)
    {
        var health = hit.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        // Yellow blink during invulnerability with adjustable intensity
        // Use emission-only blinking to avoid shader color quirks producing blue tint
        health.GrantInvulnerabilityTinted(invulnerabilitySeconds, tintColor, tintStrength, useEmissionGlow, emissionIntensity, false);
        gameObject.SetActive(false);
    }
}
