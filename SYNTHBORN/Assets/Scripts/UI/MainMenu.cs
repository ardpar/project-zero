using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthborn.UI
{
    /// <summary>
    /// Main Menu controller. Loads gameplay scene on Play, quits on Quit.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("SampleScene");
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
