using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class TextureSizeBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int changedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetImporter importerBase = AssetImporter.GetAtPath(path);

            // Nur verarbeiten, wenn der Importer wirklich ein TextureImporter ist
            if (importerBase is TextureImporter importer)
            {
                if (importer.maxTextureSize > 512)
                {
                    importer.maxTextureSize = 512;
                    importer.SaveAndReimport();
                    changedCount++;
                }
            }
        }

        Debug.Log($"Texturen beim Build auf 512 limitiert. ({changedCount} geändert)");
    }
}