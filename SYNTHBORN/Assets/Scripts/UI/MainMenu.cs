using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Synthborn.UI
{
    /// <summary>
    /// Main Menu: New Game, Load Game, Settings, Quit.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _loadGameButton;
        [SerializeField] private LoadGameScreen _loadGameScreen;
        [SerializeField] private CharacterCreationScreen _creationScreen;

        private void Start()
        {
            if (_newGameButton != null)
                EventSystem.current?.SetSelectedGameObject(_newGameButton.gameObject);
        }

        /// <summary>New Game: show slot picker in new-game mode.</summary>
        public void OnNewGameClicked()
        {
            _loadGameScreen?.Show(isNewGame: true);
        }

        /// <summary>Load Game: show slot picker in load mode (only filled slots).</summary>
        public void OnLoadGameClicked()
        {
            _loadGameScreen?.Show(isNewGame: false);
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
