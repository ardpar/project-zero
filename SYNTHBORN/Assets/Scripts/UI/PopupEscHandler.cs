using UnityEngine;
using UnityEngine.InputSystem;

namespace Synthborn.UI
{
    /// <summary>
    /// Closes the active popup panel when ESC or gamepad East (B) is pressed.
    /// Hides MenuGroup while a popup is open.
    /// </summary>
    public class PopupEscHandler : MonoBehaviour
    {
        private static PopupEscHandler _instance;

        [SerializeField] private GameObject _menuGroup;

        private GameObject _activePanel;
        private System.Action _closeCallback;

        /// <summary>True when a popup is registered and active (PauseMenu should skip ESC).</summary>
        public static bool IsActive => _instance != null && _instance._activePanel != null
            && _instance._activePanel.activeSelf;

        private void Awake()
        {
            _instance = this;
        }

        private void Update()
        {
            if (_activePanel == null || !_activePanel.activeSelf) return;

            bool escPressed = Keyboard.current != null &&
                              Keyboard.current.escapeKey.wasPressedThisFrame;
            bool bPressed = Gamepad.current != null &&
                            Gamepad.current.buttonEast.wasPressedThisFrame;

            if (escPressed || bPressed)
            {
                _closeCallback?.Invoke();
            }
        }

        /// <summary>Register a panel as active so ESC can close it. Hides menu.</summary>
        public static void Register(GameObject panel, System.Action onClose)
        {
            if (_instance == null) return;
            _instance._activePanel = panel;
            _instance._closeCallback = onClose;
            if (_instance._menuGroup != null)
                _instance._menuGroup.SetActive(false);
        }

        /// <summary>Unregister when panel is closed. Shows menu.</summary>
        public static void Unregister()
        {
            if (_instance == null) return;
            _instance._activePanel = null;
            _instance._closeCallback = null;
            if (_instance._menuGroup != null)
                _instance._menuGroup.SetActive(true);
        }
    }
}
