using UnityEditor;
using UnityEngine;

public static class RoverOrientationPresets
{
    [MenuItem("Tools/Rovers/Apply Rover_Round Preset To Selected")]
    public static void ApplyRoverRoundPreset()
    {
        var selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            EditorUtility.DisplayDialog("Rover Preset", "Selecione um ou mais GameObjects (raiz do Rover).", "OK");
            return;
        }
        int changed = 0;
        foreach (var go in selection)
        {
            var comps = go.GetComponentsInChildren<RoverStraightChase>(true);
            foreach (var c in comps)
            {
                Undo.RecordObject(c, "Apply Rover_Round Preset");
                c.directionMode = RoverStraightChase.ChargeDirectionMode.WorldX; // cruza o mapa no eixo X
                c.axis = RoverStraightChase.LocalAxis.X;   // mantém compatibilidade se voltar ao modo LocalAxis
                c.invertDirection = false;                 // ajuste se precisar inverter
                if (c.forwardReference == null)            // usa o próprio transform por padrão
                    c.forwardReference = null;             
                EditorUtility.SetDirty(c);
                changed++;
            }
        }
        EditorUtility.DisplayDialog("Rover Preset", $"Aplicado em {changed} componente(s).", "OK");
    }
}
