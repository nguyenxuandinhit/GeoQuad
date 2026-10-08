---
phase: 3
title: "US-14 — Chứng minh mẫu từng bước có căn cứ"
status: pending
priority: P2
effort: 2h
dependencies: [1, 2]
---

# Phase 03 — Chứng minh mẫu từng bước có căn cứ

## Context và phạm vi

- [SRS UC-07/FR-21](../../docs/GeoQuad_SRS.md):380; [README US-14](../../README.md):509.
- [Jira nguồn](../../docs/GeoQuad_Jira_2ngay.csv):161: parent Issue Id **55**, subtasks **56/57/58**, US-14, 3 điểm.
- [Phase01 seed chứng minh](./phase-01-start.md) và [phase02 gợi ý](./phase-02-goi-y-dinh-ly.md) là blocker.
- Scope gồm giả thiết, kết luận, SVG, thứ tự bước, căn cứ và việc ẩn nút chưa có mẫu; Should vẫn phải hoàn thành.
- Mọi path bảng ghép với `/Users/quan/HUIT_CNTT/NoSQL/GeoQuad/`; **MỚI** chỉ file thiết kế chưa tồn tại.

- Đầu vào nghiên cứu: [requirements](./research/requirements-B.md) và [TDD architecture](./research/tdd-architecture-B.md); policy ngoài SRS là assumption của plan.

## Hiện trạng và khảo sát lại bắt buộc

`ChungMinhController.Xem(string id)` tại `src/GeoQuad.Web/Areas/ApDung/Controllers/ChungMinhController.cs:11` còn placeholder tại dòng 12.
Route Area sử dụng `{id?}` ở `src/GeoQuad.Web/Program.cs:86`; giữ tên tham số `id`, không đổi thành `ma` rồi làm binding sai.
`ApDungModule.AddApDung` ở `src/GeoQuad.Web/Areas/ApDung/ApDungModule.cs:10` là DI seam, `Program.cs:49` gọi.
Trước code, re-grep những điểm trên, seed `CHUNG_MINH_CHO/CO_BUOC/CAN_CU`, caller gợi ý phase02 và view/module hiện tại.
Trace request → repository → DTO ordered → view; nếu source khác, cập nhật citation và test trước thực thi.

## Yêu cầu và kiến trúc

- Đọc một mẫu qua `ChungMinh → CHUNG_MINH_CHO → DinhLy` và `ChungMinh → CO_BUOC {thuTu} → Buoc → CAN_CU → DinhLy`.
- Xác định BR-08 bằng đường chứng minh → định lý → khái niệm hiện hữu (`KHANG_DINH` hoặc chủ sở hữu tính chất); không thêm schema mới.
- Mẫu/định lý/căn cứ/khái niệm phải đủ dữ liệu đã rà soát và phù hợp lớp; URL trực tiếp bị chặn như link danh sách.
- `thuTu` liên tục từ 1, bước 3–6, mỗi bước ≥1 căn cứ; thiếu bước/căn cứ → nội dung chưa sẵn sàng, không trình bày proof hoàn chỉnh.
- Tính chất mở `/KienThuc/ThuVien/ChiTiet/{maHinh}`; dấu hiệu/định lý nền mở hộp nội dung trong Area, tránh link sang route chi tiết chưa có.
- SVG minh họa do Area tạo bằng cấu trúc cố định, encode nhãn; không nhận HTML/SVG script từ DB hay request.
- Một repository, model đọc bất biến và view đủ dùng; không xây proof engine hay editor mới.
- `IChungMinhRepository` colocate trong repository, trả DTO typed; `ChungMinhService` dựng thứ tự/link/readiness, nhận interface và CurrentUser. Fake typed cùng phase, không mock driver records.
- Sử dụng scoped repository/service, không cache theo người dùng; kế thừa `ICurrentUser.LopHienThi` tại `Infrastructure/Auth/ICurrentUser.cs:28`.

## Data flow và dependency map

1. Người dùng theo link từ gợi ý phase02 hoặc mở URL có `id`; validate mã/limit đầu vào.
2. Query nhận `$ma/$lop`; lọc mẫu, định lý chứng minh, căn cứ liên quan theo lớp và `DA_RA_SOAT`.
3. Repository collect căn cứ distinct; sắp xếp theo `CO_BUOC.thuTu` trước collect, tránh sort theo tên bước.
4. Model giữ giả thiết/kết luận/định lý/SVG/các bước/căn cứ; validate BR-08 theo đường khái niệm hiện hữu.
5. Razor render bước 1..n và link/modal; trạng thái thiếu/404 có hướng quay lại gợi ý, không tiết lộ dữ liệu bị ẩn.
6. Catalog proof phase02 chỉ trả mã mẫu hợp lệ; phase03 xác nhận lại lúc truy cập để xử lý thay đổi dữ liệu.

Phase01 phải chứng minh counts 4 mẫu đủ căn cứ; phase02 cung cấp caller duy nhất hiện tại từ gợi ý.
Phase07 kiểm tra entry point A và điều hướng link; nếu A chưa làm thì ghi blocker integration, không sửa A.
`ApDungModule.cs` và docs B chỉ có Quân sửa tuần tự; không chạy song song với phase02/04/05.

## File inventory

| File trong prefix | Action | Cỡ dự kiến | Test impact |
|---|---|---:|---|
| src/GeoQuad.Web/Areas/ApDung/Controllers/ChungMinhController.cs:11 | Sửa | 50 dòng | URL id, filter, 404 |
| src/GeoQuad.Web/Areas/ApDung/ApDungModule.cs:10 | Sửa tuần tự | 3 dòng | DI |
| src/GeoQuad.Web/Areas/ApDung/Repositories/ChungMinhRepository.cs (gồm IChungMinhRepository) | MỚI | 90 dòng | Ordering, căn cứ, BR-08 |
| src/GeoQuad.Web/Areas/ApDung/Services/ChungMinhService.cs | MỚI | 50–70 dòng | Readiness/typed mapping |
| src/GeoQuad.Web/Areas/ApDung/Models/ChungMinhViewModel.cs | MỚI | 60 dòng | Model đọc |
| src/GeoQuad.Web/Areas/ApDung/Views/ChungMinh/Xem.cshtml | MỚI | 100 dòng | SVG, links, modal |
| tests/GeoQuad.Tests/B_ApDung/ChungMinhTests.cs | MỚI | 100 dòng | Valid model/links |
| tests/GeoQuad.Tests/B_ApDung/Doubles/ChungMinhRepositoryGia.cs | MỚI | 30–50 dòng | Unit service typed |
| tests/GeoQuad.Tests/B_ApDung/Integration/ChungMinhIntegrationTests.cs | MỚI | 140 dòng | Query thật, direct URL |
| docs/cypher/B-ap-dung.md:32 | Bổ sung US-14 | 60 dòng | Query + ký hiệu + kết quả |

Không xóa file, không thay proof seed của phase01 trong phase này. Không sửa shared Infrastructure/Program/csproj/CSS hay A.
Nếu cần status proof khác schema seed hiện hữu, ghi đề xuất docs B và dừng gate dữ liệu, không mặc định cho qua null.

## Tests Before — RED

| Mức | Tầng | Case | Kết quả bắt buộc |
|---|---|---|---|
| Critical | DB + HTTP | CM-01 | Giả thiết/kết luận, bước liên tục đúng thứ tự, mỗi bước có căn cứ |
| Critical | DB | `CO_BUOC` nhập đảo thứ tự | UI vẫn thứ tự `thuTu`, không dựa thứ tự trả ngẫu nhiên |
| High | DB + HTTP | Căn cứ tính chất/dấu hiệu/định lý nền | Tính chất link đúng hình; loại khác modal đúng nội dung |
| High | DB + HTTP | Mẫu chưa rà soát, lớp cao; URL trực tiếp | Bị ẩn/404, không lọt qua route trực tiếp |
| High | DB | Thiếu CAN_CU, thiếu concept, số bước sai | Không publish proof hoàn chỉnh; báo thiếu dữ liệu |
| High | DB + HTTP | Dấu hiệu không có proof | Gợi ý ẩn nút; URL lạ 404 thân thiện |
| Medium | HTTP | Lặp căn cứ, script trong nội dung | Distinct đúng, HTML encode, không chạy script |
| Medium | Manual | 360px, keyboard, modal | Không tràn ngang, đóng bằng bàn phím, nút ≥44px (48px cấp 1) |

Chạy RED từ placeholder và query chưa có. Unit fake chỉ kiểm tra mapping/validation; DB fixture thật mới xác nhận Cypher.
Không chấp nhận test “query chứa ORDER BY” thay kiểm tra thứ tự kết quả thật.

## Implementation Steps và Refactor

1. Khảo sát lại route/caller/seed, viết unit và integration cases trước.
2. Thực thi query thật với CM-01..04, giữ bản output đủ dùng và redacted.
3. Viết repository/model; xử lý thiếu dữ liệu và class/review gate đồng nhất với gợi ý.
4. Đăng ký DI Area và nối controller; tạo view SVG cùng căn cứ link/modal.
5. Kiểm tra gợi ý phase02 chỉ link mẫu hợp lệ; sửa trong ownership B nếu cần.
6. Refactor projection căn cứ thành helper thuần khi mapping lặp, không thêm package hay state static.
7. Bổ sung tài liệu Cypher từng ký hiệu, results CM-01; chứng minh counts/steps bằng query thật.

## Tests After và regression gate

Environment DB/HTTP của phase01, test collection serial; thiếu DB phải FAIL rõ. Host khởi chạy Program thật với Production/test Neo4j override.
.NET8 là runtime acceptance, không sửa TargetFramework để test chạy.

```sh
dotnet build GeoQuad.sln --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration'
```

Sau GREEN chạy hành trình HBH + góc vuông → gợi ý → CM-01 → căn cứ và ghi pass/fail theo từng hành động.
Đo 4 mẫu, 3–6 bước/mẫu, không mẫu thiếu căn cứ. Không coi link sang placeholder A là hoàn tất integration.

## Todo và success criteria

- [x] RED/GREEN thứ tự bước, link căn cứ, direct URL và dữ liệu chưa đủ.
- [ ] 4 mẫu có concept qua đường hiện hữu; CM-01 đúng từng bước.
- [x] Không có nút proof khi không hợp lệ/chưa có mẫu; lớp/nâng cao đúng.
- [ ] Docs Cypher đủ ba mục; view keyboard/360px; chỉ diff B.
- [x] Bàn giao phase07 entry point và dependency A chưa hoàn thành nếu có.

## Risk, security, compatibility và rollback

| Rủi ro | Likelihood × impact | Mitigation |
|---|---|---|
| OPTIONAL MATCH làm proof thiếu căn cứ trông hợp lệ | Vừa × Cao | Validate đủ ≥1 căn cứ/bước và test thiếu dữ liệu |
| Sort trước collect sai scope | Vừa × Cao | Insert bước đảo thứ tự trên Neo4j thật |
| Link căn cứ tới route không tồn tại | Cao × Vừa | Phân loại link/modal đúng README, HTTP kiểm tra |
| SVG/raw HTML XSS | Vừa × Cao | SVG cố định, Razor encode, payload test |

Giữ route `/Xem/{id}` và graph schema hiện hữu; không ghi dữ liệu học tập, không migration.
Rollback revert code/view/test proof và đoạn docs US-14; giữ seed phase01 và query gợi ý, đảm bảo phase02 ẩn link khi feature chưa sẵn sàng.

## Evidence triển khai 08/10/2026

4 mẫu/3–6 bước/BR-08 và gate link/URL/class/căn cứ thiếu đạt. Nội dung toán học vẫn chờ C review, không đánh dấu mẫu đúng toán học chỉ từ SRS. View dùng fixedSVG/title và details/anchors; chưa nghiệm thu bằng browser. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
