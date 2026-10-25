# IVRAR Group 16 — Submission audit (verified facts, pending items, blockers)

Date: 2026-10-25 · Project: Disaster Response VR (Unity 6000.3.21f1) · Local branch `feature/factory-rescue-vr`

## 1. Verdict in one paragraph
The code in this repository is a **three-stage rescue game** (Factory, Earthquake, Flood). Group 16's dossier is for a **START mass-casualty triage simulation** with a specific research title, scripts, telemetry and a human study. **The two do not match.** The repository is real, tested and buildable, but it does not implement the dossier's research question. Nothing below pretends otherwise.

## 2. Verified facts
| Fact | Evidence |
|---|---|
| Local commit `1178a33bb36bc8cc5cce61287e891ec26c5dfd89` on `feature/factory-rescue-vr` | `git log` |
| 249 files tracked by Git LFS, all objects present locally (`git lfs fsck` OK) | `git lfs ls-files`, `git lfs fsck` |
| 29 previously untracked binaries (XRI Starter Assets, TMP EmojiOne) committed as LFS pointers, not raw | pointer check on staged blobs |
| No APK, Library, Temp, Builds in the commit | staged-file check; `.gitignore` covers them |
| EditMode 57/57, PlayMode 23/23 pass (rerun after the last code change, macOS target) | Unity Test Runner, `Builds/TestSummary.txt` |
| Android build Succeeded, 0 blocking errors; 1 non-blocking OpenXR package validation `NullReferenceException` per build; 354 warnings on the full build | `Builds/android-build-report.txt` |
| APK: `Builds/Android/DisasterResponseVR.apk`, 242,528,206 bytes (231.3 MiB), SHA-256 `7775384dbee3a500e735f2079d1917684dbcad52b6a6dde542f360aac781efe1`, IL2CPP ARM64, Vulkan, min SDK 29, Quest OpenXR features enabled, 4 scenes | build report + APK manifest inspection |
| C# in `Assets/_Project`: 68 files, 7,951 lines | `find … | wc -l` |
| Real telemetry CSV files: **0**. No participant data exists | no `telemetry/` folder, no CSV |
| **Not** installed or run on any device (no `adb`, no headset) | — |

## 3. Does the project match the dossier? — NO
The dossier (Group 16) requires:
- Research title: AI-driven VR mass-casualty triage simulation, START categorisation accuracy and assessment latency.
- `STARTTriageSimulationManager.cs`, `TriageTelemetryLogger.cs`, a `CasualtyProfile` component, 10 casualties, a 4-colour triage belt, telemetry CSV, accuracy / under-triage / latency / confusion matrix, N ≥ 10 human study, Unity 2022.3 LTS.

What exists:
| Where | Triage content |
|---|---|
| Our branch `feature/factory-rescue-vr` (committed) | **None.** Survivor assess/follow/evacuate only. |
| `bhoomibhandari88/VR-Disaster-Simulator`, branch `feature/bhoomi-work` (commit `ea174c8`, not merged into ours) | START foundation: `TriageCategory`, `TriageEvaluator` (walk, breathing, RR > 30, radial pulse, commands), `VitalSigns`, `VictimData`, scene `Factory_Triage`. **No** manager, **no** telemetry logger, no CSV, no UI. |

Missing for the dossier (nothing was invented to cover these):
1. `STARTTriageSimulationManager.cs` and `TriageTelemetryLogger.cs`.
2. Merge of Bhoomi's triage code with the campaign branch (separate namespace `DisasterSimulator` and folder `Assets/Disaster_Simulator_Project`).
3. Casualty scene/belt/tag interaction, telemetry recording, confusion-matrix output.
4. The human study (consent forms, ≥ 10 participants, SUS, NASA-TLX), then plots.
5. Alignment of the written protocol: the dossier's state machine uses "Cap refill > 2 s"; Bhoomi's code uses radial pulse (1996 START revision) and shows cap refill only for display.
6. Toolchain statement: dossier says Unity 2022.3 LTS; the project is Unity 6000.3.21f1.

The approved research title was **not** changed.

## 4. Dossier fields
Draft files: `Docs/IVRAR_dossier_DRAFT/IVRAR_Group_16_Technical_Dossier.docx` and `.pdf` (edited at XML level; all 170 drop-downs and the 4-page layout preserved; the PDF was produced with LibreOffice for preview only). **Not for submission as-is.**

Filled (supported by sources I read):
- 1.1 claim 1 (START RPM): source CHEMM (US HHS) `chemm.hhs.gov/startadult.htm`; Benson, Koenig, Schultz 1996 (DOI 10.1017/S1049023X0004276X, title confirmed on the Cambridge page, volume/pages **not** confirmed). Source type: Official report/documentation. Verdict: **Correct** (1996 revision uses radial pulse, not cap refill > 2 s).
- 1.1 claim 2: Kahn et al., Ann Emerg Med 2009;54(3):424-30, DOI 10.1016/j.annemergmed.2008.12.035 — 148 patients, 3 under-triaged, 79 over-triaged; no mortality or per-patient time reported. Time ≈ 30 s (max 1 min) comes from the unattributed IAVOAD "START Triage Procedures" document (secondary). Verdict: **Correct** (reword; the "preventable mortality" part is not supported by that paper). Source type recorded as Peer-reviewed paper (DOI).
- 1.2: three-line baseline statement using the Kahn numbers; trainee baseline explicitly "not yet measured".
- The verdicts are mine from the pages I could fetch; the team should open the sources and confirm before submitting. The PubMed and Cambridge pages blocked parts of my access.

Left pending (needs real input, not fillable honestly):
| Section | Pending |
|---|---|
| 2.1 Authorship | The two scripts do not exist; authorship needs team confirmation |
| 2.2 Code snippet, 2.3 equations | Needs the real triage manager/logger. Do **not** use Bhoomi's `TriageEvaluator` as if it were the manager. |
| 3 Final commit hash, release link, LFS and clean-device ticks, 3.1 counts, 3.2 usernames/percentages | Depends on where the team finally pushes; **all 7 commits here are authored "Claude"**, so per-member history cannot be derived |
| 4 All results, plots, provenance declaration | No real data. Do not use `sample_experiment_data.csv`/`*_benchmark.csv` |
| 5 LinkedIn URL, length, AI-audit log, peer review scores | Video not made; AI-error cases need real, evidenced examples; reviews need watching the videos |
| 6 Individual learning record, sign-off, signatures, consent | Each member must complete personally |

Peer-review groups (from the guide and prefilled in the dossier): Group 17 and Group 07.

## 5. Checklists
**Section 4 protocol (when the triage build exists):** adult volunteers, N ≥ 10 (20 recommended), written consent forms kept, anonymised IDs P01…; same build as the release; 10 casualties with known ground-truth START categories; log per casualty: participant, casualty id, expected tag, assigned tag, time from approach to tag (s); also SUS (10 items, 1–5; score = 2.5 × [Σ(odd−1) + Σ(5−even)]), NASA-TLX (6 × 0–100), SSQ for headset users. Report N, mean, SD, median, one box-plot, 4×4 confusion matrix, accuracy and under-triage = (true Red tagged Yellow/Green) / true Red. Save as `telemetry/P01.csv…`; rename or delete any simulated sample file.

**Video outline (3:00–4:00, every member speaks):** 0:00 problem + one verified number (Kahn 2009) · 0:20 running build (screen/headset capture) · 1:30 state machine + one equation · 2:15 real results (N, SUS, accuracy, plot) · 3:00 reflections incl. one AI error. Upload natively to LinkedIn; no participant faces or SAP IDs.

**Section 6 / sign-off answers each member must give:** hours, role match 1–5, prior experience, prior user testing, skill learned from another programme, share of writing drafted with AI, number of wrong AI facts caught, one-line learned/change, then sign and choose consent themselves.

## 6. Push and release — not done (needs your credentials)
The VM has no GitHub credentials and Terminal on the Mac cannot be typed into by me. Run on your Mac in the project folder:

```bash
git lfs install
git remote add bhoomi https://github.com/bhoomibhandari88/VR-Disaster-Simulator.git   # skip if it already exists
git remote -v                                   # origin currently points to queenjj11/..., NOT bhoomibhandari88
git status && git log --oneline -3              # expect 1178a33 (+ audit commit)
git lfs ls-files | wc -l                        # expect 249
git push -u bhoomi feature/factory-rescue-vr    # new branch, no force; LFS objects upload via the pre-push hook
gh release create group-16-v1.0 Builds/Android/DisasterResponseVR.apk \
  --repo bhoomibhandari88/VR-Disaster-Simulator --target feature/factory-rescue-vr \
  --title "Disaster Response VR v1.0 (Quest APK)" --notes-file Docs/RELEASE_NOTES.md
```
Verify the SHA-256 of the uploaded APK matches section 2. After pushing, run `git clone` into a new folder and `git lfs pull` to confirm a clean checkout.

## 7. Risks for submission and viva
1. **Instructor's rule:** the brief says to push to the course monorepo `sunny-nanade/NMIMS-MPSTME-IVRAR-2026` (`Group_16_Bhoomi_Kritivya_Tanvi_Jia/`), with each member committing from their own account and the release under that folder. You chose your own repo; confirm this is acceptable with Dr. Nanade, or mirror it later.
2. **Topic mismatch** (section 3) is the biggest risk: the viva asks about the code you claim. The dossier cannot truthfully claim the START scripts yet.
3. No headset, no human study, no telemetry: Section 4 and the video would score nothing without real data by Monday 23:59 IST.
4. Individual turn-in on MS Teams by every member.
5. Unverified secondary source for the "30 s" figure; verify before using in the video.
6. Physical-device comfort and performance are unverified; the APK has never been installed.
