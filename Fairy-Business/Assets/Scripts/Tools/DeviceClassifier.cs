using UnityEngine;

namespace Tools
{
    public class DeviceClassifier
    {
        public static bool IsTablet()
        {
            float dpi = Screen.dpi;

            // Falls DPI nicht verfügbar ist (manche Android-Geräte)
            if (dpi == 0)
                return false; // Lieber "Handy" annehmen

            float widthInInches = Screen.width / dpi;
            float heightInInches = Screen.height / dpi;
            float diagonalInInches = Mathf.Sqrt(widthInInches * widthInInches + heightInInches * heightInInches);

            // Tablet-Threshold (üblich: > 6.5–7 Zoll)
            return diagonalInInches >= 7.0f;
        }
    }
}