using UnityEngine;

public class BladeController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Handedness hand;
    [SerializeField] private float slashSpeedThreshold = 1.0f;
    [SerializeField] private GameObject sliceParticlePrefab;
    [SerializeField] private Material dissolveMaterial;

    private Renderer bladeRenderer;
    private MaterialPropertyBlock mpb;
    private static readonly int SwingIntensityID = Shader.PropertyToID("_SwingIntensity");

    private void Awake()
    {
        bladeRenderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (InputManager.Instance != null && bladeRenderer != null)
        {
            Vector3 vel = InputManager.Instance.GetVelocity(hand);
            bladeRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(SwingIntensityID, vel.magnitude);
            bladeRenderer.SetPropertyBlock(mpb);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var fruit = other.GetComponent<Fruit>();
        if (fruit == null) return;

        Vector3 velocity = InputManager.Instance != null ? InputManager.Instance.GetVelocity(hand) : Vector3.zero;
        if (velocity.magnitude < slashSpeedThreshold) return;

        if (!fruit.TrySlice()) return;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 sliceNormal = Vector3.Cross(velocity, transform.forward).normalized;

        SpawnSliceParticles(hitPoint, fruit.Color);
        TriggerHaptic();

        GameEvents.FruitSliced(fruit.Color, hitPoint);

        MeshSlicer.Slice(fruit.gameObject, hitPoint, sliceNormal, dissolveMaterial);
    }

    private void SpawnSliceParticles(Vector3 point, FruitColor color)
    {
        if (sliceParticlePrefab == null) return;

        GameObject fx = Instantiate(sliceParticlePrefab, point, Quaternion.identity);
        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = ColorForFruit(color);
        }
        Destroy(fx, 2f);
    }

    private void TriggerHaptic()
    {
        OVRInput.Controller controller = hand == Handedness.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(1.0f, 0.8f, controller);
        CancelInvoke(nameof(StopHaptic));
        Invoke(nameof(StopHaptic), 0.08f);
    }

    private void StopHaptic()
    {
        OVRInput.Controller controller = hand == Handedness.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(0f, 0f, controller);
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