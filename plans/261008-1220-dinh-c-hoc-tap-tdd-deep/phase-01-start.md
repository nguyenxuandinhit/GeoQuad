---
title: "Phase 01 — Công việc chung, kickoff và rà soát US-03"
status: in-progress
priority: P1
effort: "2h remaining"
dependencies: []
---

# Phase 01 — Công việc chung, kickoff và rà soát US-03

## Mục tiêu và nguồn

US-03 C theo README:309–316/CSV:44; C điều phối US-01/05/06 theo README:56; kickoff đăng ký theo README:217/CSV:68. Nghiệm thu dữ liệu phải phân biệt tự biên soạn, rà toán học, khung chương trình và SGK. Người dùng đã xác nhận không có bộ SGK được chọn.

## Inventory

Đường dẫn tính từ ROOT được định nghĩa trong execution contract; kích thước là mức công việc còn lại S/M/L, không phải dòng code.

| File | Action / size | Test/evidence impact |
|---|---|---|
| docs/cypher/C-hoc-tap.md | Update / M | Kết quả thật mục “Rà soát US-03”, giải thích query |
| docs/kiem-thu.md | Update / S | Traceability, findings, người rà soát |
| tests/GeoQuad.Tests/C_HocTap/us03_review.py | Read, harden isolation nếu cần / S | Đếm và đối chiếu từng mã |
| neo4j/seed/10-kienthuc-A.cypher | Review only / S | CT_HBH_DT, CT_THOI_DT; A xử lý |
| neo4j/seed/20-dinhly-B.cypher | Preserve authorized review / S | 40 trạng thái đã đổi; không đổi bằng chứng thành SGK |
| plans/261008-1220-dinh-c-hoc-tap-tdd-deep/reports/shared-coordination.md | Create / S | Nội dung kickoff + trạng thái/evidence US01/05/06 |

`plans/...` ở đây là thư mục của plan hiện tại. Không đổi code auth của A/B chỉ để chứng minh C đã điều phối.

## Test matrix / Tests Before (RED)

| Case | Expected |
|---|---|
| Thiếu mã, sai count hoặc thiếu nguồn/trạng thái | Review fail, trả danh sách mã; không promote hàng loạt |
| Sai nền/kết luận/điều kiện dấu hiệu | Nêu sai quan hệ, không chỉ count nodes |
| A→B, B→A và seed lần 2 | Cùng graph, không nhân quan hệ |
| Lớp công thức A khác nguồn chương trình | Finding còn mở; không tự sửa lớp |
| Disclaimer thay nguồn đối chiếu | Không đủ review evidence |
| Kickoff chưa thực sự trình bày | Chưa hoàn tất task CSV68 |

## GREEN và REFACTOR

1. Đọc evidence hiện có; chạy lại `python tests/GeoQuad.Tests/C_HocTap/us03_review.py geoquad-c-us03-20261008` (container credential đã đọc qua `docker inspect`, không log secret). Kết quả 14/20/12/12/6 PASS; không sửa DB.
2. Ghi finding hai công thức lớp4 của A; phối hợp A quyết định nội dung mở rộng/lớp/nguồn và cập nhật appendix nhất quán.
3. Kiểm tra B20 sources không còn “chờ C đối chiếu SGK”, ghi phạm vi rà toán học và chương trình. Không xác nhận thay giáo viên.
4. Soạn rồi trình bày kickoff US05: TaiKhoan UUID, HOC_LOP, UNIQUE tên đăng nhập, hash password, lỗi tên trùng; lưu câu query/output và phản hồi người nghe.
5. Điều phối US01/05/06: yêu cầu owner đưa evidence compose/queryhome; register consent+duplicate+hash; lock5 lần/5phút và admin authorization. C ghi trạng thái, owner/follow-up; không tự đóng việc A/B.
6. Gộp docs lặp, bảo toàn output/thời điểm/môi trường và người review.

## Tests After / regression

Khi seed thực sự thay đổi, dùng DB C cô lập và lệnh trong execution contract; đọc docstring/source rồi chạy `python tests/GeoQuad.Tests/C_HocTap/us03_review.py geoquad-c-tests-neo4j` với env password và fixture đã được guard. Script hiện không hỗ trợ `--help`. Lưu count14/20/12/12/6, missing source/status=0, saiQuanHe=0, 2 lần idempotent. Kickoff có evidence trình bày, không thay bằng unit test.

## Tasks và AC

- [x] Chạy seed thật, hai thứ tự và lặp; review40 B20; evidence đã ghi ở C-hoc-tap.md.
- [ ] Đóng finding nguồn/lớp A bằng quyết định owner; đọc lại README yêu cầu review thống nhất với SRS NHAP→review.
- [ ] Ghi/kết thúc kickoff US05 với query và giải thích constraint.
- [ ] Hoàn thiện bảng điều phối US01/05/06 và evidence owner.
- [ ] C + người review xác nhận phần US03 riêng; story cha đợi đủ gate A/B.

Rủi ro: seed count đúng không chứng minh phát biểu đúng; Docker permission cũ không còn là lý do bỏ qua review. Gate unresolved ghi rõ, không gây chặn giả cho TDD US20/22.

## Deep scout và quy tắc evidence

Trước khi thực hiện: đọc lại HEAD/diff, file trong inventory, AC tại nguồn và trạng thái seed; ghi thay đổi hợp đồng vào phase trước khi code. Áp dụng [execution contract](research/execution-contract.md). Mỗi checkbox chỉ đánh dấu khi có lệnh/output hoặc evidence UI/kickoff; dữ liệu fixture được duyệt tạm không chứng nhận dữ liệu thật.
