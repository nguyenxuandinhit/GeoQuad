#!/usr/bin/env bash
set -euo pipefail
umask 077
script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
root=$(cd "$script_dir/.." && pwd -P)
source "$script_dir/backup-common.sh"
project=geoquad; compose_file="$root/docker-compose.yml"; output=""; quiesced=0
while (( $# )); do
  case "$1" in
    --project) (( $# >= 2 )) || gq_die 'Thiếu project'; project="$2"; shift 2 ;;
    --compose) (( $# >= 2 )) || gq_die 'Thiếu compose'; compose_file="$2"; shift 2 ;;
    --output) (( $# >= 2 )) || gq_die 'Thiếu output'; output="$2"; shift 2 ;;
    --quiesced) quiesced=1; shift ;;
    *) gq_die "Tham số không hợp lệ: $1" ;;
  esac
done
[[ "$project" =~ ^[a-z0-9][a-z0-9_-]{0,50}$ ]] || gq_die 'Project không hợp lệ'
(( quiesced == 1 )) || gq_die 'Cần --quiesced: xác nhận đã ngăn writer ngoài web (Browser/seed/job) cho đến khi backup kết thúc.'
[[ -f "$compose_file" ]] || gq_die 'Không tìm thấy Compose'
compose=(docker compose -p "$project" -f "$compose_file")
cid=$("${compose[@]}" ps -a -q neo4j)
[[ -n "$cid" && "$cid" != *$'\n'* ]] || gq_die 'Cần đúng một container Neo4j nguồn'
[[ $(docker inspect --format '{{ index .Config.Labels "com.docker.compose.project" }}' "$cid") == "$project" ]] || gq_die 'Sai project nguồn'
image=$(docker inspect --format '{{.Image}}' "$cid")
volume=$(docker inspect --format '{{range .Mounts}}{{if eq .Destination "/data"}}{{if eq .Type "volume"}}{{.Name}}{{end}}{{end}}{{end}}' "$cid")
[[ -n "$volume" ]] || gq_die 'Nguồn phải dùng named volume /data'
if [[ "$project" == geoquad-b-tests ]]; then
  [[ "$volume" == geoquad-b-tests_b-test-data && $(docker inspect --format '{{ index .Config.Labels "geoquad.test-isolation" }}' "$cid") == B-only ]] || gq_die 'Sai isolation test B'
fi
gq_user=${GQ_BACKUP_USER:-neo4j}; gq_password=${GQ_BACKUP_PASSWORD:-}
if [[ -z "$gq_password" ]]; then
  auth=$(docker inspect --format '{{range .Config.Env}}{{println .}}{{end}}' "$cid" | sed -n 's/^NEO4J_AUTH=//p')
  [[ "$auth" == */* ]] || gq_die 'Cần GQ_BACKUP_PASSWORD'
  gq_user=${auth%%/*}; gq_password=${auth#*/}; unset auth
fi
base=$(cd "$root/backups" && pwd -P)
run_id=$(date -u '+%Y%m%dT%H%M%SZ')-$$-$RANDOM
[[ -n "$output" ]] || output="$base/$run_id"
[[ "$output" == /* ]] || output="$root/$output"
[[ "$output" == "$base/"* ]] || gq_die 'Output phải nằm dưới backups của repository'
leaf=${output#"$base/"}
[[ "$leaf" =~ ^[A-Za-z0-9_-]+$ && ! -e "$output" && ! -L "$output" ]] || gq_die 'Output phải là thư mục mới, không traversal/symlink'
mkdir -m 700 "$output"
log="$output/backup.log"; touch "$log"; chmod 600 "$log"
uid=$(id -u); gid=$(id -g)
# Check Docker can access the private directory before taking the database offline.
docker run --rm --pull never --network none --user "$uid:$gid" -v "$output:/backup" --entrypoint sh "$image" -c 'test -w /backup' >> "$log" 2>&1
neo_running=$(docker inspect --format '{{.State.Running}}' "$cid")
web_id=""; web_running=false
services=$("${compose[@]}" config --services)
if [[ $'\n'"$services"$'\n' == *$'\nweb\n'* ]]; then
  web_id=$("${compose[@]}" ps -a -q web)
  [[ "$web_id" != *$'\n'* ]] || gq_die 'Chỉ hỗ trợ một web container'
  [[ -z "$web_id" ]] || web_running=$(docker inspect --format '{{.State.Running}}' "$web_id")
fi
helper="$project-backup-$run_id"; recovered=0
recover() {
  if [[ $(docker inspect --format '{{ index .Config.Labels "geoquad.backup-run" }}' "$helper" 2>/dev/null || true) == "$run_id" ]]; then
    docker rm -f "$helper" >> "$log" 2>&1 || return 1
  fi
  if [[ "$neo_running" == true ]]; then
    docker start "$cid" >> "$log" 2>&1 || return 1
    gq_wait "$cid" || return 1
  else docker stop "$cid" >> "$log" 2>&1 || return 1; fi
  if [[ "$web_running" == true ]]; then docker start "$web_id" >> "$log" 2>&1 || return 1; fi
  recovered=1
}
finish() {
  local code=$?
  trap - EXIT INT TERM
  if (( recovered == 0 )); then recover || code=1; fi
  (( code == 0 )) || printf 'Backup không hoàn tất; xem log riêng tư: %s\n' "$log" >&2
  exit "$code"
}
trap finish EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
if [[ "$web_running" == true ]]; then docker stop "$web_id" >> "$log" 2>&1; fi
if [[ "$neo_running" != true ]]; then docker start "$cid" >> "$log" 2>&1; fi
gq_wait "$cid" || gq_die 'Neo4j nguồn không healthy'
printf 'Đang chờ transaction nguồn kết thúc.\n'
drained=0
for attempt in $(seq 1 30); do
  count=$(gq_query "$cid" "SHOW TRANSACTIONS YIELD database WHERE database='neo4j' RETURN count(*) AS n" | tail -n 1 | tr -d '[:space:]')
  [[ "$count" =~ ^[0-9]+$ ]] || gq_die 'Không đọc được trạng thái transaction'
  if (( count <= 1 )); then drained=1; break; fi
  sleep 1
done
(( drained == 1 )) || gq_die 'Nguồn vẫn có transaction ngoài maintenance'
gq_manifest "$cid" "$output" source
gq_query "$cid" 'CALL dbms.components() YIELD versions RETURN versions[0] AS version' > "$output/version.txt"
printf '%s\n' "$image" > "$output/image.txt"
docker stop "$cid" >> "$log" 2>&1
[[ $(docker inspect --format '{{.State.Running}}' "$cid") == false ]] || gq_die 'Không dump DB đang online'
docker run --rm --pull never --name "$helper" --label "geoquad.backup-run=$run_id" --network none --user 0:0 \
  -v "$volume:/data" -v "$output:/backup" --entrypoint neo4j-admin "$image" database dump neo4j --to-path=/backup >> "$log" 2>&1
[[ -s "$output/neo4j.dump" ]] || gq_die 'Không có archive sau dump'
docker run --rm --pull never --network none --user 0:0 -v "$output:/backup" --entrypoint chown "$image" "$uid:$gid" /backup/neo4j.dump >> "$log" 2>&1
chmod 600 "$output/neo4j.dump"
[[ -O "$output/neo4j.dump" ]] || gq_die 'Archive không thuộc người chạy'
gq_hash "$output/neo4j.dump" > "$output/sha256.txt"
chmod 600 "$output"/*
recover || gq_die 'Không khôi phục được trạng thái dịch vụ nguồn; xem backup.log'
printf 'ARCHIVE=%s/neo4j.dump\n' "$output"
