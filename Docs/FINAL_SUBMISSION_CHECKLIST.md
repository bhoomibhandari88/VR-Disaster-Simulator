# Final submission checklist — IVRAR Group 16 (deadline Mon 26 Oct 2026, 23:59 IST)

Status key: **Complete** = verified with evidence · **Partial** = built, needs team check · **Pending** = only the team can do it · **Blocked** = cannot proceed until something else happens.

| # | Requirement | Status | Evidence / what is missing |
|---|---|---|---|
| 1 | Triage scene, 10 casualties, menu launch, full session | Complete (editor-level) | `TriageSimulation.unity`; PlayMode `TriageSceneTests` |
| 2 | EditMode tests | Complete | 109/109 pass (2026-10-25) |
| 3 | PlayMode tests | Complete | 31/31 pass |
| 4 | Findings not revealed before assessment; protocol labelled | Complete | fixed in `f054410`, covered by tests |
| 5 | Telemetry, CSV, zero-denominator handling | Complete (software) | `TriageCsv.cs`, `TriageMetrics.cs`, tests |
| 6 | Android APK exists | Partial | built from `b2b6d84`, **before the fix `f054410`**; rebuild + re-hash before release |
| 7 | Tested on physical Quest | **Pending** | never run on a device; see `MANUAL_TEST_CHECKLIST.md` |
| 8 | OpenXR validation exception | Partial | editor/build advisory in package code, build still succeeds; not proven harmless on device |
| 9 | Final-candidate DOCX | Partial | `Docs/IVRAR_dossier_DRAFT/IVRAR_Group_16_Technical_Dossier_FINAL_CANDIDATE.docx` (Sections 1–3 filled from evidence; 4–6 PENDING) |
| 10 | Final-candidate PDF | Partial | same folder, `.pdf`, 5 pages, LibreOffice preview. Re-export from Word after the team completes it |
| 11 | START source confirmed | Partial | CHEMM and Benson 1996 support both perfusion variants; the 2 s refill threshold must be confirmed by the team in the original paper |
| 12 | GitHub branch and commit | Pending (push) | local branch is ahead of `bhoomi/feature/factory-rescue-vr`; push needs the team's credentials |
| 13 | Git LFS | Complete | 249 files, `git lfs fsck` OK, retrievable via the LFS batch API |
| 14 | GitHub release | Pending | not published (needs explicit approval and a fresh APK) |
| 15 | Instructor monorepo | **Pending / flag to instructor** | brief asks for `sunny-nanade/NMIMS-MPSTME-IVRAR-2026`; our work is in the team repo. Not assumed approved — ask Dr. Nanade |
| 16 | Human study, N ≥ 10, consent | **Pending (grade-affecting)** | no participants, no data. `STUDY_OPERATIONS_CHECKLIST.md` |
| 17 | SUS, NASA-TLX, results plots, confusion matrix | Blocked by 16 | none exist; none were invented |
| 18 | Data provenance declaration | Blocked by 16 | must be answered by the team truthfully |
| 19 | LinkedIn video | Pending (grade-affecting) | not made |
| 20 | AI interaction audit (3 real cases) | Pending | team must record real AI errors |
| 21 | Peer reviews of Groups 17 and 07 | Pending | videos not watched |
| 22 | Individual contribution table (usernames, commits, %) | Pending | all commits are authored "Claude"; members must supply real contributions |
| 23 | Learning record, signatures, sign-off | Pending | each member personally; nothing signed on anyone's behalf |
| 24 | "AI-driven" wording | Pending decision | scenario is scripted; there is no ML model |
| 25 | Unity version (title sheet says 2022.3 LTS; project is 6000.3.21f1) | Pending decision | flagged in the dossier note |

## Dossier dropdowns left unchanged (unsafe to fill)
Script authorship (4 rows × 2), release runs on clean device, dossier committed, study design, provenance declaration (4), video-speaker, AI audit (3 rows × 4), peer review (2 × 5), learning record (4 × 6), sign-off (4 × 4).

## Push commands (not run)
```bash
git push -u bhoomi feature/factory-rescue-vr      # no force
# after a rebuilt APK and team agreement:
gh release create group-16-v1.0 Builds/Android/DisasterResponseVR.apk \
  --repo bhoomibhandari88/VR-Disaster-Simulator --target feature/factory-rescue-vr \
  --title "Disaster Response VR v1.0 (Quest APK)" --notes-file Docs/RELEASE_NOTES.md
```
