#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class BuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.LogWarning("OnPreprocessBuild");

        //SaveStreamingAssetPaths();
        CopyAndRenameAllRcFilesInRessourcesFolder();
    }


    private void CopyAndRenameAllRcFilesInRessourcesFolder()
    {
        string localResourcesPath = Application.dataPath + "/Resources";
        List<string> paths = StreamingAssetsExtension.GetAllRcFilesRecursively(localResourcesPath, "RemoteConfig"); // Gets list of files from StreamingAssets/directory
        foreach (var path in paths)
        {
            string finalPath = path.Replace("/RemoteConfig/", "/RemoteConfig/GeneratedTxtVariant/");
            File.Copy(localResourcesPath + path, localResourcesPath + "" + finalPath + ".txt", true);
        }
    }

    List<string> GetAllRcFiles(){
         List<string> allRcFiles = new List<string>();
        List<string> allRcFilePaths = StreamingAssetsExtension.GetSavedPathNames();
        foreach (var file in allRcFilePaths)
        {
            allRcFiles.Add(StreamingAssetsExtension.GetFileFromStreamingAssetsPath(Application.streamingAssetsPath + file));
        }
        return allRcFiles;
    }
    // code for saving a FILE that contains the folder paths within StreamingAssetsPath so we can later read those paths and read the corresponding files without having to hardcode these paths.
    // basically its a script to remember a whole folders content withint streamingAssets, because there is no OS agnostic way of doing this dynamically.
    private void SaveStreamingAssetPaths()
    {
        List<string> paths = StreamingAssetsExtension.GetAllRcFilesRecursively(Application.streamingAssetsPath, "RemoteConfig"); // Gets list of files from StreamingAssets/directory



        // You could also save paths of files in Resources
        // List<string> paths = ResourcesExtension.GetPathsRecursively(directory); // Gets list of files from Resources/directory

        string txtPath = StreamingAssetsExtension.GetRemoteConfigFilePathFileNameUnityFolder(); // writes the list of file paths to /Assets/Resources/
        if (File.Exists(txtPath))
        {
            File.Delete(txtPath);
        }
        using (FileStream fs = File.Create(txtPath)) {}
        using(StreamWriter writer = new StreamWriter(txtPath, false))
        {
            foreach (string path in paths)
            {
                    writer.WriteLine(path);
            }
        }
        
    }
}

#endif
