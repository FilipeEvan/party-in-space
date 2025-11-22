using UnityEngine;

// Anexe ao prefab Pickup_Jar. Compartilha o mesmo movimento de
// flutuar/girar do Pickup_Health, mas concede invulnerabilidade temporária
// ao jogador ao coletar.
public class PickupJar : MonoBehaviour
{
    [Header("Invulnerabilidade")]
    public float invulnerabilitySeconds = 4f;
    [Header("Visual do Brilho")]
    public Color tintColor = new Color(1f, 0.92f, 0.1f, 1f); // amarelo forte
    [Range(0f,1f)] public float tintStrength = 0.75f; // intensidade do brilho
    public bool useEmissionGlow = true;
    [Min(0f)] public float emissionIntensity = 2.5f;

    [Header("Movimento (igual ao Pickup_Health)")]
    public float rotateSpeed = 60f;
    public float yMin = 1.57f;
    public float yMax = 1.698f;
    [Tooltip("Período completo (subida+descida) em segundos. Maior = mais lento/suave.")]
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
        // Rotação em torno do Y global
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);

        // Flutuação vertical como o Pickup_Health
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

        // Piscar amarelo durante a invulnerabilidade com intensidade ajustável
        // Usa brilho apenas na emissão para evitar problemas de cor azul em alguns shaders
        health.GrantInvulnerabilityTinted(invulnerabilitySeconds, tintColor, tintStrength, useEmissionGlow, emissionIntensity, false);
        gameObject.SetActive(false);
    }
}
