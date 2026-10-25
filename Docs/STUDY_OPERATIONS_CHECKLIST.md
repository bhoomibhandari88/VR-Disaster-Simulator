# Human-study operations checklist (needs at least 10 real adult volunteers)

Companion to `STUDY_PROTOCOL.md`. Status of every item: **PENDING — TEAM INPUT REQUIRED**. No participant, consent form or human telemetry exists today.

## Before
- [ ] Ask Dr. Nanade whether faculty/ethics approval or a specific consent template is required, and whether the study must use the triage build in your own repository.
- [ ] Print a consent form: purpose, what they will do, voluntary, can stop any time, data anonymous, how long data are kept, contact. One signed copy per participant, stored by the team (never in Git).
- [ ] Keep a private key sheet (name to P01...). Do not store it in the repository or write names in any CSV or note.
- [ ] Decide once, for all participants: feedback toggle ON or OFF, perfusion variant (default capillary refill), headset type.
- [ ] Install the release APK; note the commit hash and APK SHA-256 on the questionnaire sheet.
- [ ] Rehearse with TEST + SoftwareTest; keep or delete those files in a folder named `rehearsal`.

## Each participant (about 15 minutes)
- [ ] Informed consent signed before anything else; confirm adult and voluntary.
- [ ] Short briefing (same words each time); fit headset; guardian boundary check.
- [ ] On the start panel set Participant = their ID and Data type = **HumanParticipant**, then Begin.
- [ ] Do not coach during the run; stop at once on discomfort.
- [ ] After the results panel: SUS (10 items, 1-5), NASA-TLX (6 scales, 0-100) and optional SSQ, entered into `templates/participant_questionnaires_template.csv`.
- [ ] Copy the three CSV files from the headset, rename nothing, store a read-only copy.

## After
- [ ] Check every record row has `data_provenance = HumanParticipant`; exclude partial sessions (`session_complete=false`) and say how many.
- [ ] Never edit raw files. Compute pooled accuracy, under-triage, latency (mean, SD, median) and the pooled 4x4 confusion matrix from the raw `_records.csv` files; SUS = 2.5 x (sum of (odd item - 1) + sum of (5 - even item)); NASA-TLX raw mean.
- [ ] Put raw files in the repository under `telemetry/` only if consent covers it and they contain no names; plots must regenerate from those files.
- [ ] Delete or clearly rename any `SoftwareTest`/`Pilot` file (for example `SIMULATED_...`) so it cannot be mistaken for human data.

## If fewer than 10 participants by the deadline
Report the real N and the shortfall in Section 4 and the video. The guide states that every Section 4 number must come from at least 10 real people who used the build, so fewer will cost marks in Section 4 (DBL Usability Case Study, 5 marks) and weaken the SBL results; fabricated or simulated data scores zero and is misconduct.
