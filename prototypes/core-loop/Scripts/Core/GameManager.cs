// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float RunTime { get; private set; }
    public int CurrentWave { get; private set; }
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartRun();
    }

    private void Update()
    {
        if (IsRunning)
            RunTime += Time.deltaTime;
    }

    public void StartRun()
    {
        RunTime = 0f;
        CurrentWave = 0;
        IsRunning = true;
        Time.timeScale = 1f;
    }

    public void SetWave(int wave)
    {
        CurrentWave = wave;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        IsRunning = false;
        Debug.Log($"RUN OVER — Survived {RunTime:F1}s, Wave {CurrentWave}");
        // Prototype: restart after 3 seconds
        Invoke(nameof(RestartRun), 3f);
    }

    private void RestartRun()
    {
        GameEvents.Cleanup();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
