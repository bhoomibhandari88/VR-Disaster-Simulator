# Manual test checklist (triage exercise)

Automated tests cover logic and scene wiring (see `TECHNICAL_IMPLEMENTATION_REPORT.md`). These steps need a person. Tick and date each one; leave a box empty if it was not done. **Nothing below has been done on a headset.**

## A. Unity Editor or Meta XR Simulator (about 10 minutes)
- [ ] Open `Assets/_Project/Scenes/MainMenu.unity`, press Play. The menu shows three stage cards and a **Triage Study** button (top right of the panel).
- [ ] Press **Triage Study**: the yard loads, ten labelled casualties C01-C10 are visible, start panel shows protocol line "perfusion check = capillary refill > 2 s".
- [ ] Press **Participant** and **Data type**: labels change (TEST, P01...; SoftwareTest, Pilot, HumanParticipant). Leave TEST + SoftwareTest for rehearsal.
- [ ] **Begin Exercise**: assessment panel appears, smoke and fire light are visible.
- [ ] Select a casualty (point + trigger, or Tab). The panel says "Assessing Cxx" and shows **no findings**. Press each of the six check buttons: each shows only its own result.
- [ ] Tag the casualty. Feedback text shows correct/incorrect (if the toggle is on). The tag colour appears above the casualty. Re-selecting it is refused.
- [ ] Walk or teleport away: selecting a casualty more than 8 m away shows "Move closer".
- [ ] After about 60 s the smoke/light visibly increase and the "Hazard level" counter rises (max 3).
- [ ] Tag all ten: results panel shows accuracy, under-triage, mean time, and a saved path. Open the three CSV files at that path and check the header matches `Docs/templates/participant_telemetry_template.csv`.
- [ ] **Restart** after tagging only a few: a partial summary file appears with `session_complete=false`; panel returns to start; tags reset; a new session id is used.
- [ ] **Main Menu** returns to the menu; the three campaign stages still start.
- [ ] Console has no red errors during the above.

## B. Quest headset (PENDING, needs hardware)
- [ ] Verify the file first: `shasum -a 256 DisasterResponseVR.apk` must equal `a533c6d08427da2d5268c421d9d6cb5ea64f807248abdce518c7476798ecd6f4` (see `BUILD_REPORT.md`; the older `eaa125ab…` APK predates the fix).
- [ ] `adb install -r DisasterResponseVR.apk`, launch from Unknown Sources.
- [ ] Selecting a casualty shows no findings until a check button is pressed (the fix in `f054410`).
- [ ] Start panel shows the protocol line (capillary refill > 2 s).
- [ ] Menu buttons and casualties can be selected with the controller ray; panels are readable and well placed at standing and seated height.
- [ ] Teleport and snap turn work; no nausea or discomfort reported after 10 minutes.
- [ ] Frame rate acceptable (no visible stutter with smoke and fire lights).
- [ ] CSV files are found under `Android/data/<package id>/files/TriageTelemetry/` and copy off with `adb pull`.
