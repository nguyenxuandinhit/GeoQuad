"""Require an explicitly C-owned disposable Neo4j container before fixture writes."""
from __future__ import annotations
import json
import os
import subprocess
import sys

EXPECTED = "geoquad-c-tests-neo4j"
if len(sys.argv) not in (2, 3) or sys.argv[1] != EXPECTED or (len(sys.argv) == 3 and sys.argv[2] != "--bootstrap"):
    raise SystemExit(f"Refusing DB mutation: pass the C fixture name {EXPECTED!r}")
raw = subprocess.run(["docker", "inspect", EXPECTED], check=True, capture_output=True, text=True).stdout
data = json.loads(raw)[0]
labels = data["Config"].get("Labels") or {}
ports = data["NetworkSettings"]["Ports"]
mounts = data.get("Mounts", [])
checks = {
    "running": data["State"]["Running"],
    "C owner label": labels.get("geoquad.test-owner") == "C-only",
    "compose project": labels.get("com.docker.compose.project") == "geoquad-c-tests",
    "private Bolt binding": any(x.get("HostIp") == "127.0.0.1" and x.get("HostPort") == "27688" for x in (ports.get("7687/tcp") or [])),
    "private Browser binding": any(x.get("HostIp") == "127.0.0.1" and x.get("HostPort") == "27474" for x in (ports.get("7474/tcp") or [])),
    "dedicated data volume": any(m["Destination"] == "/data" and m["Name"] == "geoquad-c-tests-data" for m in mounts),
}
failed = [name for name, ok in checks.items() if not ok]
if failed:
    raise SystemExit("Refusing DB mutation; failed fixture checks: " + ", ".join(failed))
if len(sys.argv) == 2:
    password = os.environ.get("GEOQUAD_US03_PASSWORD")
    if not password:
        raise SystemExit("GEOQUAD_US03_PASSWORD required to verify C database marker")
    marker = subprocess.run(["docker", "exec", EXPECTED, "cypher-shell", "-u", "neo4j", "-p", password,
                             "MATCH (:CFixtureMarker {id:'C-only'}) RETURN count(*) > 0 AS ok"],
                            capture_output=True, text=True, encoding="utf-8")
    if marker.returncode or "true" not in marker.stdout.lower():
        raise SystemExit("Refusing DB mutation; C-only marker is missing or unreachable")
print("C-only disposable Neo4j fixture: PASS")
