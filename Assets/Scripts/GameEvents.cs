using System;
using UnityEngine;

/// <summary>
/// Static event hub. Any component can raise or listen to these events
/// without needing a direct reference to GameManager, spawner, etc.
/// </summary>
public static class GameEvents
{
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action<FruitColor, Vector3> OnFruitSliced; // color, world hit point
    public static event Action OnFruitMissed;
    public static event Action<int> OnComboChanged;   // new combo count
    public static event Action<int> OnScoreChanged;   // new score
    public static event Action<int> OnLivesChanged;   // new lives count

    public static void GameStart() => OnGameStart?.Invoke();
    public static void GameOver() => OnGameOver?.Invoke();
    public static void FruitSliced(FruitColor color, Vector3 hitPoint) => OnFruitSliced?.Invoke(color, hitPoint);
    public static void FruitMissed() => OnFruitMissed?.Invoke();
    public static void ComboChanged(int combo) => OnComboChanged?.Invoke(combo);
    public static void ScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void LivesChanged(int lives) => OnLivesChanged?.Invoke(lives);
}

/// <summary>Fruit "flavor" — since we're using colored spheres, color doubles as fruit type.</summary>
public enum FruitColor { Red, Green, Blue, Yellow, Purple }
