using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Slash Sounds — index order: Red, Green, Blue, Yellow, Purple")]
    [SerializeField] private AudioClip[] slashSoundsByColor = new AudioClip[5];
    [SerializeField] private AudioClip missSound;
    [SerializeField] private AudioClip spawnChimeSound;
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sources")]
    [SerializeField] private AudioSource spatialSfxPrefab; // Spatial Blend must be 1.0
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnFruitSliced += HandleSliced;
        GameEvents.OnFruitMissed += HandleMissed;
        GameEvents.OnGameStart += HandleGameStart;
    }

    private void OnDisable()
    {
        GameEvents.OnFruitSliced -= HandleSliced;
        GameEvents.OnFruitMissed -= HandleMissed;
        GameEvents.OnGameStart -= HandleGameStart;
    }

    public void PlaySpawnChime(Vector3 worldPos)
    {
        if (spawnChimeSound != null) PlaySpatialClip(spawnChimeSound, worldPos, 0.6f);
    }

    private void HandleSliced(FruitColor color, Vector3 point)
    {
        int idx = (int)color;
        if (slashSoundsByColor != null && idx < slashSoundsByColor.Length && slashSoundsByColor[idx] != null)
        {
            PlaySpatialClip(slashSoundsByColor[idx], point, 1.0f);
        }
    }

    private void HandleMissed()
    {
        if (missSound != null && Camera.main != null)
            PlaySpatialClip(missSound, Camera.main.transform.position, 0.5f);
    }

    private void PlaySpatialClip(AudioClip clip, Vector3 pos, float volume)
    {
        if (spatialSfxPrefab == null) return;
        AudioSource source = Instantiate(spatialSfxPrefab, pos, Quaternion.identity);
        source.clip = clip;
        source.spatialBlend = 1.0f;
        source.volume = volume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();
        Destroy(source.gameObject, clip.length + 0.1f);
    }

    private void HandleGameStart()
    {
        if (backgroundMusic != null && musicSource != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.4f;
            musicSource.Play();
        }
    }
}