"""Cross-check US-03 README/SRS tables and a disposable Neo4j container.

Usage: set GEOQUAD_US03_PASSWORD, then run
  python tests/GeoQuad.Tests/C_HocTap/us03_review.py <container-name>
No database is created or modified by this script.
"""

from __future__ import annotations

import csv
import os
import re
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[3]
EXPECTED_COUNTS = {3: 14, 4: 20, 5: 12, 6: 12, 7: 6}
errors: list[str] = []


def check(condition: bool, message: str) -> None:
    if not condition:
        errors.append(message)


def table(path: Path, section: str) -> dict[str, list[str]]:
    text = path.read_text(encoding="utf-8")
    match = re.search(rf"^#+ {re.escape(section)}\b.*$", text, re.MULTILINE)
    if not match:
        raise ValueError(f"missing section {section} in {path}")
    rows: dict[str, list[str]] = {}
    started = False
    for line in text[match.end():].splitlines():
        if line.startswith("|"):
            cells = [cell.strip().replace("**", "").strip("`") for cell in line.strip().strip("|").split("|")]
            if not cells[0].startswith(("-", "Mã", "ma")):
                check(cells[0] not in rows, f"{section}: duplicate {cells[0]}")
                rows[cells[0]] = cells
            started = True
        elif started:
            break
    return rows


def graph(container: str, password: str, query: str) -> list[dict[str, str]]:
    result = subprocess.run(
        ["docker", "exec", container, "cypher-shell", "--format", "plain",
         "-u", "neo4j", "-p", password, query],
        capture_output=True, text=True, encoding="utf-8", check=True,
    )
    return list(csv.DictReader(result.stdout.splitlines(), skipinitialspace=True))


QUERIES = {
    3: "MATCH (n:DieuKien)-[:THUOC_LOP]->(l:Lop) RETURN n.ma AS ma,n.noiDung AS noiDung,l.so AS lop,n.nguon AS nguon,n.trangThai AS trangThai ORDER BY ma",
    4: "MATCH (n:DauHieu)-[:YEU_CAU_LA]->(a:KhaiNiem),(n)-[:YEU_CAU_CO]->(b:DieuKien),(n)-[:KHANG_DINH]->(c:KhaiNiem),(n)-[:THUOC_LOP]->(l:Lop) RETURN n.ma AS ma,n.noiDung AS noiDung,a.ma AS nen,b.ma AS dk,c.ma AS dich,l.so AS lop,n.nguon AS nguon,n.trangThai AS trangThai ORDER BY ma",
    5: "MATCH (h:KhaiNiem)-[:CO_TINH_CHAT]->(n:TinhChat)-[:THUOC_LOP]->(l:Lop) RETURN n.ma AS ma,n.noiDung AS noiDung,h.ma AS hinh,l.so AS lop,n.nguon AS nguon,n.trangThai AS trangThai ORDER BY ma",
    6: "MATCH (h:KhaiNiem)-[:CO_CONG_THUC]->(n:CongThuc)-[:THUOC_LOP]->(l:Lop) RETURN n.ma AS ma,n.ten AS ten,n.bieuThuc AS bieuThuc,h.ma AS hinh,l.so AS lop,n.nguon AS nguon,n.trangThai AS trangThai ORDER BY ma",
    7: "MATCH (n:DinhLy)-[:THUOC_LOP]->(l:Lop) WHERE NOT n:DauHieu AND NOT n:TinhChat RETURN n.ma AS ma,n.noiDung AS noiDung,l.so AS lop,n.nguon AS nguon,n.trangThai AS trangThai ORDER BY ma",
}


def unquote(value: str) -> str:
    return value.strip().strip('"')


def review(container: str, password: str) -> None:
    for number, expected_count in EXPECTED_COUNTS.items():
        readme = table(ROOT / "README.md", f"D.{number}")
        srs = table(ROOT / "docs/GeoQuad_SRS.md", f"B.{number}")
        actual_rows = graph(container, password, QUERIES[number])
        actual: dict[str, dict[str, str]] = {}
        for raw in actual_rows:
            row = {key.strip(): unquote(value) for key, value in raw.items()}
            ma = row["ma"]
            check(ma not in actual, f"D.{number}: graph duplicate {ma}")
            actual[ma] = row

        check(len(readme) == expected_count, f"D.{number}: README count {len(readme)}")
        check(len(srs) == expected_count, f"B.{number}: SRS count {len(srs)}")
        check(set(readme) == set(srs), f"D.{number}: ID sets differ README/SRS")
        missing = sorted(set(readme) - set(actual))
        check(not missing, f"D.{number}: appendix IDs missing in graph: {missing}")
        # Seed may add reviewed items beyond the appendix (A did in US-03); list them for manual review.
        extra = sorted(set(actual) - set(readme))
        for ma in extra:
            g = actual[ma]
            check(bool(g["nguon"]) and g["nguon"] != "null", f"{ma}: missing source (extra item)")
            check(g["trangThai"] in {"NHAP", "DA_RA_SOAT"}, f"{ma}: invalid status (extra item)")
        if extra:
            print(f"D.{number}: {len(extra)} graph items beyond appendix, review manually: {', '.join(extra)}")
        for ma in sorted(set(readme) & set(srs) & set(actual)):
            d, b, g = readme[ma], srs[ma], actual[ma]
            content_index = 2 if number in (5, 6) else 1
            graph_key = "ten" if number == 6 else "noiDung"
            check(d[content_index] == b[content_index] == g[graph_key],
                  f"{ma}: text/title differs README/SRS/graph")
            if number >= 4:
                check(d[-1] == b[-1] == g["lop"], f"{ma}: class differs")
            if number == 4:
                check((d[2], d[3], d[4]) == (g["nen"], g["dk"], g["dich"]),
                      f"{ma}: sign relationships differ")
            if number in (5, 6):
                check(d[1] == g["hinh"], f"{ma}: shape relationship differs")
            check(bool(g["nguon"]) and g["nguon"] != "null", f"{ma}: missing source")
            check(g["trangThai"] in {"NHAP", "DA_RA_SOAT"}, f"{ma}: invalid status")
        print(f"D.{number}/B.{number}: README={len(readme)}, SRS={len(srs)}, Neo4j={len(actual)}")


if __name__ == "__main__":
    if len(sys.argv) != 2 or not os.getenv("GEOQUAD_US03_PASSWORD"):
        sys.exit("usage: set GEOQUAD_US03_PASSWORD and pass disposable container name")
    review(sys.argv[1], os.environ["GEOQUAD_US03_PASSWORD"])
    if errors:
        print("\n".join(errors), file=sys.stderr)
        sys.exit(1)
    print("US-03 appendix and graph contract: PASS")
