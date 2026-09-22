using System.Collections;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Timing")]
    [SerializeField] private float baseSpawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float difficultyRampStartTime = 30f;
    [SerializeField] private float difficultyRampRate = 0.02f;
    [SerializeField] private float multiSpawnStartTime = 30f;
    [SerializeField][Range(0f, 1f)] private float multiSpawnChance = 0.35f;

    [Header("Arc")]
    [SerializeField] private float spawnDistance = 1.5f;
    [SerializeField] private float spawnHeightBelow = 1.0f;
    [SerializeField] private float armHeight = 0.5f;
    [SerializeField] private float horizontalDrift = 0.6f;
    [SerializeField] private float lifetime = 5f;

    [Header("Fruit Appearance")]
    [SerializeField] private float fruitScale = 0.15f;

    private float _elapsedSinceStart;
    private Coroutine _spawnRoutine;

    private void OnEnable()
    {
        GameEvents.OnGameStart += HandleGameStart;
        GameEvents.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStart -= HandleGameStart;
        GameEvents.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void HandleGameStart()
    {
        _elapsedSinceStart = 0f;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void HandleGameOver()
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float interval = GetCurrentInterval();
            yield return new WaitForSeconds(interval);

            SpawnFruit();

            if (_elapsedSinceStart >= multiSpawnStartTime && Random.value < multiSpawnChance)
            {
                SpawnFruit();
            }

            _elapsedSinceStart += interval;
        }
    }

    private float GetCurrentInterval()
    {
        float rampTime = Mathf.Max(0f, _elapsedSinceStart - difficultyRampStartTime);
        float interval = baseSpawnInterval - rampTime * difficultyRampRate;
        return Mathf.Max(minSpawnInterval, interval);
    }

    private void SpawnFruit()
    {
        Transform reference = player != null ? player : Camera.main.transform;
        Vector3 playerPos = reference.position;
        Vector3 forward = reference.forward;
        forward.y = 0f;
        forward.Normalize();
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;

        float xDrift = Random.Range(-horizontalDrift, horizontalDrift);
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 spawnPos = playerPos + forward * spawnDistance + right * xDrift;
        spawnPos.y = playerPos.y - spawnHeightBelow;

        GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fruit.name = "Fruit";
        fruit.transform.position = spawnPos;
        fruit.transform.localScale = Vector3.one * fruitScale;

        // Play spatial spawn chime
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpawnChime(spawnPos);
        }

        FruitColor color = (FruitColor)Random.Range(0, 5);
        var renderer = fruit.GetComponent<Renderer>();
        renderer.material.color = ColorForFruit(color);

        var col = fruit.GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.75f;

        var rb = fruit.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var fruitScript = fruit.AddComponent<Fruit>();
        fruitScript.Initialize(color, lifetime);

        StartCoroutine(ArcMotion(fruit.transform, armHeight));
    }

    private IEnumerator ArcMotion(Transform t, float apexHeight)
    {
        const float gravity = 9.81f;
        float initialUpSpeed = Mathf.Sqrt(2f * gravity * Mathf.Max(0.05f, apexHeight));
        Vector3 velocity = Vector3.up * initialUpSpeed;
        velocity += new Vector3(Random.Range(-0.15f, 0.15f), 0f, Random.Range(-0.1f, 0.1f));

        while (t != null)
        {
            velocity += Vector3.down * gravity * Time.deltaTime;
            t.position += velocity * Time.deltaTime;
            yield return null;
        }
    }

    private Color ColorForFruit(FruitColor c)
    {
        switch (c)
        {
            case FruitColor.Red: return Color.red;
            case FruitColor.Green: return Color.green;
            case FruitColor.Blue: return Color.blue;
            case FruitColor.Yellow: return Color.yellow;
            case FruitColor.Purple: return new Color(0.6f, 0.2f, 0.85f);
            default: return Color.white;
        }
    }
}