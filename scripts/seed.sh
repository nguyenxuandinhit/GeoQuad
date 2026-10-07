#!/usr/bin/env bash
# Nạp dữ liệu seed vào Neo4j (US-01).
# Chạy lần lượt mọi file neo4j/seed/*.cypher theo thứ tự tên bằng cypher-shell trong container neo4j.
# Seed dùng MERGE nên chạy lại nhiều lần không sinh trùng (NFR-08).
# Dùng: ./scripts/seed.sh [--dev]
set -euo pipefail

# Git Bash / MSYS trên Windows tự đổi "/seed/..." thành đường dẫn Windows.
# Tắt chuyển đổi để cypher-shell nhận đúng đường dẫn trong container.
export MSYS_NO_PATHCONV=1
export MSYS2_ARG_CONV_EXCL='*'

GOC="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$GOC"

NGUOI_DUNG='neo4j'
MAT_KHAU='geoquad123'
DEV=0
for arg in "$@"; do
  case "$arg" in
    --dev) DEV=1 ;;
    *) echo "Tham so khong biet: $arg (chi ho tro --dev)" >&2; exit 2 ;;
  esac
done

chay_thu_muc() {
  local thu_muc_may="$1"
  local thu_muc_container="$2"

  if [ ! -d "$thu_muc_may" ]; then
    echo "  (bo qua) Khong tim thay thu muc $thu_muc_may"
    return 0
  fi

  # Doc het danh sach file vao mang truoc khi chay, de "docker compose exec -T"
  # khong doc mat phan stdin con lai.
  local files=()
  local f
  while IFS= read -r f; do
    files+=("$f")
  done < <(find "$thu_muc_may" -maxdepth 1 -name '*.cypher' -type f | LC_ALL=C sort)

  if [ "${#files[@]}" -eq 0 ]; then
    echo "  (bo qua) Khong co file .cypher trong $thu_muc_may"
    return 0
  fi

  local ten
  for f in "${files[@]}"; do
    ten="$(basename "$f")"
    echo "==> $ten"
    if ! docker compose exec -T neo4j cypher-shell -u "$NGUOI_DUNG" -p "$MAT_KHAU"          --format plain -f "$thu_muc_container/$ten" </dev/null; then
      echo "LOI khi chay $ten. Dung lai." >&2
      exit 1
    fi
  done
}

echo 'Kiem tra Neo4j da san sang...'
if ! docker compose exec -T neo4j cypher-shell -u "$NGUOI_DUNG" -p "$MAT_KHAU" \
     --format plain 'RETURN 1 AS ok' </dev/null >/dev/null; then
  echo 'Khong ket noi duoc Neo4j. Chay truoc: docker compose up -d neo4j' >&2
  exit 1
fi

chay_thu_muc 'neo4j/seed' '/seed'

if [ "$DEV" -eq 1 ]; then
  echo 'Nap them du lieu dev...'
  chay_thu_muc 'neo4j/dev' '/dev-seed'
fi

echo 'Seed xong.'
