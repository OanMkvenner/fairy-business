// -----------------------------------------------------------------------------
//
// This sample example C# file can be used to quickly utilise usage of Remote Config APIs
// For more comprehensive code integration, visit https://docs.unity3d.com/Packages/com.unity.remote-config@latest
//
// -----------------------------------------------------------------------------

using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class HybrRemoteConfig : MonoBehaviour
{
    public struct userAttributes {}
    public struct appAttributes {}

    public static JObject appRemoteConfigs;

    async UniTask InitializeRemoteConfigAsync()
    {
        var options = new InitializationOptions()
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            .SetOption("com.unity.services.core.environment-name", "development");
#else
            .SetOption("com.unity.services.core.environment-name", "production");
#endif
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync(options);

        // options can be passed in the initializer, e.g if you want to set analytics-user-id or an environment-name use the lines from below:
        // var options = new InitializationOptions()
        //   .SetOption("com.unity.services.core.analytics-user-id", "my-user-id-1234")
        //   .SetOption("com.unity.services.core.environment-name", "production");
        // await UnityServices.InitializeAsync(options);

        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Awake() {
        appRemoteConfigs = new JObject();
    }
    async void Start() {
        LoadingManager.AddExpectedLoadValue(0.6f, "InitializeRemoteConfig");
        await UniTask.Yield();
        var _ = InitializeRemoteConfig();
    }
    
    public async UniTask InitializeRemoteConfig()
    {
        await UniTask.Yield();
        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        var dontAwait = FetchDefaultValues();
        if (Unity.Services.RemoteConfig.Utilities.CheckForInternetConnection())
        {
            LoadingManager.AddLoadedValue(0.1f, "InitializeRemoteConfig", "CheckForInternetConnection");
            await InitializeRemoteConfigAsync();
            LoadingManager.AddLoadedValue(0.1f, "InitializeRemoteConfig", "InitializeRemoteConfigAsync");
        } else {
            LoadingManager.AddLoadedValue(0.2f, "InitializeRemoteConfig", "No Internet");
        }
        await UniTask.Yield();

        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());

        // -- Example on how to fetch configuration settings using filter attributes:
        // var fAttributes = new filterAttributes();
        // fAttributes.key = new string[] { "sword","cannon" };
        // RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes(), fAttributes);

        // -- Example on how to fetch configuration settings if you have dedicated configType:
        // var configType = "specialConfigType";
        // RemoteConfigService.Instance.FetchConfigs(configType, new userAttributes(), new appAttributes());
        // -- Configuration can be fetched with both configType and fAttributes passed
        // RemoteConfigService.Instance.FetchConfigs(configType, new userAttributes(), new appAttributes(), fAttributes);

        // -- All examples from above will also work asynchronously, returning Task<RuntimeConfig>
        // await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
        // await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes(), fAttributes);
        // await RemoteConfigService.Instance.FetchConfigsAsync(configType, new userAttributes(), new appAttributes());
        // await RemoteConfigService.Instance.FetchConfigsAsync(configType, new userAttributes(), new appAttributes(), fAttributes);
    }

    bool defaultValuesLoaded = false;
    async UniTask FetchDefaultValues()
    {
        await UniTask.Yield();
        appRemoteConfigs = new JObject();
        // read all default RC files and merge them into the currently empty JObj
        
        TextAsset[] translationRCFiles2 = Resources.LoadAll<TextAsset>("RemoteConfig/GeneratedTxtVariant");
        
        LoadingManager.AddLoadedValue(0.0f, "InitializeRemoteConfig", "FetchDefaultValues_initial");
        foreach (var rcFile in translationRCFiles2)
        {
            await UniTask.Yield();
            if (rcFile.ToString() != "")
            {
                var rcJObjEntries = JObject.Parse(rcFile.ToString())["entries"];
                appRemoteConfigs.Merge(rcJObjEntries);
            }
        }
        LoadingManager.AddLoadedValue(0.1f, "InitializeRemoteConfig", "translationRCFiles");
        await UniTask.Yield();
        // following code is to make sure the order is correct, even if ApplyRemoteSettings is called earlier than FetchDefaultValues
        defaultValuesLoaded = true;
        if (remoteSettingsLoaded)
        {
            await ApplyRemoteSettingsAsync(cachedConfigResponse);
            cachedConfigResponse = new ConfigResponse();
        }
    }

    public void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        _ = ApplyRemoteSettingsAsync(configResponse);
    }

    ConfigResponse cachedConfigResponse;
    bool remoteSettingsLoaded = false;
    async UniTask ApplyRemoteSettingsAsync(ConfigResponse configResponse)
    {
        await UniTask.Yield();
        // make sure FetchDefaultValues() has been called already. if not, wait for it to complete
        if (!defaultValuesLoaded){
            cachedConfigResponse = configResponse;
            remoteSettingsLoaded = true;
            return;
        }
        LoadingManager.AddLoadedValue(0.1f, "InitializeRemoteConfig", "ApplyRemoteSettingsAsync");

        // merge remote values into default values if available (Merge overwrites with new values where possible)
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.LogWarning("Remote Values fetched unsuccessful, No Cached values found. Default values will be used");
                break;
            case ConfigOrigin.Cached:
                Debug.LogWarning("Remote Values fetched unsuccessful, Cached values loaded instead");
                //appRemoteConfigs.Merge(RemoteConfigService.Instance.appConfig.config);
                appRemoteConfigs = RemoteConfigService.Instance.appConfig.config;
                break;
            case ConfigOrigin.Remote:
                Debug.Log("Remote Values fetched successfully");
                //ppRemoteConfigs.Merge(RemoteConfigService.Instance.appConfig.config);
                appRemoteConfigs = RemoteConfigService.Instance.appConfig.config;
                break;
        }
        LoadingManager.AddLoadedValue(0.1f, "InitializeRemoteConfig", "ApplyRemoteSettingsAsync Merged");

        // apply all translations and other values
        LoadingManager.RunRemoteConfigFinishedCallbacks(appRemoteConfigs); //indirectly calls Localizer.ApplyAppConfigs(); if Localizer is present and registered itself to LoadingManager
    }

    public static string GetContentAsString(string keyName){
        return (string)(Utilities.GetFromRemoteConfigs(appRemoteConfigs, keyName)?? "" );
    }
    public static JToken GetContentAsJToken(string keyName){
        return Utilities.GetFromRemoteConfigs(appRemoteConfigs, keyName);
    }
    public static T GetContentAsType<T>(string keyName){
        JToken token = Utilities.GetFromRemoteConfigs(appRemoteConfigs, keyName);
        return token.ToType<T>();
    }

    public static VersionNumber GetVersionNumber(){
#if UNITY_IPHONE || UNITY_IOS
        return Utilities.ConvertVersionNumber((string)appRemoteConfigs["releasedVersionIOS"]);
#else
        return Utilities.ConvertVersionNumber((string)appRemoteConfigs["releasedVersionANDROID"]);
#endif
    }
    public static string GetStoreLink(){
#if UNITY_IPHONE || UNITY_IOS
        return (string)appRemoteConfigs["storeLinkIOS"];
#else
        return (string)appRemoteConfigs["storeLinkANDROID"];
#endif
    }
}