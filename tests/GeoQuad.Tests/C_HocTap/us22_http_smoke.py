"""US22 authenticated roadmap and idempotent learning marker smoke test."""
from __future__ import annotations
import http.cookiejar
import os
from pathlib import Path
import re
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request
import uuid
from us08_http_smoke import token

root = Path(__file__).resolve().parents[3]
container = sys.argv[1]
base = sys.argv[2].rstrip("/")
subprocess.run([sys.executable, str(root / "tests/GeoQuad.Tests/C_HocTap/http_fixture_guard.py"), container, base], check=True)
db_password = os.environ["GEOQUAD_US03_PASSWORD"]
password = os.environ["GEOQUAD_US08_PASSWORD"]
username = "us22_" + uuid.uuid4().hex[:10]
nickname = "US22_" + uuid.uuid4().hex[:10].upper()

def graph(query: str) -> str:
    return subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", db_password,
                          "--format", "plain", query], check=True, capture_output=True, text=True, encoding="utf-8").stdout

def client():
    return urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

def get(c, path):
    with c.open(base + path) as r:
        return r.geturl(), r.read().decode("utf-8")

def post(c, path, fields):
    with c.open(base + path, urllib.parse.urlencode(fields).encode("utf-8")) as r:
        return r.geturl(), r.read().decode("utf-8")

try:
    guest = client()
    target_url, _ = get(guest, "/HocTap/LoTrinh")
    assert "/TaiKhoan/DangNhap" in target_url, "guest accessed a personal roadmap"

    student = client()
    _, page = get(student, "/TaiKhoan/DangKy")
    post(student, "/TaiKhoan/DangKy", {"__RequestVerificationToken": token(page),
        "TenDangNhap": username, "MatKhau": password, "NhapLaiMatKhau": password,
        "BietDanh": nickname, "Lop": "8", "DongYQuyenRiengTu": "true"})
    url, _ = get(student, "/HocTap/HoSo")
    assert url.endswith("/HocTap/HoSo"), "temporary student registration failed"

    _, roadmap = get(student, "/HocTap/LoTrinh?muc=HINH_VUONG")
    assert "HINH_VUONG" in roadmap and "GOC_VUONG" in roadmap, "roadmap omitted selected target or prerequisites"
    assert "name=\"__RequestVerificationToken\"" in roadmap, "learning marker form is missing anti-forgery token"
    form_token = token(roadmap)
    result_url, _ = post(student, "/HocTap/LoTrinh/EmDaHieu", {"__RequestVerificationToken": form_token,
        "muc": "HINH_VUONG", "ma": "GOC_VUONG"})
    assert result_url.endswith("/HocTap/LoTrinh?muc=HINH_VUONG"), "marker did not PRG back to selected roadmap"
    row = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}})-[r:DA_HOC]->(k:KhaiNiem {{ma:'GOC_VUONG'}}) RETURN count(r) AS n,collect(toString(r.luc)) AS times")
    assert "1" in row and "datetime" not in row.lower(), "DA_HOC was not persisted exactly once"
    first_time = re.search(r"\d{4}-\d\d-\d\dT[^\s,]+", row)
    assert first_time, f"could not read persisted learning timestamp: {row}"
    post(student, "/HocTap/LoTrinh/EmDaHieu", {"__RequestVerificationToken": form_token,
        "muc": "HINH_VUONG", "ma": "GOC_VUONG"})
    again = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}})-[r:DA_HOC]->(k:KhaiNiem {{ma:'GOC_VUONG'}}) RETURN count(r) AS n,collect(toString(r.luc)) AS times")
    assert "1" in again and first_time.group(0) in again, "repeat marker changed history or timestamp"
    _, reloaded = get(student, "/HocTap/LoTrinh?muc=HINH_VUONG")
    assert "✓" in reloaded and "Đã học" in reloaded, "learned step lacks the ✓ marker after reload"
    assert "học ngay" not in reloaded, "roadmap with prerequisites claimed it can be learned right away"
    # TU_GIAC has no CAN_BIET_TRUOC edge in the seed, so its roadmap is only itself.
    _, root_target = get(student, "/HocTap/LoTrinh?muc=TU_GIAC")
    assert "Em có thể học ngay khái niệm này" in root_target, "target without prerequisites lacks the learn-now message"
    try:
        post(student, "/HocTap/LoTrinh/EmDaHieu", {"__RequestVerificationToken": "invalid", "muc": "HINH_VUONG", "ma": "GOC_VUONG"})
        raise AssertionError("invalid anti-forgery token was accepted")
    except urllib.error.HTTPError as error:
        assert error.code == 400
    print("US-22 HTTP + Neo4j: PASS (auth, target/prerequisite roadmap, CSRF, DA_HOC, idempotent timestamp, ✓ after reload, learn-now message)")
finally:
    graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}}) DETACH DELETE u")
