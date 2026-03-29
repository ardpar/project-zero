using UnityEngine;
using UnityEngine.UI;

namespace Synthborn.UI
{
    /// <summary>
    /// Reusable confirmation modal. Shows message + Yes/No buttons.
    /// Usage: ConfirmationModal.Show("Are you sure?", () => DoThing());
    /// </summary>
    public class ConfirmationModal : MonoBehaviour
    {
        private static ConfirmationModal _instance;

        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _messageText;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        private System.Action _onConfirm;

        private void Awake()
        {
            _instance = this;
            if (_panel != null) _panel.SetActive(false);

            if (_yesButton != null) _yesButton.onClick.AddListener(OnYes);
            if (_noButton != null) _noButton.onClick.AddListener(OnNo);
        }

        private void OnDestroy()
        {
            if (_yesButton != null) _yesButton.onClick.RemoveListener(OnYes);
            if (_noButton != null) _noButton.onClick.RemoveListener(OnNo);
        }

        /// <summary>Show a confirmation dialog.</summary>
        public static void Show(string message, System.Action onConfirm)
        {
            if (_instance == null)
            {
                // No modal in scene — just execute
                onConfirm?.Invoke();
                return;
            }

            _instance._onConfirm = onConfirm;
            if (_instance._messageText != null) _instance._messageText.text = message;
            if (_instance._panel != null) _instance._panel.SetActive(true);
        }

        private void OnYes()
        {
            if (_panel != null) _panel.SetActive(false);
            _onConfirm?.Invoke();
            _onConfirm = null;
        }

        private void OnNo()
        {
            if (_panel != null) _panel.SetActive(false);
            _onConfirm = null;
        }
    }
}
