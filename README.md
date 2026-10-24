# Factory Rescue VR — Industrial Fire & Collapse Rescue Simulator

A single-mission VR training-style game built in Unity 6 (6000.3.21f1, URP) for Meta Quest 3/3S, using the Tirgames factory asset pack.
You enter a dark factory hall after a transformer fire, a gas leak and a partial wall collapse. Isolate the hazards, clear the blocked
passage, find three trapped workers and lead them to the assembly point before time (8:00) or your exposure meter runs out.

> **Status honesty**: see [`Docs/AUDIT.md`](Docs/AUDIT.md) for exactly what is *implemented*, what was *tested in Meta XR Simulator*, what has
> *automated tests*, and what is **not verified on a physical headset** (nothing has been).
> This is a game, not a medical or safety-certified training tool. Exposure/health values are game abstractions.

## Requirements
- Unity **6000.3.21f1** (Unity 6.3 LTS) with **Android Build Support** (OpenJDK, SDK & NDK) for Quest builds.
- Packages (already in `Packages/manifest.json`): URP 17.3, XR Interaction Toolkit 3.6.1, OpenXR 1.18, Unity OpenXR: Meta 2.6.1, Input System 1.20, AI Navigation 2.0, TextMeshPro.
- **Git LFS** (the repo uses LFS for textures, models, audio). Run `git lfs install && git lfs pull` after cloning.
- Optional for desktop testing: **Meta XR Simulator** (macOS app) — no Meta XR SDK package is required.

## Quick start (editor + Meta XR Simulator)
1. Open the project in Unity 6000.3.21f1. Let packages resolve and scripts compile (console should show 0 errors).
2. (Re)generate scenes if needed: **Rescue Sim ▸ Build ▸ Everything**. This builds `MainMenu` and `FactoryRescue`, prefabs, materials, the NavMesh and Build Settings.
3. Install/enable Meta XR Simulator and turn on "XR Simulator runtime enabled" (toggle top-left of the simulator app).
4. Open `Assets/_Project/Scenes/FactoryRescue.unity` (or `MainMenu`) and press **Play**. The simulator window launches automatically.
5. Click once inside the simulator's headset view to give it keyboard focus.

### Meta XR Simulator keyboard controls (Keyboard and mouse profile, "same input for both sides")
| Action | Key |
|---|---|
| Move | W A S D (R/F up/down) |
| Look | Arrow keys |
| Tilt head | Q / E |
| Index trigger | T |
| Grip | U |
| A/X, B/Y | B / N |
| Thumbstick press | I |
| Pause (editor shortcut) | Esc |

Tips: aim the ray at a button by turning/pitching with the arrows (≈70°/s) or stepping back with S; clicking on a controller model in the simulator view selects that controller for manual posing, so click an empty area instead.

## Gameplay
Controls on Quest (Touch controllers): ray-point + **trigger** on menus; **grip** to grab / hold interactions; **trigger** while holding the extinguisher to spray; **left menu button** to pause; raise your **left wrist** to see the diegetic HUD (time, exposure, checklist).

Mission checklist (all data-driven from `Assets/_Project/Data/Mission_NightShiftFactoryFire.asset`):
1. **Cut main power** at the breaker on the east wall (select the lever). Spraying the electrical fire while power is live is a *mistake* (shock + penalty).
2. **Close the gas shut-off valve** — hold grip on the valve wheel until fully shut. The gas cloud thins and then stops.
3. **Extinguish the fires** — grab the extinguisher from the bench, squeeze the trigger, sweep the base of the flames. Fires regrow if abandoned.
4. **Clear the collapsed passage** — grab and carry debris blocks out of the marked gap in the partition wall.
5. **Locate** all three survivors (hold grip on them to assess) — two are behind the collapse.
6. **Evacuate** them: they follow you; keep them out of smoke and lead them into the green assembly zone at the entrance.

Scoring (0–1000): survivors 500 · hazards 300 · time bonus 200 (only on completion) · exposure penalty up to −150 · −25 per mistake. Ranks: S ≥ 900, A ≥ 750, B ≥ 550, C ≥ 350, D otherwise, INCOMPLETE if not finished. The results panel offers **Replay** and **Main Menu**.
Settings (main menu / pause): snap/smooth turn, turn angle/speed, move speed, comfort vignette, master volume (stored in PlayerPrefs).

## Project layout
```
Assets/_Project/
  Scripts/Runtime/{Core,Hazards,Interaction,Survivors,UI,Audio,Player}   # gameplay (assembly RescueSim.Runtime)
  Scripts/Editor/                                                       # scene/prefab builder, XR+Android setup, Android build
  Data/            MissionDefinition ScriptableObject
  Scenes/          MainMenu, FactoryRescue (+ baked NavMesh)
  Prefabs/ Materials/ Shaders/                                           # generated + authored
  Tests/{EditMode,PlayMode}                                              # NUnit/Unity Test Framework suites
Assets/TirgamesAssets/   original factory pack (materials upgraded to URP Lit)
Docs/                    TROUBLESHOOTING.md, CHANGES.md, AUDIT.md
```
Architecture: no singletons — the scene builder wires references through serialized fields. `MissionController` owns state/objectives/score;
`Hazard` subclasses (`FireHazard`, `GasLeakHazard`, `ElectricalHazard`) raise `Neutralized`; `PassageMonitor` polls debris; `Survivor` is a small state machine;
`ScoreCalculator`/`ObjectiveTracker` are pure C# (unit-tested). VFX and audio are procedural (no extra audio/texture assets were added).

## Tests
Unity Test Runner (Window ▸ General ▸ Test Runner): **EditMode** (score maths, objectives, vitals, settings, mission data, build settings, Android config) and
**PlayMode** (mission flow with synthetic objects, hazards, passage clearing, loading the real scenes, full programmatic run). Results are recorded in `Docs/AUDIT.md`.

## Building for Quest
See `Docs/TROUBLESHOOTING.md` and `Docs/AUDIT.md` (Android section). Menu: **Rescue Sim ▸ Configure XR and Android** (idempotent), switch platform to Android in Build Profiles, then **Rescue Sim ▸ Build ▸ Android APK (Quest)**. Output goes to `Builds/Android/` with a report in `Builds/android-build-report.txt`.
