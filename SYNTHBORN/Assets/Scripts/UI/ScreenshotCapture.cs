using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Synthborn.UI
{
    /// <summary>
    /// Captures a screenshot on the end screen (death/victory).
    /// Saves to a "Screenshots" folder in persistentDataPath.
    /// </summary>
    public class ScreenshotCapture : MonoBehaviour
    {
        [SerializeField] private Button _screenshotButton;
        [SerializeField] private Text _feedbackText;

        private void Awake()
        {
            if (_screenshotButton != null)
                _screenshotButton.onClick.AddListener(CaptureScreenshot);
        }

        private void OnDestroy()
        {
            if (_screenshotButton != null)
                _screenshotButton.onClick.RemoveListener(CaptureScreenshot);
        }

        /// <summary>Capture and save screenshot to disk.</summary>
        public void CaptureScreenshot()
        {
            string folder = Path.Combine(Application.persistentDataPath, "Screenshots");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"SYNTHBORN_{timestamp}.png";
            string fullPath = Path.Combine(folder, filename);

            ScreenCapture.CaptureScreenshot(fullPath);

            if (_feedbackText != null)
                _feedbackText.text = $"Saved: {filename}";

            Debug.Log($"[ScreenshotCapture] Saved to: {fullPath}");
        }
    }
}
