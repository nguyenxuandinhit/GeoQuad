"""Seed only the guarded C-owned integration database from repository files."""
from __future__ import annotations
import os
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[3]
guard = root / "tests/GeoQuad.Tests/C_HocTap/fixture_guard.py"
container = sys.argv[1] if len(sys.argv) > 1 else ""
subprocess.run([sys.executable, str(guard), container, "--bootstrap"], check=True)
password = os.environ.get("GEOQUAD_US03_PASSWORD")
if not password:
    raise SystemExit("GEOQUAD_US03_PASSWORD is required; no database changes made")
files = sorted((root / "neo4j/seed").glob("*.cypher"))
if not files:
    raise SystemExit("No seed files found")
marker = subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", password,
                        "MERGE (:CFixtureMarker {id:'C-only'})"], capture_output=True, text=True, encoding="utf-8")
if marker.returncode:
    print(marker.stderr, file=sys.stderr)
    raise SystemExit(marker.returncode)
subprocess.run([sys.executable, str(guard), container], check=True)
for file in files:
    result = subprocess.run(
        ["docker", "exec", "-i", container, "cypher-shell", "-u", "neo4j", "-p", password],
        input=file.read_text(encoding="utf-8"), text=True, encoding="utf-8", capture_output=True)
    if result.returncode:
        print(f"Failed at {file.name}: {result.stderr}", file=sys.stderr)
        raise SystemExit(result.returncode)
    print(f"PASS {file.name}")
