# Truy vết toàn bộ phân công Quân/B

Nguồn: [CSV](../../../docs/GeoQuad_Jira_2ngay.csv) đọc bằng csv.DictReader/utf-8-sig để không vỡ Description nhiều dòng; [SRS](../../../docs/GeoQuad_SRS.md), [README](../../../README.md). Có **27 rows Assignee=Thành viên B**; Issue ID là khóa import CSV, không phải Jira issue key.

| Story / role | Issue ID CSV | B sub-tasks | Requirement / UC | Phase |
|---|---|---|---|---|
| US-02 owner chung | 14 | 15 | NFR08/10, seed khung/kickoff | 1 |
| US-03 owner chung | 16 | 18 | NFR09, 14 DK/20 DH/6 nền | 1 |
| US-04 B contributor, A owner | 20 | 23 | BR08, 4 CM/3–6 bước | 1 |
| US-07 B contributor, A owner | 29 | 30 | FR03/BR04/09, giải thích guest | 1 |
| US-13 owner | 51 | 52/53/54 | FR20, UC06, BR07 | 2 |
| US-14 owner | 55 | 56/57/58 | FR21, UC07 | 3 |
| US-17 owner | 66 | 67/68/69 | FR40, UC09 | 4 |
| US-18 owner | 70 | 71/72/73 | FR41, UC10, BR10 | 5 |
| US-25 owner | 98 | 99/100/101 | FR70, UC15, BR05/08 | 6 |
| US-26 B contributor, A owner | 102 | 104/107 | Kiểm thử C, sửa B/360px | 7 |
| US-27 owner chung | 109 | 111 | NFR07/08/13 backup/restore | 7 |

24 SP chức năng + 10 SP parent stories chung đứng tên B =34. Không cộng điểm US04/07/26 vào B và không đếm điểm subtask hai lần. Những parent chung chưa có assignee trong CSV; README §1.2 xác định owner. B đóng các subtask của mình rồi điều phối A/C cho AC toàn story, không tự đóng US03/27 vì riêng B xong.

## Mốc thời gian và acceptance priority

CSV đặt seed/kickoff ở Sprint1 (07/10), features/integration Sprint2 (08/10). Ở source hiện tại seed B còn thiếu; plan dùng thứ tự tương đối, ưu tiên seed→US13→US14→US17→US18→US25→integration, không coi ngày cũ là ngày chưa tới. SRS nói Should có thể hoãn nhưng user yêu cầu plan toàn B và README Release1 gồm Must+Should; giữ đủ scope.

| Story | Gate để đóng |
|---|---|
| US02 | Seed 2 lần không trùng, constraints/index có thật, Quân trình bày schema/khung |
| US03 | Catalog B đủ, đúng nền/kết luận/điều kiện/lớp, A seed đủ TC/CT, C review nguồn/trạng thái |
| US04 | 4 mẫu B đủ step/căn cứ/reachable concept; cả nhóm đủ30BT/9TH/4CM |
| US07 | Quân giải thích current user/cookie/nâng cao; B guest read-only, C làm bài guest không lưu |
| US13 | HBH+gócvuông→HCN DH_HCN_2 thiếu0; HBH→thoi đủ4; TG+cạnhđối→HBH; direct HV→HCN; fallback<=3bước |
| US14 | CM01 step order/căn cứ/SVG; chưa có mẫu không nút; grade/NHAP/missing404 |
| US17 | TH01 đúng DH_HBH_2+DH_HCN_3/giải thích; lớp3 không thấy cả list/detail; 3 bối cảnh/empty state |
| US18 | 2/.9/2.19 tol1%→HCN; >=8 case cả2nhánhvuông/biên/khônghợp lệ; dấu hiệu thật/read-only |
| US25 | Validation3loại, role/CSRF, atomic create/edit, hidden preserve DA_LAM, C không thấy bàiẩn |
| US26 | A kiểm thử B; Quân kiểm thử toàn7story C +>=2query; shared defect gate resolved, UI360px |
| US27 | Dump/load cô lập thật; A README<=15phút; C demo; Quân điều phối AC chung |

## Bằng chứng baseline và hợp đồng đang có

- `README.md:46–60`: assignment/points; `README.md:130–138`: ownership; `README.md:741–755`: tích hợp.
- `src/GeoQuad.Web/Infrastructure/Neo4j/IGraphDb.cs:18`: multi-query transaction; `GraphDb.cs:41`: managed write callback.
- `src/GeoQuad.Web/Program.cs:49–50`: gọi sẵn modules B; không cần sửa Program.
- `src/GeoQuad.Web/Areas/ApDung/Controllers/GoiYController.cs:12`, `ChungMinhController.cs:12`, `TinhHuongController.cs:12–22`: đang khung.
- `src/GeoQuad.Web/Areas/QuanTri/Controllers/BaiTapController.cs:13–26`: authorize role có sẵn, handlers khung.
- `tests/GeoQuad.Tests/B_ApDung/KhungTests.cs:12`: Assert.True(true), không AC.
- `tests/GeoQuad.Tests/GeoQuad.Tests.csproj:3–29`: net8/xUnit/webref, không extra test packages.
- `docs/GeoQuad_SRS.md:367–430,484–526`: UC nghiệp vụ; `:997–1107`: catalog B; `:859–879`: NFR.

## Xung đột đã xử lý trong plan

1. Header README D.8 ghi phần B nhưng các bảng/task chỉ định A seed TH: B chỉ đọc dữ liệu A.
2. BR08 chứng minh gắn concept mà schema chưa có direct edge: validate đường cm→dh→concept, không thay đổi schema.
3. Query README là draft, thiếu grade/review ở chi tiết/fallback, optional null có thể cho placeholder qua: plan yêu cầu hoàn thiện query bằng test thật.
4. README mặc định nội dung đã review; SRS yêu cầu review nguồn/người khác: checkpoint C ký trước DA_RA_SOAT.
5. NFR03/04 số thứ tự giữa README/SRS có diễn đạt khác: nghiệm thu theo mô tả SRS (44/48px,16px,360px) thay vì suy diễn từ số hiệu.
6. B nhận quyền TH chỉ đúng backup/restore và mục docs kiểm thử, không nhận sửa shared source.

## Các quy ước thiết kế công bố

Sai số tương đối đối xứng theo max, ε trong (0,100)%, so bốn cạnh bằng bằng range/max; thứ tự thoi→vuông rồi HBH→HCN→vuông theo README. Mã bài immutable sau create. Không lưu snapshot/version đề khi sửa; giữ DA_LAM và thống kê cũ. Restore default vào DB user trong volume mới, không restore system/auth DBMS. Đều có test và nêu giới hạn trong phase, không ghi là yêu cầu có sẵn của SRS.
