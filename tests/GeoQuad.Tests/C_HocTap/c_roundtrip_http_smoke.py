"""Roundtrip US-20 -> US-23 -> US-24 through real form submissions on the guarded C fixture.

Usage: python c_roundtrip_http_smoke.py <container> http://127.0.0.1:5058
Needs GEOQUAD_US03_PASSWORD (fixture DB) and GEOQUAD_US08_PASSWORD (temporary account).
"""
from __future__ import annotations
import html
import http.cookiejar
import os
from pathlib import Path
import re
import subprocess
import sys
import urllib.parse
import urllib.request
import uuid
from us08_http_smoke import token

root = Path(__file__).resolve().parents[3]
container, base = sys.argv[1], sys.argv[2].rstrip("/")
subprocess.run([sys.executable, str(root / "tests/GeoQuad.Tests/C_HocTap/http_fixture_guard.py"), container, base], check=True)
db_password = os.environ["GEOQUAD_US03_PASSWORD"]
password = os.environ["GEOQUAD_US08_PASSWORD"]
username = "rt_" + uuid.uuid4().hex[:10]

def graph(query: str) -> str:
    return subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", db_password,
                           "--format", "plain", query], check=True, capture_output=True, text=True, encoding="utf-8").stdout

client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

def get(path):
    with client.open(base + path) as r:
        return r.geturl(), html.unescape(r.read().decode("utf-8"))

def post(path, fields):
    with client.open(base + path, urllib.parse.urlencode(fields).encode("utf-8")) as r:
        return r.geturl(), html.unescape(r.read().decode("utf-8"))

def submit(code: str, answer: str, unit: str) -> str:
    _, page = get(f"/HocTap/BaiTap/Lam/{code}")
    attempt = re.search(r'name="Token" value="([^"]+)"', page).group(1)
    _, result = post(f"/HocTap/BaiTap/Lam/{code}", {"__RequestVerificationToken": token(page),
                     "Token": attempt, "DapAn": answer, "DonVi": unit})
    return result

def suggestions() -> list[str]:
    _, page = get("/HocTap/TienDo/GoiY")
    return re.findall(r"/HocTap/BaiTap/Lam/([A-Z0-9-]+)", page)

try:
    _, signup = get("/TaiKhoan/DangKy")
    post("/TaiKhoan/DangKy", {"__RequestVerificationToken": token(signup), "TenDangNhap": username,
         "MatKhau": password, "NhapLaiMatKhau": password, "BietDanh": username.upper(), "Lop": "8", "DongYQuyenRiengTu": "true"})
    assert get("/HocTap/HoSo")[0].endswith("/HocTap/HoSo"), "temporary student registration failed"

    # US-20: three wrong submissions on a Hình thoi exercise (BT-013, answer 28 cm).
    for wrong in ("1", "2", "3"):
        assert "Chưa chính xác" in submit("BT-013", wrong, "cm"), "wrong answer was not graded as wrong"
    saved = graph(f"MATCH (:TaiKhoan {{tenDangNhap:'{username}'}})-[d:DA_LAM]->(:BaiTap {{ma:'BT-013'}}) RETURN count(d) AS n")
    assert re.search(r"\b3\b", saved), f"expected 3 persisted attempts: {saved}"

    # US-23: Hình thoi is 0/3 recent, so it must be listed under "Cần ôn lại".
    _, progress = get("/HocTap/TienDo")
    can_on = progress.split("Cần ôn lại", 1)[1].split("Tỉ lệ đúng theo khái niệm", 1)[0]
    assert "Hình thoi" in can_on, "weak concept from real submissions missing in 'Cần ôn lại'"

    # US-24: suggestions are Hình thoi exercises not yet answered correctly, at most five, no proofs.
    before = suggestions()
    assert 1 <= len(before) <= 5 and len(before) == len(set(before)), f"bad suggestion list: {before}"
    for code in before:
        info = graph(f"MATCH (b:BaiTap {{ma:'{code}'}}) RETURN b.loai AS loai, "
                     f"EXISTS {{ (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem {{ma:'HINH_THOI'}}) }} AS thoi")
        assert "CHUNG_MINH" not in info and "TRUE" in info.upper(), f"{code} is not an eligible Hình thoi exercise: {info}"

    # Answer the first suggestion correctly; it must leave the suggestion list.
    first = before[0]
    key = graph(f"MATCH (b:BaiTap {{ma:'{first}'}}) RETURN b.loai AS loai, b.dapAnDung AS chu, b.dapAnSo AS so, coalesce(b.donVi,'') AS donVi")
    values = [v.strip().strip('"') for v in key.strip().splitlines()[-1].split(",")]
    loai, chu, so, don_vi = values[0], values[1], values[2], ",".join(values[3:]).strip().strip('"')
    answer = chu if loai == "TRAC_NGHIEM" else so
    assert "Chính xác!" in submit(first, answer, don_vi), f"correct answer for {first} was not accepted"
    after = suggestions()
    assert first not in after, f"correctly answered {first} is still suggested: {after}"

    print(f"Roundtrip US-20 -> US-23 -> US-24: PASS (3 wrong BT-013 persisted, Hình thoi in 'Cần ôn lại', "
          f"{len(before)} eligible suggestions, {first} removed after correct answer)")
finally:
    graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}}) DETACH DELETE u")
