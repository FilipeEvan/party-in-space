using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public float invincibilityDuration = 1.2f;
    public float blinkInterval = 0.1f;
    public bool enableKnockback = true;
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.2f;
    [Header("Death/Animation")]
    public Animator animator; // opcional: arraste seu Animator aqui
    public string deathStateName = "CharacterArmature_Death";
    [Header("Heal/Animation")]
    public string healTriggerName = "Heal"; // deixa vazio se preferir usar estado
    public string healStateName = "";       // ex.: "CharacterArmature_Heal"

    bool invincible;
    Coroutine invincibleRoutine;
    struct RendererState { public Renderer r; public bool initial; }
    RendererState[] rendererStates;
    Coroutine knockbackRoutine;

    public event Action<int, int> HealthChanged; // (current, max)
    public event Action Died;

    void Awake()
    {
        currentHealth = Mathf.Max(0, maxHealth);
        var rends = GetComponentsInChildren<Renderer>(true);
        rendererStates = new RendererState[rends.Length];
        for (int i = 0; i < rends.Length; i++)
        {
            rendererStates[i] = new RendererState { r = rends[i], initial = rends[i].enabled };
        }
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0 || invincible) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            HandleDeath();
            return;
        }
        StartInvincibility();
    }

    public void TakeDamage(int amount, Vector3 hitFromPosition)
    {
        if (amount <= 0 || currentHealth <= 0 || invincible) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            HandleDeath();
            return;
        }
        if (enableKnockback)
        {
            Vector3 dir = (transform.position - hitFromPosition);
            if (dir == Vector3.zero) dir = -transform.forward;
            dir.y = 0f;
            ApplyKnockback(dir.normalized);
        }
        StartInvincibility();
    }

    public void TakeDamage(int amount, Component attacker)
    {
        if (attacker == null) { TakeDamage(amount); return; }
        TakeDamage(amount, attacker.transform.position);
    }

    // Recupera vida até o máximo. Retorna true se curou algo.
    public bool Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return false;
        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        bool healed = currentHealth > before;
        if (healed)
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
            PlayHealAnimation();
        }
        return healed;
    }

    void PlayHealAnimation()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        // evita log de erro do Unity tentando resetar um trigger inexistente
        if (!string.IsNullOrEmpty(healTriggerName) && HasTriggerParam(healTriggerName))
        {
            animator.ResetTrigger(healTriggerName);
            animator.SetTrigger(healTriggerName);
            return;
        }

        // fallback: tenta encontrar um trigger que contenha "heal"
        if (string.IsNullOrEmpty(healTriggerName))
        {
            string found = FindTriggerByKeyword("heal");
            if (!string.IsNullOrEmpty(found))
            {
                animator.ResetTrigger(found);
                animator.SetTrigger(found);
                return;
            }
        }

        // fallback por estado
        if (!string.IsNullOrEmpty(healStateName) && HasState(healStateName))
        {
            animator.Play(healStateName, 0, 0f);
        }
    }

    bool HasTriggerParam(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return false;
        var pars = animator.parameters;
        for (int i = 0; i < pars.Length; i++)
            if (pars[i].type == AnimatorControllerParameterType.Trigger && pars[i].name == name)
                return true;
        return false;
    }

    string FindTriggerByKeyword(string keyword)
    {
        if (animator == null || string.IsNullOrEmpty(keyword)) return null;
        string k = keyword.ToLowerInvariant();
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name.ToLowerInvariant().Contains(k))
                return p.name;
        }
        return null;
    }

    bool HasState(string state)
    {
        if (animator == null || string.IsNullOrEmpty(state)) return false;
        int idSimple = Animator.StringToHash(state);
        if (animator.HasState(0, idSimple)) return true;
        string layerName = animator.GetLayerName(0);
        int idLayer = Animator.StringToHash(layerName + "." + state);
        if (animator.HasState(0, idLayer)) return true;
        int idBase = Animator.StringToHash("Base Layer." + state);
        return animator.HasState(0, idBase);
    }

    void HandleDeath()
    {
        // Garante que quaisquer efeitos sejam limpos
        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
            invincibleRoutine = null;
        }
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }
        invincible = false;
        RestoreOriginalRendererStates();

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        
        // Tenta localizar Animator automaticamente se não estiver setado
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null && !string.IsNullOrEmpty(deathStateName))
        {
            try { animator.Play(deathStateName, 0, 0f); }
            catch { /* ignora se o estado não existir */ }
        }

        Debug.Log("Player morreu (vida = 0)");
        Died?.Invoke();
    }

    void StartInvincibility()
    {
        GrantInvulnerability(invincibilityDuration);
    }

    // Public: allow power-ups (e.g., Pickup_Jar) to grant invulnerability
    // with the same blink effect. Calling this resets any active timer.
    public void GrantInvulnerability(float seconds)
    {
        seconds = Mathf.Max(0.01f, seconds);
        if (invincibleRoutine != null) StopCoroutine(invincibleRoutine);
        invincibleRoutine = StartCoroutine(InvincibilityForSeconds(seconds));
    }

    // Same as GrantInvulnerability but applies a color blink (e.g., yellow)
    // instead of toggling visibility. Strength controls blend with original [0..1].
    // Optionally toggles emission glow for materials that support _EmissionColor.
    public void GrantInvulnerabilityTinted(float seconds, Color tint, float strength = 0.35f, bool enableEmissionGlow = true, float emissionIntensity = 2.0f, bool tintAlbedo = true)
    {
        seconds = Mathf.Max(0.01f, seconds);
        strength = Mathf.Clamp01(strength);
        if (invincibleRoutine != null) StopCoroutine(invincibleRoutine);
        invincibleRoutine = StartCoroutine(InvincibilityTintCo(seconds, tint, strength, enableEmissionGlow, emissionIntensity, tintAlbedo));
    }

    IEnumerator InvincibilityForSeconds(float seconds)
    {
        invincible = true;
        float endTime = Time.time + seconds;
        bool visible = true;
        while (Time.time < endTime)
        {
            SetBlinkVisibility(visible);
            visible = !visible;
            yield return new WaitForSeconds(blinkInterval);
        }
        RestoreOriginalRendererStates();
        invincible = false;
        invincibleRoutine = null;
    }

    // Blink by tinting materials slightly with a color (e.g., yellow)
    IEnumerator InvincibilityTintCo(float seconds, Color tint, float strength, bool withEmission, float emissionIntensity, bool tintAlbedo)
    {
        invincible = true;
        float endTime = Time.time + seconds;

        // Collect material color states
        var mats = CollectMaterialStates();
        bool useTint = mats.Count > 0;
        bool toggled = false;

        while (Time.time < endTime)
        {
            if (useTint)
            {
                ApplyTint(mats, tint, strength, toggled, withEmission, emissionIntensity, tintAlbedo);
                toggled = !toggled;
            }
            else
            {
                // Fallback to visibility blink if no color property found
                SetBlinkVisibility(toggled);
                toggled = !toggled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        // Restore
        if (useTint) RestoreMaterialColors(mats);
        else RestoreOriginalRendererStates();
        invincible = false;
        invincibleRoutine = null;
    }

    class MatState
    {
        public Material mat;
        public string colorProp; // optional
        public Color original;
        public string emissionProp; // optional
        public Color originalEmission;
        public bool emissionKeywordInitiallyEnabled;
    }

    List<MatState> CollectMaterialStates()
    {
        var list = new List<MatState>(16);
        foreach (var rs in rendererStates)
        {
            var r = rs.r;
            if (r == null) continue;
            var materials = r.materials; // clones per renderer; safe to change
            for (int i = 0; i < materials.Length; i++)
            {
                var m = materials[i];
                if (m == null) continue;
                string prop = GetMainColorProperty(m);
                string eprop = GetEmissionProperty(m);
                if (prop == null && eprop == null) continue;
                var st = new MatState { mat = m, colorProp = prop, emissionProp = eprop };
                if (prop != null) st.original = m.GetColor(prop);
                if (eprop != null)
                {
                    st.originalEmission = m.GetColor(eprop);
                    st.emissionKeywordInitiallyEnabled = m.IsKeywordEnabled("_EMISSION");
                }
                list.Add(st);
            }
        }
        return list;
    }

    void ApplyTint(List<MatState> mats, Color tint, float strength, bool on, bool withEmission, float emissionIntensity, bool tintAlbedo)
    {
        for (int i = 0; i < mats.Count; i++)
        {
            var st = mats[i];
            if (st.mat == null) continue;
            if (tintAlbedo && st.colorProp != null)
            {
                Color target = on ? Color.Lerp(st.original, tint, strength) : st.original;
                st.mat.SetColor(st.colorProp, target);
            }
            if (withEmission && st.emissionProp != null)
            {
                if (on)
                {
                    st.mat.EnableKeyword("_EMISSION");
                    Color em = tint * Mathf.Max(0f, emissionIntensity);
                    st.mat.SetColor(st.emissionProp, em);
                }
                else
                {
                    if (!st.emissionKeywordInitiallyEnabled)
                        st.mat.DisableKeyword("_EMISSION");
                    st.mat.SetColor(st.emissionProp, st.originalEmission);
                }
            }
        }
    }

    void RestoreMaterialColors(List<MatState> mats)
    {
        for (int i = 0; i < mats.Count; i++)
        {
            var st = mats[i];
            if (st.mat == null) continue;
            if (st.colorProp != null) st.mat.SetColor(st.colorProp, st.original);
            if (st.emissionProp != null)
            {
                st.mat.SetColor(st.emissionProp, st.originalEmission);
                if (!st.emissionKeywordInitiallyEnabled) st.mat.DisableKeyword("_EMISSION");
            }
        }
    }

    string GetMainColorProperty(Material m)
    {
        if (m == null) return null;
        // Common property names (Standard/URP/HDRP variants)
        if (m.HasProperty("_BaseColor")) return "_BaseColor";
        if (m.HasProperty("_Color")) return "_Color";
        if (m.HasProperty("_TintColor")) return "_TintColor";
        return null;
    }

    string GetEmissionProperty(Material m)
    {
        if (m == null) return null;
        if (m.HasProperty("_EmissionColor")) return "_EmissionColor";
        return null;
    }

    void SetBlinkVisibility(bool show)
    {
        if (rendererStates == null) return;
        for (int i = 0; i < rendererStates.Length; i++)
        {
            var rs = rendererStates[i];
            if (rs.r == null) continue;
            // Se show=true, usa o estado original; se show=false, desliga tudo
            rs.r.enabled = show ? rs.initial : false;
        }
    }

    void RestoreOriginalRendererStates()
    {
        if (rendererStates == null) return;
        for (int i = 0; i < rendererStates.Length; i++)
        {
            var rs = rendererStates[i];
            if (rs.r == null) continue;
            rs.r.enabled = rs.initial;
        }
    }

    void ApplyKnockback(Vector3 dir)
    {
        if (!enableKnockback) return;

        var rb = GetComponent<Rigidbody>();
        var rb2d = GetComponent<Rigidbody2D>();
        var movement = GetComponent<PlayerMovement>();

        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackCo(dir, rb, rb2d, movement));
    }

    IEnumerator KnockbackCo(Vector3 dir, Rigidbody rb, Rigidbody2D rb2d, PlayerMovement movement)
    {
        float t = 0f;
        float duration = Mathf.Max(0.01f, knockbackDuration);

        bool reenableMovement = false;
        if (movement != null && movement.enabled)
        {
            movement.enabled = false;
            reenableMovement = true;
        }

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.AddForce((Vector2)(dir.normalized * knockbackForce), ForceMode2D.Impulse);
            yield return new WaitForSeconds(duration);
            rb2d.linearVelocity = Vector2.zero;
        }
        else if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dir.normalized * knockbackForce, ForceMode.VelocityChange);
            yield return new WaitForSeconds(duration);
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            // Sem rigidbody: mover manualmente
            Vector3 start = transform.position;
            Vector3 target = start + dir.normalized * (knockbackForce * 0.1f);
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / duration);
                transform.position = Vector3.Lerp(start, target, a);
                yield return null;
            }
        }

        if (reenableMovement && movement != null)
            movement.enabled = true;

        knockbackRoutine = null;
    }
}
