#!/usr/bin/env bash
# Helpers owned by B. Manifest excludes account identifiers/password hashes.
gq_die() { printf '%s\n' "$*" >&2; exit 1; }
gq_hash() {
  if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | cut -d ' ' -f 1
  else shasum -a 256 "$1" | cut -d ' ' -f 1; fi
}
gq_wait() {
  local cid="$1" state tries=0
  while (( tries < 90 )); do
    state=$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$cid") || return 1
    [[ "$state" == healthy ]] && return 0
    [[ "$state" == exited || "$state" == dead || "$state" == unhealthy ]] && return 1
    sleep 2; tries=$((tries+1))
  done
  return 1
}
gq_query() {
  docker exec -e "NEO4J_USERNAME=$gq_user" -e "NEO4J_PASSWORD=$gq_password" "$1" \
    cypher-shell --access-mode read --format plain --history in-memory --fail-fast "$2"
}
gq_manifest() {
  local cid="$1" dir="$2" prefix="$3"
  gq_query "$cid" 'CALL db.awaitIndexes(60)' >/dev/null
  gq_query "$cid" 'RETURN COUNT { () } AS nodes,COUNT { ()-[]->() } AS edges,COUNT { (:TaiKhoan) } AS accounts,COUNT { ()-[:DA_LAM]->() } AS histories' > "$dir/$prefix-manifest.txt"
  gq_query "$cid" 'SHOW CONSTRAINTS YIELD name,type,labelsOrTypes,properties RETURN name,type,labelsOrTypes,properties ORDER BY name' >> "$dir/$prefix-manifest.txt"
  gq_query "$cid" 'SHOW INDEXES YIELD name,type,labelsOrTypes,properties RETURN name,type,labelsOrTypes,properties ORDER BY name' >> "$dir/$prefix-manifest.txt"
  gq_query "$cid" "MATCH (n) WHERE n:KhaiNiem OR n:DinhLy OR n:CongThuc OR n:DieuKien OR n:ChungMinh RETURN n.ma AS ma,labels(n) AS labels,properties(n) AS content ORDER BY ma" >> "$dir/$prefix-manifest.txt"
  # No account id/username/hash in the retained manifest; preserve history via its digest.
  gq_query "$cid" 'MATCH ()-[r:DA_LAM]->() RETURN properties(r) AS history' > "$dir/$prefix-history.tmp"
  LC_ALL=C sort "$dir/$prefix-history.tmp" -o "$dir/$prefix-history.tmp"
  printf 'history-sha256=%s\n' "$(gq_hash "$dir/$prefix-history.tmp")" >> "$dir/$prefix-manifest.txt"
  rm -- "$dir/$prefix-history.tmp"
  chmod 600 "$dir/$prefix-manifest.txt"
}
