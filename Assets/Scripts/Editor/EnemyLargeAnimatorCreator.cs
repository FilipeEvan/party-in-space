using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Tools > Enemies > Create Enemy Large Animator Controller
// - Cria um Animator Controller com estados "CharacterArmature|Run" (default)
//   e "CharacterArmature|Punch" usando os clipes encontrados no projeto.
// - Atribui o controller ao Animator do objeto selecionado (se houver).
// - Adiciona parâmetros comuns: bool "Run", float "Speed", trigger "Punch".
// - Transição automática Punch -> Run com Exit Time para voltar após o ataque.
namespace EditorUtilities
{
    public static class EnemyLargeAnimatorCreator
    {
        const string DefaultRunStateName = "CharacterArmature|Run";
        const string DefaultPunchStateName = "CharacterArmature|Punch";

        [MenuItem("Tools/Enemies/Create Enemy Large Animator Controller")] 
        public static void CreateController()
        {
            // Escolhe onde salvar
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Enemy Large Animator Controller",
                "EnemyLarge.controller",
                "controller",
                "Selecione o local para salvar o Animator Controller.");
            if (string.IsNullOrEmpty(path)) return;

            // Procura clipes de animação
            var allClipGuids = AssetDatabase.FindAssets("t:AnimationClip");
            var allClips = allClipGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null)
                .ToArray();

            AnimationClip runClip = allClips.FirstOrDefault(c => c.name.ToLowerInvariant().Contains("run"));
            AnimationClip punchClip = allClips.FirstOrDefault(c => c.name.ToLowerInvariant().Contains("punch"));

            if (runClip == null)
            {
                EditorUtility.DisplayDialog(
                    "Run clip não encontrado",
                    "Não encontrei nenhum AnimationClip contendo 'run' no nome.\n" +
                    "Crie/importe o clipe e tente novamente.",
                    "OK");
                return;
            }

            if (punchClip == null)
            {
                // Ainda criamos o controller apenas com Run
                if (!EditorUtility.DisplayDialog(
                    "Punch clip não encontrado",
                    "Não encontrei nenhum AnimationClip contendo 'punch' no nome.\n" +
                    "Deseja criar o controller apenas com o estado de corrida?",
                    "Criar assim mesmo",
                    "Cancelar"))
                {
                    return;
                }
            }

            // Cria o controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Parâmetros comuns para compatibilidade com EnemyLargeChase
            controller.AddParameter("Run", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Punch", AnimatorControllerParameterType.Trigger);

            var layer = controller.layers[0];
            var sm = layer.stateMachine;

            // Estado RUN (default)
            var runState = sm.AddState(DefaultRunStateName);
            runState.motion = runClip;
            sm.defaultState = runState;

            AnimatorState punchState = null;
            if (punchClip != null)
            {
                punchState = sm.AddState(DefaultPunchStateName);
                punchState.motion = punchClip;

                // Transição opcional via Trigger
                var toPunch = runState.AddTransition(punchState);
                toPunch.hasExitTime = false;
                toPunch.hasFixedDuration = true;
                toPunch.duration = 0.05f;
                toPunch.AddCondition(AnimatorConditionMode.If, 0, "Punch");

                // Volta automático para Run após terminar o Punch
                var toRun = punchState.AddTransition(runState);
                toRun.hasExitTime = true;
                toRun.exitTime = 0.95f; // quase final do clipe
                toRun.hasFixedDuration = true;
                toRun.duration = 0.05f;
            }

            // Salva mudanças
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Tenta atribuir ao selecionado se tiver Animator
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                var anim = selected.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    Undo.RecordObject(anim, "Assign EnemyLarge Controller");
                    anim.runtimeAnimatorController = controller;
                    EditorUtility.SetDirty(anim);
                }
            }

            // Mensagem de sucesso
            string msg = "Animator Controller criado em: " + path +
                         "\nEstados: " + DefaultRunStateName + (punchClip != null ? ", " + DefaultPunchStateName : "") +
                         "\nSe um objeto com Animator estiver selecionado, o controller foi atribuído.";
            EditorUtility.DisplayDialog("Enemy Large Controller", msg, "OK");
        }
    }
}

