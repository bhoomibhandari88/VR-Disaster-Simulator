namespace DisasterSimulator
{
    /// <summary>
    /// Provides pure evaluation logic implementing the adult START 
    /// (Simple Triage and Rapid Treatment) mass-casualty disaster triage protocol.
    /// </summary>
    public static class TriageEvaluator
    {
        /// <summary>
        /// Evaluates a victim's triage classification given their vital signs and the responder's airway maneuver.
        /// </summary>
        /// <param name="vitals">The victim's vital signs data.</param>
        /// <param name="airwayWasRepositioned">
        /// Indicates whether the responder has manually repositioned the victim's airway (e.g. head-tilt/chin-lift).
        /// Under START protocol, an apneic victim must have their airway opened before they can be determined Expectant.
        /// </param>
        /// <returns>The calculated TriageCategory under START rules.</returns>
        public static TriageCategory EvaluateCategory(VitalSigns vitals, bool airwayWasRepositioned)
        {
            if (vitals == null)
            {
                return TriageCategory.None;
            }

            // Step 1: Ambulatory status check ("Can the patient walk?")
            // Any patient who can walk is directed to a designated collection point and tagged Minimal (Green).
            if (vitals.CanWalk)
            {
                return TriageCategory.Minimal;
            }

            // Step 2: Spontaneous Respiration & Airway status
            if (!vitals.IsBreathing)
            {
                if (airwayWasRepositioned)
                {
                    // If airway repositioning successfully restores breathing, tag Immediate (Red).
                    if (vitals.AirwayRepositionRestoresBreathing)
                    {
                        return TriageCategory.Immediate;
                    }

                    // Victim was not breathing initially AND still does not breathe after airway repositioning.
                    // Under START mass-casualty protocol, they are classified as Expectant (Black).
                    return TriageCategory.Expectant;
                }
                else
                {
                    // The responder has not yet repositioned the airway on an apneic patient.
                    // Under START, a victim cannot be declared Expectant without attempting an airway maneuver.
                    // An unmanaged airway is an urgent life threat -> Immediate (Red).
                    return TriageCategory.Immediate;
                }
            }

            // Step 2b: Respiration Rate check
            // Standard adult START rule:
            // - respirationRate > 30 bpm -> Immediate (Red)
            // - respirationRate <= 30 bpm -> continue to perfusion assessment
            if (vitals.RespirationRate > 30)
            {
                return TriageCategory.Immediate;
            }

            // Step 3: Perfusion / Circulation check
            // In this simulator, Radial Pulse is the primary deterministic criterion:
            // - Radial pulse ABSENT -> Immediate (Red) indicating hypoperfusion / shock.
            // - Radial pulse PRESENT -> continue to mental status assessment.
            // (Note: Capillary refill time is available in VitalSigns for realistic VR examination/display,
            // but radial pulse serves as the primary evaluation check to avoid contradictory states).
            if (!vitals.HasRadialPulse)
            {
                return TriageCategory.Immediate;
            }

            // Step 4: Mental Status check
            // Victim can follow simple commands -> Delayed (Yellow).
            // Victim cannot follow simple commands (unresponsive or altered) -> Immediate (Red).
            if (!vitals.CanFollowCommands)
            {
                return TriageCategory.Immediate;
            }

            // Victim passed all criteria: Non-ambulatory, breathing <= 30, radial pulse present, follows commands.
            return TriageCategory.Delayed;
        }

        /// <summary>
        /// Calculates the ground-truth ExpectedCategory for a victim, assuming the complete START
        /// protocol is correctly performed (including airway repositioning if the patient is apneic).
        /// </summary>
        /// <param name="vitals">The victim's vital signs data.</param>
        /// <returns>The gold-standard ExpectedCategory under START guidelines.</returns>
        public static TriageCategory GetExpectedCategory(VitalSigns vitals)
        {
            // In a fully executed protocol, an apneic victim is always provided an airway repositioning attempt.
            return EvaluateCategory(vitals, airwayWasRepositioned: true);
        }

        /// <summary>
        /// Compares the player's assigned category against the ground-truth expected category.
        /// </summary>
        /// <param name="assignedCategory">The category assigned by the player.</param>
        /// <param name="expectedCategory">The correct protocol category.</param>
        /// <returns>True if the player's assignment matches the expected result.</returns>
        public static bool IsAssessmentAccurate(TriageCategory assignedCategory, TriageCategory expectedCategory)
        {
            if (assignedCategory == TriageCategory.None)
            {
                return false;
            }

            return assignedCategory == expectedCategory;
        }
    }
}
