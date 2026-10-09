"""Real HTTP/Neo4j checks for C on the guarded disposable fixture only."""
from __future__ import annotations
import html
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
container = sys.argv[1] if len(sys.argv) > 1 else ""
base = sys.argv[2].rstrip("/") if len(sys.argv) > 2 else ""
subprocess.run([sys.executable, str(root / "tests/GeoQuad.Tests/C_HocTap/http_fixture_guard.py"), container, base], check=True)
db_password = os.environ["GEOQUAD_US03_PASSWORD"]
password = os.environ["GEOQUAD_US08_PASSWORD"]
username = "us20_" + uuid.uuid4().hex[:10]
marker = "US20_" + uuid.uuid4().hex[:10].upper()

def graph(query: str) -> str:
    return subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p", db_password,
                          "--format", "plain", query], check=True, capture_output=True, text=True, encoding="utf-8").stdout

def opener():
    return urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

def get(client, path):
    with client.open(base + path) as response:
        return response.geturl(), html.unescape(response.read().decode("utf-8"))

def post(client, path, fields):
    with client.open(base + path, urllib.parse.urlencode(fields).encode()) as response:
        return response.geturl(), html.unescape(response.read().decode("utf-8"))

proof_draft = False
try:
    # Guest: GET must not render the key or explanation before submission.
    guest = opener()
    _, page = get(guest, "/HocTap/BaiTap/Lam/BT-014")
    assert "name=\"DapAnDung\"" not in page and "giaiThich" not in page, "answer data leaked on GET"
    # Related knowledge hints at the answer, so it may only appear after submission.
    for hint in ("CT_THOI_DT", "Diện tích hình thoi", "Kiến thức đã dùng", "96 cm²"):
        assert hint not in page, f"answer hint leaked on GET: {hint}"
    _, mcq = get(guest, "/HocTap/BaiTap/Lam/BT-012")
    for letter in "ABCD":
        assert f'for="answer-{letter}"><strong>{letter}.</strong>' in mcq, f"MCQ option {letter} is not a labelled big button"
    match = re.search(r'name="Token" value="([^"]+)"', page)
    assert match, "missing protected attempt token"
    form_token = token(page)
    no_csrf = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    _, no_csrf_page = get(no_csrf, "/HocTap/BaiTap/Lam/BT-014")
    no_csrf_match = re.search(r'name="Token" value="([^"]+)"', no_csrf_page)
    try:
        no_csrf.open(base + "/HocTap/BaiTap/Lam/BT-014", urllib.parse.urlencode({"Token": no_csrf_match.group(1), "DapAn": "96", "DonVi": "cm²"}).encode())
        raise AssertionError("POST without anti-forgery token was accepted")
    except urllib.error.HTTPError as e:
        assert e.code == 400
    _, result = post(guest, "/HocTap/BaiTap/Lam/BT-014", {"__RequestVerificationToken": form_token,
        "Token": match.group(1), "DapAn": "96,0", "DonVi": "cm²", "Dung": "false", "DapAnDung": "1"})
    assert "Chính xác!" in result and "Đáp án đúng" in result, "guest result did not use server grading"
    assert "96" in result
    assert "Đăng nhập để lưu tiến độ" in result, "guest result does not invite login"
    assert "Kiến thức đã dùng" in result and "Diện tích hình thoi" in result, "guest result lacks related knowledge"
    assert "/KienThuc/ThuVien/ChiTiet/HINH_THOI" in result, "guest result lacks concept detail link"

    # Student: correct attempt persists once; replay with a changed answer cannot rewrite it.
    client = opener()
    _, signup = get(client, "/TaiKhoan/DangKy")
    _, signup_result = post(client, "/TaiKhoan/DangKy", {"__RequestVerificationToken": token(signup), "TenDangNhap": username,
        "MatKhau": password, "NhapLaiMatKhau": password, "BietDanh": marker, "Lop": "8", "DongYQuyenRiengTu": "true"})
    profile_url, profile = get(client, "/HocTap/HoSo")
    assert profile_url.endswith("/HocTap/HoSo") and marker in profile, (
        f"temporary account did not authenticate; final={profile_url}; signup form error="
        + re.sub(r"<[^>]+>", " ", signup_result)[-400:])
    _, page = get(client, "/HocTap/BaiTap/Lam/BT-014")
    match = re.search(r'name="Token" value="([^"]+)"', page)
    form_token = token(page)
    graph(f"CREATE (:SmokeMarker {{name:'{marker}'}})")
    _, result = post(client, "/HocTap/BaiTap/Lam/BT-014", {"__RequestVerificationToken": form_token,
        "Token": match.group(1), "DapAn": "96", "DonVi": "cm²", "Dung": "false", "TaiKhoanId": "forged"})
    assert "Chính xác!" in result
    assert "Đăng nhập để lưu tiến độ" not in result, "student result shows guest login invite"
    assert "Kiến thức đã dùng" in result, "student result lacks related knowledge"
    before_replay = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}})-[d:DA_LAM]->(b:BaiTap {{ma:'BT-014'}}) RETURN count(d) AS n,collect(d.dapAnDaChon) AS answers")
    assert "1" in before_replay and "96" in before_replay, "authenticated submission was not persisted"
    try:
        post(client, "/HocTap/BaiTap/Lam/BT-014", {"__RequestVerificationToken": form_token,
            "Token": match.group(1), "DapAn": "95", "DonVi": "cm²"})
        raise AssertionError("changed-answer replay was accepted")
    except urllib.error.HTTPError as e:
        assert e.code == 409
    rows = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}})-[d:DA_LAM]->(b:BaiTap {{ma:'BT-014'}}) RETURN count(d) AS n,collect(d.dapAnDaChon) AS answers,collect(d.dung) AS correct,collect(d.donViDaChon) AS units")
    assert "1" in rows and "96" in rows and "true" in rows.lower() and "cm²" in rows

    # US-21 AC: B reviewed CM-01, so BT-027 shows the sample solution and links to the proof page.
    _, proof = get(guest, "/HocTap/BaiTap/Lam/BT-027")
    assert "XemLoiGiai" in proof and 'name="DapAn"' not in proof, "proof exercise must offer the sample, not grading"
    _, sample = post(guest, "/HocTap/BaiTap/XemLoiGiai/BT-027", {"__RequestVerificationToken": token(proof)})
    assert 'href="/ApDung/ChungMinh/Xem/CM-01"' in sample, "sample solution lacks the README URL to CM-01"
    with guest.open(base + "/ApDung/ChungMinh/Xem/CM-01") as r:
        assert r.status == 200, f"CM-01 link returned HTTP {r.status}"

    # Gate: the same exercise hides the sample again while the proof is a draft.
    graph("MATCH (cm:ChungMinh {ma:'CM-01'}) SET cm.trangThai = 'NHAP'")
    proof_draft = True
    _, proof = get(guest, "/HocTap/BaiTap/Lam/BT-027")
    assert "XemLoiGiai" not in proof, "unreviewed proof link was exposed"
    try:
        post(guest, "/HocTap/BaiTap/XemLoiGiai/BT-027", {"__RequestVerificationToken": token(proof)})
        raise AssertionError("unreviewed proof was exposed")
    except urllib.error.HTTPError as e:
        assert e.code == 404, f"unreviewed proof returned HTTP {e.code}"
    print("US-20/21 HTTP + Neo4j: PASS (no key/hint leak, big MCQ buttons, CSRF, guest no-persist + login invite, "
          "related knowledge after submit, server grading, immutable replay, BT-027 -> CM-01 link 200, draft proof gate)")
finally:
    if proof_draft:
        graph("MATCH (cm:ChungMinh {ma:'CM-01'}) SET cm.trangThai = 'DA_RA_SOAT'")
    graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}}) DETACH DELETE u")
    graph(f"MATCH (n) WHERE n.name='{marker}' DETACH DELETE n")
