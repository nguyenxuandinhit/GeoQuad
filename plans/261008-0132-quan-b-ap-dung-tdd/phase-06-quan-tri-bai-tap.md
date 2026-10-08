---
phase: 6
title: "US-25 — Quản trị bài tập bằng giao dịch nguyên tử"
status: pending
priority: P2
effort: 5h
dependencies: [1]
---

# Phase 06 — Quản trị bài tập bằng giao dịch nguyên tử

## Context và phạm vi

- [SRS UC-15/FR-70/BR-05/08](../../docs/GeoQuad_SRS.md):484; [README US-25](../../README.md):563.
- [Jira nguồn](../../docs/GeoQuad_Jira_2ngay.csv):286: parent Issue Id **98**, subtasks **99/100/101**, US-25, 5 điểm.
- [Phase01](./phase-01-start.md) là blocker schema/seed/fixtures; [phase07](./phase-07-backup-kiem-thu-ban-giao.md) kiểm tra bài ẩn với C và backup.
- Toàn bộ thêm/sửa/ẩn/hiện, bộ lọc/tìm kiếm, ba loại bài, preview học sinh và history preservation đều trong scope.
- Paths bảng ghép `/Users/quan/HUIT_CNTT/NoSQL/GeoQuad/`; **MỚI** là tên file dự kiến chưa tồn tại.

- Đầu vào nghiên cứu: [requirements](./research/requirements-B.md) và [TDD architecture](./research/tdd-architecture-B.md); policy ngoài SRS là assumption của plan.

## Source evidence và deep rescout

`BaiTapController` ở `src/GeoQuad.Web/Areas/QuanTri/Controllers/BaiTapController.cs:14` đã có `[Authorize(Roles = VaiTro.QuanTri)]` dòng 13.
`Index/Them/Sua` ở dòng 17/21/25 là GET placeholder; thêm POST phải giữ authorize ở cấp controller và anti-CSRF từng mutation.
`QuanTriModule.AddQuanTri` tại `Areas/QuanTri/QuanTriModule.cs:10` được `Program.cs:50` gọi, không sửa Program.
`IGraphDb.WriteTransactionAsync` ở `Infrastructure/Neo4j/IGraphDb.cs:18`; `GraphDb.cs:38–41` thực sự dùng managed write transaction.
Trước code re-grep mọi điểm trên, constraint `BaiTap.ma`, properties/relations seed A/C và mọi reader `hienThi` phần C.
Trace form → validate → transaction → redirect/preview; enumerate caller C theo path:line trong báo cáo thực thi, không ghi “update all callers”.

## Requirements và quyết định kiến trúc

- Authorize server mọi GET/POST/preview; khách bị challenge, học sinh denied; ẩn nút UI không thay authorization.
- List lọc lớp/loại/trạng thái hiện-ẩn, tìm mã/đề; giữ sort theo mã và số lần làm; không join làm nhân bản count.
- Quản trị có quyền đọc/sửa bài mọi lớp và trạng thái, kể cả NHAP/ẩn; dùng `$lopLoc` do admin chọn, không `$lop=ICurrentUser.LopHienThi`. Catalog kiến thức liên quan vẫn phải đủ nội dung/đã review nhưng không giới hạn lớp theo admin; readiness public thuộc ApDung, không áp vào list/editor QuanTri.
- Ba loại: trắc nghiệm đúng 4 phương án + đúng 1 A–D; đáp án số hữu hạn + sai số >0 + đơn vị; chứng minh có lời giải mẫu.
- Mã mới unique; khi sửa `ma` bất biến lấy từ route, không cho posted hidden field đổi khóa; body khác route bị reject, không âm thầm nhận mã sửa khác.
- Đề/giải thích/nguồn/status/độ khó/lớp/khái niệm/định lý/công thức validate whitelist server; độ khó 1–3, lớp 1–12, ≥1 concept.
- Đáp án số bằng 0 hợp lệ, phân biệt với bỏ trống; tham số FLOAT dùng double theo driver, không gửi decimal trực tiếp.
- Property map do server dựng từ whitelist, không nhận dictionary tuỳ ý qua model binding; changing loại phải remove/null hóa toàn bộ field của loại cũ.
- Preview chỉ render DTO đã validate như layout học sinh, không ghi Neo4j và không cần public nội dung chưa lưu.
- Create/edit trong một `WriteTransactionAsync`; xác minh **toàn bộ** IDs lớp/concept/SU_DUNG tồn tại, đúng label và hợp lệ trước commit.
- `IBaiTapQuanTriRepository` colocate trong repository, trả DTO typed; fake typed phục vụ unit controller/validator mapping. DB rollback dùng GraphDbGayLoi bọc runner thật chèn exception sau mutation, không giả lập kết quả Cypher.
- Không để `MATCH` drop mã thiếu rồi commit partial: so sánh tập IDs đã tìm với tập yêu cầu, thiếu bất kỳ thì throw validation exception để rollback.
- Edit chỉ thay `THUOC_LOP/LIEN_QUAN_DEN/SU_DUNG`, giữ node và toàn bộ `DA_LAM`; hide/show chỉ SET `hienThi`, không `DETACH DELETE`.
- Không tự viết chấm bài C; bài hidden phải được C lọc khi đọc, phase07 xác nhận list và URL C.

## Data flow và dependency map

1. GET list/editor lấy catalog DB, authenticated QUAN_TRI; server filter parameterized.
2. POST token + DTO bất biến; validate type-specific/common fields và normalize duplicate related IDs.
3. Preview render model từ DTO tại partial Area; action không gọi write API.
4. Save transaction đọc đủ IDs/labels và current node; route mã edit là authority, duplicate create bị unique constraint chặn.
5. Managed callback có thể retry: không tạo file/gửi HTTP/side effect ngoài DB; consume cursor trước return. Transaction cập nhật whitelist, xóa **chỉ ba loại quan hệ nội dung** rồi dựng lại toàn bộ; lỗi bất kỳ rollback thuộc tính và quan hệ.
6. Commit rồi PRG redirect; lỗi validation/constraint hiện message tiếng Việt, giữ input, không lộ truy vấn/credentials.
7. Hide/show kiểm tra node tồn tại, SET flag trong write; verify history counts không đổi.

Phase01 cung cấp schema/catalog/DB isolated. Không cần phase02..05 về logic; vẫn serialize docs B để tránh file conflict.
Phase07 cần C hoàn thành query visible/historical stats để nghiệm thu hidden; đề xuất docs B nếu reader C thiếu filter, không sửa C.

## File inventory và ownership

| File trong prefix | Action | Cỡ dự kiến | Test impact |
|---|---|---:|---|
| src/GeoQuad.Web/Areas/QuanTri/Controllers/BaiTapController.cs:14 | Sửa | 150 dòng | Auth, token, PRG/preview |
| src/GeoQuad.Web/Areas/QuanTri/QuanTriModule.cs:10 | Sửa | 6 dòng | DI scoped |
| src/GeoQuad.Web/Areas/QuanTri/Repositories/BaiTapQuanTriRepository.cs (gồm IBaiTapQuanTriRepository) | MỚI | 220 dòng | Atomic create/edit/hide |
| src/GeoQuad.Web/Areas/QuanTri/Services/KiemTraBaiTap.cs | MỚI | 120 dòng | Whitelist/type validation |
| src/GeoQuad.Web/Areas/QuanTri/Models/BaiTapQuanTriViewModel.cs | MỚI | 110 dòng | Immutable DTO/list/editor |
| src/GeoQuad.Web/Areas/QuanTri/Views/BaiTap/Index.cshtml | MỚI | 100 dòng | Filters/action list |
| src/GeoQuad.Web/Areas/QuanTri/Views/BaiTap/Soan.cshtml | MỚI | 150 dòng | Editor/types/CSRF |
| src/GeoQuad.Web/Areas/QuanTri/Views/BaiTap/_XemTruoc.cshtml | MỚI | 90 dòng | Layout học sinh/encode |
| tests/GeoQuad.Tests/B_ApDung/QuanTriBaiTapTests.cs | MỚI | 170 dòng | Validation/field whitelist |
| tests/GeoQuad.Tests/B_ApDung/Doubles/BaiTapQuanTriRepositoryGia.cs | MỚI | 40–60 dòng | Unit typed controller seam |
| tests/GeoQuad.Tests/B_ApDung/Integration/GraphDbGayLoi.cs | MỚI | 60–90 dòng | Fault injection runner thật |
| tests/GeoQuad.Tests/B_ApDung/Integration/QuanTriBaiTapIntegrationTests.cs | MỚI | 220 dòng | DB rollback, HTTP auth |
| docs/cypher/B-ap-dung.md:32 | Bổ sung US-25 | 130 dòng | Query + ký hiệu + outcomes |

Không xóa file; không sửa Infrastructure/Program/csproj/CSS chung hoặc code C; không thêm dependency package.

## Tests Before — RED matrix

| Mức | Tầng | Case | Kết quả bắt buộc |
|---|---|---|---|
| Critical | HTTP | Khách/học sinh mọi GET/POST/preview | Challenge/denied, DB không đổi |
| High | HTTP + DB | Admin claim lớp8 mở bài lớp12/NHAP/ẩn | Quản lý được theo bộ lọc tự chọn, không bị public gate chặn |
| Critical | Unit + HTTP | TN 0/2 đáp án đúng, 3 phương án, không concept | Reject cùng message cụ thể, không lưu |
| Critical | DB | ID liên quan không tồn tại/nhãn sai, lỗi giữa edit | Rollback toàn bộ node/relations; dữ liệu cũ giống trước |
| Critical | DB | Edit/hide bài đã có DA_LAM | History count/properties nguyên, node không xóa |
| High | DB + HTTP | Mã trùng, hai create concurrent cùng mã | Một tạo được, một lỗi duplicate; chỉ một node |
| High | Unit + DB | Edit gửi đổi ma/property trái whitelist | Mã giữ nguyên; body khác route reject; property lạ không ghi |
| High | DB | Đổi TN → số → chứng minh | Field loại cũ bị loại; field loại mới đúng |
| High | Unit + HTTP | Số thiếu unit/sai số ≤0/NaN; CM thiếu lời giải | Reject; input còn nguyên |
| High | HTTP | CSRF thiếu/sai; mutation GET | 400 hoặc không action; không write |
| High | HTTP + DB | Preview | Giao diện giống học sinh, số node/relations không đổi |
| Medium | DB + HTTP | Lọc lớp/loại/status/search + lịch sử nhiều lượt | List đúng, count không bị duplicate |
| Medium | HTTP/manual | Show lại, 360px, script trong đề | Flag đúng, keyboard/nút ≥44px (48px cấp 1), encode |

Viết RED trước repository/controller; fake không đủ chứng nhận rollback. DB test thật snapshot props/relations trước và sau failure.
History fixture được phase01 tạo trong DB test riêng, không sửa seed C hay tài khoản dev.

## Implementation và Refactor

1. Rescout schema/readers/lifetime, viết RED auth/token/validation/atomicity/history trước happy path.
2. Implement validator thuần và model binding whitelist; quyết định sai số 0,01 mặc định đáp án số theo BR-05 nếu field bỏ trống.
3. Implement list/catalog; create/edit transaction query hằng, kiểm tra đủ related IDs, xử lý uniqueness bằng message duplicate.
4. Implement hide/show preserve history, preview read-only; thêm actions POST anti-CSRF và PRG.
5. Tạo list/editor/preview responsive trong Area, $ delimiters KaTeX rõ nếu shared issue còn.
6. Refactor property mapping type-specific thành một helper thuần; remove stale fields nhất quán, không ghép Cypher từ input.
7. Chạy Neo4j thật cho mỗi mutation/rollback; ghi docs từng query, ký hiệu, outcomes tạo/sửa/ẩn/restore.

## Tests After và regression gate

Dùng runtime .NET8; fixture serial, DB riêng `geoquad-b-tests` phase01; environment DB thiếu phải FAIL rõ.
HTTP host chạy child Program thật Production/test DB override, real login + cookie + anti-CSRF; không chỉ invoke controller trực tiếp.

```sh
dotnet build GeoQuad.sln --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration'
```

Sau GREEN đo snapshot DA_LAM trước/edit/hide/show vẫn nguyên; tạo ba loại bài và preview; phase07 test hidden với reader C thật.
Không coi unit validator GREEN là AC hidden/history đã pass; ghi dependency C chưa hoàn tất nếu có.

## Todo, risks, security, compatibility và rollback

- [x] RED/GREEN authorize+CSRF, cả ba loại, immutable mã/whitelist.
- [x] Create/edit transaction reject mọi ID thiếu và rollback toàn bộ; duplicate concurrent có bằng chứng.
- [x] Hide/show preserve history; preview không write; list đủ filter/search.
- [ ] Docs ba mục mỗi query; 360px/keyboard, phase07 reader C integration.

| Rủi ro | Likelihood × impact | Mitigation |
|---|---|---|
| MATCH silently drop ID và commit partial | Cao × Cao | So sánh tập đủ IDs trong transaction + failure snapshot |
| Edit thay node/xóa history | Vừa × Cao | Giữ ma/node, DELETE chỉ 3 relations nội dung, DA_LAM tests |
| Overpost/CSRF/auth bypass | Vừa × Cao | Server authorize/whitelist/token mọi action |
| Đổi loại để lại đáp án cũ | Cao × Vừa | Clear stale fields và DB assertion từng chuyển loại |

Không migration schema; mọi bài seed cũ đọc được. Mã bất biến giữ integrations C và lịch sử. Rollback code/views/test quản trị; với edit sai phục hồi chỉ nội dung từ snapshot/backup phase07, giữ DA_LAM, không chạy seed đè data người dùng.

## Evidence triển khai 08/10/2026

Unit/DB/HTTP kiểm tra3loại, mọi routeadmin role/CSRF, immutablecode, whitelist, failed transaction, duplicateconcurrent, history và preview đạt. C chưa triển khai reader bài ẩn/history; browser360px/keyboard chưa chạy. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
