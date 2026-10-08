---
title: GeoQuad B implementation and isolated recovery
date: 2026-10-08
summary: "Implemented B with .NET8, real Neo4j HTTP tests and offline recovery; external review/UI/Windows gates remain open"
---

# GeoQuad B implementation and isolated recovery

## Work

Executed the accepted B TDD plan with --auto on feature/B-ap-dung. Added B catalog/proofs, hint QPP, public class/review gates, scenarios/measurement, admin exercise transactions and offline backup/restore. Shared infrastructure/A/C/SRS files were preserved. No commit/push.

Installed isolated official .NET8 SDK/runtime in temporary storage and started Docker Desktop. All destructive test graph resets first validate B-only project/volume/loopback/readonly seed markers. Production B seed stays NHAP; reviewed content only exists as synthetic state in test graph pending human C review.

## Findings and verification

Seed RED failed for missing B seed. Real HTTP admin editor found a driver List<string>/array mapping issue. First recovery drill found unsupported toString(map); trap recovered source, and sorted history output fixed the manifest digest. Final review corrected history fixtures to SRS: one DA_LAM relationship per attempt with luc/dung/answer/duration, rather than an aggregate soLan property.

Final .NET8 build: zero warnings/errors. Unit solution regression:118 passed. Full B integration/HTTP/load/drill:14 passed, zero failures/skips. Four admin/recovery tests rerun after history fixture correction:4 passed. Hint load:1000 requests/100 workers/0errors, p95=400.4095ms. Latest restore matched134nodes/301edges, application account password and three history relationships; fresh system auth and unchanged source. Script syntax, whitespace and plan validation passed. Conditional code-simplifier made three scoped behavior-preserving edits; root reviewed final code/test evidence.

## Handoff

Plan remains in-progress with partial checked tasks. C must review independent mathematical/SGK sources before seed publishing (especially CM-04 grounds). A review B and C readers/hidden exercises/two Cypher queries remain unavailable because C endpoints are still skeletons. Browser inventory is empty, so360px/keyboard/4G/KaTeX visual acceptance remains untested. No pwsh/Windows runtime: native PowerShell scripts have been statically reviewed but Windows ACL/drill acceptance remains open. A README/clean-machine and C demo acceptance remain with owners.

Docs/cypher/B-ap-dung.md contains actual query parameters/output and backup protocol; docs/kiem-thu.md and plan reports record limitations. Private archives/logs remain under ignored backups; restore resources created by tests were removed using run labels. Local journal only; AgentWiki publish skipped.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.
