---
title: "Phase 03 — US-19 ngân hàng bài tập"
status: in-progress
priority: P1
effort: "2h remaining"
dependencies: []
---

# Phase 03 — US-19 ngân hàng bài tập

## Mục tiêu và contract

README:635–650: GET `/HocTap/BaiTap` kết hợp cả5 bộ lọc; khách không history; học sinh có số lượt/đã đúng. Public dùng LopHienThi, nội dung vượt Lop thực gắn nhãn “Nâng cao”. malformed numeric query không được mở rộng kết quả. NHAP/ẩn/vượt lớp hiển thị bị loại.

## Inventory

| ROOT/file | Action / size | Impact |
|---|---|---|
| src/GeoQuad.Web/Areas/HocTap/Models/BaiTapModels.cs | Verify / S | Typed filters và list DTO |
| src/GeoQuad.Web/Areas/HocTap/Repositories/{IBaiTapRepository,BaiTapRepository}.cs | Verify / S | Parameterized Cypher, EXISTS concept, history aggregation |
| src/GeoQuad.Web/Areas/HocTap/Services/BaiTapService.cs | Verify / S | Normalize/invalid values |
| src/GeoQuad.Web/Areas/HocTap/Controllers/BaiTapController.cs | Verify Index / S | ModelState gate |
| src/GeoQuad.Web/Areas/HocTap/Views/BaiTap/Index.cshtml | Inspect/fix / M | Responsive filters, KaTeX, keyboard |
| tests/GeoQuad.Tests/C_HocTap/{BaiTapServiceTests.cs,us19_http_smoke.py} | Harden / S | Existing5 tests/HTTP, fixture isolation |

## RED matrix

| Case | Expected |
|---|---|
| lớp8+CAP2+numeric+khó2+thoi | BT014 và các bài đúng toàn bộ filter |
| class/cap contradiction, invalid lop=abc, SQL/Cypher-like mã | Empty/error hợp lệ; không bypass server gate |
| hidden/NHAP/out of display class | Không list; không lộ answer trong list DTO |
| Bài có2 concept, user2attempt/1correct | Một card, số lượt2, đã đúng=true |
| Guest, other user | Không history của user khác |
| Preview grade9 | Có nhãn nâng cao, thay đổi preview không ghi user class |
| View360px/keyboard | Không ngangscroll; label/filter/clear usable,44px |

## GREEN / REFACTOR

1. Giữ code Index đã làm và HTTP evidence; đọc gate schema hiện tại trước mở rộng detail.
2. Bổ sung fixture guard theo execution contract; giữ query params cho mọi filter.
3. Browser360px/desktop: kết hợp5filter, clear/reset, no-results, validation, keyboard, KaTeX; sửa view trong phạm vi C nếu fail.
4. Refactor reuse gate cho phase4/6 qua hợp đồng typed repository, không ép mọi query vào một chuỗi generic khó kiểm chứng.

## Tests After

`dotnet test tests/GeoQuad.Tests --filter "FullyQualifiedName~C_HocTap.BaiTapServiceTests"`.
`python tests/GeoQuad.Tests/C_HocTap/us19_http_smoke.py <C_CONTAINER> <C_HTTP_URL>`; chỉ chạy sau guard và app kết nối DB C. Ghi screenshot360px/desktop và nhờ B cross-review; không gọi21/21 là evidence UI.

## Tasks và AC

- [x] Models/repository/service/Index+view hiện có;5 unit cases + HTTP filter/history/malformed đã đạt ở phiên trước.
- [ ] Harden isolation guard, thêm regression GET không answer nếu cần.
- [ ] UI360px/desktop, keyboard,44px và KaTeX có evidence.
- [ ] B cross-review và cập nhật docs AC US19.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.
