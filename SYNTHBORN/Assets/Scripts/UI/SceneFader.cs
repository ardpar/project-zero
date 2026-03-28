using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Synthborn.UI
{
    /// <summary>
    /// Fade-to-black on scene load/unload.
    /// Add to a DontDestroyOnLoad Canvas with a full-screen black Image.
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        private static SceneFader _instance;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 0.3f;

        private void Awake()
        {
            if (_instance != null) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            if (_fadeImage != null) _fadeImage.color = Color.clear;
        }

        /// <summary>Fade out, load scene, fade in.</summary>
        public static void LoadScene(string sceneName)
        {
            if (_instance != null)
                _instance.StartCoroutine(_instance.FadeAndLoad(sceneName));
            else
                SceneManager.LoadScene(sceneName);
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            // Fade to black
            yield return Fade(0f, 1f);

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);

            // Wait one frame for scene to load
            yield return null;

            // Fade from black
            yield return Fade(1f, 0f);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (_fadeImage == null) yield break;
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(from, to, t / _fadeDuration);
                _fadeImage.color = new Color(0, 0, 0, a);
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, to);
        }
    }
}
