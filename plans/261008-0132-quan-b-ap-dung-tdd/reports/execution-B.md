# Báo cáo triển khai B — 08/10/2026

Đã triển khai phần B trên `feature/B-ap-dung`: seed, gợi ý, chứng minh, tình huống/đo đạc, quản trị bài tập và script backup/restore. Plan còn in-progress vì review nội dung C, kiểm thử chéo A/C, UI và Windows chưa nghiệm thu. Nội dung SRS/ảnh người dùng giữ nguyên; lần bàn giao này được user cho phép commit/push trên một nhánh và tạo PR.

## Môi trường và phạm vi

- SDK8.0.425/runtime8.0.31 được cài riêng trong `/private/tmp/geoquad-dotnet8`, không đổi target net8/roll-forward. Docker Desktop đã bật; Neo4j5.26.31 từ dòng image5.26, test project/volume/ports/readonlyseed guard trước mọi reset.
- B owns Areas/ApDung, QuanTri, seed20/21, B_ApDung tests, docs B và scripts backup/restore. Bổ sung helper backup-common.sh/.ps1 để dùng chung manifest/native flow; thêm runsettings opt-in test. Không package, Program/Infrastructure/csproj/schema/core/A/C mutation.
- Production seed B giữ NHAP; public test dùng graph cô lập với reviewTestOnly. Gợi ý/class/proof không mở bản nháp khi triển khai seed nguyên trạng.

## TDD và sửa lỗi có evidence

| Phase | RED và GREEN |
|---|---|
| 1 | SeedBTests trước seed fail `Thiếu seed B: 20-dinhly-B.cypher`; fixture guard ban đầu phát hiện Docker Desktop mount /host_mnt, bổ sung đúng alias host +read-only; seed/schema/order/invariants sau đó đạt |
| 2 | Typed service tests và DB/HTTP expectations trước implementation; bốn AC, direct/inherited/QPP, draft/null/class/input sau implementation đạt |
| 3 | Mẫu thiếu căn cứ TC của A bị ẩn trong RED; sau full seed đạt, URL/link cùng readiness predicate; orderedsteps/class/draft còn được kiểm tra |
| 4 | Controller khung không có model tình huống trong RED; full seed đã rà soát của test trả3context/9scenario; null/missing-classAP_DUNG bị chặn |
| 5 | Unit classifier/input và HTTP practice trước implementation; 23 unitcases và HTTP guard/no-write đạt |
| 6 | Tests mutation/HTTP trước repository/controller; lần đầu editor thực phát hiện driver không cast List<string> thành string[], sửa mapping; rollback, races, types/history/roles/CSRF/preview đạt |
| 7 | BackupRestoreTests trước script fail thiếu backup.sh; drill đầu phát hiện toString(map) không được hỗ trợ, trap khôi phục source; đổi sort/hash history output, dump/load tiếp theo đạt. Flow fault tests bổ sung sau script chứng minh recovery/health/stopped/checksum |

Runtime .NET8 được cài sau khi preflight SDK10 test abort thiếu runtime8; không dùng preflight này làm acceptance. Các lỗi build/query/mapping được sửa trong ownership B, không skip hoặc giảm assertion để đạt. Rà soát cuối sửa fixture DA_LAM theo SRS: mỗi lượt một cạnh luc/dung/dapAnDaChon/thoiGianGiay thay vì property soLan; production query không đổi, counters admin được kiểm tra7cạnh thật và backup3cạnh thật.

## Kiểm chứng cuối

- Build solution bằng .NET8: 0 warning/0 error.
- Unit regression toàn solution: **118 passed, 0 failed/skipped**.
- Integration/HTTP/load/drill full suite: **14 passed, 0 failed/skipped**, 3m29s. Load p95 **400,4095ms**, 1000request/100workers, 0errors; CPU10, Unix27, reportUTC09:01:09. Sau rà soát fixture lịch sử theo mỗi lượt một DA_LAM, chạy lại **4tests quản trị/restore: 4passed, 0fail/skip**, 1m09s.
- Shell syntax và git diff whitespace: đạt.
- Mọi const repository query được audit trên graph test thật, ghi tham số/output/counters vào docs; mutation audit trong transaction rollback.
- Simplify threshold vượt400LOC/200singlefile; code-simplifier làm gọn3file: ChungMinhService/GoiYController/KiemTraBaiTap. ✓ Step 3.S: Simplify ran — scoped changes. Root review inline và final regression; chưa có A reviewer độc lập.

## Còn phụ thuộc

Xem [docs/kiem-thu.md](../../../docs/kiem-thu.md) và [content-review-B.md](content-review-B.md): reviewer C/chứng cứ SGK (nhất là CM-04), A test B, C reader bài ẩn/history và≥2Cypher, browser360px/keyboard/4G/KaTeX, PowerShellWindows/ACL, README máy sạch và demo chung. C hiện còn controller khung. Không đóng các gate này bằng dữ liệu/test B.

Nguồn/graph dev không bị reset. Test source còn chạy để chạy lại; restore destination do test tạo được cleanup theo exact marker. Archive và log drill riêng tư ở backups gitignored; không đưa vào commit. Tài liệu và test đã có lệnh để owner tiếp tục nghiệm thu.

## Bàn giao Git/CSV theo yêu cầu mới

- User chọn cập nhật CSV trước, không yêu cầu Jira online trong lượt này. 27 dòng B:5Done/21In Progress/1To Do; status và comment mới không làm đổi giá trị nguồn/A/C. CSV validation xác minh119records/18columns/4Labelsheaders và27comments.
- Origin main có9commit mới tới95a8519: A controllers/services/repos/views/tests và sharedCSS. Đã fast-forward chính feature/B-ap-dung, không sửa code A; regression mới: build net8 sạch, **415unit passed** (gồm11case serviceB mới). Full14integration trên trạng thái tích hợp trước push đạt14passed/0fail/skip,3m03s; link căn cứ B→trang hình A đạtHTTP200.
- Thêm test service chứng minh cho sequence trùng/thiếu/out-of-order/thiếu grounds và forwarding lớp/badge; service tình huống cho invalidcontext/emptycontexts/grade forwarding. Backend53/57/68/72/100 có unit evidence để chuyển Done trong CSV.
- Packaging SRS Markdown/ảnh nguyên trạng từ docx đã tracked để link plan không hỏng ở remote. Commit/PR chỉ chứa B và artifact nguồn cần thiết; A mới đã thuộc base main.

Kết quả publish gate cuối: **415unit +14integration passed**, build0warnings/errors; tải p95=427.4867ms,1000request/100workers/0error; restore134nodes/301edges,3DA_LAM riêng, AuthHelper password và manifest đúng. CSV preservation/plan validation/git whitespace đạt.
