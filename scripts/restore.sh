#!/usr/bin/env bash
set -euo pipefail
umask 077
script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
root=$(cd "$script_dir/.." && pwd -P)
source "$script_dir/backup-common.sh"
archive=""; target="geoquad-b-restore-$(date -u '+%Y%m%d%H%M%S')-$$"
while (( $# )); do
  case "$1" in
    --archive) (( $# >= 2 )) || gq_die 'Thiếu archive'; archive="$2"; shift 2 ;;
    --target-project) (( $# >= 2 )) || gq_die 'Thiếu target'; target="$2"; shift 2 ;;
    *) gq_die "Tham số không hợp lệ: $1" ;;
  esac
done
[[ "$target" =~ ^geoquad-b-restore-[a-z0-9_-]{1,45}$ ]] || gq_die 'Restore chỉ tạo đích geoquad-b-restore-* mới, không ghi đè dev'
[[ -f "$archive" && ! -L "$archive" && $(basename "$archive") == neo4j.dump ]] || gq_die 'Archive phải là file neo4j.dump hiện có, không symlink'
dir=$(cd "$(dirname "$archive")" && pwd -P); archive="$dir/neo4j.dump"
base=$(cd "$root/backups" && pwd -P)
[[ "$dir" == "$base/"* ]] || gq_die 'Archive phải nằm trong backups của repository'
[[ -O "$archive" && -f "$dir/sha256.txt" && -f "$dir/image.txt" && -f "$dir/source-manifest.txt" ]] || gq_die 'Thiếu ownership/checksum/image/manifest'
expected=$(cat "$dir/sha256.txt")
[[ "$expected" =~ ^[a-f0-9]{64}$ && $(gq_hash "$archive") == "$expected" ]] || gq_die 'Checksum archive không hợp lệ'
image=$(cat "$dir/image.txt")
[[ "$image" =~ ^sha256:[a-f0-9]{64}$ ]] || gq_die 'Image ID không hợp lệ'
docker image inspect "$image" >/dev/null
volume="${target}_data"; container="$target-neo4j"
if docker volume inspect "$volume" >/dev/null 2>&1 || docker inspect "$container" >/dev/null 2>&1; then gq_die 'Đích đã tồn tại; không overwrite'; fi
gq_user=neo4j; gq_password=${GQ_RESTORE_PASSWORD:-geoquad_restore_123}
[[ -n "$gq_password" ]] || gq_die 'Cần mật khẩu Neo4j đích'
log="$dir/restore-$target.log"; touch "$log"; chmod 600 "$log"
docker volume create --label "geoquad.restore-run=$target" "$volume" >> "$log" 2>&1
docker run --rm --pull never --network none --user 0:0 -v "$volume:/data" -v "$dir:/backup:ro" \
  --entrypoint neo4j-admin "$image" database load neo4j --from-path=/backup >> "$log" 2>&1
docker run -d --pull never --name "$container" --label "geoquad.restore-run=$target" -v "$volume:/data" \
  -p 127.0.0.1:27687:7687 -p 127.0.0.1:27474:7474 -e "NEO4J_AUTH=neo4j/$gq_password" \
  -e NEO4J_server_memory_heap_initial__size=256m -e NEO4J_server_memory_heap_max__size=512m \
  --health-cmd 'NEO4J_USERNAME=neo4j NEO4J_PASSWORD="${NEO4J_AUTH#neo4j/}" cypher-shell --format plain "RETURN 1" >/dev/null' \
  --health-interval 5s --health-timeout 10s --health-retries 30 --health-start-period 30s "$image" >> "$log" 2>&1
gq_wait "$container" || gq_die "Restore không healthy; giữ container $container để kiểm tra"
gq_manifest "$container" "$dir" "$target"
cmp -s "$dir/source-manifest.txt" "$dir/$target-manifest.txt" || gq_die 'Manifest nguồn/đích không khớp; giữ đích để kiểm tra'
printf 'RESTORED=%s\nBOLT=bolt://127.0.0.1:27687\n' "$container"
# User database only. system/auth is newly initialized; source remains untouched.
