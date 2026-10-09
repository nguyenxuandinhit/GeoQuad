"""US23/24 private history, rolling weakness, recommendation and next-step checks."""
from __future__ import annotations
import http.cookiejar
import html
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
username = "us23_" + uuid.uuid4().hex[:10]
nickname = "US23_" + uuid.uuid4().hex[:10].upper()
marker = "US23_" + uuid.uuid4().hex[:10].upper()

def graph(query: str) -> str:
    return subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", db_password,
                          "--format", "plain", query], check=True, capture_output=True, text=True, encoding="utf-8").stdout

def open_client():
    return urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

def get(c, path):
    with c.open(base + path) as r:
        return r.geturl(), html.unescape(r.read().decode("utf-8"))

def post(c, path, fields):
    with c.open(base + path, urllib.parse.urlencode(fields).encode("utf-8")) as r:
        return r.geturl(), html.unescape(r.read().decode("utf-8"))

extra_users: list[str] = []

def register(name: str):
    c = open_client()
    _, signup = get(c, "/TaiKhoan/DangKy")
    post(c, "/TaiKhoan/DangKy", {"__RequestVerificationToken": token(signup), "TenDangNhap": name,
        "MatKhau": password, "NhapLaiMatKhau": password, "BietDanh": name.upper(), "Lop": "8", "DongYQuyenRiengTu": "true"})
    url, _ = get(c, "/HocTap/HoSo")
    assert url.endswith("/HocTap/HoSo"), f"temporary student registration failed: {name}"
    return c

def seed_attempts(name: str, exercise: str, results: list[bool]):
    for n, ok in enumerate(results):
        graph(f"""MATCH (u:TaiKhoan {{tenDangNhap:'{name}'}}), (b:BaiTap {{ma:'{exercise}'}})
                  MERGE (u)-[d:DA_LAM {{maLan:'{marker}_{name}_{n}'}}]->(b)
                  SET d.dung = {str(ok).lower()}, d.luc = '2026-10-09T00:0{n}:00Z'""")

try:
    student = register(username)

    # Ten attempts over two exercises: 7/10 all time, while the newest five are 2/5.
    graph(f"""
        MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}})
        UNWIND range(0,9) AS n
        MATCH (b:BaiTap {{ma:CASE WHEN n % 2 = 0 THEN 'BT-014' ELSE 'BT-020' END}})
        MERGE (u)-[d:DA_LAM {{maLan:'{marker}_' + toString(n)}}]->(b)
        SET d.dung = CASE WHEN n < 5 OR n IN [5,6] THEN true ELSE false END,
            d.luc = '2026-10-' + CASE WHEN n < 5 THEN '01T00:' ELSE '08T00:' END + right('0' + toString(n),2) + ':00Z'
    """)

    _, progress = get(student, "/HocTap/TienDo")
    assert "Hình thoi" in progress and "70%" in progress and "40%" in progress, f"all-time or last-five progress is incorrect: {progress[:1800]}"
    can_on = progress.split("Cần ôn lại", 1)[1].split("Tỉ lệ đúng theo khái niệm", 1)[0] if "Cần ôn lại" in progress else ""
    assert "Hình thoi" in can_on, "weak concept missing from the 'Cần ôn lại' list"
    assert 'role="img"' in progress and "Hình thoi: đúng 7 trên 10 lượt, 70%" in progress, "bar chart lacks text percentage/label"
    _, suggestions = get(student, "/HocTap/TienDo/GoiY")
    assert "Nên ôn" in suggestions or "ôn" in suggestions.lower(), "weak concept was not identified"
    assert "BT-014" not in suggestions and "BT-020" not in suggestions, "ever-correct exercise was recommended"
    links = re.findall(r"/HocTap/BaiTap/Lam/([A-Z0-9-]+)", suggestions)
    assert 1 <= len(links) <= 5, f"expected one to five unique suggestions, got {links}"
    assert len(links) == len(set(links)), "duplicate recommendations were rendered"
    difficulty = [int(re.search(r'\d+', graph(f"MATCH (b:BaiTap {{ma:'{code}'}}) RETURN b.doKho AS doKho")).group(0)) for code in links]
    assert difficulty == sorted(difficulty), f"recommendation difficulty did not increase: {difficulty}"
    next_url, _ = get(student, "/HocTap/BaiTap/KeTiep/BT-014")
    assert "/HocTap/BaiTap/Lam/" in next_url and not next_url.endswith("BT-014"), "student KeTiep did not use an eligible recommendation"

    # UC-13 3a: enough data (>= 3 attempts) and nothing weak -> congratulate, harder first, roadmap link.
    strong_name = "us23s_" + uuid.uuid4().hex[:8]
    extra_users.append(strong_name)
    strong = register(strong_name)
    seed_attempts(strong_name, "BT-012", [True, True, True])
    _, strong_progress = get(strong, "/HocTap/TienDo")
    assert "Chúc mừng" in strong_progress, "3a progress page did not congratulate"
    _, strong_tips = get(strong, "/HocTap/TienDo/GoiY")
    assert "Chúc mừng" in strong_tips and "/HocTap/LoTrinh" in strong_tips, "3a suggestions lack congratulation or roadmap link"
    strong_links = re.findall(r"/HocTap/BaiTap/Lam/([A-Z0-9-]+)", strong_tips)
    strong_diff = [int(re.search(r'\d+', graph(f"MATCH (b:BaiTap {{ma:'{code}'}}) RETURN b.doKho AS doKho")).group(0)) for code in strong_links]
    assert 1 <= len(strong_links) <= 5 and strong_diff == sorted(strong_diff, reverse=True), f"3a difficulty not decreasing: {strong_diff}"

    # Only 2 attempts: not enough data, so no congratulation and no harder-first jump.
    new_name = "us23n_" + uuid.uuid4().hex[:8]
    extra_users.append(new_name)
    newbie = register(new_name)
    seed_attempts(new_name, "BT-013", [False, False])
    _, new_tips = get(newbie, "/HocTap/TienDo/GoiY")
    assert "Chúc mừng" not in new_tips, "student with too little data was congratulated"
    new_links = re.findall(r"/HocTap/BaiTap/Lam/([A-Z0-9-]+)", new_tips)
    new_diff = [int(re.search(r'\d+', graph(f"MATCH (b:BaiTap {{ma:'{code}'}}) RETURN b.doKho AS doKho")).group(0)) for code in new_links]
    assert new_diff == sorted(new_diff), f"not-enough-data difficulty not increasing: {new_diff}"

    # README AC US-23 with the dev seed: hocsinh8 sees Hình thoi (2/5) but not Hình bình hành (2 attempts).
    # Requires the app in Development (creates hocsinh8). Dev seed MERGEs by maLan, so rerunning is safe.
    dev_seed = (root / "neo4j/dev/31-lam-bai-mau-C.cypher").read_text(encoding="utf-8")
    subprocess.run(["docker", "exec", "-i", container, "cypher-shell", "-u", "neo4j", "-p", db_password],
                   input=dev_seed, check=True, capture_output=True, text=True, encoding="utf-8")
    demo = open_client()
    _, login = get(demo, "/TaiKhoan/DangNhap")
    post(demo, "/TaiKhoan/DangNhap", {"__RequestVerificationToken": token(login), "TenDangNhap": "hocsinh8", "MatKhau": "Hocsinh@123"})
    _, demo_progress = get(demo, "/HocTap/TienDo")
    demo_can_on = demo_progress.split("Cần ôn lại", 1)[1].split("Tỉ lệ đúng theo khái niệm", 1)[0] if "Cần ôn lại" in demo_progress else ""
    assert "Hình thoi" in demo_can_on and "Hình bình hành" not in demo_can_on, f"dev-seed AC failed: {demo_can_on[:600]}"

    print("US-23/24 HTTP + Neo4j: PASS (private all-time/last-five history, 70%/40%, 'Cần ôn lại' list + text bars, "
          "ever-correct exclusion, <=5 unique candidates, student next step, 3a congratulate harder-first, "
          "not-enough-data no congratulation, hocsinh8 dev-seed AC)")
finally:
    for name in [username, *extra_users]:
        graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{name}'}}) DETACH DELETE u")
