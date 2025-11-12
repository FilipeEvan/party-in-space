// Editor utility to configure a Heal animation on the player's Animator.
// Adds a Trigger parameter (default: "Heal"), creates a Heal state with a clip
// (auto-detected by name contains "heal"), and wires transitions.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class HealAnimationSetup
{
    [MenuItem("Tools/Setup/Configure Heal Animation")]
    public static void Configure()
    {
        var player = Object.FindObjectOfType<PlayerHealth>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Heal Animation Setup", "PlayerHealth não encontrado na cena.", "OK");
            return;
        }

        var animator = player.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Heal Animation Setup", "Animator não encontrado no Player.", "OK");
            return;
        }

        // Resolve AnimatorController (suporta Override Controller)
        var rc = animator.runtimeAnimatorController;
        AnimatorController controller = null;
        if (rc is AnimatorOverrideController ov)
        {
            controller = ov.runtimeAnimatorController as AnimatorController;
        }
        else
        {
            controller = rc as AnimatorController;
        }

        if (controller == null)
        {
            EditorUtility.DisplayDialog("Heal Animation Setup", "AnimatorController não é editável (não é AnimatorController).", "OK");
            return;
        }

        // Escolhe clipe de cura por heurística (nome contém "heal")
        var clip = FindHealClip();
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Heal Animation Setup", "Nenhum AnimationClip com nome contendo 'heal' foi encontrado no projeto.\n\nCrie/import um clip de cura e rode o menu novamente.", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(controller, "Configure Heal Animation");

        // Adiciona Trigger "Heal" se não existir
        const string TriggerName = "Heal";
        if (!controller.parameters.Any(p => p.name == TriggerName))
        {
            controller.AddParameter(TriggerName, AnimatorControllerParameterType.Trigger);
        }

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Verifica se já há um estado de heal
        var healState = sm.states.FirstOrDefault(s => s.state != null && (s.state.name.Contains("Heal") || s.state.motion == clip)).state;
        if (healState == null)
        {
            healState = sm.AddState("Heal");
            healState.motion = clip;
        }
        else
        {
            healState.motion = clip; // garante o clip correto
        }

        // Transition AnyState -> Heal (condition: Trigger Heal)
        bool anyToHealExists = sm.anyStateTransitions.Any(t => t.destinationState == healState);
        if (!anyToHealExists)
        {
            var t = sm.AddAnyStateTransition(healState);
            t.hasExitTime = false;
            t.hasFixedDuration = true;
            t.duration = 0.05f;
            t.interruptionSource = TransitionInterruptionSource.None;
            t.canTransitionToSelf = false;
            t.AddCondition(AnimatorConditionMode.If, 0, TriggerName);
        }

        // Transition Heal -> Default state (after clip end)
        var defaultState = sm.defaultState;
        if (defaultState != null && defaultState != healState)
        {
            bool healToDefaultExists = healState.transitions.Any(tr => tr.destinationState == defaultState);
            if (!healToDefaultExists)
            {
                var back = healState.AddTransition(defaultState);
                back.hasExitTime = true;
                back.exitTime = 0.95f;
                back.hasFixedDuration = true;
                back.duration = 0.05f;
                back.interruptionSource = TransitionInterruptionSource.None;
            }
        }

        // Atualiza PlayerHealth defaults para bater com o setup
        player.healTriggerName = TriggerName;
        player.healStateName = string.Empty;
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(controller);

        EditorUtility.DisplayDialog("Heal Animation Setup", $"Configuração concluída:\n- Trigger: '{TriggerName}'\n- Estado: '{healState.name}'\n- Clip: '{clip.name}'\n\nToque de cura será disparado via PlayerHealth.Heal().", "Fechar");
    }

    static AnimationClip FindHealClip()
    {
        var guids = AssetDatabase.FindAssets("t:AnimationClip");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;
            var n = clip.name.ToLowerInvariant();
            if (n.Contains("heal") || n.Contains("recover") || n.Contains("recupera"))
                return clip;
        }
        return null;
    }
}
#endif
