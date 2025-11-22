using System.Collections.Generic;
using UnityEngine;

// Anexe este script ao Enemy_Large (ou qualquer inimigo).
// Ele encontra o AnimationClip de caminhada do player e substitui
// os clipes com a palavra-chave "walk" neste Animator para usar o mesmo clipe via AnimatorOverrideController.
public class UsePlayerWalkAnimation : MonoBehaviour
{
    [Header("Referências")]
    public Animator enemyAnimator; // padrão: GetComponentInChildren<Animator>()
    public Animator playerAnimator; // padrão: Animator do Player (Health/Movement)

    [Header("Correspondência")]
    [Tooltip("Palavra-chave usada para encontrar clipes de caminhada nos controllers do Player e do Inimigo.")]
    public string walkKeyword = "walk";
    [Tooltip("Palavra-chave usada para encontrar clipes de corrida nos controllers do Player e do Inimigo.")]
    public string runKeyword = "run";

    [Tooltip("Aplicar override automaticamente no Start.")]
    public bool applyOnStart = true;

    void Reset()
    {
        enemyAnimator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (enemyAnimator == null) enemyAnimator = GetComponentInChildren<Animator>();
        if (playerAnimator == null)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) playerAnimator = ph.GetComponentInChildren<Animator>();
            if (playerAnimator == null)
            {
                var pm = FindObjectOfType<PlayerMovement>();
                if (pm != null) playerAnimator = pm.GetComponentInChildren<Animator>();
            }
        }
    }

    void Start()
    {
        if (applyOnStart) ApplyOverride();
    }

    public void ApplyOverride()
    {
        if (enemyAnimator == null || enemyAnimator.runtimeAnimatorController == null) return;
        if (playerAnimator == null || playerAnimator.runtimeAnimatorController == null) return;

        var playerWalk = FindClipWithKeyword(playerAnimator.runtimeAnimatorController, walkKeyword);
        var playerRun  = FindClipWithKeyword(playerAnimator.runtimeAnimatorController, runKeyword);
        // Pelo menos um precisa existir
        if (playerWalk == null && playerRun == null) return;

        var baseController = enemyAnimator.runtimeAnimatorController;
        var overrideController = new AnimatorOverrideController(baseController);

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        overrideController.GetOverrides(overrides);

        bool changed = false;
        for (int i = 0; i < overrides.Count; i++)
        {
            var original = overrides[i].Key;
            if (original == null) continue;
            var name = original.name.ToLowerInvariant();
            if (playerRun != null && name.Contains(runKeyword.ToLowerInvariant()))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, playerRun);
                changed = true;
            }
            else if (playerWalk != null && name.Contains(walkKeyword.ToLowerInvariant()))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, playerWalk);
                changed = true;
            }
        }

        if (changed)
        {
            overrideController.ApplyOverrides(overrides);
            enemyAnimator.runtimeAnimatorController = overrideController;
        }
    }

    static AnimationClip FindClipWithKeyword(RuntimeAnimatorController controller, string keyword)
    {
        if (controller == null || string.IsNullOrEmpty(keyword)) return null;
        string k = keyword.ToLowerInvariant();
        foreach (var clip in controller.animationClips)
        {
            if (clip == null) continue;
            if (clip.name.ToLowerInvariant().Contains(k)) return clip;
        }
        return null;
    }
}
