using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace EditorUtilities
{
    public static class NavMeshAgentTools
    {
        [MenuItem("Tools/Navigation/Disable NavMeshAgent in Selected Hierarchy")]
        public static void DisableAgentsInSelection()
        {
            var roots = Selection.gameObjects;
            if (roots == null || roots.Length == 0)
            {
                EditorUtility.DisplayDialog("Disable Agents", "Selecione um ou mais GameObjects na Hierarchy.", "OK");
                return;
            }
            int count = 0;
            foreach (var r in roots)
            {
                var agents = r.GetComponentsInChildren<NavMeshAgent>(true);
                foreach (var a in agents)
                {
                    Undo.RecordObject(a, "Disable NavMeshAgent");
                    a.enabled = false; // evita erro em runtime ao instanciar
                    EditorUtility.SetDirty(a);
                    count++;
                }
            }
            EditorUtility.DisplayDialog("Disable Agents", $"Desativados {count} NavMeshAgents nas seleções.", "OK");
        }

        [MenuItem("Tools/Navigation/Strip NavMeshAgent from Prefabs in Folder...")]
        public static void StripAgentsInFolder()
        {
            string folder = EditorUtility.OpenFolderPanel("Escolha a pasta com prefabs", "Assets", "");
            if (string.IsNullOrEmpty(folder)) return;
            if (!folder.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog("Pasta inválida", "Selecione uma pasta dentro de Assets.", "OK");
                return;
            }
            string rel = "Assets" + folder.Substring(Application.dataPath.Length);

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { rel });
            int modified = 0;
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var contents = PrefabUtility.LoadPrefabContents(path);
                var agents = contents.GetComponentsInChildren<NavMeshAgent>(true);
                if (agents.Length > 0)
                {
                    foreach (var a in agents)
                    {
                        Object.DestroyImmediate(a, true);
                    }
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    modified++;
                }
                PrefabUtility.UnloadPrefabContents(contents);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Strip Agents", $"Prefabs modificados: {modified}", "OK");
        }
    }
}

