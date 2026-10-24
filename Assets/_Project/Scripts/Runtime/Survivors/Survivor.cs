using System;
using RescueSim.Audio;
using RescueSim.Core;
using RescueSim.Player;
using RescueSim.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace RescueSim.Survivors
{
    public enum SurvivorState { Unfound, Trapped, Following, Rescued, Lost }

    /// <summary>
    /// A person to rescue. Flow: Unfound -> (player approaches and looks) Trapped -> (hold select to check on them)
    /// Following -> (reaches the exit zone) Rescued. Survivors lose health in hazard zones and can be Lost.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class Survivor : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] string survivorName = "Survivor";

        [Header("References")]
        [SerializeField] PlayerVitals vitals;
        [SerializeField] Transform followTarget;
        [SerializeField] ExitZone exitZone;
        [SerializeField] Transform visual;
        [SerializeField] Renderer beaconRenderer;
        [SerializeField] Image progressRing;
        [SerializeField] TMP_Text statusLabel;
        [SerializeField] SpatialSound distressSound;
        [SerializeField] SpatialSound statusSound;
        [SerializeField] MissionController mission;

        [Header("Behaviour")]
        [SerializeField, Min(1f)] float findDistance = 7f;
        [SerializeField, Range(10f, 120f)] float findViewAngle = 70f;
        [SerializeField, Min(0.5f)] float assessSeconds = 2.5f;
        [SerializeField, Min(0.2f)] float walkSpeed = 1.5f;
        [SerializeField, Min(0.5f)] float followDistance = 1.8f;
        [Tooltip("Objective that must be complete before this survivor can move (e.g. the passage is cleared).")]
        [SerializeField] string requiresObjective = "";
        [Tooltip("Health lost per second per unit of hazard exposure rate.")]
        [SerializeField, Min(0f)] float vulnerability = 0.6f;

        static readonly Color UnfoundColor = new Color(1f, 0.55f, 0.1f, 0.6f);
        static readonly Color TrappedColor = new Color(1f, 0.9f, 0.2f, 0.7f);
        static readonly Color FollowColor = new Color(0.2f, 0.7f, 1f, 0.6f);
        static readonly Color SafeColor = new Color(0.2f, 1f, 0.4f, 0.6f);
        static readonly Color LostColor = new Color(0.9f, 0.1f, 0.1f, 0.5f);

        XRSimpleInteractable _interactable;
        NavMeshAgent _agent;
        bool _holding;
        float _assess;
        float _distressTimer;
        bool _blockedWarned;
        MaterialPropertyBlock _mpb;
        Vector3 _homePosition;
        Quaternion _lyingPose;
        Quaternion _standingPose;
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorLegacyId = Shader.PropertyToID("_Color");

        public string Name => survivorName;
        public SurvivorState State { get; private set; } = SurvivorState.Unfound;
        public float Health { get; private set; } = 1f;
        public float AssessProgress => _assess;

        public event Action<Survivor> Found;
        public event Action<Survivor> Rescued;
        public event Action<Survivor> Lost;

        public void Configure(PlayerVitals v, Transform follow, ExitZone exit, MissionController m)
        {
            vitals = v; followTarget = follow; exitZone = exit; mission = m;
        }

        void Awake()
        {
            _interactable = GetComponent<XRSimpleInteractable>();
            _agent = GetComponent<NavMeshAgent>();
            _mpb = new MaterialPropertyBlock();
            if (progressRing != null && progressRing.sprite == null) progressRing.sprite = UIFactory.WhiteSprite;
            _homePosition = transform.position;
            if (visual != null)
            {
                _standingPose = Quaternion.identity;
                _lyingPose = Quaternion.Euler(-82f, 0f, 0f);
                visual.localRotation = _lyingPose;
            }
        }

        void OnEnable()
        {
            _interactable.selectEntered.AddListener(OnSelectEntered);
            _interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            _interactable.selectEntered.RemoveListener(OnSelectEntered);
            _interactable.selectExited.RemoveListener(OnSelectExited);
        }

        void Start()
        {
            if (_agent != null)
            {
                _agent.speed = walkSpeed;
                _agent.stoppingDistance = followDistance;
                _agent.updateRotation = false;
                _agent.enabled = false; // enabled only when following, avoids snapping to a distant navmesh
            }
            Refresh();
        }

        void OnSelectEntered(SelectEnterEventArgs _) { _holding = true; }
        void OnSelectExited(SelectExitEventArgs _) { _holding = false; }

        void Update()
        {
            switch (State)
            {
                case SurvivorState.Unfound: TickUnfound(); break;
                case SurvivorState.Trapped: TickTrapped(); break;
                case SurvivorState.Following: TickFollowing(); break;
            }
            if (State == SurvivorState.Unfound || State == SurvivorState.Trapped || State == SurvivorState.Following)
                TickHealth();
            TickVisual();
        }

        // ---- State ticks -----------------------------------------------------------------------

        void TickUnfound()
        {
            PulseDistress();
            if (followTarget == null) return;
            Vector3 to = transform.position - followTarget.position;
            if (to.sqrMagnitude > findDistance * findDistance) return;
            float angle = Vector3.Angle(followTarget.forward, to);
            if (angle > findViewAngle) return;
            SetState(SurvivorState.Trapped);
            Found?.Invoke(this);
        }

        void TickTrapped()
        {
            PulseDistress();
            bool canMove = CanMove();
            if (_holding)
            {
                if (!canMove)
                {
                    if (!_blockedWarned)
                    {
                        _blockedWarned = true;
                        if (statusLabel != null) statusLabel.text = "Path blocked. Clear the passage first.";
                    }
                    return;
                }
                _assess = Mathf.Min(1f, _assess + Time.deltaTime / assessSeconds);
                if (_assess >= 1f) BeginFollowing();
            }
            else
            {
                _assess = Mathf.Max(0f, _assess - Time.deltaTime * 0.5f);
                _blockedWarned = false;
            }
            Refresh();
        }

        void TickFollowing()
        {
            if (followTarget == null) return;
            Vector3 target = followTarget.position;
            target.y = transform.position.y;
            Vector3 toTarget = target - transform.position;
            float dist = toTarget.magnitude;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.SetDestination(followTarget.position);
                Vector3 v = _agent.desiredVelocity; v.y = 0f;
                if (v.sqrMagnitude > 0.01f) Face(v);
            }
            else if (dist > followDistance)
            {
                // Fallback when no NavMesh is available: steer directly and keep to the ground.
                Vector3 step = toTarget.normalized * (walkSpeed * Time.deltaTime);
                Vector3 next = transform.position + step;
                if (Physics.Raycast(next + Vector3.up * 1.5f, Vector3.down, out var hit, 3f, ~0, QueryTriggerInteraction.Ignore))
                    next.y = hit.point.y;
                transform.position = next;
                Face(toTarget);
            }

            if (exitZone != null && exitZone.Contains(transform.position))
                SetState(SurvivorState.Rescued);
        }

        void TickHealth()
        {
            if (vitals == null) return;
            float rate = vitals.SampleRate(transform.position);
            if (rate <= 0f) return;
            Health = Mathf.Max(0f, Health - rate * vulnerability * Time.deltaTime);
            if (Health <= 0f) SetState(SurvivorState.Lost);
        }

        // ---- Transitions -----------------------------------------------------------------------

        bool CanMove() => string.IsNullOrEmpty(requiresObjective) || mission == null || mission.Objectives == null || mission.Objectives.IsComplete(requiresObjective);

        void BeginFollowing()
        {
            if (_agent != null)
            {
                _agent.enabled = true;
                if (!_agent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out var hit, 3f, NavMesh.AllAreas))
                    _agent.Warp(hit.position);
            }
            SetState(SurvivorState.Following);
            if (statusSound != null) statusSound.PlayOneShot(SpatialSound.Cue.Confirm);
        }

        void SetState(SurvivorState s)
        {
            if (State == s) return;
            State = s;
            if (s == SurvivorState.Rescued || s == SurvivorState.Lost)
            {
                if (_agent != null && _agent.enabled) _agent.ResetPath();
                if (distressSound != null) distressSound.Stop();
            }
            Refresh();
            if (s == SurvivorState.Rescued) { statusSound?.PlayOneShot(SpatialSound.Cue.Success); Rescued?.Invoke(this); }
            else if (s == SurvivorState.Lost) { statusSound?.PlayOneShot(SpatialSound.Cue.Failure); Lost?.Invoke(this); }
        }

        // ---- Presentation ----------------------------------------------------------------------

        void PulseDistress()
        {
            if (distressSound == null) return;
            _distressTimer -= Time.deltaTime;
            if (_distressTimer > 0f) return;
            _distressTimer = UnityEngine.Random.Range(2.5f, 4f);
            distressSound.PlayOneShot(SpatialSound.Cue.DistressWhistle);
        }

        void Face(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
        }

        void TickVisual()
        {
            if (visual == null) return;
            Quaternion target = (State == SurvivorState.Following || State == SurvivorState.Rescued) ? _standingPose
                              : State == SurvivorState.Lost ? _lyingPose : _lyingPose;
            visual.localRotation = Quaternion.Slerp(visual.localRotation, target, 3f * Time.deltaTime);
        }

        void Refresh()
        {
            Color c = State switch
            {
                SurvivorState.Unfound => UnfoundColor,
                SurvivorState.Trapped => TrappedColor,
                SurvivorState.Following => FollowColor,
                SurvivorState.Rescued => SafeColor,
                _ => LostColor
            };
            if (beaconRenderer != null)
            {
                beaconRenderer.GetPropertyBlock(_mpb);
                _mpb.SetColor(ColorId, c);
                _mpb.SetColor(ColorLegacyId, c);
                beaconRenderer.SetPropertyBlock(_mpb);
                beaconRenderer.enabled = State != SurvivorState.Rescued || true;
            }
            if (progressRing != null)
            {
                progressRing.gameObject.SetActive(State == SurvivorState.Trapped);
                progressRing.fillAmount = _assess;
            }
            if (statusLabel != null)
            {
                statusLabel.text = State switch
                {
                    SurvivorState.Unfound => "",
                    SurvivorState.Trapped => _assess > 0f ? "Checking..." : "Hold GRIP to check on them",
                    SurvivorState.Following => "Follow me out",
                    SurvivorState.Rescued => "Safe",
                    _ => "Lost"
                };
            }
        }

        /// <summary>Test hook: put the survivor straight into a state without playing the interaction.</summary>
        public void ForceState(SurvivorState s)
        {
            if (s == SurvivorState.Trapped && State == SurvivorState.Unfound) { SetState(s); Found?.Invoke(this); return; }
            SetState(s);
        }
    }
}
