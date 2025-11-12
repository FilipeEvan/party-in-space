using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Executa automaticamente o setup do controller padrão quando scripts recompilam.
// Procura por Assets/SpaceKit/Characters/EnemyLarge.controller e aplica a configuração
// apenas uma vez, marcando o asset com o label "EnemyLargeSetupDone".
namespace EditorUtilities
{
    public static class AutoRunSetupEnemyLarge
    {
        const string TargetPath = "Assets/SpaceKit/Characters/EnemyLarge.controller";
        const string DoneLabel = "EnemyLargeSetupDone";

        [InitializeOnLoadMethod]
        static void RunOnce()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetPath);
            if (controller == null) return; // caminho não existe

            var labels = AssetDatabase.GetLabels(controller);
            foreach (var l in labels)
            {
                if (l == DoneLabel) return; // já configurado
            }

            // Aplica configuração
            if (EnemyLargeAnimatorSetup.SetupController(controller))
            {
                var newLabels = new System.Collections.Generic.List<string>(labels);
                if (!newLabels.Contains(DoneLabel)) newLabels.Add(DoneLabel);
                AssetDatabase.SetLabels(controller, newLabels.ToArray());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Enemy Large controller configurado automaticamente: " + TargetPath);
            }
        }
    }
}

