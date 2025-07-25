using UnityEngine;

public class CardScanLogger : MonoBehaviour {
    private void Start() {
        CameraOpencvLib.StartScanning();
    }
    private void Update() {
        var newScan = CameraOpencvLib.GetNewScanResult();
        if (newScan != null) Debug.Log($"Found Scan of name: {newScan.name} Rotation: {newScan.rotation} Position: {newScan.pos} and more...");
    }
}