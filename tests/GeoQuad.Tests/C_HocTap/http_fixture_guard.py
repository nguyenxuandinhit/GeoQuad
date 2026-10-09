"""Verify a local C-owned app is connected to the guarded disposable Neo4j DB."""
from __future__ import annotations
import html
import json
import os
from pathlib import Path
import subprocess
import sys
import urllib.request
import uuid

root = Path(__file__).resolve().parents[3]
guard = Path(__file__).with_name("fixture_guard.py")
container, base = sys.argv[1], sys.argv[2].rstrip("/")
if base != "http://127.0.0.1:5058":
    raise SystemExit("Refusing HTTP mutations: test app must use http://127.0.0.1:5058")
subprocess.run([sys.executable, str(guard), container], check=True)

ps = r"$c=Get-NetTCPConnection -LocalPort 5058 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1; if(-not $c){exit 2}; $p=Get-CimInstance Win32_Process -Filter ('ProcessId='+$c.OwningProcess); if($p){[Console]::Write($p.CommandLine)}"
owner = subprocess.run(["powershell.exe", "-NoProfile", "-Command", ps], capture_output=True, text=True)
command = owner.stdout.lower()
if owner.returncode or str(root).lower() not in command or "geoq" not in command:
    raise SystemExit("Refusing HTTP mutations: port 5058 is not owned by the GeoQuad app from this checkout")

password = os.environ.get("GEOQUAD_US03_PASSWORD")
if not password:
    raise SystemExit("GEOQUAD_US03_PASSWORD required to verify the app/database handshake")
sentinel = "GQ_C_SENTINEL_" + uuid.uuid4().hex[:12].upper()
create = f"""
MERGE (b:BaiTap {{ma:'{sentinel}'}})
SET b.de='C APP FIXTURE {sentinel}', b.loai='TRAC_NGHIEM', b.doKho=1,
    b.hienThi=true, b.trangThai='DA_RA_SOAT'
WITH b
MATCH (l:Lop {{so:8}}),(k:KhaiNiem {{ma:'HINH_THOI',trangThai:'DA_RA_SOAT'}})
MERGE (b)-[:THUOC_LOP]->(l)
MERGE (b)-[:LIEN_QUAN_DEN]->(k)
"""
cleanup = ["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", password,
           f"MATCH (b:BaiTap {{ma:'{sentinel}'}}) DETACH DELETE b"]
try:
    created = subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", password, create],
                             text=True, encoding="utf-8", capture_output=True)
    if created.returncode:
        raise SystemExit("Refusing HTTP mutations: could not create C sentinel in guarded DB")
    with urllib.request.urlopen(base + "/HocTap/BaiTap?khaiNiem=HINH_THOI", timeout=8) as response:
        page = html.unescape(response.read().decode("utf-8"))
    if sentinel not in page:
        count = subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", password,
                                f"MATCH (b:BaiTap {{ma:'{sentinel}'}}) RETURN count(b)"],
                               capture_output=True, text=True, encoding="utf-8").stdout.strip()
        raise SystemExit(f"Refusing HTTP mutations: app did not read sentinel {sentinel}; guarded DB count={count!r}")
finally:
    subprocess.run(cleanup, capture_output=True, text=True, encoding="utf-8")
print("C app/database sentinel handshake: PASS")
