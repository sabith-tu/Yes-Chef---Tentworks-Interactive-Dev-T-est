using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<float> OnTimerTick;

    public event Action<int, int> OnScoreUpdated;

    public event Action OnGameEnded;

    public event Action OnGameStarted;

    public event Action<bool> OnPauseChanged;

    private const float GameDuration = 180f;
    private const string HighScoreKey = "HighScore";

    private float remainingTime;
    private int currentScore;
    private int highScore;
    private bool isGameActive;
    private bool isPaused;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public float RemainingTime => remainingTime;
    public bool IsGameActive => isGameActive;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Update()
    {
        if (!isGameActive || isPaused)
            return;

        remainingTime -= Time.deltaTime;
        OnTimerTick?.Invoke(remainingTime);

        if (remainingTime <= 0f)
            EndGame();
    }

    public void StartGame()
    {
        isGameActive = true;
        isPaused = false;
        currentScore = 0;
        remainingTime = GameDuration;

        Time.timeScale = 1f;

        OnScoreUpdated?.Invoke(currentScore, highScore);
        OnTimerTick?.Invoke(remainingTime);
        OnGameStarted?.Invoke();
    }

    public void AddScore(int points)
    {
        if (!isGameActive)
            return;

        currentScore += points;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        OnScoreUpdated?.Invoke(currentScore, highScore);
    }

    public void TogglePause()
    {
        if (!isGameActive)
            return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        OnPauseChanged?.Invoke(isPaused);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EndGame()
    {
        isGameActive = false;
        remainingTime = 0f;
        Time.timeScale = 1f;

        OnTimerTick?.Invoke(0f);
        OnGameEnded?.Invoke();
    }
}
