/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Internal;

    #if UNITY_IOS
    using UnityEditor.iOS.Xcode;
    #endif

    internal sealed class VideoKitBuildHandler :  IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        private const string CachePath = @"Assets/__VIDEOKIT_DELETE_THIS__";

        int IOrderedCallback.callbackOrder => -1;

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            // Register failure listener
            EditorApplication.update += FailureListener;
            // Embed settings
            EmbedSettings(report);
            // Add photo library usage description
            if (report.summary.platform == BuildTarget.iOS)
                AddPhotoLibraryUsageDescription(report);
            // Set AndroidX
            if (report.summary.platform == BuildTarget.Android)
                SetAndroidXImportSettings ();
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) => ClearSettings();

        private static void EmbedSettings (BuildReport report) {
            // Clear settings
            ClearSettings();
            // Create build token
            var platform = ToPlatformId(report.summary.platform);
            var bundleId = Application.identifier;
            var settings = VideoKitProjectSettings.CreateSettings();
            var client = new VideoKitClient(settings.accessKey);
            try {
                settings.buildToken = Task.Run(() => client.CreateBuildToken()).Result;
                settings.sessionToken = Task.Run(() => client.CreateSessionToken(settings.buildToken, bundleId, platform)).Result;
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: {ex.Message}");
                Debug.LogException(ex);
            }
            // Create asset
            Directory.CreateDirectory(CachePath);
            AssetDatabase.CreateAsset(settings, $"{CachePath}/VideoKit.asset");
            // Add to build
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList() ?? new List<UnityEngine.Object>();
            assets.Add(settings);
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static void AddPhotoLibraryUsageDescription (BuildReport report) {
            #if UNITY_IOS
            var description = VideoKitProjectSettings.instance.PhotoLibraryUsageDescription;
            var outputPath = report.summary.outputPath;
            if (!string.IsNullOrEmpty(description)) {
                // Read plist
                var plistPath = Path.Combine(outputPath, @"Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                // Add photo library descriptions
                plist.root.SetString(@"NSPhotoLibraryUsageDescription", description);
                plist.root.SetString(@"NSPhotoLibraryAddUsageDescription", description);
                // Write to file
                File.WriteAllText(plistPath, plist.WriteToString());
            }
            #endif
        }

        private static void SetAndroidXImportSettings () {
            // Find GUID
            var guids = AssetDatabase.FindAssets("videokit-androidx-core");
            if (guids.Length == 0)
                return;
            // Update importer
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var importer = PluginImporter.GetAtPath(path) as PluginImporter;
            importer.SetCompatibleWithPlatform(BuildTarget.Android, VideoKitProjectSettings.instance.EmbedAndroidX);
        }

        private static void ClearSettings () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets != null) {
                assets.RemoveAll(asset => asset && asset.GetType() == typeof(VideoKitSettings));
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            AssetDatabase.DeleteAsset(CachePath);
        }

        private void FailureListener () {
            if (BuildPipeline.isBuildingPlayer)
                return;
            EditorApplication.update -= FailureListener;
            (this as IPostprocessBuildWithReport).OnPostprocessBuild(null);
        }

        private static string ToPlatformId (BuildTarget target) => target switch {
            BuildTarget.Android             => "ANDROID",
            BuildTarget.iOS                 => "IOS",
            BuildTarget.StandaloneLinux64   => "LINUX",
            BuildTarget.StandaloneOSX       => "MACOS",
            BuildTarget.StandaloneWindows   => "WINDOWS",
            BuildTarget.StandaloneWindows64 => "WINDOWS",
            BuildTarget.WebGL               => "WEB",
            _                               => null,
        };
    }
}