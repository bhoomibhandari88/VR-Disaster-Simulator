using UnityEngine;

public class Fruit : MonoBehaviour
{
    public FruitColor Color { get; private set; }
    public bool IsSliced { get; private set; }

    private float _lifetime;
    private float _timer;

    public void Initialize(FruitColor color, float lifetime)
    {
        Color = color;
        _lifetime = lifetime;
        _timer = 0f;
        IsSliced = false;
    }

    private void Update()
    {
        if (IsSliced) return;

        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
        {
            GameEvents.FruitMissed();
            Destroy(gameObject);
        }
    }

    /// <summary>Call when a blade hits this fruit. Returns false if already sliced this frame (prevents double-hits).</summary>
    public bool TrySlice()
    {
        if (IsSliced) return false;
        IsSliced = true;
        return true;
    }
}
