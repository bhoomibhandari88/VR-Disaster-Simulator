using RescueSim.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RescueSim.UI
{
    /// <summary>Main menu: mission card, start, settings and quit.</summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] MissionDefinition mission;
        [SerializeField] string missionSceneName = "FactoryRescue";
        [SerializeField] Transform head;
        [SerializeField] SceneTransitions transitions;
        [SerializeField] VRPanel mainPanel;
        [SerializeField] SettingsPanel settingsPanel;
        [SerializeField] Button startButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;
        [SerializeField] TMP_Text missionTitle;
        [SerializeField] TMP_Text missionTagline;

        void OnEnable()
        {
            startButton?.onClick.AddListener(StartMission);
            settingsButton?.onClick.AddListener(OpenSettings);
            quitButton?.onClick.AddListener(Quit);
            settingsPanel?.BackButton?.onClick.AddListener(CloseSettings);
        }

        void OnDisable()
        {
            startButton?.onClick.RemoveListener(StartMission);
            settingsButton?.onClick.RemoveListener(OpenSettings);
            quitButton?.onClick.RemoveListener(Quit);
            settingsPanel?.BackButton?.onClick.RemoveListener(CloseSettings);
        }

        void Start()
        {
            if (mission != null)
            {
                if (missionTitle != null) missionTitle.text = mission.title;
                if (missionTagline != null) missionTagline.text = mission.tagline;
            }
            settingsPanel?.Hide();
            mainPanel?.ShowInFrontOf(head);
        }

        void StartMission() => transitions?.Load(missionSceneName);
        void OpenSettings() { mainPanel.Hide(); settingsPanel.ShowInFrontOf(head); }
        void CloseSettings() { settingsPanel.Hide(); mainPanel.ShowInFrontOf(head); }
        void Quit() => transitions?.Quit();
    }
}
