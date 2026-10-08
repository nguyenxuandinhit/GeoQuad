---
phase: 1
title: "Khung kiểm thử, seed B và review nội dung"
status: pending
priority: P1
effort: "4h"
dependencies: []
---

# Phase 1 — Khung kiểm thử, seed B và review nội dung

## Overview

Quân xác minh khung chung, dựng môi trường test độc lập rồi bổ sung seed B theo TDD. Phase chi tiết đầu tiên của `--deep`; source hiện có đủ giao diện `IGraphDb` và đăng ký DI, chưa có repository/service B. Không khởi tạo lại PHẦN 0.

## Context và yêu cầu

- Jira: US-02 parent 14/subtask 15; US-03 parent 16/subtask 18; US-04 parent 20/subtask 23; US-07 parent 29/subtask 30. B là owner US-02/03; A owner US-04/07.
- [README §5–6](../../README.md): seed 14 điều kiện, 20 dấu hiệu, 6 định lý nền; 4 chứng minh × 3–6 bước.
- [SRS §4, B.3/B.4/B.7/B.9](../../docs/GeoQuad_SRS.md): BR-08/13, NFR-05/08/09/10.
- [IGraphDb](../../src/GeoQuad.Web/Infrastructure/Neo4j/IGraphDb.cs), [GraphDb](../../src/GeoQuad.Web/Infrastructure/Neo4j/GraphDb.cs), [ICurrentUser](../../src/GeoQuad.Web/Infrastructure/Auth/ICurrentUser.cs) giữ nguyên.
- Inventory baseline: controller/module B khung, `KhungTests` chỉ Assert.True(true); seed A có 4 placeholder `DinhLy` mà B phải hoàn thiện; SRS Markdown/image là file user chưa commit, giữ nguyên.

## File inventory — Quân sở hữu

| Hành động | File | Cỡ dự kiến | Test impact |
|---|---|---|---|
| Tạo | `neo4j/seed/20-dinhly-B.cypher` | 150–250 dòng | Đủ mã/quan hệ và chạy lặp |
| Tạo | `neo4j/seed/21-chungminh-B.cypher` | 100–200 dòng | 4 mẫu, bước/căn cứ đúng |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml` | 35–60 dòng | Neo4j test riêng |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Integration/Neo4jFixture.cs` | 150–220 dòng | Container test, seed có thể đảo thứ tự |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Integration/SeedBTests.cs` | 120–180 dòng | Đếm và invariants thực tế |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Http/MayChuThu.cs` | 120–180 dòng | Chạy web thật/HTTP ở phase sau |
| Tạo | `tests/GeoQuad.Tests/B_ApDung/Doubles/CurrentUserGia.cs` | 30–50 dòng | Unit lớp/guest/nâng cao |
| Sửa | `tests/GeoQuad.Tests/B_ApDung/KhungTests.cs` | 10–30 dòng | Thay test mẫu bằng guard DI hợp lệ |
| Bổ sung | `docs/cypher/B-ap-dung.md` | 100–180 dòng | Truy vấn/nguồn/review/kết quả |

Không sửa `Program.cs`, csproj, `Infrastructure/**`, schema/seed khung, seed A/C. Test mới dùng reference web/xUnit sẵn có, không thêm Moq/TestHost/Testcontainers.

## Architecture và isolation

Compose test dùng project `geoquad-b-tests`, image cùng dòng Neo4j 5 Community, port 17687/17474, volume riêng; mount `../../../../neo4j/seed:/seed:ro` tính từ file compose trong Integration. Verify `CALL dbms.components()` >=5.9 trước test quantified paths. Community chỉ dùng database `neo4j` trong container riêng, không tạo database thứ hai trên dev server.

Fixture yêu cầu opt-in `GQ_B_INTEGRATION=1`, nhận `GQ_B_TEST_URI/USER/PASSWORD`, chỉ chấp nhận host loopback và port 17687; xác minh marker test/container project trước seed hoặc reset. Không có config thì **fail rõ ràng**, không skip silently. Integration/HTTP dùng cùng xUnit collection không song song, reset chỉ graph test sau khi guard isolation đạt. Test seed chạy file thực tế qua cypher-shell `-f` hoặc parser hiểu quote/comment, không `Split(';')`; không tự tạo data giống implementation rồi tự xác nhận. File compose authority là `Integration/compose.neo4j.yml`.

`MayChuThu` chạy Program thật bằng process `dotnet run --no-build --no-launch-profile --project src/GeoQuad.Web -- --urls http://127.0.0.1:15080`, env Production + Neo4j override vào fixture. Không chạy demo seeder; fixture tạo tài khoản test với AuthHelper thật. HttpClient giữ cookie, đọc token chống CSRF từ HTML và đăng nhập thật; không gắn role giả để thay kiểm chứng middleware. Dừng process/server sau test.

Seam unit: phase01 chỉ dựng fixture/current-user double; phase2/3/4/6 khai báo interface repository typed và fake tương ứng trong ownership của mình. Không fake `IGraphDb/IRecord` để thay typed service tests. Interface colocate trong file repository để giữ inventory nhỏ; DTO bất biến trong Models. Decorator lỗi ở phase6 chỉ bọc query runner thật để kiểm chứng rollback, không làm database giả.

## Tests Before — RED

1. Viết SeedBTests trước seed: assert chính xác tập 14/20/6 mã từ phụ lục; seed B thiếu thì test FAIL nêu file/nhãn thiếu, không skip.
2. Assert mỗi dấu hiệu có đúng một nền/kết luận/lớp, ≥1 điều kiện, nguồn/trạng thái hợp lệ. Kiểm tra `trangThai IS NULL` riêng, không chỉ `<>`.
3. CM-01..04 có giả thiết/kết luận, 3–6 bước, thứ tự 1..n, căn cứ; reachable concept qua CHUNG_MINH_CHO/KHANG_DINH đáp ứng BR-08 mà không thêm quan hệ schema mới.
4. Đọc schema/seed khung hiện hữu, assert 13 UNIQUE + fulltext và 3 cấp/12 lớp/7 hình/6 yếu tố/7 cạnh đặc biệt; không viết test chỉ so hai hằng với nhau.
5. Test skeleton module có thể đăng ký trên ServiceCollection, và CurrentUser helper cho guest lớp8/12-nâng cao đã có baseline; không nhân bản toàn bộ tests Chung.

## Test matrix

| Mức | Scenario | Kết quả mong đợi |
|---|---|---|
| Critical | Seed thiếu file B | RED bằng thiếu file/nội dung |
| Critical | A→B→A→B, snapshot node/edge | Số/tập khóa, hướng/thuộc tính quan hệ không đổi |
| Critical | B→A trên graph test mới | Placeholder TC_* được A hoàn thiện, không nhân đôi |
| High | Placeholder DH_* từ A | B gắn nhãn/thuộc tính đúng, giữ quan hệ AP_DUNG |
| High | Mỗi mẫu CM và mọi căn cứ | Liên kết đúng mã, nội dung không trống; bước liên tục |
| High | Trạng thái NHAP/chưa C review | Không bật DA_RA_SOAT/không hiện công khai |
| Medium | US-02/07 giải thích trước nhóm | Quân giải thích ràng buộc, MERGE, lớp cookie/nâng cao bằng query thật |

## Implementation Steps — GREEN

1. Scout lại branch/source; tạo nhánh `feature/B-ap-dung` khi bắt đầu code, cập nhật từ main theo quy trình nhóm; giữ thay đổi của người khác.
2. Có .NET 8 SDK/runtime hoặc dùng SDK .NET8 trong container; không đổi target net8.0, không dùng roll-forward làm gate phát hành. Mở Docker bằng thao tác user/environment được phép; xác minh daemon chạy.
3. Dựng fixture và seed schema/khung trước. Viết seed `20` dùng MERGE `DinhLy {ma}` rồi SET nhãn phụ/nội dung/nguồn; `DL_NEN_*` không gắn TinhChat/DauHieu; mỗi mục có THUOC_LOP.
4. Seed `21`: MERGE `ChungMinh`, `Buoc` theo mã ổn định `CM-01-B1`…; CO_BUOC {thuTu} từ1; CHUNG_MINH_CHO, CAN_CU đúng phụ lục. TC_* của A chỉ MERGE nhãn gốc + ma, không gán nội dung/nhãn phụ.
5. C review toán học và nguồn SGK/chương trình, ghi tên/ngày/query kết quả. Soạn trạng thái NHAP trước checkpoint; chỉ đổi thành DA_RA_SOAT sau chứng cứ review. Không tự coi SRS/README là nguồn độc lập chứng minh độ đúng toán học.
6. Chạy seed hai lần và đảo A/B trong graph test; ghi số nút/cạnh thực tế, không hardcode tổng có/không tài khoản demo.
7. Giải thích US-02/07, ghi query ký hiệu/kết quả và đề xuất sửa khung chung vào docs B.

## Refactor

Tách helper seed/execution/HTTP chỉ khi dùng lại thực tế; fixture đọc repository/seed thật, không chứa bản sao thuật toán. Chỉ refactor B tests/seed, giữ contracts lớp/mã cho A/C.

## Tests After và regression gate

Các lệnh sau chạy tại root repo; password dưới đây chỉ dành container test dùng một lần, không lấy mật khẩu dev:

```bash
dotnet --list-runtimes
dotnet build GeoQuad.sln --nologo --disable-build-servers -m:1
docker compose -p geoquad-b-tests -f tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml up -d --wait
export GQ_B_TEST_URI='bolt://127.0.0.1:17687'
export GQ_B_TEST_USER='neo4j'
export GQ_B_TEST_PASSWORD='geoquad_test_123'
export GQ_B_INTEGRATION='1'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category!=Integration' --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung.Integration.SeedBTests' --nologo
```

SeedBTests gắn `[Trait("Category", "Integration")]`; build, unit và DB test phải exit0, báo test count >0. Toàn solution unit regression dùng `--filter 'Category!=Integration'`; full integration chạy với fixture được cấu hình, không coi unit-only là full pass. Cleanup container test chỉ sau guard project/volume, không `down -v` dev.

## Todo và Success Criteria

- [x] RED seed test thất bại vì thiếu nội dung B, ghi output ngắn.
- [x] Fixture cô lập và các guard đạt; không mutation DB dev.
- [ ] Đủ 14 điều kiện +20 dấu hiệu +6 nền +4 mẫu; review C được ghi trước DA_RA_SOAT.
- [x] Lặp và đảo thứ tự không trùng, không mất liên kết A/C.
- [ ] US-02/07 kickoff và docs query có bằng chứng thực tế.
- [x] GREEN + regression đạt, module/fixture sẵn cho phase2–6.

## Risks, Security và Dependencies

- Docker/.NET8 thiếu: dừng gate runtime, báo prerequisites; không claim seed/test HTTP đã qua.
- A/C thay schema/seed sau planning: scout trước từng phase; khác contract thì ghi proposal, đồng bộ với owner rồi điều chỉnh plan.
- Các lỗi auth counter/demo multi-class và KaTeX escape tìm ở review trước thuộc phần0: ghi dependency/proposal, owner chung sửa. Không mở scope sửa chung; phase7 không đóng AC bị lỗi chung cản.
- C chưa review: trạng thái seed chưa được công bố; US-03 toàn dự án còn phụ thuộc A và checkpoint C.
- Kết quả phase1 mở khóa phase2,4,6; phase3/5 còn dependency cụ thể.

## Evidence triển khai 08/10/2026

Seed/schema/order/guard đã đạt trên .NET8/Neo4j thật. Seed B vẫn NHAP: C chưa ký review nguồn/toán học, US-02/07 kickoff chưa có reviewer/ngày. Phiếu review ở reports/content-review-B.md; test công khai chỉ mô phỏng trạng thái review trong graph test. Xem [execution-B](reports/execution-B.md) và [kiểm thử B](../../docs/kiem-thu.md).
