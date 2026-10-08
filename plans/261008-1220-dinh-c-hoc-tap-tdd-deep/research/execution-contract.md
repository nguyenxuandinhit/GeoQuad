# Execution contract — C implementation and verification

ROOT: `C:/Users/Admin/Documents/Taiieu/nosql/GeoQuad`. Branch: `feature/C-hoc-tap`. Existing uncommitted WIP is preserved; no reset, stash, commit, push, or Jira edits were made.

## Application boundaries

- C application code is under `src/GeoQuad.Web/Areas/HocTap/**`; C unit and HTTP smoke code is under `tests/GeoQuad.Tests/C_HocTap/**`.
- Controllers use typed services and repositories backed by shared `IGraphDb`; Cypher values are parameters. Public content requires `hienThi=true`, `DA_RA_SOAT` and an eligible class. Personalized history/roadmap use the authenticated account id and current DB `HOC_LOP` class.
- US20 attempts store a protected attempt token and immutable attempt payload. Guests receive server grading but no `DA_LAM`. US22 learning marks use an account lock and `MERGE ... ON CREATE`, preserving the first `luc`. US23 counts full history and computes weakness from up to five recent distinct attempts, with at least three attempts and a strict `< 0.6` rate.

## Disposable C Neo4j fixture

`tests/GeoQuad.Tests/C_HocTap/Integration/compose.neo4j.yml` defines `geoquad-c-tests-neo4j`, label `geoquad.test-owner=C-only`, loopback Bolt `127.0.0.1:27688`, Browser HTTP `127.0.0.1:27474`, and dedicated named volume `geoquad-c-tests-data`. No production data volume or read-only seed mount is used. `seed_fixture.py` streams the eight `neo4j/seed/*.cypher` files through `cypher-shell` after `fixture_guard.py` verifies the exact container, labels, ports, volume and marker. `GEOQUAD_US03_PASSWORD` is read from the test environment and never printed.

The HTTP smoke scripts call `http_fixture_guard.py` before account/attempt writes. It requires `http://127.0.0.1:5058`, checks that the listening process command line belongs to this checkout's GeoQuad app, creates a temporary UUID exercise only inside the guarded C database, reads it through `/HocTap/BaiTap?khaiNiem=HINH_THOI`, and deletes the sentinel in `finally`. A failed process or DB handshake stops the smoke test before its student fixtures.

## Commands and observed verification

```powershell
docker compose -p geoquad-c-tests -f tests/GeoQuad.Tests/C_HocTap/Integration/compose.neo4j.yml up -d --wait
python tests/GeoQuad.Tests/C_HocTap/seed_fixture.py geoquad-c-tests-neo4j
python tests/GeoQuad.Tests/C_HocTap/us03_review.py geoquad-c-tests-neo4j
dotnet test tests/GeoQuad.Tests --filter "FullyQualifiedName~C_HocTap" --no-restore
dotnet build GeoQuad.sln --no-restore
```

The fixture seeded twice with all eight scripts passing. US03 review output was 14 conditions, 20 signs, 12 properties, 12 formulas and 6 foundational theorems. C unit suite: 58/58. Full solution build: 0 warnings, 0 errors. Five guarded HTTP smoke checks passed: US08, US19, US20/21, US22 and US23/24. US23 HTTP fixture observed 70% all-time and 40% on the latest five; recommendations excluded ever-correct items, were unique, limited to five, and increased by difficulty.

A broader non-integration run excluding `B_ApDung` passed 412 tests. The full `Category!=Integration` run was 468/472: four B-owned `BackupFlowTests` failed because this Windows host lacks `chmod`/the expected WSL shell. No B files were changed for those failures.

## Still open

- No browser UI surface was available for real 360 px/desktop screenshots or keyboard review.
- Independent B↔C/C↔A reviews, A's two Cypher queries in Neo4j Browser, group rehearsal and timing evidence remain for the team.
- `BT-027`/`CM-01` in B's seed is still `NHAP`; B must complete its content review before the public proof path can pass.
- DB acceptance used guarded real Neo4j over HTTP smoke scripts; no separate C# xUnit integration fixture was added.