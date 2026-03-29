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

        /// <summary>New Game: go directly to character creation (auto-picks first empty slot).</summary>
        public void OnNewGameClicked()
        {
            // Find first empty slot
            int emptySlot = -1;
            for (int i = 0; i < 3; i++)
            {
                if (!Synthborn.Core.Persistence.SaveManager.SlotExists(i))
                {
                    emptySlot = i;
                    break;
                }
            }

            if (emptySlot >= 0)
            {
                _creationScreen?.Show(emptySlot);
            }
            else
            {
                // All slots full — show slot picker to overwrite one
                _loadGameScreen?.Show(isNewGame: true);
            }
        }

        /// <summary>Load Game: show saved games list.</summary>
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
