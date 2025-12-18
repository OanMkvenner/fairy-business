using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public string localizationString;

    private TextMeshProUGUI uiText;

    private void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }

    public void InitLocalizationString(string locaString)
    {
        localizationString = locaString;
    }
}
