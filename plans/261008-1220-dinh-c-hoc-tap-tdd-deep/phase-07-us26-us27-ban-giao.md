---
title: "Phase 07 — US-26 kiểm thử chéo và US-27 demo"
status: in-progress
priority: P1
effort: "5h remaining"
dependencies: [1, 2, 3, 4, 5, 6]
---

# Phase 07 — US-26 kiểm thử chéo và US-27 demo

## Scope / nguồn

README:741–752,770–787; CSV:309/312/324. C kiểm thử A (US09/10/11/12/15/16), B kiểm thử C, C sửa lỗi riêng. C sở hữu docs/demo.md; setupREADME≤15phút do A và backup/restore do B, C điều phối/ghi evidence, không thay phần demo bằng đóng US27 cha.

## Inventory

| ROOT/file | Action / size | Evidence |
|---|---|---|
| docs/kiem-thu.md | Complete / L | Story/AC/pass/output/notes/reviewer |
| docs/demo.md | Create / M | Kịch bản README12, thời lượng, người thao tác |
| docs/cypher/C-hoc-tap.md | Complete / M | Queries thật và giải thích/output |
| tests/GeoQuad.Tests/C_HocTap/** | Regression/fix / M | C failures sửa theo RED |
| src/GeoQuad.Web/Areas/HocTap/** | Fix only failing AC / M | Preserve unrelated WIP |
| docs/cypher/A-tra-cuu.md | Read/review / S | ≥2 query A chạy trong Neo4j Browser |
| plans/.../reports/shared-coordination.md | Update / S | A setup/B backup/crossreview status |

## Tests Before — RED matrix

| Case | Expected |
|---|---|
| C→A mọi AC6stories, default/advanced/guest | Ghi riêng từng AC, fail có reproduction/owner |
| ≥2 Cypher A trong Neo4j Browser | Output và giải thích symbol, không thay bằng shell-only |
| B→C independent review | Có người review/output, không tự ký hộ |
| Critical signup/lookup/suggestion/exercise failure | Must gate fail, sửa owner rồi retest |
| Demo devuser/login/2bài/proof/roadmap/progress | Không NHAP/hidden exposed, history cập nhật đúng |
| 360px+desktop44px/keyboard/KaTeX | Screenshot/evidence thật, không mark từ HTML-only |
| Setup/backup group incomplete | US27 parent còn mở; C demo có thể done riêng |

## GREEN / REFACTOR

1. Lập bảng AC C→A đọc toàn bộ source stories, cùng A xử lý finding; chạy ≥2 CypherA trong Browser và giải thích từng ký hiệu/result.
2. B review C; C tạo RED reproduce riêng rồi sửa đúng file ownership, retest, không merge/commit thay người khác.
3. docs/demo.md: data prep/versions/ports; C loginhocsinh8, làm2bài (MCQ+số), BT027 mẫu, targetvuông, weak40%/recommendations, đổi biệt danh; ghi thời lượng thật, expected result và rollback testdata.
4. Tránh demo2bài làm thay đổi dev40% ngoài dự kiến: dùng accountdemo riêng cho làm bài hoặc reset riêng DBdemo trước đoạn progress; không reset DB đang dùng thật.
5. Rehearsal full team: từng người query/explain; A setup≤15phút, B backup/restore, C demo; ghi thời gian/owner/blocker.
6. Refactor docs để output thật và expected tách rõ, đảm bảo nguồn/review/README/SRS không mâu thuẫn.

## Tests After / commands

`dotnet build GeoQuad.sln` (đường dẫn đã xác nhận, scout lại nếu repo đổi). Unit C theo execution contract; full solution unit filter loại Integration. B DB suite chỉ chạy với fixture/env riêng của B, không kéo vào C DB. Seed2lần và HTTP/UI C có evidence sau code freeze. Không tự chạy các lệnh này trong phiên lập plan.

## Tasks / AC

- [x] Draft `docs/demo.md` with C demo sequence and explicit open gates.
- [ ] C→A đủ6stories và ≥2 queries Browser; findings+owner+retest.
- [ ] B→C crossreview, mọi lỗi nghiêm trọng C sửa/retest.
- [ ] Full C unit/DB/HTTP và UI360px/desktop, docs completed.
- [ ] docs/demo.md + rehearsal/thời gian/evidence C; phối hợp A/B demo.
- [ ] Review chính người thứ hai theo DoD; story/subtask trạng thái dựa evidence, không đổi Jira nếu chưa được yêu cầu.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.

## Execution evidence — 2026-10-08

- Added `docs/demo.md` with an 8–10 minute C demo flow and safe demo-data reset guidance.
- Group rehearsal, A query review in Neo4j Browser, B↔C cross-review, and actual 360 px/desktop screenshots have not happened in this session; keep these acceptance tasks open.
