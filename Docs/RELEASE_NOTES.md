# Disaster Response VR — group-16-v1.0 (Quest APK)

**Asset:** `DisasterResponseVR.apk` (242,528,206 bytes, SHA-256 `7775384dbee3a500e735f2079d1917684dbcad52b6a6dde542f360aac781efe1`). Debug-signed, IL2CPP ARM64, Vulkan, min SDK 29.

**What it is:** a three-stage VR disaster-rescue game (Factory, Earthquake, Flood) with main menu, progression, scoring and stars. It is **not** the START triage simulation described in the Group 16 dossier.

**Install (Quest 3/3S, developer mode):** `adb install -r DisasterResponseVR.apk`; launch from Unknown Sources.

**Verification:** EditMode 57/57 and PlayMode 23/23 pass in Unity 6000.3.21f1 (macOS editor); playthroughs are driven by XR Interaction Toolkit events, not a headset.

**Known limitations:** never installed or run on a physical headset; comfort and frame rate unverified; one non-blocking OpenXR package validation exception during the build; no human-subject data.
