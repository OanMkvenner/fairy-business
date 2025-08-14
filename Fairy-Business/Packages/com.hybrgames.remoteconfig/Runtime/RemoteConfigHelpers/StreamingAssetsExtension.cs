using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public static class StreamingAssetsExtension
{
    /// <summary>
    /// Recursively traverses each folder under <paramref name="path"/> and returns the list of file paths. 
    /// It will only work in Editor mode.
    /// <returns>List of file path strings relative to the streamingAssetsPath</returns>
    public static List<string> GetAllRcFilesRecursively(string streamingAssetsPath, string subfolder = ""){
        return InternalGetAllRcFilesRecursively(streamingAssetsPath + "/" + subfolder, streamingAssetsPath);
    }
    public static List<string> InternalGetAllRcFilesRecursively(string mainRemoteConfigFolder, string removeInitialFolderString){
        List<string> allRcFiles = new List<string>();
        if (!Directory.Exists(mainRemoteConfigFolder)){
            Debug.LogWarning($"Cant find Folder {mainRemoteConfigFolder}");
            return allRcFiles; 
        } 
        foreach (string file in Directory.EnumerateFiles(mainRemoteConfigFolder, "*.rc"))
        {
            // add all entries of this folder
            string result = file.Replace(removeInitialFolderString, ""); // only keep relative path
            result = result.Replace('\\', '/');
            allRcFiles.Add(result);
        }
        foreach (string folder in Directory.EnumerateDirectories(mainRemoteConfigFolder))
        {
            // add subfolder entries as well
            allRcFiles.AddRange(InternalGetAllRcFilesRecursively(folder, removeInitialFolderString));
        }
        return allRcFiles;
    }
    /*
    public static List<string> GetPathsRecursively(string path, ref List<string> paths)
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, path);
        DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
        foreach (var file in dirInfo.GetFiles())
        {
            if (!file.Name.Contains(".meta"))
            {
                paths.Add(Path.Combine(path, file.Name)); // With file extension
            }
        }

        foreach (var dir in dirInfo.GetDirectories())
        {
            GetPathsRecursively(Path.Combine(path, dir.Name), ref paths);
        }

        return paths;
    }*/

    public static string GetFileFromStreamingAssetsPath(string txtPath){
        
        string fs = "";
        if (txtPath.Contains("://") || txtPath.Contains(":///"))
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(txtPath))
            {
                // Request and wait for the desired page.
                webRequest.SendWebRequest();

                while (!webRequest.isDone) {
                }
                fs = webRequest.downloadHandler.text;
            }
        }
        else
        {
            fs = System.IO.File.ReadAllText(txtPath);
        }
        return fs;
    }

    public static string GetRemoteConfigFilePathFileName(){
        return Path.Combine(Application.streamingAssetsPath, "StreamingAssetPaths.txt");
    }
    public static string GetRemoteConfigFilePathFileNameUnityFolder(){
        return Path.Combine(Application.streamingAssetsPath, "StreamingAssetPaths.txt");
    }

    // gets a list of saved path names of all RemoteConfig files, so we can specifically target and read them
    public static List<string> GetSavedPathNames()
    {
        string txtPath = GetRemoteConfigFilePathFileName();

        List<string> filePathsList = new List<string>();

        string fs = GetFileFromStreamingAssetsPath(txtPath);
        string[] fLines = Regex.Split(fs, "\n|\r|\r\n");
        foreach (string line in fLines)
        {
            if (line.Length > 0){
                filePathsList.Add(line);
            }
        }

        return filePathsList;
    }

    
}
