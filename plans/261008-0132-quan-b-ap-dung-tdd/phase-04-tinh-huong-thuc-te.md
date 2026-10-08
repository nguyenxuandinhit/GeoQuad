---
phase: 4
title: "US-17 — Tình huống thực tế theo lớp và bối cảnh"
status: pending
priority: P1
effort: 2h
dependencies: [1]
---

# Phase 04 — Tình huống thực tế theo lớp và bối cảnh

## Context và phạm vi

- [SRS UC-09/FR-40/BR-04/08](../../docs/GeoQuad_SRS.md):406; [README US-17](../../README.md):525.
- [Jira nguồn](../../docs/GeoQuad_Jira_2ngay.csv):195: parent Issue Id **66**, subtasks **67/68/69**, US-17, 3 điểm.
- [Phase01](./phase-01-start.md) là blocker dữ liệu B. Seed tình huống là của A; Quân chỉ đọc và báo thiếu, không sửa A.
- [Phase05](./phase-05-thuc-hanh-do-dac.md) dùng chi tiết và form thực hành; [phase07](./phase-07-backup-kiem-thu-ban-giao.md) kiểm chứng link liên phần.
- Mọi path bảng ghép với `/Users/quan/HUIT_CNTT/NoSQL/GeoQuad/`; **MỚI** là file dự kiến, chưa có.

- Đầu vào nghiên cứu: [requirements](./research/requirements-B.md) và [TDD architecture](./research/tdd-architecture-B.md); policy ngoài SRS là assumption của plan.

## Hiện trạng và deep rescout

`TinhHuongController.Index` tại `src/GeoQuad.Web/Areas/ApDung/Controllers/TinhHuongController.cs:11` và `Xem` dòng 15 còn placeholder.
`DoDac` dòng 21 đã có `[HttpPost]` dòng 19 và anti-CSRF dòng 20; giữ chỗ cho phase05, không biến thành GET.
`ICurrentUser.LopHienThi` tại `Infrastructure/Auth/ICurrentUser.cs:28` lấy class filter theo người dùng.
Trước code, re-grep controller/module, seed A `AP_DUNG/TRONG_BOI_CANH/LIEN_QUAN_DEN`, seed B dấu hiệu và mọi query hiện có.
Trace list và direct URL cùng access gate; không chỉ trích line query rồi giả định filter được dùng ở action chi tiết.

## Requirements và kiến trúc

- Ba bối cảnh theo seed luôn có thẻ kể cả bối cảnh rỗng; list có nhãn lớp, bộ lọc bối cảnh và trạng thái rỗng với gợi ý bối cảnh khác.
- Chi tiết có mô tả, SVG, “Kiến thức dùng ở đây”, liên kết định lý/công thức và “Vì sao cách này đúng”.
- TH-01 phải dẫn `DH_HBH_2` + `DH_HCN_3`; lớp 3 không thấy TH-01 mặc định.
- Danh sách và URL trực tiếp đều yêu cầu tình huống/kiến thức liên quan đã rà soát, concept ≥1, lớp đủ.
- `lopCan` là max lớp của **toàn bộ** kiến thức bắt buộc `AP_DUNG`; không lọc bỏ kiến thức lớp cao trước max rồi làm tình huống lọt.
- Nếu một mã kiến thức chỉ là placeholder từ seed A và thiếu nội dung/lớp/trạng thái hợp lệ, ẩn tình huống phụ thuộc nó.
- Không dùng điều kiện permissive `lopCan IS NULL OR ...` trong truy vấn mẫu README:532 để coi thiếu dữ liệu là phù hợp mọi lớp.
- Không sửa dữ liệu A; công cụ phase01 xác minh seed B làm đầy placeholders hoặc ghi blocker A.
- `ITinhHuongRepository` colocate trong file repository, trả DTO typed; `TinhHuongService` dựng list/empty/readiness nhận interface. Fake typed của phase này, không để service biết IRecord.
- Nâng cao dùng `LopHienThi`; nhãn “Nâng cao” theo lớp thật, không theo giới hạn 12.
- Nút thực hành chỉ khi `thucHanh=true` và kiến thức đủ; phase05 sẽ nối POST, không ghi tiến độ khách.

## Data flow và phụ thuộc

1. GET list nhận `boiCanh`, `ICurrentUser` scoped; validate mã filter, dùng tham số `$boiCanh/$lop`.
2. Repository duyệt bối cảnh → tình huống; thu thập toàn bộ `AP_DUNG` và concept liên quan trước đánh giá tính sẵn sàng.
3. Tính `lopCan=max(l.so)`, đồng thời kiểm tra không thiếu/mất kiến thức bắt buộc và nội dung `DA_RA_SOAT`.
4. Chỉ project tình huống hợp lệ; không có list thì trả empty state và những bối cảnh thực sự có nội dung cùng lớp.
5. GET `/Xem/{id}` dùng chính readiness/class policy; project kiến thức distinct và đúng đường liên kết chủ sở hữu hoặc đích.
6. View render tiếng Việt/SVG do Area tạo/KaTeX có delimiter rõ; nội dung encode, không render HTML không tin cậy.
7. DB hỏng trả thông báo lỗi thân thiện, không đánh đồng với bối cảnh rỗng và không lộ stack trace.

Blocker phase01 là các dấu hiệu B và fixture; blocker ngoài nhóm là `neo4j/seed/12-tinhhuong-A.cypher` đủ 9 tình huống/3 bối cảnh.
Phase05 phụ thuộc action/view chi tiết này. Module và docs B chạy tuần tự; phase04 không sở hữu `DoDac` trước phase05.
Nếu thư viện A còn placeholder, link giữ đúng hợp đồng nhưng phase07 phải ghi integration chưa pass.

## File inventory và ownership

| File trong prefix | Action | Cỡ dự kiến | Test impact |
|---|---|---:|---|
| src/GeoQuad.Web/Areas/ApDung/Controllers/TinhHuongController.cs:11 | Sửa Index/Xem | 70 dòng | List/direct URL |
| src/GeoQuad.Web/Areas/ApDung/ApDungModule.cs:10 | Sửa tuần tự | 3 dòng | DI scoped |
| src/GeoQuad.Web/Areas/ApDung/Repositories/TinhHuongRepository.cs (gồm ITinhHuongRepository) | MỚI | 140 dòng | AP_DUNG đủ, max class |
| src/GeoQuad.Web/Areas/ApDung/Services/TinhHuongService.cs | MỚI | 60–80 dòng | Readiness/empty state |
| src/GeoQuad.Web/Areas/ApDung/Models/TinhHuongViewModel.cs | MỚI | 80 dòng | List/detail readiness |
| src/GeoQuad.Web/Areas/ApDung/Views/TinhHuong/Index.cshtml | MỚI | 90 dòng | Cards/list/empty |
| src/GeoQuad.Web/Areas/ApDung/Views/TinhHuong/Xem.cshtml | MỚI | 110 dòng | Kiến thức, SVG, phase05 seam |
| tests/GeoQuad.Tests/B_ApDung/TinhHuongTests.cs | MỚI | 100 dòng | Readiness/links |
| tests/GeoQuad.Tests/B_ApDung/Doubles/TinhHuongRepositoryGia.cs | MỚI | 30–50 dòng | Unit typed/grade policy |
| tests/GeoQuad.Tests/B_ApDung/Integration/TinhHuongIntegrationTests.cs | MỚI | 170 dòng | Gate DB + HTTP |
| docs/cypher/B-ap-dung.md:32 | Bổ sung US-17 | 80 dòng | Query, ký hiệu, seed results |

Không xóa file; không sửa shared CSS/layout/csproj/Program/Infrastructure hoặc seed A.
Style scoped trong Area view nếu cần 360px; giữ seam partial thực hành cho phase05.

## Tests Before — RED

| Mức | Tầng | Case | Kết quả bắt buộc |
|---|---|---|---|
| Critical | DB + HTTP | TH-01 lớp 8 | Hiện DH_HBH_2/DH_HCN_3 và giải thích đúng |
| Critical | DB + HTTP | TH-01 lớp 3, cả list và URL | Không thấy list; direct URL không trả nội dung |
| Critical | DB | Một AP_DUNG là placeholder thiếu lớp/nội dung | Tình huống bị ẩn, không null bypass |
| High | DB | Một AP_DUNG lớp cao, còn lại lớp thấp | max đủ toàn bộ, bị ẩn khi chưa nâng cao |
| High | HTTP | Nâng cao bật | Hiện đúng nội dung kèm “Nâng cao” |
| High | DB + unit | Thiếu concept, chưa rà soát, thiếu lời giải | Không publish như tình huống hoàn chỉnh |
| Medium | HTTP | Bối cảnh rỗng/mã không tồn tại | Thông báo rõ; gợi ý bối cảnh có nội dung |
| Medium | HTTP | thucHanh true/false; AP_DUNG lặp | Nút đúng điều kiện, kiến thức distinct |
| Medium | Manual | 360px, keyboard, DB unavailable | Responsive/nút ≥44px (48px cấp 1); lỗi DB khác empty |

Viết assertion nội dung/path/status cụ thể trước implementation. Integration query thật, không dùng mock để chứng nhận filter Cypher.

## Implementation Steps và Refactor

1. Rescout; chạy diagnostic counts/readiness cho toàn bộ 9 tình huống để xác nhận blocker A/B.
2. Viết RED cho lớp 3, direct URL và placeholder AP_DUNG trước happy path.
3. Viết repository list/detail parameterized; dùng chung readiness predicate nội bộ Area, không sửa shared service.
4. Tạo model/controller/views, map link kiến thức theo README, SVG cố định và empty state.
5. Refactor projection detail/summary để tránh duplicate knowledge, giữ max class trước projection.
6. Nối nút/form seam phase05 chỉ khi dữ liệu hợp lệ, chưa thực hiện measurement trong phase này.
7. Ghi Cypher, từng ký hiệu, output TH-01/lớp 3 vào docs B; đề xuất lỗi shared nếu gặp.

## Tests After và gates

Dùng DB biệt lập phase01, collection serial; thiếu environment DB phải FAIL, không skip. Host HTTP chạy Program thật.
.NET8 acceptance; không chỉnh TargetFramework/csproj hoặc culture chung.

```sh
dotnet build GeoQuad.sln --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration'
```

Đọc lại docs Cypher và kết quả seed thật; manual kiểm tra ba bối cảnh, list/detail, lớp 3/8/nâng cao tại 360px.
Nếu shared KaTeX delimiter chưa sửa, render rõ `$...$` trong Area và ghi dependency docs B, không sửa site.js.

## Todo và success criteria

- [x] RED/GREEN cho class gate trên list + URL + AP_DUNG thiếu.
- [x] 3 bối cảnh và 9 tình huống đủ readiness trên full seed; thiếu nào được ghi blocker.
- [x] TH-01 liên kết đúng dấu hiệu, lời giải và nút thực hành.
- [ ] Docs đủ query/ký hiệu/output; responsive 360px và nhãn nâng cao.
- [x] Bàn giao phase05/07, diff chỉ B.

## Rủi ro, bảo mật, tương thích và rollback

| Rủi ro | Likelihood × impact | Mitigation |
|---|---|---|
| Placeholder A có mã nhưng không đủ nội dung | Cao × Cao | Fail closed + integration thiếu AP_DUNG |
| Direct URL bypass lớp | Vừa × Cao | Cùng readiness gate list/detail và HTTP test |
| max lớp sau filter làm rò kiến thức | Vừa × Cao | Aggregate toàn bộ trước filter, mixed-class fixture |
| Liên kết A chưa triển khai | Cao × Vừa | Ghi hợp đồng/bằng chứng, phase07 gate |

Cypher tham số, không raw HTML, không ghi dữ liệu người dùng. Giữ route hiện hữu, không thêm node/schema/migration.
Rollback code/view/test list/detail và docs US-17; giữ seed A/B nguyên; phase05 phải cùng rollback nếu đã phụ thuộc view mới.

## Evidence triển khai 08/10/2026

Audit query trả3bối cảnh/9tình huống trên graph test đã rà soát. TH-01/test lớp/missingAP_DUNG và gate URL đạt. Readiness của production phụ thuộc seedB được C review; visual360px còn chờ browser. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
