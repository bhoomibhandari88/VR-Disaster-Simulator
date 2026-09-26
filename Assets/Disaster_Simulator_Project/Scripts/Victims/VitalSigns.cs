using System;
using UnityEngine;

namespace DisasterSimulator
{
    /// <summary>
    /// Represents the physiological vital signs and clinical presentation of a victim
    /// evaluated under the adult START (Simple Triage and Rapid Treatment) protocol.
    /// </summary>
    [Serializable]
    public class VitalSigns
    {
        [Header("1. Ambulatory Status (Minor / Walking Wounded)")]
        [Tooltip("Can the victim stand and walk upon instruction? If true, classified as Minimal (Green).")]
        [SerializeField] private bool canWalk;

        [Header("2. Respiration (Airway & Breathing)")]
        [Tooltip("Is the victim breathing spontaneously upon initial arrival?")]
        [SerializeField] private bool isBreathing = true;

        [Tooltip("Respiration rate in breaths per minute. Under START: > 30 is Immediate; <= 30 continues to perfusion assessment.")]
        [Range(0, 60)]
        [SerializeField] private int respirationRate = 18;

        [Tooltip("If the victim is not breathing initially, does manual airway repositioning (head-tilt/chin-lift or jaw-thrust) restore breathing?")]
        [SerializeField] private bool airwayRepositionRestoresBreathing;

        [Header("3. Perfusion (Circulation)")]
        [Tooltip("Is a radial pulse palpable at the wrist? This is the PRIMARY deterministic perfusion criterion in this simulation.")]
        [SerializeField] private bool hasRadialPulse = true;

        [Tooltip("Capillary refill time in seconds. Displayed for clinical realism (normal <= 2.0s). Radial pulse is primary to avoid contradictory logic.")]
        [Range(0.5f, 6.0f)]
        [SerializeField] private float capillaryRefillSeconds = 1.5f;

        [Header("4. Mental Status")]
        [Tooltip("Can the victim follow simple verbal commands (e.g., 'Squeeze my fingers', 'Open your eyes')?")]
        [SerializeField] private bool canFollowCommands = true;

        [Header("Supplemental Clinical Data (UI / Inspection Display)")]
        [Tooltip("Heart rate in beats per minute (displayed on VR vitals monitor).")]
        [Range(0, 220)]
        [SerializeField] private int heartRateBpm = 75;

        [Tooltip("Observable clinical findings displayed when inspecting the victim (e.g., severe bleeding, burn depth, trauma).")]
        [TextArea(2, 4)]
        [SerializeField] private string visualObservations = "No obvious external life threats visible.";

        #region Properties

        /// <summary>
        /// True if victim is ambulatory (can walk).
        /// </summary>
        public bool CanWalk
        {
            get => canWalk;
            set => canWalk = value;
        }

        /// <summary>
        /// True if victim is breathing spontaneously without manual airway repositioning.
        /// </summary>
        public bool IsBreathing
        {
            get => isBreathing;
            set => isBreathing = value;
        }

        /// <summary>
        /// Breaths per minute. START rule: > 30 bpm triggers Immediate (Red).
        /// </summary>
        public int RespirationRate
        {
            get => respirationRate;
            set => respirationRate = Mathf.Max(0, value);
        }

        /// <summary>
        /// True if manual airway repositioning successfully restores spontaneous breathing in an apneic victim.
        /// </summary>
        public bool AirwayRepositionRestoresBreathing
        {
            get => airwayRepositionRestoresBreathing;
            set => airwayRepositionRestoresBreathing = value;
        }

        /// <summary>
        /// Primary perfusion check: true if radial pulse is palpable.
        /// Absent radial pulse triggers Immediate (Red).
        /// </summary>
        public bool HasRadialPulse
        {
            get => hasRadialPulse;
            set => hasRadialPulse = value;
        }

        /// <summary>
        /// Capillary refill time in seconds. Displayed for clinical realism and VR examination.
        /// In standard START, > 2 seconds indicates poor perfusion.
        /// In this simulation, HasRadialPulse is the primary determinant to ensure clear, non-contradictory evaluation.
        /// </summary>
        public float CapillaryRefillSeconds
        {
            get => capillaryRefillSeconds;
            set => capillaryRefillSeconds = Mathf.Max(0f, value);
        }

        /// <summary>
        /// True if victim can understand and obey simple instructions.
        /// Inability triggers Immediate (Red).
        /// </summary>
        public bool CanFollowCommands
        {
            get => canFollowCommands;
            set => canFollowCommands = value;
        }

        /// <summary>
        /// Heart rate in beats per minute for UI monitors and pulse audio.
        /// </summary>
        public int HeartRateBpm
        {
            get => heartRateBpm;
            set => heartRateBpm = Mathf.Max(0, value);
        }

        /// <summary>
        /// Descriptive clinical observations shown during visual inspection.
        /// </summary>
        public string VisualObservations
        {
            get => visualObservations;
            set => visualObservations = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor with standard healthy baseline values.
        /// </summary>
        public VitalSigns() { }

        /// <summary>
        /// Constructor for configuring specific triage scenario parameters.
        /// </summary>
        public VitalSigns(
            bool canWalk,
            bool isBreathing,
            int respirationRate,
            bool airwayRepositionRestoresBreathing,
            bool hasRadialPulse,
            float capillaryRefillSeconds,
            bool canFollowCommands,
            int heartRateBpm = 75,
            string visualObservations = "")
        {
            this.canWalk = canWalk;
            this.isBreathing = isBreathing;
            this.respirationRate = Mathf.Max(0, respirationRate);
            this.airwayRepositionRestoresBreathing = airwayRepositionRestoresBreathing;
            this.hasRadialPulse = hasRadialPulse;
            this.capillaryRefillSeconds = Mathf.Max(0f, capillaryRefillSeconds);
            this.canFollowCommands = canFollowCommands;
            this.heartRateBpm = Mathf.Max(0, heartRateBpm);
            this.visualObservations = visualObservations;
        }

        #endregion
    }
}
