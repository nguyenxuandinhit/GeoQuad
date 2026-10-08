---
phase: 5
title: "US-18 — Thực hành đo đạc và sai số tương đối"
status: pending
priority: P2
effort: 4h
dependencies: [1, 4]
---

# Phase 05 — Thực hành đo đạc và sai số tương đối

## Context và phạm vi

- [SRS UC-10/FR-41/BR-10](../../docs/GeoQuad_SRS.md):419; [README US-18](../../README.md):549.
- [Jira nguồn](../../docs/GeoQuad_Jira_2ngay.csv):206: parent Issue Id **70**, subtasks **71/72/73**, US-18, 5 điểm.
- [Phase01](./phase-01-start.md) cung cấp dấu hiệu/fixture; [phase04](./phase-04-tinh-huong-thuc-te.md) cung cấp route/detail/readiness.
- Scope đầy đủ ≥8 unit cases, form, kiểm tra 4 tam giác, phân loại theo sai số, dấu hiệu đã dùng và cặp lệch lớn nhất.
- Paths bảng ghép `/Users/quan/HUIT_CNTT/NoSQL/GeoQuad/`; **MỚI** là file dự kiến chưa có symbol source.

- Đầu vào nghiên cứu: [requirements](./research/requirements-B.md) và [TDD architecture](./research/tdd-architecture-B.md); policy ngoài SRS là assumption của plan.

## Hiện trạng và rescout bắt buộc

`TinhHuongController.DoDac` tại `src/GeoQuad.Web/Areas/ApDung/Controllers/TinhHuongController.cs:21` là POST placeholder, có anti-CSRF dòng 20.
Area route `{id?}` ở `src/GeoQuad.Web/Program.cs:86`; giữ `id` cho tình huống.
Rescout action/model/view/repository phase04 và các DH_* seed phase01; trace POST → validator → classifier thuần → đọc căn cứ → view.
Check lifetime trước state mới: service scoped/thuần, request model mới mỗi POST; `IGraphDb` singleton `Program.cs:22` không giữ số đo người dùng.
Không suy từ model mẫu của scout rằng code đã có; tìm path/symbol trước sửa và ghi khác biệt.

## Requirements và quyết định số học

- Nhập AB, BC, CD, DA, AC, BD, một đơn vị chung (cm/m theo form), sai số mặc định 1%; input string chấp nhận dấu phẩy/dấu chấm.
- Parse riêng ở Area, chỉ một dấu thập phân, không separator hàng nghìn/hai loại dấu; không thay culture toàn ứng dụng; từ chối chuỗi sai, NaN/Infinity, ≤0, sai số ≤0 hoặc ≥100% (quyết định local, SRS chưa định nghĩa khoảng).
- So sánh đối xứng `|x-y|/max(x,y) <= saiSoPhanTram/100`; biên dùng `<=`, không dùng sai số tuyệt đối của bài tập BR-05.
- Kiểm tra cả ABC, ACD, ABD, BCD có bất đẳng thức tam giác nghiêm ngặt, scale theo max trước cộng để tránh double overflow; giả định tứ giác lồi đúng scope SRS.
- Không thêm ràng buộc Cayley–Menger hay yêu cầu 6 cạnh khớp tọa độ chính xác làm mẫu đường chéo làm tròn 2,19 bị loại.
- Ưu tiên nhánh bốn cạnh bằng nhau (max-min trong sai số) → thoi → nếu chéo bằng thì vuông (`DH_THOI_1`, `DH_HV_5`).
- Nếu chưa đạt bốn cạnh: hai cặp cạnh đối bằng → HBH → chéo bằng → HCN → AB/BC kề bằng → vuông (`DH_HBH_2`, `DH_HCN_3`, `DH_HV_1`).
- Kết luận kèm trace dấu hiệu thực sự đi qua; không giả định xấp xỉ bằng là quan hệ bắc cầu.
- Không khớp thì “Chưa xác định được hình đặc biệt” và cặp lệch lớn nhất trong 6 cặp cạnh cùng cặp đường chéo, không so cạnh với chéo.
- Không làm tròn số trước phân loại; chỉ làm tròn khi hiển thị. Kết quả phải nói dựa trên sai số đã chọn và giả định hình lồi; không khẳng định số đo gần nhau tạo chứng minh chính xác.
- Đọc nội dung DH_* từ DB đã rà soát/phù hợp lớp; thiếu căn cứ chặn kết quả hoàn chỉnh, không tạo nội dung hardcode để bypass.

## Data flow và dependencies

1. GET chi tiết phase04 tạo form token; POST nhận id và strings, server xác nhận tình huống visible + `thucHanh=true` trước tính.
2. Validator parse số/đơn vị/sai số thành model bất biến; lỗi giữ nguyên input và nêu ô lỗi, không chạy classifier.
3. `KiemTraHinhDang` MỚI thuần kiểm tra tam giác rồi phân loại; trả tên hình, mã dấu hiệu, trace, tỷ lệ lệch theo chính sách trên.
4. Repository đọc đúng tập dấu hiệu tham số hóa; class/review/readiness gate trước render link.
5. Razor render số đo, sai số, kết luận, căn cứ và gợi ý đo lại; không ghi TaiKhoan/DA_LAM/DA_HOC cho cả khách lẫn học sinh.
6. HTTP invalid token 400; id không hợp lệ/ẩn/không thực hành bị từ chối, không nhận số đo qua URL GET.

Phase04 action/view/repository là blocker; phase01 có các DH_HBH_2/HCN_3/THOI_1/HV_1/HV_5.
Module/docs B tuần tự, không chạy cùng phase02/03/04; phase07 nhận ma trận/manual kết quả đo.

## File inventory và ownership

| File trong prefix | Action | Cỡ dự kiến | Test impact |
|---|---|---:|---|
| src/GeoQuad.Web/Areas/ApDung/Controllers/TinhHuongController.cs:21 | Sửa DoDac | 50 dòng | Token, readiness, invalid |
| src/GeoQuad.Web/Areas/ApDung/ApDungModule.cs:10 | Sửa tuần tự | 3 dòng | DI |
| src/GeoQuad.Web/Areas/ApDung/Services/KiemTraHinhDang.cs | MỚI | 130 dòng | Toàn bộ numeric branches |
| src/GeoQuad.Web/Areas/ApDung/Models/DoDacViewModel.cs | MỚI | 100 dòng | Parsing/local validation |
| src/GeoQuad.Web/Areas/ApDung/Repositories/DoDacRepository.cs | MỚI | 45 dòng | DH content/class |
| src/GeoQuad.Web/Areas/ApDung/Views/TinhHuong/Xem.cshtml | Sửa sau phase04 | 70 dòng | Form/kết luận |
| tests/GeoQuad.Tests/B_ApDung/DoDacTests.cs | MỚI | 190 dòng | ≥8 cases và trace |
| tests/GeoQuad.Tests/B_ApDung/Integration/DoDacIntegrationTests.cs | MỚI | 130 dòng | HTTP token/DB căn cứ |
| docs/cypher/B-ap-dung.md:32 | Bổ sung US-18 | 45 dòng | Query + ký hiệu + output |

Không xóa file; không sửa shared culture/KaTeX/csproj hay logic calculator A.

## Tests Before — RED matrix

Tuple dưới là `(AB,BC,CD,DA,AC,BD)` cùng đơn vị, sai số 1% nếu không ghi khác; assert cả kết luận và trace.

| Mức | Tầng | Case | Kết quả bắt buộc |
|---|---|---|---|
| Critical | Unit + HTTP | `(2,.9,2,.9,2.19,2.19)` | HCN; HBH_2 → HCN_3, mẫu SRS qua |
| Critical | Unit | `(1,1,1,1,1.414,1.414)` | Vuông; THOI_1 → HV_5, nhánh thoi |
| Critical | Unit | `(1,.995,.991,.986,1.4,1.4)` | Vuông; HBH_2 → HCN_3 → HV_1, nhánh chữ nhật |
| High | Unit | `(1,1,1,1,1.2,1.6)` | Thoi, chéo không bằng, THOI_1 |
| High | Unit | `(2,1,2,1,2,2.5)` | HBH, chéo khác, HBH_2 |
| High | Unit | `(2,1,1.8,1.1,2,2.2)` | Không xác định, cặp lệch/tỷ lệ đúng |
| High | Unit | 0/âm/NaN/Infinity/chuỗi sai; tam giác `1+1=2` | Lỗi cụ thể, không kết luận |
| High | Unit | Hai số 100 và 99; 100 và 98.99 | 1% đúng biên; 1.01% không bằng, đổi thứ tự vẫn giống |
| High | Unit | `2,00`/`2.00`, đơn vị lạ, sai số 0/≥100 | Parse đúng số thập phân; reject unit/sai số |
| High | DB + HTTP | Căn cứ thiếu/lớp cao, id ẩn/không thực hành | Không render kết quả/link bị chặn |
| Medium | HTTP | Token thiếu/sai; GET DoDac | 400/không action POST; DB không có ghi |
| Medium | Manual | 360px, lỗi input, số đo giữ lại | Không tràn, label/validation rõ, nút ≥44px (48px cấp 1) |

Hai case vuông khác trace thật: bộ thứ ba không đạt max-min bốn cạnh 1%, nhưng từng cặp đối và AB/BC đạt; không nhân đôi cùng tuple.
Unit fake không chứng nhận nội dung dấu hiệu; integration dùng DB thật và HTTP fixture phase01.

## Implementation và Refactor

1. Rescout phase04 và seed; viết RED classifier/parse/boundary/HTTP token trước.
2. Implement parser local và classifier thuần; tách comparator, triangle check, diagnostic pairs nhỏ dễ kiểm tra.
3. Thực thi query căn cứ thật; validate toàn bộ mã trả về đủ status/class, không hide riêng căn cứ rồi còn kết luận.
4. Nối POST có anti-CSRF và readiness; render lại view chi tiết với model input và result.
5. Refactor chỉ trong Area: immutable result chứa trace, comparator dùng một công thức cho mọi nhánh.
6. Ghi query DH_* cùng giải thích ký hiệu/output; ghi chính sách sai số/branch trong docs B.

## Tests After và regression gate

Dùng .NET8, DB phase01 biệt lập, serial collection; thiếu environment fixture phải FAIL rõ. HTTP chạy Program thật ở Production/test DB override.

```sh
dotnet build GeoQuad.sln --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration'
```

GREEN ≥8 numeric cases và toàn bộ matrix; manual TH-01 ở 360px với dấu phẩy/dấu chấm, invalid token và lớp 3.
Assert count quan hệ học tập trước/sau POST không đổi; không bỏ qua test có thiếu Neo4j.

## Todo, risks, security và rollback

- [x] RED/GREEN ≥8 cases; hai nhánh vuông có trace khác nhau.
- [x] Mẫu 2/.9/2.19 qua; biên sai số đối xứng và lỗi tam giác cụ thể.
- [x] Server gate tình huống/đơn vị/CSRF/class, không ghi học tập.
- [ ] Docs đủ ba mục, 360px qua; bàn giao phase07.

| Rủi ro | Likelihood × impact | Mitigation |
|---|---|---|
| Nhánh vuông bị test trùng/không reachable | Cao × Cao | Tuple tolerance không bắc cầu + assert trace |
| Reject mẫu vì ép hình chính xác | Vừa × Cao | RED mẫu 2.19, chỉ 4 tam giác theo scope |
| Số parse locale/culture chung gây regression | Vừa × Cao | Parser string local, tests dấu phẩy/chấm |
| POST vượt class hoặc thiếu token | Vừa × Cao | Readiness phase04 + anti-CSRF HTTP test |

Giữ schema/routes, không lưu số đo hay sửa dữ liệu seed. Rollback classifier/form/POST về seam phase04; giữ list/detail phase04 hoạt động.

## Evidence triển khai 08/10/2026

23unitcases + HTTP gate/class/CSRF/practice/no-write đạt; công thức abs/max đối xứng, commonunit, giả định tứ giác lồi được ghi UI. Browser360px chưa chạy. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
