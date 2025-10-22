using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Editor
{
    public class TextureSizeBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        private const int MaxResolution = 512;

        public void OnPreprocessBuild(BuildReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer != null && importer.maxTextureSize > MaxResolution)
                {
                    importer.maxTextureSize = MaxResolution;
                    importer.SaveAndReimport();
                }
            }
        }
    }
}