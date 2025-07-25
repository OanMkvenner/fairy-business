using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
#if UNITY_IPHONE || UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.Text;

public class XCodePostprocess : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_IOS

    [PostProcessBuild(1000)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        try {
            UnityEngine.Debug.Log("Starting XCode postprocess");

            #if !OFFLINE
            //CopyFirebaseConfigFile(pathToBuiltProject);
            #endif
            //FixPListForFirebase(pathToBuiltProject);
            FixPListForNoEncryption(pathToBuiltProject);
            //FixPListCFBundleURLTypes(pathToBuiltProject);
            EnableCpp17(pathToBuiltProject);
            // deprecated for multitasking in iOS 13 / iPadOS
            RemoveDeprecatedUIApplicationExitsOnSuspend(pathToBuiltProject);

            FixXcodeProj(pathToBuiltProject);

            LocalizePList(pathToBuiltProject);
        } catch (System.Exception e) {
            UnityEngine.Debug.LogError("Error in XCode postprocess\n" + e.ToString());
        }
    }


    static string GetTeamID()
    {
        return System.Environment.GetEnvironmentVariable("IOS_TEAM_ID");
    }


    static string GetCodeSignIdentity()
    {
        return System.Environment.GetEnvironmentVariable("IOS_CODE_SIGN_IDENTITY");
    }


    static string GetProvisioningProfileSpecifier()
    {
        return System.Environment.GetEnvironmentVariable("IOS_PROVISIONING_PROFILE_SPECIFIER");
    }


    static string GetCodeSignEntitlements()
    {
        return System.Environment.GetEnvironmentVariable("IOS_CODE_SIGN_ENTITLEMENTS");
    }


    static void FixXcodeProj(string path)
    {
        string xcodeProject = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        var pbx = new PBXProject();
        pbx.ReadFromFile(xcodeProject);

        string unityTargetGuid = pbx.GetUnityMainTargetGuid();
        string unityFrameworkGuid = pbx.GetUnityFrameworkTargetGuid();


        ///////////////////////
        // Add Capabilities
        ///////////////////////
        pbx.AddCapability(unityTargetGuid, PBXCapabilityType.PushNotifications);

        string teamID = (GetTeamID()?.Trim() ?? "");
        if (!string.IsNullOrEmpty(teamID)) {
            pbx.SetTeamId(unityTargetGuid, teamID);
            pbx.SetTeamId(unityFrameworkGuid, teamID);
        }

        ///////////////////////
        // Add Files
        ///////////////////////
        /*
        if (!pbx.ContainsFileByRealPath($"{path}/GoogleService-Info.plist", PBXSourceTree.Source)) {
            var fileGuid = pbx.AddFile($"{path}/GoogleService-Info.plist", "GoogleService-Info.plist", PBXSourceTree.Source);
            pbx.AddFileToBuild(unityTargetGuid, fileGuid);
            pbx.AddFileToBuild(unityFrameworkGuid, fileGuid);
        }

        ///////////////////////
        // Add ShellScrips
        ///////////////////////
        bool foundScript = false;
        var phases = pbx.GetAllBuildPhasesForTarget(unityTargetGuid);
        foreach (var guid in phases) {
            var name = pbx.GetBuildPhaseName(guid);
            if (name == "Crashalitics-Run-Script") {
                foundScript = true;
            }
        }
        if (!foundScript) {
            pbx.AddShellScriptBuildPhase(unityTargetGuid, "Crashalitics-Run-Script", "/bin/sh", "\"${PODS_ROOT}/FirebaseCrashlytics/run\"");
        }
        */

        ////////////////////////////
        // Add/Change Build config properties
        ////////////////////////////

        var buildConfigs = pbx.BuildConfigNames();

        string codeSignIdentity = GetCodeSignIdentity();
        string provisioningProfileSpecifier = GetProvisioningProfileSpecifier();
        string codeSignEntitlements = GetCodeSignEntitlements();

        foreach (var configName in buildConfigs) {
            var configGuid = pbx.BuildConfigByName(unityFrameworkGuid, configName);

            pbx.SetBuildPropertyForConfig(configGuid, "CLANG_CXX_LANGUAGE_STANDARD", "c++17");
            pbx.SetBuildPropertyForConfig(configGuid, "CODE_SIGN_STYLE", "Manual");
            if (!string.IsNullOrEmpty(codeSignIdentity)) {
                pbx.SetBuildPropertyForConfig(configGuid, "CODE_SIGN_IDENTITY", codeSignIdentity);
            }
            if (!string.IsNullOrEmpty(teamID)) {
                pbx.SetBuildPropertyForConfig(configGuid, "DEVELOPMENT_TEAM", teamID);
            }
            pbx.SetBuildPropertyForConfig(configGuid, "PROVISIONING_PROFILE_SPECIFIER", "");
        }

        foreach (var configName in buildConfigs) {
            var configGuid = pbx.BuildConfigByName(unityTargetGuid, configName);

            pbx.SetBuildPropertyForConfig(configGuid, "CLANG_CXX_LANGUAGE_STANDARD", "c++17");
            pbx.SetBuildPropertyForConfig(configGuid, "CODE_SIGN_STYLE", "Manual");
            if (!string.IsNullOrEmpty(codeSignIdentity)) {
                pbx.SetBuildPropertyForConfig(configGuid, "CODE_SIGN_IDENTITY", codeSignIdentity);
            }
            if (!string.IsNullOrEmpty(teamID)) {
                pbx.SetBuildPropertyForConfig(configGuid, "DEVELOPMENT_TEAM", teamID);
            }
            if (!string.IsNullOrEmpty(provisioningProfileSpecifier)) {
                pbx.SetBuildPropertyForConfig(configGuid, "PROVISIONING_PROFILE_SPECIFIER", provisioningProfileSpecifier);
            }
            if (!string.IsNullOrEmpty(codeSignEntitlements)) {
                pbx.SetBuildPropertyForConfig(configGuid, "CODE_SIGN_ENTITLEMENTS", codeSignEntitlements);
            }
        }

        {
            pbx.AddFrameworkToProject(unityTargetGuid, "UnityFramework.framework", false);
        }
        //===========================


        pbx.WriteToFile(xcodeProject);
    }

    static void EnableCpp17(string path)
    {
        string xcodeProject = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        string[] lines = File.ReadAllLines(xcodeProject);
        for (int i = 0; i < lines.Length; ++i) {
            if (lines[i].Contains("CLANG_CXX_LANGUAGE_STANDARD")) {
                lines[i] = "CLANG_CXX_LANGUAGE_STANDARD = \"c++17\";";
            }
        }
        File.WriteAllLines(xcodeProject, lines);
    }

    public static string GetGoogleServiceBuildType()
    {
        return System.Environment.GetEnvironmentVariable("GOOGLE_SERVICE_BUILD_TYPE");
    }

    static string GetGoogleServiceConfigDir()
    {
        string googleServiceBuildType = GetGoogleServiceBuildType();

        string builtTypeSubFolder = "prod";
        if (!string.IsNullOrEmpty(googleServiceBuildType)) {
            builtTypeSubFolder = googleServiceBuildType;
        }

        return builtTypeSubFolder;
    }


    static string GetGoogleServiceReverseClientId()
    {
        string googleServiceBuildType = GetGoogleServiceBuildType();

        if (googleServiceBuildType == "prod") {
            return "com.googleusercontent.apps.892541213268-kr6uqtgs90tac59fl6r1prless9krfd6";
        } else {
            return "com.googleusercontent.apps.892541213268-kr6uqtgs90tac59fl6r1prless9krfd6";
        }
    }

    /*
    static void CopyFirebaseConfigFile(string path)
    {
        string builtTypeSubFolder = GetGoogleServiceConfigDir();
        if (!string.IsNullOrEmpty(builtTypeSubFolder)) {
            builtTypeSubFolder = $"{builtTypeSubFolder}/";
        }

        UnityEngine.Debug.Log(Application.dataPath + "/GoogleServicesConfigs/" + builtTypeSubFolder + "GoogleService-Info.plist" + " -> " + path + "/GoogleService-Info.plist");
        File.Copy(Application.dataPath + "/GoogleServicesConfigs/" + builtTypeSubFolder + "GoogleService-Info.plist",
                  path + "/GoogleService-Info.plist", true);
    }

    private static void FixPListForFirebase(string path)
    {
        string plist = path + "/Info.plist";

        string[] plistLines = File.ReadAllLines(plist);

        for (int i = plistLines.Length - 1; i >= 0; --i) {
            var l = plistLines[i];
            if (l.Contains("</dict>")) {
                plistLines[i] = "<key>NSPhotoLibraryUsageDescription</key>\n";
                plistLines[i] += "<string>Advertisement would like to store a photo.</string>\n";

                plistLines[i] += "<key>NSLocationWhenInUseUsageDescription</key>\n";
                plistLines[i] += "<string>Cloud Messaging. Anonymous Analytics.</string>\n";

                plistLines[i] += "<key>UIBackgroundModes</key>\n";
                plistLines[i] += "<array>\n";
                plistLines[i] += "  <string>remote-notification</string>\n";
                plistLines[i] += "</array>\n";

                plistLines[i] += "</dict>\n";
                break;
            }
        }

        File.WriteAllLines(plist, plistLines);
    }
    */

    private static void FixPListForNoEncryption(string path)
    {
        string plist = path + "/Info.plist";

        string[] plistLines = File.ReadAllLines(plist);

        for (int i = plistLines.Length - 1; i >= 0; --i) {
            var l = plistLines[i];
            if (l.Contains("</dict>")) {
                plistLines[i] = "<key>ITSAppUsesNonExemptEncryption</key><false/>\n";
                plistLines[i] += "</dict>\n";
                break;
            }
        }

        File.WriteAllLines(plist, plistLines);
    }

    /*
    private static void FixPListCFBundleURLTypes(string path)
    {
        string plist = path + "/Info.plist";

        string[] plistLines = File.ReadAllLines(plist);


        string reverseId = GetGoogleServiceReverseClientId();

        for (int i = plistLines.Length - 1; i >= 0; --i) {
            var l = plistLines[i];
            if (l.Contains("</dict>")) {
                plistLines[i] = "<key>CFBundleURLTypes</key>\n";
                plistLines[i] += "<array>\n";
                plistLines[i] += "  <dict>\n";
                plistLines[i] += "    <key>CFBundleTypeRole</key>\n";
                plistLines[i] += "    <string>Editor</string>\n";
                plistLines[i] += "    <key>CFBundleURLSchemes</key>\n";
                plistLines[i] += "    <array>\n";
                plistLines[i] += "      <string>" + reverseId + "</string>\n";
                plistLines[i] += "    </array>\n";
                plistLines[i] += "  </dict>\n";
                plistLines[i] += "</array>\n";
                plistLines[i] += "</dict>\n";
                break;
            }
        }

        File.WriteAllLines(plist, plistLines);
    }
    */

    private static void RemoveDeprecatedUIApplicationExitsOnSuspend(string path)
    {
        string plistPath = path + "/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);


        if (plist.root.values.ContainsKey("UIApplicationExitsOnSuspend")) {
            plist.root.values.Remove("UIApplicationExitsOnSuspend");
        }
        if (plist.root.values.ContainsKey("UIRequiredDeviceCapabilities")) {
            var elem = plist.root["UIRequiredDeviceCapabilities"];
            elem.AsArray().values.RemoveAll(item => item.AsString() == "metal");
        }

        plist.WriteToFile(plistPath);
    }

    private static void LocalizePList(string path)
    {
        string xcodeProject = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        var pbx = new PBXProject();
        pbx.ReadFromFile(xcodeProject);

        string stringsTemplate =
 @"/*
Automatically generated by XCodePostprocess
*/

CFBundleDisplayName = ""{0}"";
";
        AddLocalizedPList(pbx, path, "en", string.Format(stringsTemplate, BuildsManager.PrepareAppName("Finding Atlantis")));
        AddLocalizedPList(pbx, path, "fr", string.Format(stringsTemplate, BuildsManager.PrepareAppName("Finding Atlantis")));
        AddLocalizedPList(pbx, path, "de", string.Format(stringsTemplate, BuildsManager.PrepareAppName("Finding Atlantis")));
        AddLocalizedPList(pbx, path, "pl", string.Format(stringsTemplate, BuildsManager.PrepareAppName("Finding Atlantis")));

        pbx.WriteToFile(xcodeProject);
    }

    private static void AddLocalizedPList(PBXProject pbx, string path, string locale, string stringsContent)
    {
        string lprojPath = $"{path}/{locale}.lproj";
        if (!System.IO.Directory.Exists(lprojPath)) {
            System.IO.Directory.CreateDirectory(lprojPath);
        }

        System.IO.File.WriteAllBytes($"{lprojPath}/InfoPlist.strings", Encoding.UTF8.GetBytes(stringsContent));

        var guid = pbx.AddFolderReference(lprojPath, $"{locale}.lproj");
        pbx.AddFileToBuild(pbx.GetUnityMainTargetGuid(), guid);
    }

#endif
}
