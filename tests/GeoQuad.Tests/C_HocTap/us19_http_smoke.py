"""US-19 acceptance on a disposable Neo4j instance and its local web app.

Set GEOQUAD_US03_PASSWORD (DB) and GEOQUAD_US08_PASSWORD (temporary student).
Usage: python us19_http_smoke.py <disposable-container> <local-http-url>
Creates only uniquely named fixtures; cleans them up in finally.
"""
import html
import http.cookiejar
import os
import re
import subprocess
import sys
import subprocess
import urllib.parse
import urllib.request
import uuid

from us08_http_smoke import token


def main():
    container, base = sys.argv[1], sys.argv[2].rstrip('/')
    subprocess.run([sys.executable, str(__import__('pathlib').Path(__file__).with_name('http_fixture_guard.py')), container, base], check=True)
    password = os.environ['GEOQUAD_US08_PASSWORD']
    db_password = os.environ['GEOQUAD_US03_PASSWORD']
    marker = 'US19_' + uuid.uuid4().hex[:8].upper()
    username = 'us19_' + uuid.uuid4().hex[:8]
    opener = urllib.request.build_opener(
        urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

    def graph(query):
        return subprocess.run(['docker', 'exec', container, 'cypher-shell',
            '--format', 'plain', '-u', 'neo4j', '-p', db_password, query],
            capture_output=True, text=True, encoding='utf-8', check=True).stdout

    def get(path):
        with opener.open(base + path) as response:
            return html.unescape(response.read().decode('utf-8'))

    def post(path, fields):
        with opener.open(base + path, urllib.parse.urlencode(fields).encode()) as response:
            return html.unescape(response.read().decode('utf-8'))

    try:
        # Distinguishable rows exercise every public gate without changing seed data.
        graph(f"""UNWIND [
          {{suffix:'VALID', lop:8, visible:true, state:'DA_RA_SOAT'}},
          {{suffix:'HIGH', lop:9, visible:true, state:'DA_RA_SOAT'}},
          {{suffix:'HIDDEN', lop:8, visible:false, state:'DA_RA_SOAT'}},
          {{suffix:'DRAFT', lop:8, visible:true, state:'NHAP'}}
        ] AS row
        CREATE (b:BaiTap {{ma:'{marker}_'+row.suffix, de:'Acceptance fixture',
          loai:'DAP_AN_SO', doKho:2, hienThi:row.visible, trangThai:row.state,
          smokeMarker:'{marker}'}})
        WITH b,row MATCH (l:Lop {{so:row.lop}}),
          (k:KhaiNiem {{ma:'HINH_THOI'}}), (k2:KhaiNiem {{ma:'HINH_BINH_HANH'}})
        CREATE (b)-[:THUOC_LOP]->(l), (b)-[:LIEN_QUAN_DEN]->(k),
          (b)-[:LIEN_QUAN_DEN]->(k2)""")
        home = get('/')
        post('/Home/ChonLop', {'__RequestVerificationToken': token(home), 'lop':'8'})
        page = get('/HocTap/BaiTap?cap=CAP_2&lop=8&loai=DAP_AN_SO&doKho=2&khaiNiem=HINH_THOI')
        assert page.count(marker + '_VALID') == 2, 'Expected one card and one link'
        for suffix in ('HIGH', 'HIDDEN', 'DRAFT'):
            assert marker + '_' + suffix not in page, suffix + ' leaked'
        assert 'Đã làm:' not in page, 'Guest saw account history'
        assert 'BT-014' in page, 'Combined seed filter missed BT-014'
        assert 'không phù hợp' in get('/HocTap/BaiTap?lop=9'), 'URL class override accepted'
        assert 'Bộ lọc không hợp lệ' in get('/HocTap/BaiTap?lop=abc'), 'Malformed class widened filter'
        assert 'Bộ lọc không hợp lệ' in get('/HocTap/BaiTap?doKho=abc'), 'Malformed difficulty widened filter'
        # BR-03: contradictory level/grade explains the problem instead of an unexplained empty list.
        contradictory = get('/HocTap/BaiTap?cap=CAP_1&lop=8')
        assert 'Lớp 8 không thuộc Cấp 1' in contradictory and 'BT-014' not in contradictory
        assert marker not in get('/HocTap/BaiTap?khaiNiem=%27%29%20RETURN%201'), 'Invalid filter accepted'

        signup = get('/TaiKhoan/DangKy')
        post('/TaiKhoan/DangKy', {'__RequestVerificationToken': token(signup),
            'TenDangNhap':username, 'MatKhau':password, 'NhapLaiMatKhau':password,
            'BietDanh':'US19 test', 'Lop':'8', 'DongYQuyenRiengTu':'true'})
        graph(f"""MATCH (u:TaiKhoan {{tenDangNhap:'{username}'}}),
          (b:BaiTap {{ma:'{marker}_VALID'}})
          CREATE (u)-[:DA_LAM {{luc:datetime(), dung:false}}]->(b),
            (u)-[:DA_LAM {{luc:datetime(), dung:true}}]->(b)""")
        page = get('/HocTap/BaiTap?cap=CAP_2&lop=8&loai=DAP_AN_SO&doKho=2&khaiNiem=HINH_THOI')
        assert re.search(r'Đã làm:\s*2 lần\s*·\s*Đã đúng', page), 'History not aggregated correctly'
        assert page.count(marker + '_VALID') == 2, 'Multiple concept links duplicated card'
        page = get('/HocTap/BaiTap')
        assert marker + '_HIGH' not in page, 'Student saw class 9'
        post('/Home/NangCao', {'__RequestVerificationToken':token(page), 'bat':'true'})
        page = get('/HocTap/BaiTap')
        assert marker + '_HIGH' in page and 'Nâng cao' in page, 'Advanced preview missing'
        print('US-19 HTTP/Neo4j: PASS (combined filter, visibility/review/class, guest, history, preview)')
    finally:
        graph(f"MATCH (n) WHERE n.smokeMarker='{marker}' OR "
              f"(n:TaiKhoan AND n.tenDangNhap='{username}') DETACH DELETE n")


if __name__ == '__main__':
    main()
