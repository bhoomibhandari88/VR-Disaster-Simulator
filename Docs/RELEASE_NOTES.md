# Disaster Response VR — group-16-v1.0 (Quest APK) — DRAFT, not yet published

**Asset:** `DisasterResponseVR.apk`, 242,633,136 bytes (231.4 MiB), SHA-256 `eaa125abb997b220653f94c382ad78136708748555e57c69b1f8c13d94107c8c`. Debug-signed, IL2CPP ARM64, Vulkan, min SDK 29, Meta Quest OpenXR feature enabled, 5 scenes. Built 2026-10-25 with Unity 6000.3.21f1 (build duration 22 min). The APK is not committed to Git; it is attached to the release only.

**What it is:** a three-stage VR disaster-rescue game (Factory, Earthquake, Flood) with main menu, progression, scoring and stars, **plus** a stand-alone START mass-casualty triage exercise (**Triage Study** button on the main menu): ten casualties in a burning factory yard, six START checks per casualty, Green/Yellow/Red/Black tags, results panel, CSV telemetry (records, summary, confusion matrix). START is implemented from published descriptions and is a training aid, **not clinically validated**.

**Install (Quest 3/3S, developer mode):** `adb install -r DisasterResponseVR.apk`; launch from Unknown Sources.

**Verification (macOS editor, 2026-10-25):** EditMode 107/107 and PlayMode 29/29 pass (57 + 23 campaign tests unchanged, plus 50 EditMode cases and 6 PlayMode tests for the triage exercise). Playthroughs are driven by XR Interaction Toolkit events and the real UI buttons, not by a headset.

**APK check:** Android build reported Succeeded (1 non-blocking error, 354 warnings). The package contains the 5 scenes (`level0`-`level4`) and the Quest manifest entries (`com.oculus.supportedDevices`, `android.hardware.vr.headtracking`).

**Not verified:** the APK has **never been installed or run on a physical headset** (no device, no `adb` here), so comfort, frame rate, controller interaction and panel placement are untested. No human-subject data exists. The one build error is a `NullReferenceException` inside the Unity OpenXR package's Meta Quest validation rules (`MetaQuestFeature.CreateValidationRules`); the build still completes and the cause is unresolved.

**Publish only after the team agrees** (exact commands are in the final report; no force-push):

```bash
git push -u bhoomi feature/factory-rescue-vr
gh release create group-16-v1.0 Builds/Android/DisasterResponseVR.apk \
  --repo bhoomibhandari88/VR-Disaster-Simulator --target feature/factory-rescue-vr \
  --title "Disaster Response VR v1.0 (Quest APK)" --notes-file Docs/RELEASE_NOTES.md
```

Afterwards confirm the uploaded asset's SHA-256 matches the value above.
