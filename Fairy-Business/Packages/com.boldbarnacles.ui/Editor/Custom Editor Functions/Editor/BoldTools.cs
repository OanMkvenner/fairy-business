using UnityEditor;
using UnityEngine;

public class BoldTools : MonoBehaviour {
	[MenuItem("BoldTools/Reserialize Everything %[")]
	static void ReserializeAll(){
		AssetDatabase.ForceReserializeAssets();
		AssetDatabase.SaveAssets();
	}
}
