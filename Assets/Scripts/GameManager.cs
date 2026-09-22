using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Combo { get; private set; }
    public bool IsPlaying { get; private set; }

    [Header("Config")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int comboFor2x = 5;
    [SerializeField] private int comboFor3x = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnFruitSliced += HandleFruitSliced;
        GameEvents.OnFruitMissed += HandleFruitMissed;
    }

    private void OnDisable()
    {
        GameEvents.OnFruitSliced -= HandleFruitSliced;
        GameEvents.OnFruitMissed -= HandleFruitMissed;
    }

    public void StartGame()
    {
        Score = 0;
        Lives = startingLives;
        Combo = 0;
        IsPlaying = true;

        GameEvents.ScoreChanged(Score);
        GameEvents.LivesChanged(Lives);
        GameEvents.ComboChanged(Combo);
        GameEvents.GameStart();
    }

    private void EndGame()
    {
        IsPlaying = false;
        GameEvents.GameOver();
    }

    private void HandleFruitSliced(FruitColor color, Vector3 hitPoint)
    {
        if (!IsPlaying) return;

        Combo++;
        int multiplier = 1;
        if (Combo >= comboFor3x) multiplier = 3;
        else if (Combo >= comboFor2x) multiplier = 2;

        Score += multiplier;

        GameEvents.ComboChanged(Combo);
        GameEvents.ScoreChanged(Score);
    }

    private void HandleFruitMissed()
    {
        if (!IsPlaying) return;

        Combo = 0;
        GameEvents.ComboChanged(Combo);

        Lives--;
        GameEvents.LivesChanged(Lives);

        if (Lives <= 0)
        {
            EndGame();
        }
    }
}
