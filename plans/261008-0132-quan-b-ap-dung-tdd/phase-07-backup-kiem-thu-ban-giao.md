---
phase: 7
title: "US-26/27 — Backup, kiểm thử chéo và bàn giao"
status: pending
priority: P1
effort: "4h"
dependencies: [1, 2, 3, 4, 5, 6]
---

# Phase 7 — US-26/27: Backup, kiểm thử chéo và bàn giao

## Overview

Quân hoàn tất sub-task US-26 (104,107) và US-27 (111), điều phối US-27 parent109. Không nhận việc viết README của A hoặc demo của C. Phụ thuộc phase1–6, C đã triển khai đủ chức năng/bài tập và A có reviewer kiểm thử B. Không đóng story chung chỉ vì phần B đạt.

## Context, scout và requirements

- [Jira](../../docs/GeoQuad_Jira_2ngay.csv): B kiểm thử C, sửa B, script backup/restore; A kiểm thử B.
- [README §10](../../README.md), [SRS NFR-01..13 và nghiệm thu](../../docs/GeoQuad_SRS.md), [research backup](research/tdd-architecture-B.md).
- Deep scout trước code: đọc toàn bộ phase1–6 sau implementation, schema/constraints thực tế, version/image ID source, `docker compose config`, `neo4j-admin database dump/load --help`, seed/lịch sử C và docs của A/C.
- `neo4j:5-community` là tag có thể trôi; chỉ dùng cú pháp dump/load được help của image nguồn xác nhận. Dùng **cùng image ID/digest** để dump/load/drill, không tự kéo tag mới lúc recovery.
- Community yêu cầu database offline; dừng web trước Neo4j, không `compose exec` admin vào DB đang chạy. Không dùng API online backup Enterprise.

## File inventory và ownership

| Hành động | File | Cỡ dự kiến | Test impact |
|---|---|---|---|
| Tạo | `scripts/backup.sh`, `scripts/backup.ps1` | 90–150 dòng mỗi file | Offline dump, exitcode, finally/trap |
| Tạo | `scripts/restore.sh`, `scripts/restore.ps1` | 100–180 dòng mỗi file | Mặc định restore đích riêng, bảo vệ dev |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Integration/BackupRestoreTests.cs` | 120–180 dòng | Dump→load→manifest thật |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Integration/PerformanceBTests.cs` | 80–120 dòng | 100 concurrent, p95/error rate |
| Bổ sung | `docs/cypher/B-ap-dung.md` | 40–80 dòng | Hướng dẫn backup, query/checklist demo B |
| Bổ sung | `docs/kiem-thu.md` | 70–120 dòng | Chỉ mục kết quả B kiểm thử C và A kiểm thử B |
| Sửa theo lỗi có chứng cứ | File B thuộc phase1–6 | Theo lỗi | Viết regression trước sửa |

Không sửa docs/cypher C, seed C, README hoặc demo thay chủ sở hữu. File kiểm thử chung append mục có heading `Quân/B`; cập nhật trên bản mới nhất, không ghi đè mục nhóm khác. Script TH là ngoại lệ ownership được phân công rõ, không mở quyền sửa mọi file chung.

## Architecture — offline backup và restore an toàn

1. Backup nhận compose/project/thư mục nguồn có kiểm tra; capture trạng thái running của web/Neo4j và imageID/version. Tạo archive directory/quyền ghi trước downtime; dừng web và quiesce mọi writer khác (kể cả Browser/seed), **rồi mới** capture manifest khi Neo4j còn online. Sau manifest dừng Neo4j, không cho writer hoạt động đến lúc dump xong. Source vốn stopped thì tạm start riêng Neo4j trong maintenance chỉ để đọc manifest rồi dừng; state cuối khôi phục đúng ban đầu. Test script luôn dùng project `geoquad-b-tests`, không dev.
2. `backups/<timestamp>` chứa manifest+checksum cạnh archive. Dùng `umask 077`/file0600-dir0700 cho shell; PowerShell giới hạn ACL cho current user trên NTFS, fail rõ nếu không thiết lập được. Manifest so DA_LAM bằng run-specific IDs/test marker hoặc số liệu không định danh, không in mật khẩu/username/hash lên log. Dump user DB chứa băm mật khẩu/dữ liệu học, không commit archive/manifest cá nhân, tận dụng backups gitignore.
3. Chờ mọi transaction writer đang chạy kết thúc trước manifest và xác nhận maintenance không writer ngoài web; chạy one-off admin cùng volume source và imageID nguồn sau khi Neo4j offline. Có thể dùng `docker compose run ... neo4j-admin` chỉ nếu imageID service vẫn đúng capture; nếu tag drift thì dùng `docker run` ảnh capture với mount volume chính xác. Kiểm tra admin helper đọc data volume và ghi thư mục0700 trước downtime; map UID/GID hoặc helper có giới hạn để đặt ownership output cho người chạy, không chmod rộng. Sau dump, kiểm tra quyền/ownership host của archive/manifest/log trước báo success.
4. Shell trap/PowerShell finally khởi động lại đúng dịch vụ vốn running; đợi Neo4j healthy mới start web. Nếu dump hoặc health fail, trả nonzero; thông báo trạng thái recovery và đường archive, không báo success giả.
5. Restore mặc định tạo container/volume mới và port loopback riêng (Bolt27687/Browser27474), ảnh capture, database `neo4j`, không overwrite volume dev. Check input archive/path/checksum trước create/load; đích phải mới hoặc empty có guard.
6. Restore database `neo4j` **không** tự khôi phục `system` users Neo4j. Tạo auth DBMS đích qua env test mới; đây là recovery application database, không tuyên bố backup toàn DBMS. Nếu nhóm cần system DB thì cập nhật protocol riêng sau đối chiếu help/image; không mở scope tự động.
7. Restore ghi đè nguồn chỉ có nhánh explicit, nêu tên đích, backup trước, dừng DB, xác nhận riêng trong thời điểm execution; default drill luôn cô lập. Không `docker compose down -v` root.

## Tests Before — RED

- Viết test harness kiểm tra script chưa có/không executable, input archive thiếu, checksum sai, path vượt thư mục cho phép, destination dev bị từ chối trước load.
- Với shell/PowerShell command runner trong harness, test lỗi dump vẫn khôi phục trạng thái running cũ, Neo4j health fail không start web sớm, archive thiếu không success. Fake command test chỉ kiểm chứng flow/guard, không thay drill thật.
- Tạo graph test chứa đủ seed B và tài khoản/DA_LAM; xuất manifest kỳ vọng; BackupRestoreTests thất bại cho đến khi dump/load tạo lại đúng graph đích riêng.
- B nhận lỗi A review: viết test mô tả lỗi trong B trước sửa; không đánh dấu test C chưa chạy là green.

## Test matrix

| Mức | Scenario | Expected |
|---|---|---|
| Critical | Dump khi DB còn online | Script dừng DB trước admin; không dump dev ở test |
| Critical | Restore default | Volume mới, source manifest/graph giữ nguyên |
| Critical | Archive hỏng/sai checksum | Nonzero trước load, nguồn không đổi |
| Critical | Error sau stop | Trap/finally phục hồi trạng thái cũ; health fail được báo |
| Critical | Writer đến đúng ranh giới backup | Writer đã drain/chặn trước manifest; dump/drill khớp, không false-corruption |
| High | Archive/manifest/failure logs tạo bởi Docker | Owner-only access trên host, checksum đọc được bởi người chạy |
| High | Dump/load full app database | Catalog/nút/cạnh/constraints/DA_LAM khớp manifest |
| High | Neo4j system DB mới | Auth đích hoạt động, tài khoản ứng dụng restored; không claim system restored |
| High | B hide bài có lịch sử → C list/Lam/tiến độ | Bài ẩn không đọc qua C, số thống kê/lịch sử không mất |
| High | US-08/19/20/21/22/23/24 của C | Tất cả AC ghi đạt/không hoặc chưa kiểm thử với lý do |
| High | Gợi ý B với 100 user concurrent | p95<=2s theo NFR-01; giữ raw report/hardware ở docs test |
| Medium | 360px và desktop, 4G mô phỏng | UI usable, target44/48px, chữ16px, SVGtitle, KaTeX; tải<=3s/<=2MB NFR-02 |

## Implementation Steps — GREEN

1. Implement script tham số/guard trước, rồi workflow offline; PowerShell kiểm tra `$LASTEXITCODE` cho từng native command, không dựa riêng `$ErrorActionPreference`.
2. Thực thi dump/load trên test source và volume mới, dùng credential test, chờ database khởi động, đọc manifest qua Driver/query thực. Khi lỗi, giữ archive/log và chỉ cleanup tài nguyên test do run tạo.
3. Lưu bảng story/AC/expected/actual/browser/version/date/reviewer; Quân chạy lại ít nhất 2 Cypher **của C** và giải thích. Nếu C chưa xong thì integration dependency còn pending; tiếp tục backup/test B độc lập, không đóng US-26.
4. A kiểm thử B; sửa trong B và chạy regression. Lỗi khung chung như auth/KaTeX giao owner phần0, kiểm chứng fix trước đóng gate có liên quan.
5. NFR performance dùng HTTP thật app+fixture, script test load C# tại B test harness (không thêm thư viện/npm) đủ 100 concurrent và lưu percentile95. Cookie lớp phù hợp, ít nhất1000 request, request errors tính fail; warmup ghi rõ. Đánh giá payload/time qua DevTools/browser ở 360px/desktop và network4G.
6. Hướng dẫn chạy hai nền tảng shell/PowerShell trong docs B; A đưa vào README và thử máy sạch<=15phút; C đưa backup recovery và bước demo B vào kịch bản chung theo phân công.

## Refactor

Loại trùng shell helper trong phạm vi scripts backup/restore B, nhưng không đổi seed scripts chung. Không thêm snapshot bài tập/versioning/online backup để mở rộng yêu cầu. Viết lại docs actual results sau khi fix, giữ lịch sử failed case cần thiết.

## Tests After và executable gates

Tại root repo, giữ fixture/env phase1; `BackupRestoreTests` và HTTP/load case đều Category=Integration, integration serial. Các test đầy đủ chọn bằng namespace, không theo số test kế hoạch ấn định.

```bash
bash -n scripts/backup.sh
bash -n scripts/restore.sh
dotnet build GeoQuad.sln --nologo --disable-build-servers -m:1
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration' --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration' --nologo
dotnet test GeoQuad.sln --filter 'Category!=Integration' --nologo
```

Thực nghiệm script canonical sau khi đã viết và help/runtime đã đạt:

```bash
./scripts/backup.sh --project geoquad-b-tests --compose tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml --quiesced
./scripts/restore.sh --archive backups/<run-id>/neo4j.dump --target-project geoquad-b-restore-<run-id>
```

`<run-id>` lấy **đúng output backup**; đây là contract CLI mới sẽ tạo, không phải script hiện đã tồn tại. Do test compose chỉ có Neo4j, script phải phát hiện service web có/không thay vì gọi stop/start web vô điều kiện. Tương đương PowerShell script cùng flags theo parser riêng và verify exitcodes; chạy thực tế trên PowerShell/Windows trước claim hỗ trợ Windows. Shell lint không thay DB restore test.

## Todo và Success Criteria

- [x] RED→GREEN flow/guard scripts và regression B có evidence.
- [x] Backup/load thật + checksum/manifest + DA_LAM match, nguồn không đổi.
- [ ] A review B, Quân review C và ≥2 Cypher C có ghi kết quả.
- [ ] UI/KaTeX/guest/admin/CSRF/class và NFR performance được kiểm chứng.
- [ ] A hoàn thiện README, C hoàn thiện demo; Quân kiểm tra AC chung US-27.
- [x] Todo/phase status cập nhật theo bằng chứng, chưa chạy ghi chưa kiểm thử.

## Risk Assessment / Security / Next Steps

Khung chung hoặc C chưa hoàn thiện sẽ chặn nghiệm thu liên phần, không chặn viết script trong B. Không tự sửa C/khung chung, không dùng fake data để nói đã kiểm thử cross-C. Dump user DB không phải backup full DBMS; bảo vệ archive như dữ liệu tài khoản. Các run commands trong phase là việc implementation tương lai; vòng planning chưa dừng DB, chưa dump/load hay triển khai code.

## Evidence triển khai 08/10/2026

Shell dump/load/manifest/history/source thật và fault-flow đạt; load1000request/100concurrent/0error đạt. PowerShell7.2/Windows chưa chạy; C controllers/Cypher và reviewA/C chưa xong; browser inventory trống. Không đóng US-26/27. Inventory bổ sung backup-common.sh/.ps1, runsettings và BackupFlowTests.cs để thực hiện protocol trong scope B. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
