using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup startPanel;

    [SerializeField]
    private CanvasGroup hudPanel;

    [SerializeField]
    private CanvasGroup pausePanel;

    [SerializeField]
    private CanvasGroup gameOverPanel;

    [Header("Start Panel")]
    [SerializeField]
    private TextMeshProUGUI controlsText;

    [SerializeField]
    private Button startButton;

    [Header("HUD")]
    [SerializeField]
    private TextMeshProUGUI hudScoreText;

    [SerializeField]
    private TextMeshProUGUI hudHighScoreText;

    [SerializeField]
    private TextMeshProUGUI hudTimerText;

    [SerializeField]
    private Button pauseButton;

    [SerializeField]
    private Button hudQuitButton;

    [Header("Pause Panel")]
    [SerializeField]
    private Button resumeButton;

    [SerializeField]
    private Button pauseQuitButton;

    [Header("Game Over Panel")]
    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [SerializeField]
    private TextMeshProUGUI gameOverHighScoreText;

    [SerializeField]
    private GameObject newHighScoreBanner;

    [SerializeField]
    private Button restartButton;

    [Header("Transition")]
    [SerializeField]
    private float panelFadeDuration = 0.25f;

    [SerializeField]
    private float panelSlideDistance = 30f;

    private bool newHighAchievedThisGame;
    private Coroutine timerPulseCoroutine;
    private bool timerIsUrgent;
    private CanvasGroup activePanel;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (hudQuitButton != null)
            hudQuitButton.onClick.AddListener(OnQuitClicked);
        if (pauseQuitButton != null)
            pauseQuitButton.onClick.AddListener(OnQuitClicked);

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnGameStarted += HandleGameStarted;
            gm.OnTimerTick += HandleTimerTick;
            gm.OnScoreUpdated += HandleScoreUpdated;
            gm.OnGameEnded += HandleGameEnded;
            gm.OnPauseChanged += HandlePauseChanged;
        }

        if (controlsText != null)
        {
            controlsText.text =
                "<b>Controls</b>\n\n"
                + "<b>Move : </b>    WASD / Arrow Keys\n"
                + "<b>Interact : </b>E  (near a station)\n"
                + "<b>Pause : </b>   Escape\n\n"
                + "Walk to the <b>Refrigerator</b> — the ingredient menu opens automatically.\n"
                + "Prepare ingredients at the <b>Table</b> (chop) or <b>Stove</b> (cook).\n"
                + "Deliver to a <b>Customer Window</b> and press E to serve.\n"
                + "Use the <b>Trash</b> to discard wrong ingredients.";
        }

        HideAllImmediate();
        ShowPanel(startPanel);
    }

    private void OnDestroy()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;
        gm.OnGameStarted -= HandleGameStarted;
        gm.OnTimerTick -= HandleTimerTick;
        gm.OnScoreUpdated -= HandleScoreUpdated;
        gm.OnGameEnded -= HandleGameEnded;
        gm.OnPauseChanged -= HandlePauseChanged;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            GameManager.Instance?.TogglePause();
    }

    private void HandleGameStarted()
    {
        newHighAchievedThisGame = false;
        timerIsUrgent = false;
        UpdateScoreUI(GameManager.Instance.CurrentScore, GameManager.Instance.HighScore);
        UpdateTimerUI(GameManager.Instance.RemainingTime);
        ShowPanel(hudPanel);
    }

    private void HandleTimerTick(float remaining)
    {
        UpdateTimerUI(remaining);
    }

    private void HandleScoreUpdated(int score, int highScore)
    {
        if (score > 0 && score == highScore)
            newHighAchievedThisGame = true;

        UpdateScoreUI(score, highScore);
        StartCoroutine(
            PunchScale(hudScoreText != null ? hudScoreText.transform : null, 1.25f, 0.08f)
        );
    }

    private void HandleGameEnded()
    {
        int score = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;

        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {score}";

        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = $"Best: {highScore}";

        if (newHighScoreBanner != null)
            newHighScoreBanner.SetActive(newHighAchievedThisGame);

        ShowPanel(gameOverPanel);
    }

    private void HandlePauseChanged(bool paused)
    {
        ShowPanel(paused ? pausePanel : hudPanel);
    }

    private void OnStartClicked() => GameManager.Instance?.StartGame();

    private void OnPauseClicked() => GameManager.Instance?.TogglePause();

    private void OnResumeClicked() => GameManager.Instance?.TogglePause();

    private void OnRestartClicked() => GameManager.Instance?.StartGame();

    private void OnQuitClicked() => GameManager.Instance?.QuitGame();

    private void HideAllImmediate()
    {
        SetPanelImmediate(startPanel, 0f);
        SetPanelImmediate(hudPanel, 0f);
        SetPanelImmediate(pausePanel, 0f);
        SetPanelImmediate(gameOverPanel, 0f);
    }

    private void ShowPanel(CanvasGroup incoming)
    {
        if (activePanel != null && activePanel != incoming)
            StartCoroutine(FadePanel(activePanel, 0f));

        if (incoming != null)
            StartCoroutine(FadePanel(incoming, 1f));

        activePanel = incoming;
    }

    private IEnumerator FadePanel(CanvasGroup cg, float target)
    {
        if (cg == null)
            yield break;

        float start = cg.alpha;
        float elapsed = 0f;

        if (target > 0f)
        {
            cg.gameObject.SetActive(true);
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        RectTransform rt = cg.GetComponent<RectTransform>();
        Vector2 restPos = rt != null ? rt.anchoredPosition : Vector2.zero;
        Vector2 offscreenOffset = new Vector2(0f, -panelSlideDistance);

        if (rt != null && target > 0f)
            rt.anchoredPosition = restPos + offscreenOffset;

        while (elapsed < panelFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panelFadeDuration);

            cg.alpha = Mathf.Lerp(start, target, t);

            if (rt != null)
            {
                if (target > 0f)
                    rt.anchoredPosition = Vector2.Lerp(restPos + offscreenOffset, restPos, t);
            }

            yield return null;
        }

        cg.alpha = target;
        if (rt != null && target > 0f)
            rt.anchoredPosition = restPos;

        if (target <= 0f)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.gameObject.SetActive(false);
        }
    }

    private static void SetPanelImmediate(CanvasGroup cg, float alpha)
    {
        if (cg == null)
            return;
        cg.alpha = alpha;
        cg.interactable = alpha > 0f;
        cg.blocksRaycasts = alpha > 0f;
        cg.gameObject.SetActive(alpha > 0f);
    }

    private void UpdateTimerUI(float remaining)
    {
        if (hudTimerText == null)
            return;

        remaining = Mathf.Max(0f, remaining);
        int m = Mathf.FloorToInt(remaining / 60f);
        int s = Mathf.FloorToInt(remaining % 60f);
        hudTimerText.text = $"{m}:{s:00}";

        bool urgent = remaining <= 30f;
        hudTimerText.color = urgent ? Color.red : Color.white;

        if (urgent && !timerIsUrgent)
        {
            timerIsUrgent = true;
            if (timerPulseCoroutine != null)
                StopCoroutine(timerPulseCoroutine);
            timerPulseCoroutine = StartCoroutine(TimerUrgencyPulse());
        }
    }

    private void UpdateScoreUI(int score, int highScore)
    {
        if (hudScoreText != null)
            hudScoreText.text = $"Score: {score}";
        if (hudHighScoreText != null)
            hudHighScoreText.text = $"Best:  {highScore}";
    }

    private IEnumerator TimerUrgencyPulse()
    {
        while (hudTimerText != null && timerIsUrgent)
        {
            yield return PunchScale(hudTimerText.transform, 1.15f, 0.18f);
            yield return new WaitForSecondsRealtime(0.6f);
        }
    }

    private IEnumerator PunchScale(Transform t, float peakScale, float halfDuration)
    {
        if (t == null)
            yield break;

        Vector3 origin = t.localScale;
        Vector3 peak = origin * peakScale;

        for (float e = 0f; e < halfDuration; e += Time.unscaledDeltaTime)
        {
            if (t == null)
                yield break;
            t.localScale = Vector3.Lerp(origin, peak, e / halfDuration);
            yield return null;
        }
        for (float e = 0f; e < halfDuration; e += Time.unscaledDeltaTime)
        {
            if (t == null)
                yield break;
            t.localScale = Vector3.Lerp(peak, origin, e / halfDuration);
            yield return null;
        }
        if (t != null)
            t.localScale = origin;
    }
}
