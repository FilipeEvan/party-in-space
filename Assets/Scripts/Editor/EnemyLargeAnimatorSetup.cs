using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Configura um Animator Controller existente com parâmetros e transições:
// - Parâmetros: bool Run, float Speed, trigger Punch
// - Estados: Idle (default), Run, Punch
// - Transições: Idle<->Run por bool Run; AnyState->Punch por trigger Punch;
//               Punch->Run (Run==true) e Punch->Idle (Run==false) com Exit Time.
// Use com um objeto que tenha Animator selecionado ou escolha o asset manualmente.
namespace EditorUtilities
{
    public static class EnemyLargeAnimatorSetup
    {
        [MenuItem("Tools/Enemies/Setup Enemy Large Controller (Idle/Run/Punch)")]
        public static void SetupSelected()
        {
            AnimatorController controller = null;
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                var anim = selected.GetComponentInChildren<Animator>();
                if (anim != null) controller = anim.runtimeAnimatorController as AnimatorController;
            }

            if (controller == null)
            {
                // Escolhe via file picker
                var path = EditorUtility.OpenFilePanel("Selecione um Animator Controller", "Assets", "controller");
                if (string.IsNullOrEmpty(path)) return;
                if (path.StartsWith(Application.dataPath))
                {
                    string rel = "Assets" + path.Substring(Application.dataPath.Length);
                    controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(rel);
                }
            }

            if (controller == null)
            {
                EditorUtility.DisplayDialog("Controller não encontrado",
                    "Selecione um GameObject com Animator ou escolha um asset .controller.", "OK");
                return;
            }

            if (SetupController(controller))
            {
                EditorUtility.DisplayDialog("Enemy Large Controller",
                    "Controller configurado com Idle/Run/Punch, parâmetros e transições.", "OK");
            }
        }

        public static bool SetupAtPath(string relativeAssetPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(relativeAssetPath);
            if (controller == null) return false;
            return SetupController(controller);
        }

        public static bool SetupController(AnimatorController controller)
        {
            if (controller == null) return false;
            Undo.RecordObject(controller, "Setup Enemy Large Controller");

            // Parâmetros
            EnsureParameter(controller, "Run", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Punch", AnimatorControllerParameterType.Trigger);

            var layer = controller.layers[0];
            var sm = layer.stateMachine;

            // Estados (usa o nome do clipe quando possível)
            var idleClip = FindClip("idle");
            var runClip = FindClip("run");
            var punchClip = FindClip("punch");

            var idle = FindOrCreateStateWithClip(sm, idleClip != null ? idleClip.name : "Idle", idleClip);
            var run = FindOrCreateStateWithClip(sm, runClip != null ? runClip.name : "CharacterArmature|Run", runClip);
            var punch = FindOrCreateStateWithClip(sm, punchClip != null ? punchClip.name : "CharacterArmature|Punch", punchClip);

            if (idle == null)
            {
                idle = sm.AddState("Idle");
            }

            if (run == null)
            {
                run = sm.AddState("CharacterArmature|Run");
            }

            sm.defaultState = idle; // Idle como padrão

            // Transições Idle<->Run por bool Run
            EnsureTransitionWithBool(idle, run, true, "Run");
            EnsureTransitionWithBool(run, idle, false, "Run");

            // AnyState -> Punch por Trigger
            EnsureAnyStateTo(sm, punch, "Punch");

            // Punch -> Run (Run==true) e Punch -> Idle (Run==false)
            EnsureExitTo(punch, run, true, "Run");
            EnsureExitTo(punch, idle, false, "Run");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        static void EnsureParameter(AnimatorController c, string name, AnimatorControllerParameterType type)
        {
            if (!c.parameters.Any(p => p.name == name && p.type == type))
            {
                c.AddParameter(name, type);
            }
        }

        static AnimationClip FindClip(string keyword)
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip " + keyword);
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null && clip.name.ToLowerInvariant().Contains(keyword.ToLowerInvariant()))
                    return clip;
            }
            return null;
        }

        static AnimatorState FindOrCreateStateWithClip(AnimatorStateMachine sm, string name, AnimationClip clip)
        {
            var state = sm.states.FirstOrDefault(s => s.state != null && s.state.name == name).state;
            if (state == null)
            {
                // tenta por palavra-chave
                string key = name.ToLowerInvariant();
                state = sm.states.FirstOrDefault(s => s.state != null && s.state.name.ToLowerInvariant().Contains(key)).state;
            }
            if (state == null)
            {
                state = sm.AddState(name);
            }
            if (clip != null) state.motion = clip;
            return state;
        }

        static void EnsureTransitionWithBool(AnimatorState from, AnimatorState to, bool value, string param)
        {
            if (from == null || to == null) return;
            var existing = from.transitions.FirstOrDefault(t => t.destinationState == to);
            if (existing == null)
            {
                existing = from.AddTransition(to);
                existing.hasExitTime = false;
                existing.duration = 0.1f;
            }
            existing.conditions = new[] { new AnimatorCondition
            {
                mode = value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                parameter = param,
                threshold = 0
            }};
        }

        static void EnsureAnyStateTo(AnimatorStateMachine sm, AnimatorState to, string triggerParam)
        {
            if (sm == null || to == null) return;
            var anyTrans = sm.anyStateTransitions.FirstOrDefault(t => t.destinationState == to);
            if (anyTrans == null)
            {
                anyTrans = sm.AddAnyStateTransition(to);
                anyTrans.hasExitTime = false;
                anyTrans.duration = 0.05f;
            }
            anyTrans.conditions = new[] { new AnimatorCondition
            {
                mode = AnimatorConditionMode.If,
                parameter = triggerParam,
                threshold = 0
            }};
        }

        static void EnsureExitTo(AnimatorState fromPunch, AnimatorState toState, bool runValue, string runParam)
        {
            if (fromPunch == null || toState == null) return;
            var existing = fromPunch.transitions.FirstOrDefault(t => t.destinationState == toState);
            if (existing == null)
            {
                existing = fromPunch.AddTransition(toState);
                existing.hasExitTime = true;
                existing.exitTime = 0.95f;
                existing.duration = 0.05f;
            }
            existing.conditions = new[] { new AnimatorCondition
            {
                mode = runValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                parameter = runParam,
                threshold = 0
            }};
        }
    }
}
