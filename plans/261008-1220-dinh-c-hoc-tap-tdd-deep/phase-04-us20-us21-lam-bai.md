---
title: "Phase 04 — US-20 làm/chấm bài và US-21 chứng minh mẫu"
status: in-progress
priority: P1
effort: "8h remaining"
dependencies: []
---

# Phase 04 — US-20 làm/chấm bài và US-21 chứng minh mẫu

## Mục tiêu / contract

README:652–677; SRS:440–443,517, BR05/09. US20 Must chấm MCQ/số; US21 Should xem proof, không chấm tự động.

- GET `Lam/{id}` trả đề/options/đơn vị/loại và token lượt làm được server bảo vệ; DTO không có đáp án, giải thích, loiGiaiMau. Không nhúng bí mật trong HTML/data/script.
- POST `Lam/{id}` + anti-forgery nhận lựa chọn hoặc số, đơn vị, token; lấy khóa chấm từ DB server. Fields dung/tk/lop/dapAnDung do client gửi bị bỏ qua.
- Số nhập một dấu `,` hoặc `.`, trim; không thousand separator, NaN/Infinity/exponent. Dùng decimal và sai số seed (default.01), biên inclusive. Âm là số hợp lệ để chấm sai; BR10 đo hình không áp dụng nhầm cho đáp án số tổng quát.
- UI số gắn đơn vị cố định do đề yêu cầu; POST đơn vị phải khớp chính xác sau trim (BT028 không đơn vị thì chuỗi rỗng). Không hỗ trợ đổi đơn vị trong R1. Input rỗng/sai format trả lỗi, không lưu lượt.
- Token protected bằng IDataProtectionProvider chứa maLan random, bài, owner/guestNonce, openedAtUTC. GuestNonce là cookie random được C bảo vệ, HttpOnly/SameSiteLax/Secure khi HTTPS; không yêu cầu AddSession/UseSession. Cookie mất/đổi thì token invalid. Thời gian server hiện tại−openedAt, finite integer0..86400; token quá24h/bị sửa/sai bài/user từ chối. Dùng TimeProvider injectable để test, không tin clock client.
- Một lượt mới hợp lệ có DA_LAM riêng theo BR09; retry cùng maLan chỉ ghi một quan hệ, token mới là lượt mới. Kết quả lần đầu immutable: ON CREATE ghi đáp án/dung/luc/giây; replay cùng payload trả persisted result, khác payload trả409 “lượt đã nộp”, không ghi đè hay hiển thị kết quả vừa chấm khác history. Ghi trong transaction sau recheck account+bài visible/reviewed/lớp; failure DB không thông báo đã lưu.
- POST→PRG: GET kết quả bằng maLan+owner đã xác nhận; khách dùng CookieTempData mặc định chỉ chứa payload nhỏ được bảo vệ (maLan/bài/dung/đáp án≤64ký tự/nonce/time), bind guestNonce; giải thích/khóa đáp án lấy lại server sau khi xác minh result và public gate, không tạo DA_LAM. Guest refresh khi TempData hết thì quay về làm bài, không tự chấm/save lại. Không cho đoán maLan xem kết quả người khác.
- POST `XemLoiGiai/{id}` + CSRF chỉ cho CHUNG_MINH: sau click mới lộ lời giải, không chấm/không DA_LAM; link `/ApDung/ChungMinh/Xem/{ma}` chỉ tới proof reviewed và trong lớp hiển thị.
- GET `KeTiep/{id}`: guest cùng lớp của bài hiện tại, mã lớn hơn, ORDER ma; cuối danh sách hiển thị hết bài và link danh sách, không wrap. Học sinh dùng đầu US24 sau phase6, loại bài hiện tại; trước đó giữ dependency mở, không giả pass.

Dependency theo task: tái sử dụng backend US19 đã có; UI/cross-review phase3 không chặn RED/GREEN US20. Task F chặn DB/HTTP; link proof B và student KeTiep có gate riêng.

## Inventory

| ROOT/file | Action / size | Tests |
|---|---|---|
| src/GeoQuad.Web/Areas/HocTap/Models/{BaiTapChiTiet,LamBaiViewModel,BaiTapNopInput,KetQuaLamBai}.cs | Create / M | Public/private DTO split |
| src/GeoQuad.Web/Areas/HocTap/Repositories/{ILamBaiRepository,LamBaiRepository}.cs | Create / L | Gate query + atomic attempt persistence |
| src/GeoQuad.Web/Areas/HocTap/Services/{ChamBai,LamBaiService,LanLamTokenService}.cs | Create / L | Grading/token/guestcookie+size/service fake tests |
| src/GeoQuad.Web/Areas/HocTap/Controllers/BaiTapController.cs | Extend / M | Lam GET/POST, XemLoiGiai, KetQua, KeTiep |
| src/GeoQuad.Web/Areas/HocTap/Views/BaiTap/{Lam,KetQua}.cshtml | Create / M | KaTeX/options/results/proof button |
| src/GeoQuad.Web/Areas/HocTap/HocTapModule.cs | Extend / S | C DI only |
| tests/GeoQuad.Tests/C_HocTap/{ChamBaiTests,LamBaiServiceTests,LanLamTokenTests}.cs | Create / M | ≥6 grading cases + failure matrix |
| tests/GeoQuad.Tests/C_HocTap/Integration/LamBaiRepositoryTests.cs | Create / M | Recheck/retry/new attempts/isolation |
| tests/GeoQuad.Tests/C_HocTap/us20_us21_http_smoke.py | Create / M | Cookie/CSRF/payload/PRG |
| docs/cypher/C-hoc-tap.md | Update / S | Query write history + explanation symbols |

Không thêm package MVC.Testing mặc định; tái sử dụng cách HTTP thật của C. maLan có sẵn ở dev seed, dùng cùng property; transaction retry dùng token stable, không random lại trong callback. Giới hạn parallel cùng token bằng lock trên TaiKhoan trong transaction trước MERGE; verify Neo4j5 lock bằng DB test (dummy property rồi REMOVE trong transaction). Nếu retry vẫn duplicate, dừng persistence task và chỉnh chiến lược lock trước GREEN.

## Tests Before — RED matrix

| Case | Expected |
|---|---|
| MCQ B, b/trim; A/C/D; E/missing | Correct; wrong; validation/no-save tương ứng |
| 96,0 và96.0,95,96±.01, ngoài±.01 | Correct/correct/wrong/inclusive/outsidewrong |
| Invalid parse, thousand separator, NaN/Infinity, empty | Validation; no-save/no-explanation |
| Sai đơn vị, BT028 unitless | Unit mismatch wrong; unitless đúng theo số |
| GET + invalidPOST payload | Không answer/giải thích/proof sample |
| Correct/incorrect authenticated vs guest | Auth DA_LAM đủ luc/dung/dapAnDaChon/thoiGianGiay/maLan; guest0 |
| Token tamper/expire/cross-user/guest cookie mất hoặc đổi,2tab,resultcookie size limit; thiếu CSRF | Reject, graph unchanged; CSRF400 |
| Bài bị ẩn/NHAP/đổi lớp; account deleted afterGET | Không save/orphan/false success |
| Retry same token/payload, same token khác payload sequential+parallel, new token | 1 attempt/result immutable; khác payload409; token mới tạo lượt mới |
| Proof button, CM01 NHAP/reviewed; proof POST grading | Button reveals sample only; link gated; no grade/history |
| KeTiep guest mã cuối | Friendly empty, no loop |

## GREEN steps

1. Viết ChamBai pure và fake service tests trước code; test RED vì lớp/method chưa có.
2. Implement parsing/grading, tách model key riêng khỏi public DTO, làm token service clock injectable.
3. Implement parameterized detail/proof link/read key; transaction rechecks + lock/MERGE maLan. Match user ID server, không CREATE user.
4. Implement Lam/POST/PRG/result auth/guestNonce gate, validation, guest path và proof click.
5. Implement guest KeTiep và interface recommendation hook để phase6 nối; ghi student path chưa nghiệm thu.
6. View mobile44px/options/number/units, KaTeX, answer after-submit; source/math links chỉ reviewed. Link missing proof hiển thị trạng thái phù hợp, không xuất NHAP.
7. Document query và ≥1 output Neo4j thật. B proof gate phối hợp phase2.

## REFACTOR / Tests After

Refactor chấm thuần tách persistence; không đưa answer vào MVC view model trước POST. Chạy `dotnet test tests/GeoQuad.Tests --filter "FullyQualifiedName~C_HocTap&Category!=Integration"`; DB/HTTP theo execution contract; mới tạo HTTP script dùng cùng `<C_CONTAINER> <C_HTTP_URL>` và guard chuẩn. UI360px/desktop + payload inspect, refresh/back/retry. Regression US08/19.

## Tasks và AC

- [ ] RED≥6 ChamBai cases và security/token/service matrix.
- [x] GREEN pure grading + private/public DTO + tokens.
- [ ] GREEN repository atomic attempts và DB lock/retry evidence.
- [ ] GREEN MVC/CSRF/PRG/guest/proof/guest KeTiep; student KeTiep chờ phase6.
- [ ] REFACTOR, DB/HTTP/UI evidence và doc Cypher US20/21.
- [ ] BT027→CM01 reviewed public sau B sửa/review; US20 nghiệm thu riêng được nếu US21 blocked.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.

## Execution evidence — 2026-10-08

- C suite: 58/58 PASS. Guarded HTTP/Neo4j smoke: PASS for GET/POST answer privacy, CSRF, guest no-save, server grading, immutable replay, proof gate.
- Remaining before phase acceptance: browser 360 px/desktop review, peer review, and B changing `BT-027`/`CM-01` from `NHAP` after its review.
