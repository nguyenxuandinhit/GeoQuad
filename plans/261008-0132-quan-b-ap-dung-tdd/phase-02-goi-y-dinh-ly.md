---
phase: 2
title: "US-13 — Gợi ý định lý và chuỗi trung gian"
status: pending
priority: P1
effort: 5h
dependencies: [1]
---

# Phase 02 — Gợi ý định lý và chuỗi trung gian

## Context và phạm vi

- [SRS UC-06, FR-20, BR-04/07/11](../../docs/GeoQuad_SRS.md):367; [README phần B](../../README.md):467–507.
- [Jira nguồn](../../docs/GeoQuad_Jira_2ngay.csv):149: parent Issue Id **51**, subtasks **52/53/54**, US-13, 8 điểm.
- [Nền dữ liệu và fixture](./phase-01-start.md) phải hoàn tất; [chứng minh mẫu](./phase-03-chung-minh-mau.md) dùng kết quả phase này.
- Triển khai đầy đủ 5a/5b/5c; README yêu cầu chuỗi tối đa 3 bước nên không hoãn 5b dù SRS ghi Should.
- Chỉ sửa phần B. Lệnh và đường dẫn dưới đây chạy từ root repository; ký hiệu **MỚI** là thiết kế dự kiến, chưa có trong source.

- Đầu vào nghiên cứu: [requirements](./research/requirements-B.md) và [TDD architecture](./research/tdd-architecture-B.md); các policy ngoài SRS được khóa là assumption trong plan.

## Hiện trạng đã xác minh và khảo sát lại

`GoiYController.Index` ở `src/GeoQuad.Web/Areas/ApDung/Controllers/GoiYController.cs:11` trả placeholder tại dòng 12.
`AddApDung` ở `src/GeoQuad.Web/Areas/ApDung/ApDungModule.cs:10` đang rỗng, được `Program.cs:49` gọi khi đăng ký DI.
`ICurrentUser.LopHienThi` ở `src/GeoQuad.Web/Infrastructure/Auth/ICurrentUser.cs:28` đã giải quyết công tắc nâng cao.
Trước thực thi, re-grep ba điểm trên và các seed B; đọc lại toàn bộ file sẽ sửa, trace GET → controller → service → repository → `IGraphDb.ReadAsync` (`Infrastructure/Neo4j/IGraphDb.cs:12`).
Ghi chênh lệch so với plan, số caller thực tế và kiểm tra vòng đời DI; chưa có service mới nên không coi tên dự kiến là symbol đã tồn tại.

## Yêu cầu và quyết định kiến trúc

- Form ba bước, mặc định Tứ giác; catalog hình/điều kiện và kết quả đều giới hạn theo nội dung đã rà soát và lớp hiển thị.
- Request bất biến gồm mã nền, đích, tập mã điều kiện; mã lạ hoặc không thuộc catalog được phép bị từ chối, không âm thầm bỏ qua.
- Tách query hằng trong repository và dựng kết quả thuần trong service; scoped service, không giữ dữ liệu người dùng trong singleton hay static.
- `IGoiYRepository` khai báo cùng `GoiYRepository.cs`, trả DTO typed từ Models; `GoiYService` nhận interface. Fake typed của phase này và CurrentUserGia phase01 giúp unit không cần IRecord/query runner giả.
- Ưu tiên suy ra trực tiếp (cùng hình hoặc đặc biệt hóa); sau đó dấu hiệu trực tiếp; chỉ khi không có dấu hiệu mới tìm chuỗi 2–3 bước.
- Điều kiện là tập mã chính xác, không suy diễn thêm giả thiết người dùng chưa chọn. Mỗi thẻ có ✓/✗, lớp, thiếu bao nhiêu điều kiện.
- Sắp xếp ổn định bằng `(soThieu, ma)`; liên kết bài tập tới `/HocTap/BaiTap?khaiNiem={dich}` theo README:467.
- Chứng minh chỉ có nút khi dữ liệu mẫu đủ, đã rà soát và truy cập được; phụ thuộc phase03 để xác nhận link thật.
- Học sinh dưới lớp 8 nhận thông báo học ở lớp 8 và gợi ý công tắc nâng cao; bật nâng cao phải có nhãn “Nâng cao”.

## Data flow và phụ thuộc

1. GET nhận request và `ICurrentUser` scoped; validate với catalog visible trước khi chạy gợi ý.
2. Repository truyền `$nen/$dich/$co/$lop` qua driver; kiểm tra quan hệ đặc biệt hóa đúng chiều.
3. Query trực tiếp duyệt `DauHieu → YEU_CAU_LA/KHANG_DINH/YEU_CAU_CO/THUOC_LOP`; service phân tập đã có/còn thiếu.
4. Fallback duyệt tối đa 3 bước, tối đa 3 chuỗi; điểm đầu cho phép hình đã biết là trường hợp đặc biệt của nền bước đầu.
5. Mỗi dấu hiệu trong chuỗi phải `DA_RA_SOAT`, có lớp ≤ giới hạn; chống lặp đỉnh/bước và thứ tự kết quả ổn định.
6. Mỗi bước hiển thị điều kiện thiếu riêng; chuỗi là hướng gợi ý, không được ghi thành chứng minh tự động hay “đã đủ” khi còn thiếu.
7. Razor encode nội dung; không ghi dữ liệu học tập. Lỗi DB tạo thông báo có kiểm soát, không trả chuỗi Cypher/credentials.

Phase01 là blocker seed/fixture; phase03 kiểm chứng chứng minh, phase07 kiểm thử chéo links A/C.
Các phase chia sẻ `ApDungModule.cs` và `docs/cypher/B-ap-dung.md` chạy tuần tự dù phase04 độc lập về chức năng.

## File inventory và ownership

Prefix đầy đủ: `/Users/quan/HUIT_CNTT/NoSQL/GeoQuad/`; mọi đường dẫn bảng ghép với prefix này.

| File trong prefix | Action | Cỡ dự kiến | Test impact |
|---|---|---:|---|
| src/GeoQuad.Web/Areas/ApDung/Controllers/GoiYController.cs:11 | Sửa | 60 dòng | HTTP GET, input/class |
| src/GeoQuad.Web/Areas/ApDung/ApDungModule.cs:10 | Sửa tuần tự | 8 dòng | DI scoped |
| src/GeoQuad.Web/Areas/ApDung/Repositories/GoiYRepository.cs (gồm IGoiYRepository) | MỚI | 160 dòng | Neo4j trực tiếp/chuỗi |
| src/GeoQuad.Web/Areas/ApDung/Services/GoiYService.cs | MỚI | 100 dòng | Sort, 5a/5b/5c |
| src/GeoQuad.Web/Areas/ApDung/Models/GoiYViewModel.cs | MỚI | 75 dòng | Validation/immutable input |
| src/GeoQuad.Web/Areas/ApDung/Views/GoiY/Index.cshtml | MỚI | 120 dòng | Form, ✓/✗, 360px |
| tests/GeoQuad.Tests/B_ApDung/Doubles/GoiYRepositoryGia.cs | MỚI | 40–60 dòng | Fake DTO cho service |
| tests/GeoQuad.Tests/B_ApDung/GoiYTests.cs | MỚI | 180 dòng | Unit fake typed + CurrentUserGia |
| tests/GeoQuad.Tests/B_ApDung/Integration/GoiYIntegrationTests.cs | MỚI | 180 dòng | Cypher thật, HTTP |
| docs/cypher/B-ap-dung.md:32 | Bổ sung US-13 | 100 dòng | Query + ký hiệu + kết quả |

Không xóa file. Không sửa Program, Infrastructure, csproj, CSS dùng chung hay phần A/C.

## Tests Before — RED

Viết kỳ vọng trước, chạy để thấy thất bại vì placeholder/logic chưa có; không dùng `Assert.True(true)` làm bằng chứng.
Unit dùng GoiYRepositoryGia typed của phase này và CurrentUserGia phase01; integration dùng Neo4j riêng và host chạy Program thật.

| Mức | Tầng | Case | Kết quả bắt buộc |
|---|---|---|---|
| Critical | Integration + unit | HBH + một góc vuông → HCN | `DH_HCN_2` đầu, thiếu 0 |
| Critical | Integration | HBH, không điều kiện → thoi | Đủ `DH_THOI_1..4`, mỗi dấu hiệu thiếu 1 |
| Critical | Unit + DB | Vuông → HCN; cùng hình | Thông báo 5a, không chạy fallback |
| High | DB | Tứ giác + cạnh đối bằng → HBH | `DH_HBH_2` thiếu 0 |
| High | DB | Không trực tiếp, có chuỗi 2/3 bước | ≤3 chuỗi, ≤3 bước, ghi rõ thiếu từng bước |
| High | DB | Nền đầu kế thừa; chuỗi có bước lớp cao/chưa rà soát | Kế thừa hợp lệ; bước bị chặn không lọt |
| High | HTTP | Mã lạ, điều kiện giả, lớp 3/nâng cao | Input báo lỗi; lớp thấp ẩn, nâng cao có nhãn |
| Medium | Unit | Không chuỗi, tie, điều kiện trùng | 5c + link bản đồ; tie theo mã; deduplicate |
| Medium | HTTP/manual | Thiếu proof, DB hỏng, màn hình 360px | Ẩn link; lỗi thân thiện; nút ≥44px (48px cấp 1), không tràn |

## Implementation và Refactor

1. Rescout và xác nhận counts seed phase01; đưa case RED vào test suite.
2. Viết repository trực tiếp/suy ra/chuỗi hằng; tích hợp DB thật trước khi làm UI.
3. Dựng service thuần, request validator và model; giữ rõ ranh giới dữ liệu có/thiếu.
4. Đăng ký scoped tại Area module, nối GET với service, tạo view accessible tiếng Việt.
5. Refactor phần sort/tập điều kiện dùng chung trong service; không gộp state request vào singleton.
6. Ghi từng truy vấn với giải thích từng ký hiệu và kết quả seed thực chạy vào docs B.

## Tests After và regression gate

Chạy từ root, dùng .NET8; không thay TargetFramework/roll-forward để coi là pass. Fixture DB thiếu phải FAIL rõ, không skip.

```sh
dotnet build GeoQuad.sln --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration'
```

Integration kế thừa environment fixture phase01, serial collection; HTTP chạy child Program thật ở Production với Neo4j test override.
Sau GREEN, kiểm thử lại bốn AC README:507 và luồng chuỗi; ghi số case và kết quả thật, không ghi pass theo suy đoán.

## Todo và done đo được

- [x] RED đủ direct/inheritance/chain/filter/input; GREEN unit và DB thật.
- [x] Tất cả AC US-13, 5a/5b/5c đúng; mỗi query có docs và kết quả thực.
- [ ] 360px/keyboard/nâng cao và link hợp lệ; chưa có proof ẩn nút.
- [x] Diff chỉ ownership B; không state dùng chung; bàn giao phase03/07.

## Rủi ro, bảo mật, tương thích và rollback

| Rủi ro | Likelihood × impact | Mitigation |
|---|---|---|
| Đảo chiều đặc biệt hóa/chuỗi rò lớp | Cao × Cao | DB matrix HBH/thoi + từng bước filter |
| Thiếu điều kiện nhưng trình bày như chứng minh | Vừa × Cao | Model phân thiếu mỗi bước, UI wording + test |
| Query chain không tương thích runtime | Vừa × Cao | Fixture Neo4j >=5.9 và execute query thật phase01 |
| Links A/C còn placeholder | Cao × Vừa | Ghi dependency docs B; phase07 kiểm tra cuối |

Cypher parameterized, validate ID catalog, Razor encode; không ghi học tập cho khách. Giữ route hiện hữu và schema seed hiện hữu.
Rollback chỉ revert code/view/test US-13 và phần docs tương ứng; seed phase01 giữ nguyên vì phase03/04 dùng; không xóa DB.

## Evidence triển khai 08/10/2026

Gợi ý direct/inherited/QPP và các gate DB/HTTP đạt; docs có query/params/output thật. Browser inventory trống nên chưa kiểm tra visual360px/keyboard. Link hợp đồng A/C được giữ, đích A đã tích hợp main95a8519; đích C còn controller khung. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
