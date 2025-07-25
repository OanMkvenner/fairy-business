using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class GameUpdater : MonoBehaviour
{
    public GameObject openStorePopup;
    public TMP_Text textVersionNumber;
    public static GameUpdater inst;
    static string storeLink = "";

    private void Awake() {
        inst = this;
    }
    private void Start() {
        LoadingManager.AddLocalizerFinishedCallback(CheckVersion);
    }
    
    public static void CheckVersion()
    {
        Debug.Log("Checking for new Version...");
        storeLink = HybrRemoteConfig.GetStoreLink();
        VersionNumber currentVersion = Utilities.GetGameVersion();
        VersionNumber highestVersion = HybrRemoteConfig.GetVersionNumber();
        //Debug.LogError(currentVersion.major + " " + currentVersion.minor+ " " + currentVersion.patch);
        //Debug.LogError(highestVersion.major + " " + highestVersion.minor+ " " + highestVersion.patch);
        if (highestVersion > currentVersion){
            // show warning and send to shop, if desired
            if (inst.openStorePopup) inst.openStorePopup.SetActive(true);
        }
        if (inst.textVersionNumber){
            inst.textVersionNumber.text = "" + currentVersion.major + "." + currentVersion.minor + "." + currentVersion.patch;
            //UniqueNameHash.Get("TextVersionNumber").GetComponent<TMPro.TMP_Text>().text = "" + currentVersion.major + "." + currentVersion.minor + "." + currentVersion.patch;
        }
        Debug.Log("Version Check complete");
    }
    
    public static void OpenStore(bool closeApp)
    {
	    Application.OpenURL(storeLink);

        if (closeApp) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_IOS // ios forbid closing the app, calling Quit cause app crash, #Apple :)
            Application.Quit();
#endif
        }
    }
}
