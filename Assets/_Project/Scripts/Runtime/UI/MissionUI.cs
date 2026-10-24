using System.Text;
using RescueSim.Core;
using RescueSim.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RescueSim.UI
{
    /// <summary>Coordinates briefing, pause, settings and results panels with the mission state.</summary>
    public class MissionUI : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] MissionController mission;
        [SerializeField] Transform head;
        [SerializeField] SceneTransitions transitions;
        [SerializeField] string menuSceneName = "MainMenu";

        [Header("Panels")]
        [SerializeField] VRPanel briefingPanel;
        [SerializeField] VRPanel pausePanel;
        [SerializeField] SettingsPanel settingsPanel;
        [SerializeField] VRPanel resultsPanel;

        [Header("Widgets")]
        [SerializeField] TMP_Text briefingTitle;
        [SerializeField] TMP_Text briefingBody;
        [SerializeField] Button beginButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button pauseSettingsButton;
        [SerializeField] Button pauseRestartButton;
        [SerializeField] Button pauseMenuButton;
        [SerializeField] TMP_Text resultsTitle;
        [SerializeField] TMP_Text resultsBody;
        [SerializeField] Button replayButton;
        [SerializeField] Button resultsMenuButton;

        readonly StringBuilder _sb = new StringBuilder(512);
        VRPanel _settingsReturnTo;

        public VRPanel Briefing => briefingPanel;
        public VRPanel Pause => pausePanel;
        public VRPanel Results => resultsPanel;
        public SettingsPanel Settings => settingsPanel;

        void OnEnable()
        {
            if (mission == null) return;
            mission.StateChanged += OnStateChanged;
            beginButton?.onClick.AddListener(OnBegin);
            resumeButton?.onClick.AddListener(OnResume);
            pauseSettingsButton?.onClick.AddListener(OpenSettingsFromPause);
            pauseRestartButton?.onClick.AddListener(Restart);
            pauseMenuButton?.onClick.AddListener(GoToMenu);
            replayButton?.onClick.AddListener(Restart);
            resultsMenuButton?.onClick.AddListener(GoToMenu);
            settingsPanel?.BackButton?.onClick.AddListener(CloseSettings);
        }

        void OnDisable()
        {
            if (mission != null) mission.StateChanged -= OnStateChanged;
            beginButton?.onClick.RemoveListener(OnBegin);
            resumeButton?.onClick.RemoveListener(OnResume);
            pauseSettingsButton?.onClick.RemoveListener(OpenSettingsFromPause);
            pauseRestartButton?.onClick.RemoveListener(Restart);
            pauseMenuButton?.onClick.RemoveListener(GoToMenu);
            replayButton?.onClick.RemoveListener(Restart);
            resultsMenuButton?.onClick.RemoveListener(GoToMenu);
            settingsPanel?.BackButton?.onClick.RemoveListener(CloseSettings);
        }

        void Start()
        {
            if (mission == null) return;
            if (mission.Mission != null)
            {
                if (briefingTitle != null) briefingTitle.text = mission.Mission.title.ToUpperInvariant();
                if (briefingBody != null) briefingBody.text = mission.Mission.briefing;
            }
            HideAll();
            OnStateChanged(mission.State);
        }

        void HideAll()
        {
            briefingPanel?.Hide(); pausePanel?.Hide(); settingsPanel?.Hide(); resultsPanel?.Hide();
        }

        void OnStateChanged(MissionState state)
        {
            switch (state)
            {
                case MissionState.Briefing:
                    HideAll(); briefingPanel?.ShowInFrontOf(head); break;
                case MissionState.Active:
                    HideAll(); break;
                case MissionState.Paused:
                    HideAll(); pausePanel?.ShowInFrontOf(head); break;
                case MissionState.Completed:
                case MissionState.Failed:
                    HideAll(); FillResults(); resultsPanel?.ShowInFrontOf(head); break;
            }
        }

        void OnBegin() => mission.BeginMission();
        void OnResume() => mission.Resume();

        /// <summary>Toggles pause from the controller menu button.</summary>
        public void TogglePause()
        {
            if (mission.State == MissionState.Active) mission.Pause();
            else if (mission.State == MissionState.Paused) { settingsPanel?.Hide(); mission.Resume(); }
        }

        void OpenSettingsFromPause()
        {
            if (settingsPanel == null) return;
            _settingsReturnTo = pausePanel;
            pausePanel.Hide();
            settingsPanel.ShowInFrontOf(head);
        }

        void CloseSettings()
        {
            settingsPanel.Hide();
            _settingsReturnTo?.ShowInFrontOf(head);
        }

        void Restart() => transitions?.ReloadCurrent();
        void GoToMenu() => transitions?.Load(menuSceneName);

        void FillResults()
        {
            if (mission.LastResult == null) return;
            var r = mission.LastResult.Value;
            bool ok = mission.State == MissionState.Completed;
            if (resultsTitle != null)
            {
                resultsTitle.text = ok ? "MISSION COMPLETE" : "MISSION FAILED";
                resultsTitle.color = ok ? UIFactory.Good : UIFactory.Bad;
            }
            if (resultsBody == null) return;
            _sb.Clear();
            if (!ok && !string.IsNullOrEmpty(mission.FailureReason)) _sb.Append(mission.FailureReason).Append("\n\n");
            _sb.Append("Survivors evacuated   ").Append(mission.SurvivorsRescued).Append('/').Append(mission.SurvivorsTotal)
               .Append("   +").Append(r.survivorPoints).Append('\n');
            _sb.Append("Hazards neutralised   ").Append(mission.HazardsNeutralized).Append('/').Append(mission.HazardsTotal)
               .Append("   +").Append(r.hazardPoints).Append('\n');
            _sb.Append("Time bonus   +").Append(r.timeBonus).Append('\n');
            _sb.Append("Exposure penalty   -").Append(r.exposurePenalty).Append('\n');
            _sb.Append("Mistakes (").Append(mission.Mistakes).Append(")   -").Append(r.mistakePenalty).Append("\n\n");
            _sb.Append("<size=64><b>SCORE  ").Append(r.total).Append("   RANK  ").Append(r.rank).Append("</b></size>");
            resultsBody.text = _sb.ToString();
        }
    }
}
