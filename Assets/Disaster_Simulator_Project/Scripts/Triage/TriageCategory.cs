namespace DisasterSimulator
{
    /// <summary>
    /// Represents the standard classification categories used in the adult START
    /// (Simple Triage and Rapid Treatment) mass-casualty triage algorithm.
    /// </summary>
    public enum TriageCategory
    {
        /// <summary>
        /// Default unassigned state. The victim has not yet been assessed or tagged.
        /// </summary>
        None = 0,

        /// <summary>
        /// GREEN (Minor / Walking Wounded):
        /// Ambulatory victims with minor injuries who can follow commands and walk.
        /// Care and transport can be safely delayed until more acute casualties are handled.
        /// </summary>
        Minimal = 1,

        /// <summary>
        /// YELLOW (Delayed):
        /// Serious, non-ambulatory injuries requiring medical care, but vitals are stable:
        /// Respiration rate is 30 or less, radial pulse is present, and simple commands can be followed.
        /// Immediate life threat is not present at the time of evaluation.
        /// </summary>
        Delayed = 2,

        /// <summary>
        /// RED (Immediate):
        /// Critical, life-threatening injuries requiring immediate intervention.
        /// Triggered by any of:
        /// - Resumes breathing only after airway repositioning
        /// - Respiration rate greater than 30 breaths/min
        /// - Absent radial pulse (poor perfusion / decompensating shock)
        /// - Inability to follow simple commands (altered mental status)
        /// </summary>
        Immediate = 3,

        /// <summary>
        /// BLACK (Expectant / Deceased):
        /// Victim is not breathing on initial assessment AND still does not breathe
        /// after manual airway repositioning.
        /// In mass-casualty scenarios with limited resources, resuscitation is not initiated.
        /// </summary>
        Expectant = 4
    }
}
