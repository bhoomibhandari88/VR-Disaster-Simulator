using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("Panels (child GameObjects of this canvas)")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Title")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text pressToStartText;

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text comboText;

    [Header("Game Over")]
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text restartText;

    private void OnEnable()
    {
        GameEvents.OnGameStart += HandleGameStart;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnScoreChanged += HandleScoreChanged;
        GameEvents.OnLivesChanged += HandleLivesChanged;
        GameEvents.OnComboChanged += HandleComboChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStart -= HandleGameStart;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnScoreChanged -= HandleScoreChanged;
        GameEvents.OnLivesChanged -= HandleLivesChanged;
        GameEvents.OnComboChanged -= HandleComboChanged;
    }

    private void Start()
    {
        ShowTitle();
    }

    private void Update()
    {
        bool waitingToStart = titlePanel.activeSelf || gameOverPanel.activeSelf;
        if (waitingToStart && InputManager.Instance != null && InputManager.Instance.ButtonPressedThisFrame())
        {
            GameManager.Instance.StartGame();
        }
    }

    private void ShowTitle()
    {
        titlePanel.SetActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (titleText != null) titleText.text = "VR SLASH";
        if (pressToStartText != null) pressToStartText.text = "PRESS A OR X TO START";
    }

    private void HandleGameStart()
    {
        titlePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(true);
    }

    private void HandleGameOver()
    {
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        if (gameOverScoreText != null) gameOverScoreText.text = $"SCORE: {GameManager.Instance.Score}";
        if (restartText != null) restartText.text = "PRESS A OR X TO RESTART";
    }

    private void HandleScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void HandleLivesChanged(int lives)
    {
        if (livesText != null) livesText.text = $"Lives: {lives}";
    }

    private void HandleComboChanged(int combo)
    {
        if (comboText == null) return;
        string suffix = combo >= 10 ? " (x3)" : combo >= 5 ? " (x2)" : "";
        comboText.text = $"Combo: {combo}{suffix}";
    }
}
