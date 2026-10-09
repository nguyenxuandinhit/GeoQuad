---
title: "Phase 06 — US-23 tiến độ, US-24 gợi ý và KeTiep"
status: in-progress
priority: P1
effort: "7h remaining"
dependencies: [4]
---

# Phase 06 — US-23 tiến độ, US-24 gợi ý và KeTiep

## Contract / nguồn

README:699–737; SRS BR06/UC13. `[Authorize]` GET TienDo và GoiY. TaiKhoanId lấy từ ICurrentUser, nhưng account/lớp được xác minh qua DB HOC_LOP mỗi request cá nhân; cookie Lop cũ không quyết định eligibility. Account bị xóa thì challenge/signout, không render lịch sử trống giả. LopHienThi không mở rộng recommendation. Không nhận account id từ request.

- Tổng lượt, distinct bài, tỷ lệ đúng và thanh ngang theo khái niệm dùng ALL history, kể cả bài bị admin ẩn để giữ lịch sử (US25). Một DA_LAM đóng góp một lần cho mỗi concept dù query có nhiều path.
- Weak concept: riêng tối đa5 lượt mới nhất theo lucDESC, maLanDESC; quan hệ legacy thiếu maLan dùng elementId làm tie trong snapshot. Có≥3 lượt và tỷ lệ `< NguongCanOn`. Default.6; config parse fail/outside[0,1] fail validation lúc DI startup, không âm thầm dùng0.
- Một bài nhiều concepts có thể đóng góp mỗi concept một lần; phân biệt đúng-ever và correctness của lượt cuối. Dev thoi2/5=40% weak, HBH2lượt không weak.
- Gợi ý≤5 unique exercises public/reviewed/visible, lớp≤Lop thực, excludeCHUNG_MINH, chưa EVER correct. Nếu có weak: gắn≥1 weakconcept, khóASC+mãASC. Không có weak: fallback khóDESC+mãASC + link roadmap. Weak nhưng không có candidate: empty message ôn khái niệm/lộ trình, không giả congratulations.
- KeTiep student dùng cùng recommendation service, excludecurrent rồi takefirst; none→trang empty hợp lệ, không loop. Guest path ở phase4 giữ sameclass+codeASC. Recheck eligibility lúc redirect và LamGET.
- Empty history: lời mời làm bài; weak empty: chúc mừng + bài khó/lộ trình, tỷ lệ toàn0 thể hiện “chưa có dữ liệu”, không chia0. Chart HTML/CSS, có text ratio/count, không bắt buộc thư viện chart.

## Inventory

| ROOT/file | Action / size | Test impact |
|---|---|---|
| src/GeoQuad.Web/Areas/HocTap/Models/TienDoModels.cs | Create / M | History/summaries/weak/recommendation DTO |
| src/GeoQuad.Web/Areas/HocTap/Services/HocTapOptions.cs | Create / S | Typed default.6/config bounds |
| src/GeoQuad.Web/Areas/HocTap/Repositories/{ITienDoRepository,TienDoRepository}.cs | Create / L | Own history, recentwindow, candidate excludes |
| src/GeoQuad.Web/Areas/HocTap/Services/{TienDoService,GoiYService}.cs | Create / L | Pure BR06 + shared next suggestion |
| src/GeoQuad.Web/Areas/HocTap/Controllers/TienDoController.cs | Replace stubs / M | Auth Index/GoiY |
| src/GeoQuad.Web/Areas/HocTap/Controllers/BaiTapController.cs | Extend KeTiep / S | Student first suggestion |
| src/GeoQuad.Web/Areas/HocTap/Views/TienDo/{Index,GoiY}.cshtml | Create / M | SCR16, chart/text/empty links |
| src/GeoQuad.Web/Areas/HocTap/HocTapModule.cs | Extend / S | Bind IConfiguration section without shared appsettings edit |
| tests/GeoQuad.Tests/C_HocTap/{TienDoServiceTests,GoiYServiceTests}.cs | Create / L | Threshold/ordering/evercorrect |
| tests/GeoQuad.Tests/C_HocTap/Integration/TienDoRepositoryTests.cs | Create / M | DA_LAM identity/multipleconcept/window |
| tests/GeoQuad.Tests/C_HocTap/us23_us24_http_smoke.py | Create / M | Own history/auth/next/privacy |
| docs/cypher/C-hoc-tap.md | Update / S | Recent5 vs allhistory Cypher explanation |

## Tests Before — RED

| Case | Expected |
|---|---|
| 0/2/3/5/6 lượt perconcept | No data/notweak/threshold applies/latest5 only |
| exactly.6,2of5=.4; config invalid | Notweak/weak/startup diagnostic |
| Alltime7of10 vs last2of5 | Chart70%, weak40%; không trộn cửa sổ |
| Timestamp tie,2concept links | Stable selection, no duplicate attempts/cards |
| Correctonce then wrong | Excluded recommendation forever until data reset |
| >5 candidates, weak2concepts | ≤5 unique, normalASC+ma |
| No weak, weak no candidates | FallbackDESC+roadmap; empty ônstate riêng |
| Hidden/NHAP/higherclass/proof/advanced on | No candidate; history vẫn giữ hidden |
| Another user's history, guest, forgeduserid, stale cookie sau phiên khác đổi lớp/xóa user | No exposure/redirect/ignore id, lớp DB hoặc challenge |
| KeTiep candidate=current/none | Exclude current, first eligible or safeempty |
| Zero denominator, screenreader/360px | No NaN, accessible chart text, no overflow |

## GREEN / REFACTOR

1. Test BR06 và normal/fallback ordering trước code; implement pure threshold/window logic với clock/tie contract rõ.
2. Repository query own histories/eligible candidates, count distinct relation identity perconcept; document OPTIONAL MATCH empty-history behavior.
3. Service tiến độ ALL vs last5, GoiY shared; config typed validated.
4. Auth controllers+views; nối student KeTiep, check ma/eligibility server. Không nhân bản thuật toán recommendation trong controller.
5. Refactor DTO và query projections; giữ history sau hidden, không leak khóa chấm từ candidate DTO.

## Tests After / Tasks

`dotnet test tests/GeoQuad.Tests --filter "FullyQualifiedName~C_HocTap&Category!=Integration"`; C DB/HTTP theo execution contract. Dev seed chỉ chạy khi đã tạo hocsinh8 Development, không import vào production. HTTP scripts tạo fixtures UUID và cleanupfinally, kiểm tra guest/account separation. UI360px/desktop chart/gợiý/empty/KeTiep.

- [ ] RED BR06/window/allhistory/order/exclusion/config/privacy.
- [x] GREEN repository/services/options/auth/views và student KeTiep.
- [ ] REFACTOR; dev40% và HTTP/DB evidence; integration roundtrip US20→US23→US24.
- [ ] UI/cross-review; US23/24 và KeTiep AC đủ mới đánh dấu done.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.

## Execution evidence — 2026-10-08

- TDD progress tests: 3/3 PASS; recommendation tests: 2/2 PASS; full C unit suite: 58/58 PASS. US-23/24 HTTP/Neo4j: PASS for all-time 70%, latest-five 40%, ever-correct exclusion, <=5 unique recommendations, and student `KeTiep`.
- Remaining before phase acceptance: broader edge-case matrix, browser 360 px/desktop and independent review.
