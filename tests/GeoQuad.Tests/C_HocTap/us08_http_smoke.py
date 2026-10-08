"""US-08 HTTP smoke test against a disposable Development instance.

Requires GEOQUAD_US08_PASSWORD in the environment. Creates a temporary
student through registration, exercises profile changes, then deletes it.
"""

from __future__ import annotations

import html
import html.parser
import http.cookiejar
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
import uuid
import subprocess
from pathlib import Path


class TokenParser(html.parser.HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.token: str | None = None

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        fields = dict(attrs)
        if tag == "input" and fields.get("name") == "__RequestVerificationToken":
            self.token = fields.get("value")


def token(page: str) -> str:
    parser = TokenParser()
    parser.feed(page)
    assert parser.token, "missing antiforgery token"
    return parser.token


def main() -> None:
    password = os.environ["GEOQUAD_US08_PASSWORD"]
    new_password = password + "-new"
    username = "smoke_c_" + uuid.uuid4().hex[:8]
    base = sys.argv[1].rstrip("/") if len(sys.argv) > 1 else "http://localhost:5080"
    if len(sys.argv) > 2:
        subprocess.run([sys.executable, str(Path(__file__).with_name("http_fixture_guard.py")), sys.argv[2], base], check=True)
    opener = urllib.request.build_opener(
        urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar())
    )

    def get(path: str) -> tuple[str, str]:
        with opener.open(base + path) as response:
            return response.geturl(), response.read().decode("utf-8")

    def post(path: str, fields: dict[str, str]) -> tuple[str, str]:
        body = urllib.parse.urlencode(fields).encode("utf-8")
        with opener.open(base + path, body) as response:
            return response.geturl(), response.read().decode("utf-8")

    _, signup_page = get("/TaiKhoan/DangKy")
    post("/TaiKhoan/DangKy", {
        "__RequestVerificationToken": token(signup_page),
        "TenDangNhap": username,
        "MatKhau": password,
        "NhapLaiMatKhau": password,
        "BietDanh": "Smoke-C",
        "Lop": "7",
        "DongYQuyenRiengTu": "true",
    })
    profile_url, profile = get("/HocTap/HoSo")
    assert profile_url.endswith("/HocTap/HoSo"), "registration did not reach profile"
    assert 'value="Smoke-C"' in profile, "new profile was not persisted"

    container = sys.argv[2] if len(sys.argv) > 2 else None
    victim = "smoke_v_" + uuid.uuid4().hex[:8]

    def graph(query: str) -> str:
        return subprocess.run(["docker", "exec", container, "cypher-shell", "-u", "neo4j", "-p",
                               os.environ["GEOQUAD_US03_PASSWORD"], "--format", "plain", query],
                              check=True, capture_output=True, text=True, encoding="utf-8").stdout

    if container:
        # CSRF: profile POSTs without the antiforgery token are rejected and change nothing.
        for path, fields in (("/HocTap/HoSo/DoiThongTin", {"DoiThongTin.BietDanh": "Hacked", "DoiThongTin.Lop": "12"}),
                             ("/HocTap/HoSo/DoiMatKhau", {"DoiMatKhau.MatKhauCu": password, "DoiMatKhau.MatKhauMoi": "Hacked123",
                                                          "DoiMatKhau.XacNhanMatKhau": "Hacked123"}),
                             ("/HocTap/HoSo/XoaTaiKhoan", {"XoaTaiKhoan.MatKhauXacNhan": password})):
            try:
                post(path, fields)
                raise AssertionError(f"{path} accepted a POST without antiforgery token")
            except urllib.error.HTTPError as error:
                assert error.code == 400, f"{path} returned {error.code} without antiforgery token"
        state = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}})-[:HOC_LOP]->(l) RETURN u.bietDanh AS b, l.so AS lop")
        assert "Smoke-C" in state and "Hacked" not in state and "12" not in state, f"CSRF-less POST changed data: {state}"

        # Cross-account: a forged id in the form must not touch another account (id comes from the auth cookie).
        victim_client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
        with victim_client.open(base + "/TaiKhoan/DangKy") as r:
            victim_page = r.read().decode("utf-8")
        victim_client.open(base + "/TaiKhoan/DangKy", urllib.parse.urlencode({
            "__RequestVerificationToken": token(victim_page), "TenDangNhap": victim, "MatKhau": password,
            "NhapLaiMatKhau": password, "BietDanh": "Victim", "Lop": "5", "DongYQuyenRiengTu": "true"}).encode("utf-8")).close()
        victim_id = re.search(r'"([0-9a-f-]{36})"', graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{victim}'}}) RETURN u.id AS id")).group(1)
        _, profile = get("/HocTap/HoSo")
        post("/HocTap/HoSo/DoiThongTin", {"__RequestVerificationToken": token(profile), "Id": victim_id,
             "TaiKhoanId": victim_id, "DoiThongTin.BietDanh": "Smoke-C", "DoiThongTin.Lop": "7"})
        victim_state = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{victim}'}})-[:HOC_LOP]->(l) RETURN u.bietDanh AS b, l.so AS lop")
        assert "Victim" in victim_state and "5" in victim_state, f"forged id changed another account: {victim_state}"
        _, profile = get("/HocTap/HoSo")

    target_url, profile = post("/HocTap/HoSo/DoiThongTin", {
        "__RequestVerificationToken": token(profile),
        "DoiThongTin.BietDanh": "Smoke-C-8",
        "DoiThongTin.Lop": "8",
    })
    assert target_url.endswith("/HocTap/HoSo"), "class update did not redirect"
    assert 'value="Smoke-C-8"' in profile, "display name was not persisted"
    assert re.search(r"Lớp\s+8\s+·", profile), "class 8 was not shown"

    wrong_url, wrong_page = post("/HocTap/HoSo/DoiMatKhau", {
        "__RequestVerificationToken": token(profile),
        "DoiMatKhau.MatKhauCu": "wrong-password",
        "DoiMatKhau.MatKhauMoi": new_password,
        "DoiMatKhau.XacNhanMatKhau": new_password,
    })
    assert wrong_url.endswith("/DoiMatKhau"), "wrong old password was accepted"
    assert "không chính xác" in html.unescape(wrong_page), "missing old-password error"

    _, profile = get("/HocTap/HoSo")
    change_url, profile = post("/HocTap/HoSo/DoiMatKhau", {
        "__RequestVerificationToken": token(profile),
        "DoiMatKhau.MatKhauCu": password,
        "DoiMatKhau.MatKhauMoi": new_password,
        "DoiMatKhau.XacNhanMatKhau": new_password,
    })
    assert change_url.endswith("/HocTap/HoSo"), "valid password change failed"

    wrong_url, _ = post("/HocTap/HoSo/XoaTaiKhoan", {
        "__RequestVerificationToken": token(profile),
        "XoaTaiKhoan.MatKhauXacNhan": password,
    })
    assert wrong_url.endswith("/XoaTaiKhoan"), "old password deleted account"

    if container:
        # Learning history that account deletion must remove with the node (NFR-06).
        graph(f"""MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}}), (b:BaiTap {{ma:'BT-014'}}), (k:KhaiNiem {{ma:'HINH_THOI'}})
                  CREATE (u)-[:DA_LAM {{maLan:'SMOKE-US08-{username}', dung:true, luc:datetime()}}]->(b)
                  MERGE (u)-[:DA_HOC {{luc:datetime()}}]->(k)""")

    _, profile = get("/HocTap/HoSo")
    delete_url, _ = post("/HocTap/HoSo/XoaTaiKhoan", {
        "__RequestVerificationToken": token(profile),
        "XoaTaiKhoan.MatKhauXacNhan": new_password,
    })
    assert delete_url.rstrip("/") == base, "account deletion did not redirect home"
    after_url, _ = get("/HocTap/HoSo")
    assert "/TaiKhoan/DangNhap" in after_url, "deleted account remained signed in"

    if container:
        left = graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}}) RETURN count(u) AS n")
        orphan = graph(f"MATCH ()-[d:DA_LAM {{maLan:'SMOKE-US08-{username}'}}]->() RETURN count(d) AS n")
        assert re.search(r"\b0\b", left) and re.search(r"\b0\b", orphan), "deleted account or its history remained"
        graph(f"MATCH (u:TaiKhoan {{tenDangNhap:'{victim}'}}) DETACH DELETE u")
        print(f"US-08 HTTP profile/password/delete: PASS ({username}; CSRF 400 x3, cross-account forged id ignored, history removed)")
    else:
        print(f"US-08 HTTP profile/password/delete: PASS ({username}; DB checks skipped, no container)")


if __name__ == "__main__":
    main()
