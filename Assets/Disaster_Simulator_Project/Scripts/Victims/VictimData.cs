using System;
using UnityEngine;

namespace DisasterSimulator
{
    /// <summary>
    /// Component attached to victim GameObjects in the scene.
    /// Manages victim profile data, vital signs, examination interaction states,
    /// and triage tagging status.
    /// </summary>
    [DisallowMultipleComponent]
    public class VictimData : MonoBehaviour
    {
        [Header("Victim Profile")]
        [Tooltip("Unique victim identifier (e.g. 'VIC-01').")]
        [SerializeField] private string victimId = "VIC-01";

        [Tooltip("Victim display name.")]
        [SerializeField] private string victimName = "Unidentified Civilian";

        [Tooltip("Clinical context / trauma description shown upon examination.")]
        [TextArea(2, 4)]
        [SerializeField] private string injuryDescription = "Apparent blunt force trauma. Non-ambulatory.";

        [Header("Physiological Vital Signs")]
        [Tooltip("The clinical vital signs configured for this victim scenario.")]
        [SerializeField] private VitalSigns vitalSigns = new VitalSigns();

        [Header("Triage Assessment State")]
        [Tooltip("The triage category assigned by the player/responder. Kept separate from ExpectedCategory.")]
        [SerializeField] private TriageCategory assignedCategory = TriageCategory.None;

        [Tooltip("The ground-truth category calculated automatically via the START triage algorithm.")]
        [SerializeField] private TriageCategory expectedCategory = TriageCategory.None;

        [Header("Examination Flags")]
        [Tooltip("Has the responder approached and performed an examination on this victim?")]
        [SerializeField] private bool isExamined;

        [Tooltip("Has the responder performed a manual airway repositioning maneuver on this victim?")]
        [SerializeField] private bool airwayWasRepositioned;

        [Tooltip("Has a triage tag been attached to this victim?")]
        [SerializeField] private bool isTagged;

        #region Events

        /// <summary>
        /// Invoked when the player assigns a triage category tag to this victim.
        /// Useful for UI updates, sound effects, and score tracking.
        /// </summary>
        public event Action<VictimData, TriageCategory> OnTriageTagAssigned;

        /// <summary>
        /// Invoked when the player inspects or examines this victim's vital signs.
        /// </summary>
        public event Action<VictimData> OnVictimExamined;

        /// <summary>
        /// Invoked when the player repositions this victim's airway.
        /// </summary>
        public event Action<VictimData> OnAirwayRepositioned;

        #endregion

        #region Properties

        public string VictimId => victimId;
        public string VictimName => victimName;
        public string InjuryDescription => injuryDescription;
        public VitalSigns Vitals => vitalSigns;

        /// <summary>
        /// The triage category tag assigned by the player.
        /// </summary>
        public TriageCategory AssignedCategory => assignedCategory;

        /// <summary>
        /// The correct protocol category evaluated from the victim's vital signs.
        /// </summary>
        public TriageCategory ExpectedCategory => expectedCategory;

        /// <summary>
        /// True if the victim has been examined by the player.
        /// </summary>
        public bool IsExamined => isExamined;

        /// <summary>
        /// True if manual airway repositioning has been performed.
        /// </summary>
        public bool AirwayWasRepositioned => airwayWasRepositioned;

        /// <summary>
        /// True if a triage tag has been applied to this victim.
        /// </summary>
        public bool IsTagged => isTagged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            UpdateExpectedCategory();
        }

        private void OnValidate()
        {
            // Keep the ExpectedCategory updated in the Inspector when tweaking vital signs
            UpdateExpectedCategory();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Re-evaluates and updates the ground-truth ExpectedCategory using the START protocol algorithm.
        /// </summary>
        public void UpdateExpectedCategory()
        {
            if (vitalSigns != null)
            {
                expectedCategory = TriageEvaluator.GetExpectedCategory(vitalSigns);
            }
        }

        /// <summary>
        /// Marks this victim as examined by the player and notifies listeners.
        /// </summary>
        public void MarkExamined()
        {
            if (!isExamined)
            {
                isExamined = true;
                OnVictimExamined?.Invoke(this);
            }
        }

        /// <summary>
        /// Simulates manual airway repositioning (e.g. head-tilt/chin-lift or jaw-thrust) by the player.
        /// </summary>
        public void RepositionAirway()
        {
            airwayWasRepositioned = true;
            OnAirwayRepositioned?.Invoke(this);
        }

        /// <summary>
        /// Assigns a triage category tag to this victim (the player's answer).
        /// </summary>
        /// <param name="category">The category tag chosen by the player.</param>
        public void AssignTriageTag(TriageCategory category)
        {
            assignedCategory = category;
            isTagged = (category != TriageCategory.None);

            OnTriageTagAssigned?.Invoke(this, category);
        }

        /// <summary>
        /// Checks whether the player's assigned tag matches the ground-truth protocol category.
        /// </summary>
        /// <returns>True if the assigned tag is correct; otherwise false.</returns>
        public bool IsTagCorrect()
        {
            return TriageEvaluator.IsAssessmentAccurate(assignedCategory, expectedCategory);
        }

        /// <summary>
        /// Evaluates the victim's current status taking into account whether the airway has been repositioned.
        /// </summary>
        /// <returns>Current evaluation result under START rules.</returns>
        public TriageCategory EvaluateCurrentStatus()
        {
            return TriageEvaluator.EvaluateCategory(vitalSigns, airwayWasRepositioned);
        }

        #endregion
    }
}
