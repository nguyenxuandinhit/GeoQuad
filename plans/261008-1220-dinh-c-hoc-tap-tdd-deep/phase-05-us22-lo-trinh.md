---
title: "Phase 05 — US-22 lộ trình cá nhân"
status: in-progress
priority: P1
effort: "6h remaining"
dependencies: []
---

# Phase 05 — US-22 lộ trình cá nhân

## Contract / nguồn

README:679–697; SRS UC12:445–456. `[Authorize]` GET `/HocTap/LoTrinh?muc={ma}` và POST `/HocTap/LoTrinh/EmDaHieu`; dùng lớp DB hiện tại qua HOC_LOP, preview không mở rộng target. ICurrentUser cung cấp identity, cookie Lop chỉ là snapshot; account không tồn tại thì signout/challenge. Chọn target mặc định KhaiNiem loại HINH có lớp cao nhất≤Lop, tie mã tăng. Đọc enum/property thật từ A seed lúc scout, không tự đặt property graph mới.

Query trả nodes + cạnh `target -[:CAN_BIET_TRUOC]-> prerequisite` reachable tối đa số khái niệm schema hiện tại (13, scout lại). Public chỉ reviewed và lớp phù hợp. Nếu reachable có prerequisite NHAP/vượt lớp/thiếu node: báo lộ trình chưa đủ dữ liệu, không âm thầm bỏ bước để giả đúng. Không expose tên/nội dung NHAP trong lỗi.

Service Kahn topo: cạnh đồ thị xử lý là prerequisite→dependent; tie ma tăng, unique nodes; mục tiêu cuối. Cycle hoặc vượt bound báo lỗi an toàn; không infinite traversal. DAG diamond/chain bất cân bằng phải đúng, không chỉ sort max(depth) rồi hy vọng. Target độc lập: một bước + “có thể học ngay”.

View mỗi bước có checkmark, link `/KienThuc/ThuVien/ChiTiet/{ma}`, nút44px “Em đã hiểu”; phần trăm số distinct DA_HOC trong lộ trình / số bước, target cũng tính. POST nhận ma+muc, revalidate account/class/trạng thái/membership ở server; Lock account theo cùng thứ tự phase4, rồi recheck account/HOC_LOP/membership trong transaction; MERGE DA_HOC, ON CREATE luc, repeated POST không đổi luc. Concurrent POST phải đúng một DA_HOC, dùng cùng lock primitive phase4. PRG về muc an toàn nội bộ, không nhận returnUrl ngoài.

## Inventory

| ROOT/file | Action / size | Test impact |
|---|---|---|
| src/GeoQuad.Web/Areas/HocTap/Models/LoTrinhModels.cs | Create / M | Target/nút/cạnh/state DTO |
| src/GeoQuad.Web/Areas/HocTap/Repositories/{ILoTrinhRepository,LoTrinhRepository}.cs | Create / L | Read graph, membership, MERGE |
| src/GeoQuad.Web/Areas/HocTap/Services/LoTrinhService.cs | Create / L | Default target, topo, percent/errors |
| src/GeoQuad.Web/Areas/HocTap/Controllers/LoTrinhController.cs | Replace stub / M | Authorize/CSRF/EmDaHieu |
| src/GeoQuad.Web/Areas/HocTap/Views/LoTrinh/Index.cshtml | Create / M | SCR15, links/checkmarks/% |
| src/GeoQuad.Web/Areas/HocTap/HocTapModule.cs | Extend / S | Register C services |
| tests/GeoQuad.Tests/C_HocTap/{LoTrinhThuTuTests,LoTrinhServiceTests}.cs | Create / M | Pure topology + fake repos |
| tests/GeoQuad.Tests/C_HocTap/Integration/LoTrinhRepositoryTests.cs | Create / M | Persistence/class/transaction |
| tests/GeoQuad.Tests/C_HocTap/us22_http_smoke.py | Create / M | Auth cookie/CSRF/PRG |
| docs/cypher/C-hoc-tap.md | Update / S | Traversal/MERGE query explanations |

## Tests Before — RED

| Case | Expected |
|---|---|
| hocsinh8 target vuông | Nền/HCN/thoi trước vuông, vuông cuối; unique |
| hocsinh4 default/options + advanced on | Không hình thang cân lớp6; default stable eligible shape |
| Diamond, uneven chain, no prereq | Topo correct/dedup; one-step message |
| Cycle/self-loop/bound exceeded | Controlled data error, no hang/partial fake roadmap |
| Missing/invalid/NHAP/outofclass target | NotFound hoặc error gate; no leaked unpublished content |
| NHAP/outofclass prereq | Incomplete-roadmap error, no unsafe mark |
| Learned0/half/all; duplicate relation fixture | Percent0/appropriate/100 distinct concepts |
| GuestGET/POST, missingCSRF, otheraccount | Login redirect/400/no unauthorized writes |
| Mark twice/concurrent/reload, ma outside target closure | One DA_HOC, luc unchanged, persisted✓; outside reject |
| Hai phiên: account deleted hoặc class changed ở phiên A, phiên B cookie cũ GET/POST | DB grade gate/challenge; recheck blocks stale mutation |

## GREEN / REFACTOR

1. Tests topo và default target RED; implement typed graph + Kahn.
2. Query graph không lộ drafts; bổ sung query metadata đủ phát hiện prerequisites bị lọc thay vì mất silent.
3. Implement service states and profile real-grade gate; auth controller và idempotent POST transaction recheck.
4. View target selector, ordered steps, progress, errors/nohistory/independent; links A existing contract.
5. Refactor topology thuần khỏi repository; document symbol `*0..`, DISTINCT và chiều cạnh.

DB/HTTP phụ thuộc scaffold F trong execution contract; F có thể tạo độc lập trước khi phase4 business code hoàn tất. RED unit US22 không phụ thuộc phase4.

## Tests After / Tasks

`dotnet test tests/GeoQuad.Tests --filter "FullyQualifiedName~C_HocTap.LoTrinh&Category!=Integration"`; DB/HTTP C theo execution contract. Browser360px/desktop chọn vuông và reload✓, guest redirect, preview gate.

- [ ] RED topology/default/gate/persistence cases.
- [x] GREEN graph repository + topology service + Authorize/CSRF/POST/view.
- [ ] REFACTOR và Neo4j/HTTP/UI evidence, query Cypher được giải thích.
- [ ] AC US22 đạt độc lập; finding source A giữ gate dữ liệu tương ứng nếu ảnh hưởng.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.

## Execution evidence — 2026-10-08

- TDD topology tests: 4/4 PASS; service tests: 3/3 PASS; US-22 HTTP/Neo4j: PASS for auth, target/prerequisite traversal, CSRF, persisted `DA_HOC`, and idempotent timestamp.
- Remaining before phase acceptance: browser 360 px/desktop and independent review.
