---
title: "Phase 02 — US-04 C, US-08 và proof gate"
status: in-progress
priority: P1
effort: "3h remaining"
dependencies: []
---

# Phase 02 — US-04 C, US-08 và proof gate

## Mục tiêu

Đóng phần seed C và phần hồ sơ; lập lịch review4 proof B21 để AC BT027→CM01 có thể được nghiệm thu. Nguồn README:324–328,620–633,670–677; SRS BR08/09. US04 story cha gồm dữ liệu A/B ngoài phần C.

## Inventory

| ROOT/file | Action / size | Impact |
|---|---|---|
| neo4j/seed/30-baitap-C.cypher | Verify / S | BT011..030, toán học và phân bố |
| neo4j/dev/31-lam-bai-mau-C.cypher | Verify / S | 7 lượt, maLan idempotent; không production |
| neo4j/seed/21-chungminh-B.cypher | Review with B / M | 4 proof, CM04 chiều suy luận; B sửa/promote sau evidence |
| src/GeoQuad.Web/Areas/HocTap/Controllers/HoSoController.cs | Verify/fix if fail / S | Binding prefix, auth, CSRF |
| src/GeoQuad.Web/Areas/HocTap/Models/HoSoModels.cs; Repositories/{IHoSoRepository,HoSoRepository}.cs; Services/{IHoSoService,HoSoService}.cs; Views/HoSo/Index.cshtml | Verify / M | Class cookie, password, deletion |
| tests/GeoQuad.Tests/C_HocTap/HoSoServiceTests.cs | Extend / S | Fake repository/current user |
| tests/GeoQuad.Tests/C_HocTap/us08_http_smoke.py | Extend / M | Cookie, CSRF, graph deletion |
| docs/cypher/C-hoc-tap.md | Update / S | AC thật và proof blockers |

## Test matrix / RED

| Case | Expected |
|---|---|
| C seed counts/types/cấp/thoi; repeated seed | 20=7/10/3, cấp2=15/cấp3=5, thoi≥5, không tăng graph |
| Mỗi bài/tình huống/proof | Có đường liên kết khái niệm đúng BR08 |
| Dev rerun | 7 lượt, thoi gần5 đúng2=40%; MATCH user thật, không tạo id=hocsinh8 |
| Đổi lớp7→8 | Cookie cấp lại, thư viện lớp mới; lớp ngoài miền reject |
| Password cũ sai, input trống; guest/cross-account | Không đổi hoặc xóa; không tin tk từ request |
| Thiếu CSRF POST | 400, graph không đổi |
| Xóa user có DA_LAM/DA_HOC | Node/lịch sử gone, logout; không xóa tài khoản khác |
| CM01/CM04 NHAP, lỗi suy luận hoặc thiếu nguồn | Không public link; ghi finding gửi B |
| UI360px/desktop | Không tràn ngang; controls≥44px, errors đọc được |

## GREEN và REFACTOR

1. Giữ seed đã push và evidence DB30/9/4. Kiểm toán toán học BT011..030 nếu seed bị sửa sau baseline.
2. Review B21 từng bước với nguồn nội bộ; CM04 phải phân biệt định lý thuận và đảo/điều kiện cần. Ghi rationale; phối hợp B sửa và duyệt, không dùng test-fixture promote để đóng AC.
3. Bổ sung test CSRF và xóa user đã có lịch sử vào HTTP harness cô lập; sửa C nếu test đỏ.
4. Kiểm tra360px/desktop profile bằng browser thật và đánh dấu evidence. Không có browser thì giữ task mở.
5. Refactor binding/validation nếu cần, không đổi hợp đồng auth shared.

## Tests After

`dotnet test tests/GeoQuad.Tests --filter "FullyQualifiedName~C_HocTap.HoSoServiceTests"`.
HTTP hiện có: `python tests/GeoQuad.Tests/C_HocTap/us08_http_smoke.py <C_CONTAINER> <C_HTTP_URL>` sau isolation guard và cấu hình execution contract. Static seed contract: `python tests/GeoQuad.Tests/C_HocTap/seed_contract.py`. Ghi count/output thật; full US04 chỉ pass khi4 proof production được rà soát.

## Tasks và AC

- [x] Seed C push/merge và DB toàn nhóm30/9/4 có evidence; dev rerun ổn định.
- [x] US08 unit + HTTP register/đổi lớp/password/xóa/logout đã có evidence.
- [ ] Review4 proof, gửi B sửa CM04 nếu cần, xác nhận BT027→CM01 public sau duyệt.
- [ ] Bổ sung CSRF/xóa lịch sử/cross-account và chụp UI360px/desktop.
- [ ] Ghi C seed subtask đạt riêng; không đóng US04 cha bằng counts khi proof chưa duyệt.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.
