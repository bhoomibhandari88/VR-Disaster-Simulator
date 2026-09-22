using UnityEngine;

public enum Handedness { Left, Right }

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Hand Anchors (auto-found from OVRCameraRig if left empty)")]
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;

    public Vector3 LeftVelocity { get; private set; }
    public Vector3 RightVelocity { get; private set; }

    private Vector3 _prevLeftPos;
    private Vector3 _prevRightPos;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (leftHandAnchor == null || rightHandAnchor == null)
        {
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
            {
                leftHandAnchor = rig.leftHandAnchor;
                rightHandAnchor = rig.rightHandAnchor;
            }
        }

        if (leftHandAnchor != null) _prevLeftPos = leftHandAnchor.position;
        if (rightHandAnchor != null) _prevRightPos = rightHandAnchor.position;
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f) return;

        if (leftHandAnchor != null)
        {
            LeftVelocity = (leftHandAnchor.position - _prevLeftPos) / Time.deltaTime;
            _prevLeftPos = leftHandAnchor.position;
        }

        if (rightHandAnchor != null)
        {
            RightVelocity = (rightHandAnchor.position - _prevRightPos) / Time.deltaTime;
            _prevRightPos = rightHandAnchor.position;
        }
    }

    /// <summary>True on the frame either A (right controller) or X (left controller) is pressed.</summary>
    public bool ButtonPressedThisFrame()
    {
        return OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three);
    }

    public Vector3 GetVelocity(Handedness hand)
    {
        return hand == Handedness.Left ? LeftVelocity : RightVelocity;
    }
}
