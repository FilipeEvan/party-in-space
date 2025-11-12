// Ensures heart textures are imported as Sprites with transparency.
// - Auto-runs once on editor load
// - Adds a menu: Tools/Setup/Configure Heart Sprites
// - Applies to files under Assets/Resources/Hearts

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

static class HeartSpritesSetup
{
    const string HeartsPath = "Assets/Resources/Hearts";

    [InitializeOnLoadMethod]
    static void ConfigureOnLoad()
    {
        // Try to configure silently on editor load (no dialog)
        ConfigureHearts(false);
    }

    [MenuItem("Tools/Setup/Configure Heart Sprites")]
    static void ConfigureFromMenu()
    {
        ConfigureHearts(true);
    }

    static void ConfigureHearts(bool showSummary)
    {
        // If folder doesn't exist, nothing to do
        if (!AssetDatabase.IsValidFolder(HeartsPath))
        {
            if (showSummary)
                EditorUtility.DisplayDialog("Heart Sprites", $"Folder not found: {HeartsPath}", "OK");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { HeartsPath });
        int changed = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            { importer.textureType = TextureImporterType.Sprite; dirty = true; }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
            if (!importer.alphaIsTransparency)
            { importer.alphaIsTransparency = true; dirty = true; }
            if (importer.mipmapEnabled)
            { importer.mipmapEnabled = false; dirty = true; }
            if (!importer.sRGBTexture)
            { importer.sRGBTexture = true; dirty = true; }

            // Reasonable defaults for UI
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (dirty)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                changed++;
            }
        }

        if (showSummary)
            EditorUtility.DisplayDialog("Heart Sprites", $"Configured {changed} sprite(s) under {HeartsPath}.", "OK");
    }
}

// Also ensure any new/changed textures under the folder get correct settings.
class HeartSpritesPostprocessor : AssetPostprocessor
{
    const string HeartsPath = "Assets/Resources/Hearts";

    void OnPreprocessTexture()
    {
        if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(HeartsPath)) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.sRGBTexture = true;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
#endif

