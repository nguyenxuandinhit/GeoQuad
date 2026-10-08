"""Static US-04 C contract checks. Run with Python 3; no packages required."""

from __future__ import annotations
import ast
import re
from pathlib import Path

root = Path(__file__).resolve().parents[3]
seed = (root / 'neo4j/seed/30-baitap-C.cypher').read_text(encoding='utf-8')
dev = (root / 'neo4j/dev/31-lam-bai-mau-C.cypher').read_text(encoding='utf-8')
errors: list[str] = []

def check(condition: bool, message: str) -> None:
    if not condition:
        errors.append(message)

def field(block: str, name: str) -> str | None:
    match = re.search(r'\b' + re.escape(name) + r":'([^']*)'", block)
    return match.group(1) if match else None

def number(block: str, name: str) -> float | None:
    match = re.search(r'\b' + re.escape(name) + r':([0-9]+(?:\.[0-9]+)?)', block)
    return float(match.group(1)) if match else None

def array(block: str, name: str) -> list[str]:
    match = re.search(r'\b' + re.escape(name) + r':(\[[^\]]*\])', block)
    return ast.literal_eval(match.group(1)) if match else []

rows = {}
for match in re.finditer(r"\{ma:'(BT-\d{3})'", seed):
    start = match.start()
    following = re.search(r"\{ma:'BT-\d{3}'", seed[match.end():])
    end = match.end() + following.start() if following else seed.index('] AS row', match.end())
    block = seed[start:end]
    ma = match.group(1)
    check(ma not in rows, f'duplicate {ma}')
    rows[ma] = block

check(set(rows) == {f'BT-{n:03d}' for n in range(11, 31)}, 'BT-011..BT-030 coverage')
counts = {kind: sum(field(row, 'loai') == kind for row in rows.values()) for kind in ['TRAC_NGHIEM', 'DAP_AN_SO', 'CHUNG_MINH']}
check(counts['TRAC_NGHIEM'] >= 7, 'at least 7 MCQ')
check(counts['DAP_AN_SO'] >= 7, 'at least 7 numeric')
check(counts['CHUNG_MINH'] >= 3, 'at least 3 proof')
check(sum(field(row, 'cap') == 'CAP_2' and 6 <= number(row, 'lop') <= 9 for row in rows.values()) == 15, '15 lower-secondary exercises')
check(sum(field(row, 'cap') == 'CAP_3' and 10 <= number(row, 'lop') <= 12 for row in rows.values()) == 5, '5 upper-secondary exercises')
check(sum('HINH_THOI' in array(row, 'khaiNiem') for row in rows.values()) >= 5, 'at least 5 rhombus exercises')
for ma, row in rows.items():
    kind = field(row, 'loai')
    check(bool(field(row, 'de')), f'{ma}: prompt')
    check(1 <= number(row, 'doKho') <= 3, f'{ma}: difficulty')
    check(bool(array(row, 'khaiNiem')), f'{ma}: concept')
    check(bool(array(row, 'dinhLy') or array(row, 'congThuc')), f'{ma}: knowledge relation')
    if kind == 'TRAC_NGHIEM':
        options = array(row, 'phuongAn')
        check(len(options) == 4 and len(set(options)) == 4, f'{ma}: 4 distinct options')
        check(field(row, 'dapAnDung') in 'ABCD', f'{ma}: A-D key')
    elif kind == 'DAP_AN_SO':
        check(number(row, 'dapAnSo') is not None, f'{ma}: numeric answer')
        check(number(row, 'saiSo') == 0.01, f'{ma}: tolerance')
        check(field(row, 'donVi') is not None, f'{ma}: unit field')
        check(bool(field(row, 'giaiThich')), f'{ma}: explanation')
    elif kind == 'CHUNG_MINH':
        check(bool(field(row, 'loiGiaiMau')), f'{ma}: sample proof')

expected_numeric = {
    'BT-013': 4*7, 'BT-014': 12*16/2, 'BT-015': round(5*2**0.5, 2),
    'BT-017': 12*7, 'BT-019': (8+14)*5/2, 'BT-020': 4*6,
    'BT-023': 2*((13**2-(10/2)**2)**0.5), 'BT-025': 180-112,
    'BT-028': 5+2-1, 'BT-030': abs(3*4-0*1),
}
for ma, value in expected_numeric.items():
    check(number(rows[ma], 'dapAnSo') == value, f'{ma}: arithmetic should be {value}')

# Evaluate the four BT-026 vector equalities using one generic parallelogram.
points = {'A': (0, 0), 'B': (2, 0), 'D': (1, 1), 'C': (3, 1)}
def vec(name: str) -> tuple[int, int]:
    x, y = points[name[0]], points[name[1]]
    return (y[0]-x[0], y[1]-x[1])
correct = []
for i, option in enumerate(array(rows['BT-026'], 'phuongAn')):
    match = re.fullmatch(r'Vectơ ([A-D]{2}) = vectơ ([A-D]{2})', option)
    check(match is not None, f'BT-026 option {i+1}: parse vector equality')
    if match and vec(match.group(1)) == vec(match.group(2)):
        correct.append('ABCD'[i])
check(correct == [field(rows['BT-026'], 'dapAnDung')], f'BT-026: exactly one correct option; actual {correct}')

# B + D - A determines the missing vertex in BT-024.
bt024_options = [ast.literal_eval(x) for x in array(rows['BT-024'], 'phuongAn')]
bt024_answer = (4 + 2 - 1, 1 + 3 - 1)
bt024_correct = ['ABCD'[i] for i, value in enumerate(bt024_options) if value == bt024_answer]
check(bt024_correct == [field(rows['BT-024'], 'dapAnDung')], f'BT-024: unique coordinate answer; actual {bt024_correct}')

check('MATCH (tk:TaiKhoan {tenDangNhap:' in dev, 'dev seed must locate existing demo account by username')
check("MERGE (tk:TaiKhoan {id:'hocsinh8'})" not in dev, 'dev seed must not create fake account id')
check('MATCH (l:Lop {so: row.lop})' in seed, 'C seed must MATCH existing class')
check('MERGE (k:KhaiNiem' not in seed, 'C seed must not create concept placeholders')
check('MERGE (tk)-[d:DA_LAM {maLan:row.maLan}]->(b)' in dev, 'dev rerun must be idempotent')

sample = []
for m in re.finditer(r"\{maLan:'([^']+)', baiTap:'([^']+)', luc:'([^']+)', dung:(true|false), dapAn:'([^']*)'\}", dev):
    ma_lan, ma_bai, luc, dung, answer = m.groups()
    sample.append((ma_lan, ma_bai, luc, dung == 'true', answer))
check(len(sample) == 7 and len({x[0] for x in sample}) == 7, 'seven unique demo attempts')
for ma_lan, ma_bai, _, correct_flag, answer in sample:
    if correct_flag:
        row = rows[ma_bai]
        expected = field(row, 'dapAnDung') if field(row, 'loai') == 'TRAC_NGHIEM' else number(row, 'dapAnSo')
        actual = answer if isinstance(expected, str) else float(answer)
        check(actual == expected, f'{ma_lan}: marked correct but submitted {answer} vs {expected}')
rhombus = [x for x in sample if 'HINH_THOI' in array(rows[x[1]], 'khaiNiem')]
check(len(rhombus) == 5 and sum(x[3] for x in rhombus) == 2, 'last five rhombus attempts: 2 correct')

if errors:
    print('FAILED (' + str(len(errors)) + '):')
    for e in errors: print('- ' + e)
    raise SystemExit(1)
print(f'PASS: {len(rows)} exercises; {counts}; {len(rhombus)} rhombus attempts')

