# Final audit — what is real, what is tested, what is not

Date: 2026-10-24/25 · Unity 6000.3.21f1 · macOS (Apple Silicon M3) · Meta XR Simulator · branch `feature/factory-rescue-vr` (local commits only, **nothing pushed**).

Legend — **Implemented**: code/assets exist. **Auto-tested**: covered by Unity Test Runner (EditMode/PlayMode) with real results. **Simulator-tested**: exercised by hand in Meta XR Simulator. **Headset-verified**: run on a physical Quest — **none of this project has been headset-verified.**

## 1. Verified facts (with how)
| Claim | Evidence |
|---|---|
| Project compiles (macOS + Android targets), 0 console errors | Unity console after compile on both targets; EditMode/PlayMode assemblies compile |
| EditMode tests: **39 / 39 passed** | Unity Test Runner, EditMode tab (re-run after final macOS switch: 39/39) |
| PlayMode tests: **13 / 13 passed** (after fixing 6 test-harness failures and 1 real bug, below) | Unity Test Runner, PlayMode tab (re-run after final macOS switch: 13/13) |
| Android build **succeeded**: 0 errors, 356 warnings, 1 h 03 min, `FactoryRescueVR.apk` ≈ 231 MB (IL2CPP, ARM64, Vulkan, min SDK 29) | `Builds/android-build-report.txt` written by `AndroidBuild.BuildApk()` from `BuildReport` |
| Meta XR Simulator provides HMD + both controllers, tracked | `[XRProbe] HMD present=True tracked=True pos=(0,1.70,0) left=True right=True` |
| Controller buttons reach Unity | `[XRProbe] LeftHand/RightHand triggerPressed PRESSED/released` |
| VR UI ray hover + click works | `[XRProbe] Right Controller/Near-Far Interactor UI hit: Button_Begin Mission`, then trigger → briefing closed, mission started ("Mission started…" toast) |

## 2. Feature matrix
| Feature | Implemented | Auto-tested | Simulator-tested | Headset |
|---|---|---|---|---|
| OpenXR + Meta Quest config (Editor/Android) | ✔ | ✔ (Android settings test) | ✔ (runtime launches) | ✘ |
| XR rig (XRI Starter Assets), locomotion, comfort settings | ✔ | ✔ (single rig/camera/EventSystem) | ✔ (rig tracks; locomotion not exercised) | ✘ |
| Main menu scene (start/settings/quit) | ✔ | ✔ (loads clean, no missing scripts) | ✘ | ✘ |
| Briefing panel + Begin Mission | ✔ | ✔ | ✔ | ✘ |
| Hazard rendering: fires, gas cloud, ambient/emergency lighting | ✔ | — | ✔ (visual check; scene is dark) | ✘ |
| Power isolation (breaker) | ✔ | ✔ (logic: Cut idempotent, event) | ✘ | ✘ |
| Gas valve mitigation | ✔ | ✔ (via programmatic full run in real scene) | ✘ | ✘ |
| Extinguisher + fire intensity/regrowth + live-electric mistake | ✔ | ✔ (spray/regrow/extinguish logic) | ✘ | ✘ |
| Debris carry-out / passage clearing | ✔ | ✔ (PassageMonitor logic) | ✘ | ✘ |
| Survivor locate / follow / evacuate / lost | ✔ | ✔ (states via `ForceState`, real scene) | ✘ | ✘ |
| Mission progression, failure (time, overcome), pause/resume | ✔ | ✔ | ✘ (pause UI) | ✘ |
| Scoring + rank + results data | ✔ | ✔ (11 score tests + full run ≥ 800) | ✘ (results panel visual) | ✘ |
| Results panel with Replay / Main Menu | ✔ | partly (scene reload test) | ✘ | ✘ |
| Diegetic wrist HUD | ✔ | — | ✘ | ✘ |
| Settings (snap/smooth turn, speed, vignette, volume) | ✔ | ✔ (GameSettings) | ✘ | ✘ |
| Procedural spatial audio, VFX | ✔ | — | ✘ (audio not audited) | ✘ |

"Auto-tested" logic tests call the real runtime classes, but they do **not** prove the physical VR interactions (grabbing, gripping, spraying) feel or work correctly — those were not exercised in the simulator.

## 3. Bugs found and fixed during verification
1. `No MissionDefinition assigned` after scene rebuild (asset reference lost) → builder reloads the asset after scene operations.
2. Filled UI images ignored `fillAmount` (null sprite) → shared white sprite.
3. `PassageMonitor` threw `NullReferenceException` when created in code without a hide-list (found by PlayMode test) → null-guarded.
4. Builder created duplicate empty `Materials N` / `Prefabs N` folders → `EnsureDir` now imports an existing folder instead of recreating it; duplicates removed.
5. Pack materials rendered pink (Built-in shader in URP) → converted to URP Lit.
6. "UI dead" investigation: not a bug — the ray was passing over non-clickable panel area; diagnostics (`XRInputProbe`) kept for future troubleshooting.

## 4. Known issues / risks
- **Never run on a headset.** Frame rate, thermals, comfort and real controller ergonomics are unknown. The APK is installable but unverified.
- APK ≈ 231 MB and a 63-minute first build: the pack textures are large and the project already contains Unity AI Inference (Sentis) and Unity AI Assistant packages (not added by this work) which add ~1,300 shader variants/compile time. Removing unused packages is a user decision; texture compression/atlasing would reduce size.
- Scene is dark; reflection probes/lightmaps come from the pack. Emergency/fire lights give local light only. Needs visual tuning on a headset.
- Briefing "Begin Mission" is at the bottom of a tall panel; with a near-horizontal controller ray it can be hard to hit in the desktop simulator (stepping back or pitching resolves it). Consider moving it higher/lower the panel on a real device.
- Project Validation (Quest tab) shows 4 optional warnings (OpenXR PoseControl/StickControl hints, Composition Layers package, SSAO on the desktop PC quality tier). Android uses the Mobile URP asset (no SSAO; MSAA 4×, HDR off).
- Platform switches rewrite some pack `.meta` files and a few settings assets (Unity re-serialization, plus `UnityConnectSettings` diagnostics flag). These incidental changes were deliberately **not committed**.
- LFS-type binaries from the XRI sample (png/fbx/wav) and `TextMesh Pro/Sprites/EmojiOne.png` are untracked — the authoring VM has no Git LFS. Add them from a machine with Git LFS before pushing, otherwise the scene's rig/UI may reference missing sprites/models on a fresh clone.
- `Assets/Resources/PerformanceTest*.json` are generated transiently by the performance-testing package during builds; they are not committed.

## 5. Recommended next steps (needs you)
1. Install the APK on a Quest (developer mode, `adb install -r Builds/Android/FactoryRescueVR.apk`) and play through once; log fps with OVR Metrics.
2. Run the full loop in the simulator or headset: breaker → valve → extinguish → debris → survivors → exit → results → replay; adjust tuning (fire time 4 s, regrowth 3 %/s, exposure rates).
3. Add LFS binaries and review `git status`, then approve a push to `feature/factory-rescue-vr` (remote is `bhoomibhandari88/VR-Disaster-Simulator`; no push was made).
