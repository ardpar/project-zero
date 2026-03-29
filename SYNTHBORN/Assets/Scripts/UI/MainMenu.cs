using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Synthborn.UI
{
    /// <summary>
    /// Main Menu controller. Loads gameplay scene on Play, quits on Quit.
    /// Sets default selected button for gamepad navigation.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        private void Start()
        {
            // Select play button for gamepad navigation
            if (_playButton != null)
                EventSystem.current?.SetSelectedGameObject(_playButton.gameObject);
        }

        public void OnPlayClicked()
        {
            SceneFader.LoadScene("SampleScene");
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
