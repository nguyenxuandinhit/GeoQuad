# Requirements evidence — Quân / thành viên B

Ngày kiểm tra: 2026-10-08. Research-only; không triển khai. Nguồn: `docs/GeoQuad_Jira_2ngay.csv` (đọc bằng Python `csv.DictReader`, UTF-8 BOM, giữ dòng vật lý của mô tả multiline), `docs/GeoQuad_SRS.md`, `README.md`. SRS quyết định nghiệp vụ, CSV quyết định phân công, README quyết định ownership và hợp đồng kỹ thuật. Ba tài liệu này không phải ba nguồn độc lập xác nhận tính đúng toán học/chương trình GDPT.

## Phạm vi và truy vết CSV

B có US-13 (8), US-14 (3), US-17 (3), US-18 (5), US-25 (5): **24 điểm chức năng**. B điều phối story chung US-02 (3), US-03 (5), US-27 (2): **34 điểm đứng tên**, không cộng điểm cha US-04/US-07/US-26. B thực hiện seed điều kiện/dấu hiệu/định lý nền và chứng minh; A seed tình huống. B giải thích US-02/07, kiểm thử phần C, sửa phần B, viết backup/restore. Bằng chứng: README:46–62.

Tất cả 27 dòng CSV có `Assignee=Thành viên B`:

| Issue ID | Parent ID | Loại | Nội dung | Dòng CSV | SP |
|---|---|---|---|---|---|
| 15 | 14 | Sub-task | [US-02] Kickoff: giải thích 00-schema (ràng buộc, chỉ mục full-text) và 01-khung (MERGE, đặc biệt hóa, cần biết trước); chạy seed 2 lần | 32 | — |
| 18 | 16 | Sub-task | [US-03] Seed 14 điều kiện + 20 dấu hiệu + 6 định lý nền (neo4j/seed/20-dinhly-B.cypher) | 43 | — |
| 23 | 20 | Sub-task | [US-04] Seed 4 chứng minh mẫu, mỗi bài 3–6 bước có căn cứ (neo4j/seed/21-chungminh-B.cypher) | 56 | — |
| 30 | 29 | Sub-task | [US-07] Kickoff: giải thích ICurrentUser, cookie lớp của khách, "Xem trước kiến thức nâng cao" | 87 | — |
| 51 | 4 | Story | US-13 Nhập hình đã biết, điều kiện đã có và hình cần chứng minh | 149–157 | 8 |
| 52 | 51 | Sub-task | [US-13] Cypher: viết và chạy thử truy vấn trên Neo4j Browser | 158 | — |
| 53 | 51 | Sub-task | [US-13] Backend: Repository + Service + unit test | 159 | — |
| 54 | 51 | Sub-task | [US-13] Giao diện: Controller + View, kiểm tra AC | 160 | — |
| 55 | 4 | Story | US-14 Xem chứng minh mẫu từng bước kèm căn cứ | 161–169 | 3 |
| 56 | 55 | Sub-task | [US-14] Cypher: viết và chạy thử truy vấn trên Neo4j Browser | 170 | — |
| 57 | 55 | Sub-task | [US-14] Backend: Repository + Service + unit test | 171 | — |
| 58 | 55 | Sub-task | [US-14] Giao diện: Controller + View, kiểm tra AC | 172 | — |
| 66 | 6 | Story | US-17 Xem tình huống thực tế gắn với kiến thức | 195–202 | 3 |
| 67 | 66 | Sub-task | [US-17] Cypher: viết và chạy thử truy vấn trên Neo4j Browser | 203 | — |
| 68 | 66 | Sub-task | [US-17] Backend: Repository + Service + unit test | 204 | — |
| 69 | 66 | Sub-task | [US-17] Giao diện: Controller + View, kiểm tra AC | 205 | — |
| 70 | 6 | Story | US-18 Nhập số đo thật (cạnh, đường chéo) | 206–213 | 5 |
| 71 | 70 | Sub-task | [US-18] Cypher: viết và chạy thử truy vấn trên Neo4j Browser | 214 | — |
| 72 | 70 | Sub-task | [US-18] Backend: Repository + Service + unit test | 215 | — |
| 73 | 70 | Sub-task | [US-18] Giao diện: Controller + View, kiểm tra AC | 216 | — |
| 98 | 9 | Story | US-25 Thêm, sửa, ẩn bài tập | 286–294 | 5 |
| 99 | 98 | Sub-task | [US-25] Cypher: viết và chạy thử truy vấn trên Neo4j Browser | 295 | — |
| 100 | 98 | Sub-task | [US-25] Backend: Repository + Service + unit test | 296 | — |
| 101 | 98 | Sub-task | [US-25] Giao diện: Controller + View, kiểm tra AC | 297 | — |
| 104 | 102 | Sub-task | [US-26] Kiểm thử chéo phần C: chạy hết AC, tự chạy lại 2 câu Cypher của C, ghi docs/kiem-thu.md | 308 | — |
| 107 | 102 | Sub-task | [US-26] Sửa lỗi phần B và hoàn thiện giao diện đáp ứng (360px) | 311 | — |
| 111 | 109 | Sub-task | [US-27] Script sao lưu/khôi phục Neo4j (neo4j-admin database dump/load), thử khôi phục một lần | 323 | — |

Shared parent IDs: US-02=14, US-03=16, US-04=20, US-07=29, US-26=102, US-27=109. B sub-task 15/18/23/30 không biến Quân thành owner của toàn story chung khác; owner A giữ US-04/07/26 (README:55–58). Sub-task 52/56/67/71/99 là Cypher + docs, 53/57/68/72/100 là repository/service/unit test, 54/58/69/73/101 là UI/AC.

## Ma trận chấp nhận và test-first

| Story | Kết quả phải đạt; ca test cần viết trước | Bằng chứng |
|---|---|---|
| US-02 | Giải thích schema/khung trước nhóm; seed hai lần node/relationship count ổn định, constraints UNIQUE và full-text; 3 cấp,12 lớp,7 hình,6 yếu tố,7 cạnh đặc biệt hóa. B xác minh; không mặc định sửa file phần 0. | CSV:23–32; README:241–251 |
| US-03 | B seed 14 điều kiện,20 dấu hiệu,6 định lý nền. Mỗi dấu hiệu 1 YEU_CAU_LA,1 KHANG_DINH,≥1 YEU_CAU_CO,THUOC_LOP; nền không nhãn phụ. Toàn story còn 12 tính chất/12 công thức A; C review nguồn và ghi evidence. Seed lặp và A→B/B→A không trùng. | CSV:33–43; README:309–316,72; SRS:874 |
| US-04 | CM-01…04, giả thiết/kết luận,3–6 bước,thuTu liên tục từ1,mỗi bước có CAN_CU,đúng CHUNG_MINH_CHO; reachable concept. Mẫu đối chiếu đầy đủ,các căn cứ A chưa seed không được hiện dưới dạng placeholder. B không seed tình huống/bài tập A/C. | CSV:45–56; README:318–328,931–937; SRS:520,1100–1107 |
| US-07 | ICurrentUser,khách mặc định lớp8,cookie lớp/nâng cao,LopHienThi; đọc luồng B khi chưa login; không ghi DA_HOC/DA_LAM. Lớp thấp ẩn cao, bật preview có nhãn Nâng cao. | CSV:79–87; README:284–298; SRS:516,521 |
| US-13 | HBH+góc vuông→HCN:DH_HCN_2 đứng đầu,thiếu0; HBH→thoi không tích gì:4 dấu hiệu,thiếu1 mỗi cái; TG+cạnh đối bằng→HBH:DH_HBH_2 thiếu0; vuông→HCN suy ra trực tiếp;nền=đích thông báo; sorting soThieu rồi ma;✓/✗,lớp,link mẫu/bài tập. Chain≤3 bước nếu không có trực tiếp; không kết quả gợi ý bản đồ; lớp<8 gợi ý preview. Unknown IDs/duplicate condition IDs không làm sai số thiếu. | CSV:149–160; README:467–507; SRS:375–378,519 |
| US-14 | CM-01…04:giả thiết,kết luận,SVG,bước đúng thứ tự,căn cứ clickable;tính chất link hình A,dấu hiệu/nền hộp thông tin;không có mẫu ẩn nút; mã lạ/NHAP/không hợp lớp không lộ chi tiết. | CSV:161–172; README:509–523; SRS:383–391,516,525 |
| US-17 | 3 bối cảnh,danh sách theo lớp,empty context thông báo/gợi ý;TH-01 link DH_HBH_2/DH_HCN_3 và vì sao đúng;lớp3 không thấy;SVG,lời giải,kiến thức,nút đo chỉ thucHanh=true. Test direct URL,review state,dependency placeholder/AP_DUNG thiếu lớp. | CSV:195–205; README:525–547; SRS:409–417,516,525 |
| US-18 | 6 số đo,chung đơn vị,sai số mặc định1%;finite,>0,thỏa tam giác;2,00/0,90/2,19→HCN+căn cứ;≥8 ca:HCN,vuông các nhánh,thoi,HBH,chưa xác định,không hợp lệ,biên tolerance;kết quả chưa xác định chỉ cặp lệch nhiều nhất. POST direct chỉ tình huống có thực hành và hợp lớp/review. | CSV:206–216; README:549–561; SRS:420–430,522 |
| US-25 | Server role QUAN_TRI,CSRF,XSS;lọc lớp/loại/trạng thái,tìm mã/đề;create/edit/preview/hide-show. TN4 phương án/1 đáp án A–D;số có giá trị/saiSo>0/đơn vị;chứng minh có lời giải;≥1 concept,mã trùng báo lỗi. Transaction thuộc tính+quan hệ;validation/duplicate failure rollback;hidden giữ DA_LAM và C không hiện;đổi loại không giữ field đáp án cũ. | CSV:286–297; README:563–612; SRS:484–495,870 |
| US-26 | B kiểm thử mọi AC C,chạy≥2 Cypher C,ghi docs/kiem-thu.md;A test/review B;B sửa lỗi mình;360px/chữ16px/nút44px(48px cấp1),SVG title,không chỉ màu. | CSV:298–311; README:77,743–747 |
| US-27 | Backup/restore neo4j-admin dump/load,Community dừng DB,./backups,restore thử một lần;B điều phối A README máy sạch≤15 phút và C demo. | CSV:313–323; README:749–755; SRS:872 |

## Ownership và phụ thuộc

B sở hữu `src/GeoQuad.Web/Areas/ApDung/**`, `src/GeoQuad.Web/Areas/QuanTri/**`, `neo4j/seed/20-dinhly-B.cypher`, `neo4j/seed/21-chungminh-B.cypher`, `tests/GeoQuad.Tests/B_ApDung/**`, `docs/cypher/B-ap-dung.md`; tích hợp B phụ trách `scripts/backup.*`, `scripts/restore.*` và phần kiểm thử C trong `docs/kiem-thu.md`. Không sửa Program.cs/site.css/file phần0; đề xuất thay đổi chung ghi docs B (README:68,128–138,749–752).

- Khung chung/C cung cấp IGraphDb/ICurrentUser/IAuthHelper/auth/layout/routes (README:40,284–298).
- A cung cấp TC_* cho proof,CT_* và toàn bộ 3 bối cảnh/9 tình huống,chi tiết hình cho liên kết (README:312,322,510,526).
- B cung cấp DH_*/DL_NEN_* cho A/C; C review nội dung và bài BT-027 liên hệ CM-01 (README:313–324,944).
- Node tham chiếu chéo MERGE bằng nhãn gốc+ma,không thêm nhãn phụ/trường;mảng chủ SET dữ liệu. Node chưa được review không hiển thị (README:72).
- B kiểm thử C,A kiểm thử B,C kiểm thử A;chỉ mỗi người sửa trong phạm vi mình (README:744–745).

## Xung đột đã có hướng xử lý trong plan

1. **Lịch:** SRS:1172,1111 đặt US-04 S2; CSV:45–53/README:62,307,763 yêu cầu seed S1 07/10 trước UI 08/10. Dùng CSV/README cho thứ tự phụ thuộc; ghi lịch 2 ngày là mốc lịch sử đã tới/qua,không coi 07/10 là ngày tương lai.
2. **Tình huống owner:** README:910 header D.8 PHẦN B xung đột README:49,322,526 và CSV:55 giao A. Chọn A seed,B tiêu thụ;không chuyển ownership.
3. **DA_RA_SOAT:** README:73 nói nhóm đã review nhưng SRS:874,905,908 yêu cầu nguồn/peer review và phân lớp chưa chốt. Giữ gate C review/ghi evidence trước trạng thái;không coi appendix là nguồn curriculum độc lập.
4. **Proof concept BR-08:** SRS:520 yêu cầu concept nhưng SRS:618/README:203 schema LIEN_QUAN_DEN không có ChungMinh. Dùng đường CM→CHUNG_MINH_CHO→DH→KHANG_DINH→KN làm association,document/test;không tự bổ sung schema chung.
5. **README query skeleton:** Gợi ý không return lớp,fallback không lọc lớp/điều kiện thiếu;proof/scenario detail không lọc reviewed/lớp;scenario list cho phép lopCan NULL. Plan cần query đúng BR04/13 trên mọi endpoint,không copy nguyên query (README:477–505,511–519,528–545;SRS:516,525).
6. **Should:** US14/18/25 và chain là Should nhưng user yêu cầu toàn phần B và không --yagni. Giữ đầy đủ scope,không defer mặc định.

## Thiết kế chưa được SRS quy định, cần ghi assumption

- Sai số tương đối: đề xuất |a-b|/max(a,b)≤ε,biên inclusive;độ dài/tolerance finite;phạm vi tolerance và cặp so sánh khi chẩn đoán phải khai báo rõ. SRS/README chỉ nêu1% và sai lệch lớn nhất,chưa cho công thức.
- Bốn bất đẳng thức tam giác được README yêu cầu;chưa quy định kiểm tra tính nhất quán đầy đủ của cả sáu số đo. Không claim kiểm tra tam giác là đủ chứng minh sáu khoảng cách tạo cùng tứ giác lồi.
- Mã bài sau create: khuyến nghị immutable để giữ URL/history;SRS/README chưa nói được đổi mã. Tách validator create/edit.
- TN lưu một `dapAnDung` chuỗi;UI có thể chọn single đáp án nhưng phải test server reject payload không hợp lệ/đa đáp án thay vì chỉ tin UI.

Thứ tự khuyến nghị: khung→test/seed B+review→gợi ý/mẫu→tình huống/đo→quản trị→cross-test C/backup/restore. Đây là nghiên cứu yêu cầu nội bộ;không xác nhận độc lập SGK/chương trình,không chạy runtime/Neo4j,không thực hiện task ngoài repo.
