using System.IO;
using UnityEngine;

namespace Tools
{
    public class ScreenshotRecorder : MonoBehaviour
    {
        public int frameRate = 30;
        public string folder = "RecordingFrames";
        private int frameCount;

        void Start()
        {
            Time.captureFramerate = frameRate;
            Directory.CreateDirectory(folder);
        }

        void Update()
        {
            string filename = Path.Combine(folder, $"frame_{frameCount:D04}.png");
            ScreenCapture.CaptureScreenshot(filename);
            frameCount++;
        }
    }
}