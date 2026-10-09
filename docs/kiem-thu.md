# Kiểm thử GeoQuad

## Quân/B — kết quả ngày 08/10/2026

Người thực hiện: agent theo yêu cầu `ak-cook --auto` của Quân; chưa có chữ ký review độc lập của A/C. Branch `feature/B-ap-dung`. Chạy .NET SDK8.0.425/runtime8.0.31, Neo4j5.26.31 Community trên Docker Desktop/macOS; chỉ project test `geoquad-b-tests`, không mutation database dev.

### Chạy lại

```sh
docker compose -p geoquad-b-tests -f tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml up -d --wait
dotnet build GeoQuad.sln --nologo --disable-build-servers -m:1
dotnet test GeoQuad.sln --no-build --filter 'Category!=Integration' --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --no-build --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration' --settings tests/GeoQuad.Tests/B_ApDung/Integration/neo4j.runsettings --nologo
bash -n scripts/backup.sh scripts/restore.sh scripts/backup-common.sh
```

Máy này dùng host `/private/tmp/geoquad-dotnet8/dotnet` vì SDK8 được cài riêng trong phiên. Máy khác có .NET8 dùng `dotnet`. Runsettings chứa credential test disposable và opt-in rõ; fixture kiểm tra URI, project/volume marker, loopback ports và seed mount read-only trước reset. B integration dùng xUnit collection serial. Không chạy hai integration process cùng lúc.

### Kết quả B

| Gate | Kết quả và evidence |
|---|---|
| Build net8 | Đạt, 0 warning/0 error |
| Unit regression toàn solution | 415 đạt, 0 fail/skip sau tích hợp origin/main@95a8519 và bổ sung11case service B |
| Integration/HTTP/load/drill full | 14 đạt sau tích hợp95a8519, 0 fail/skip, 3m03s; link căn cứ B→trang hình A HTTP200; 4/4case quản trị/restore đã chạy lại sau sửa fixture lịch sử theo SRS |
| Seed/schema thật | Đúng 14 điều kiện/20 dấu hiệu/6 định lý nền/4 chứng minh; 13 unique constraints/fulltext, core3/12/7/6/7; A→B→A→B và B→A giữ snapshot |
| US-13 | Bốn AC README; direct/inheritance; QPP2–3 bước tối đa3; condition missingCount/mã ổn định; draft/null/class gate và HTTP400 |
| US-14 | 4 mẫu, bước liên tục/căn cứ; thiếu/draft/căn cứ A chưa hoàn thiện không có link và URL404; advanced/class qua HTTP |
| US-17 | 3 bối cảnh/9 tình huống trong graph test đã rà soát; TH-01 đúng dấu hiệu/lời giải; list/URL cùng gate; một AP_DUNG thiếu lớp loại cả tình huống |
| US-18 | 23 unit cases: comma/dot, positive finite, đơn vị, tolerance biên đối xứng, tam giác, hai trace vuông, extreme finite; HTTPCSRF/class/practice, input còn nguyên; không tăng node/edge |
| US-25 | Ba loại, số0, clearing field loại cũ, mã bất biến; mọi route admin từ chối guest/học sinh; role/login/token thật; admin lớp8 quản lý lớp12/NHAP/ẩn; preview encode/không write; transaction rollback thật, ID sai/thiếu, duplicate concurrent một winner; DA_LAM giữ7 quan hệ lượt làm riêng và các property luc/dung/đáp án/thời gian |
| Backup shell | Dump offline và restore mới cùng imageID; manifest/constraints/catalog/digest khớp; account password ứng dụng và DA_LAM đúng; source không đổi; fresh system/auth |
| Flow lỗi shell | 4 cases: dumpfail phục hồi trạng thái; unhealthy không startweb; nguồn stopped được trả stopped; checksum hỏng từ chối; không success giả |
| Cypher B | Mọi repository query chạy thật, params/output/counters trong [docs B](cypher/B-ap-dung.md); mutation audit rollback |
| Tải gợi ý | HTTP thật, 10 warmup +1000 requests/100 workers; 0 lỗi; p95=427.4867ms≤2s. Giá trị/hardware/raw timings mới nhất tại [performance-B.json](../plans/261008-0132-quan-b-ap-dung-tdd/reports/performance-B.json) |
| Review mã | Root đọc guard/class/query/mutation/scripts và kết quả test; code-simplifier làm gọn3file trong B, không đổi publiccontract/query/transaction; không có independent A review |

Số integration cuối và output tổng hợp được ghi tại [execution-B.md](../plans/261008-0132-quan-b-ap-dung-tdd/reports/execution-B.md). Test integration thường bao nhiều assertion/case trong một Fact; không lấy test count làm thay thế độ phủ AC. Báo cáo [backup-drill-B.json](../plans/261008-0132-quan-b-ap-dung-tdd/reports/backup-drill-B.json) là drill mới nhất, chỉ giữ counts/run path/flags, không password/hash.

Seed phát hành B đã chuyển `DA_RA_SOAT`: C đã duyệt 14 điều kiện, 20 dấu hiệu, 6 định lý nền ngày 08/10/2026 (US-03); B đã duyệt 4 chứng minh mẫu CM-01..CM-04 ngày 09/10/2026 (US-04).

### Quân/B kiểm thử C (US-26) — Đạt trên commit cebc5bb

Kiểm thử đối chiếu theo bảng AC trong `docs/cypher/C-hoc-tap.md` và tài khoản `hocsinh8 / Hocsinh@123` (kèm seed mẫu `31-lam-bai-mau-C.cypher`):

| Story C | Expected | Kết quả thực tế (commit cebc5bb) |
|---|---|---|
| US-08 | Hồ sơ đổi lớp/biệt danh/password/xóa tài khoản | **Đạt:** `HoSoController` cập nhật biệt danh, lớp theo cấp, đổi mật khẩu và xóa tài khoản có xác thực an toàn. |
| US-19 | List/filter bài reviewed/visible theo lớp | **Đạt:** Lọc theo cấp/lớp (BR-03), báo lỗi rõ ràng nếu cấp/lớp mâu thuẫn; chỉ hiện bài `DA_RA_SOAT` và đang bật hiển thị. |
| US-20 | Làm/chấm ba loại | **Đạt:** Chấm bài phía server, bảo vệ token lượt làm qua Data Protection; khách không ghi điểm (BR-09); trắc nghiệm 4 nút lớn A–D. |
| US-21 | Ghi DA_LAM đúng lượt/đúng sai; link CM-01 | **Đạt:** Ghi nhận `DA_LAM` bất biến theo lượt làm; khi CM-01 chuyển `DA_RA_SOAT`, bài BT-027 tự hiện nút "Xem lời giải mẫu" và link chuẩn tới CM-01. |
| US-22 | Lộ trình prerequisite | **Đạt:** Duyệt `CAN_BIET_TRUOC*0..12` topo-sort chuẩn xác, đánh dấu `DA_HOC` idempotent. |
| US-23 | Tiến độ và thống kê lịch sử | **Đạt:** Thống kê tỷ lệ hoàn thành, phát hiện khái niệm yếu (<60% trên 5 lượt gần nhất theo BR-06). |
| US-24 | Gợi ý bài kế tiếp | **Đạt:** Gợi ý 4 nhánh theo UC-13, ưu tiên bài ôn lại cho phần yếu và bài kế tiếp theo lộ trình. |
| ≥2 Cypher C | Query C thật + giải thích/output | **Đạt:** Chạy 2 câu Cypher cốt lõi (Lộ trình `CAN_BIET_TRUOC*0..12` và Lịch sử `DA_LAM` với 5 lượt gần nhất) cho kết quả chính xác, hiệu năng dưới 10ms. |
| Hide B → C | Bài ẩn không có trong list/Lam/gợi ý, lịch sử vẫn được tính | **Đạt:** Bài bị admin ẩn ở B không xuất hiện trong danh sách học sinh của C, nhưng lịch sử `DA_LAM` cũ vẫn giữ nguyên. |

## Như/A kiểm thử chéo phần B — kết quả ngày 09/10/2026

Người thực hiện: **Hồ Ngọc Phương Như (thành viên A)**. Commit kiểm: `0ca57e0` trên `main`.
Môi trường: Windows 11, .NET SDK 8, Neo4j 5.26 Community trên Docker Desktop, database dev
`geoquad-neo4j` (181 nút / 419 quan hệ) sau khi `scripts/seed.sh` chạy hai lần.
Cách kiểm: gửi **HTTP thật** tới `http://localhost:5080` rồi đọc HTML trả về, không mock, không
đọc mã thay cho chạy. Kịch bản kiểm lưu ngoài repo (thư mục tạm của phiên làm việc).

### AC của US-13, US-14, US-17, US-18, US-25 — 16/16 đạt

| AC (README mục 8) | Kết quả thực tế |
|---|---|
| US-13 · HBH + "Có một góc vuông" + đích HCN → `DH_HCN_2` đứng đầu, thiếu 0 | **Đạt:** mã đầu tiên trên trang là `DH_HCN_2`, nhãn "Thiếu 0 điều kiện ✓ Đã có: Có một góc vuông" |
| US-13 · HBH, không tích gì, đích hình thoi → 4 dấu hiệu `DH_THOI_1..4`, mỗi cái thiếu 1 | **Đạt:** đúng 4 mã `DH_THOI_1..4`, đếm được đúng 4 lần "Thiếu 1 điều kiện" |
| US-13 · Tứ giác + "Các cạnh đối bằng nhau" → `DH_HBH_2` thiếu 0 | **Đạt:** `DH_HBH_2 · Lớp 8 · Thiếu 0 điều kiện ✓` đứng đầu, các `DH_HBH` khác xuống dưới với "Thiếu 1" |
| US-13 · Hình vuông → hình chữ nhật: thông báo suy ra trực tiếp | **Đạt:** trang trả về đúng một câu *"Mọi Hình vuông đều là Hình chữ nhật."*, không liệt kê dấu hiệu |
| US-14 · CM-01 hiển thị các bước đúng thứ tự | **Đạt:** 4 `<li id="buoc-1..4">` theo thứ tự `CO_BUOC.thuTu` |
| US-14 · Mỗi bước có căn cứ là liên kết | **Đạt:** tính chất là thẻ `<a href="/KienThuc/ThuVien/ChiTiet/HINH_BINH_HANH">TC_HBH_2 · …</a>`; định lý nền là `<details><summary>Căn cứ DL_NEN_5</summary>` — đúng hai kiểu README yêu cầu |
| US-14 · Ẩn nút nếu chưa có chứng minh mẫu | **Đạt:** `/ApDung/ChungMinh/Xem/CM-99` trả 404 thân thiện, không 500 |
| US-17 · TH-01 liên kết tới `DH_HCN_3` (và `DH_HBH_2`) kèm giải thích | **Đạt:** cả hai mã có trên trang, kèm khối "Vì sao cách này đúng" |
| US-17 · Học sinh lớp 3 không thấy TH-01 | **Đạt:** danh sách lớp 3 không chứa TH-01; mở trực tiếp `/ApDung/TinhHuong/Xem/TH-01` trả 404 |
| US-18 · AB=CD=2,00 m; BC=DA=0,90 m; AC=BD=2,19 m (sai số 1%) → hình chữ nhật kèm dấu hiệu đường chéo bằng nhau | **Đạt:** trả về `DH_HCN_3 · Hình bình hành có hai đường chéo bằng nhau là hình chữ nhật.` kèm phần giải thích |
| US-18 · Số đo không lập được tứ giác → báo lỗi | **Đạt:** bộ 20/1/1/1/1/1 bị từ chối kèm thông báo, không ghi nhận kết quả |
| US-25 · Trắc nghiệm không có đúng một đáp án đúng → từ chối kèm thông báo | **Đạt:** *"Chọn đúng một đáp án A–D."* |
| US-25 · Chưa gắn khái niệm → từ chối kèm thông báo | **Đạt:** *"Chọn ít nhất một khái niệm hợp lệ."* |
| US-25 · Mã trùng → từ chối kèm thông báo | **Đạt:** *"Mã bài đã tồn tại."* (thử lại với `BT-001`) |
| US-25 · Bài ẩn không hiện với học sinh | **Đạt:** ẩn `BT-001` → danh sách của `hocsinh8` không còn, `/HocTap/BaiTap/Lam/BT-001` trả 404; hiện lại thì thấy lại. Trạng thái đã trả về như cũ sau khi kiểm |
| US-25 · Học sinh mở trang quản trị bị từ chối | **Đạt:** `hocsinh8` mở `/QuanTri/BaiTap` trả **403** |

**Không tìm thấy regression nào.** Bốn chứng minh CM-01…CM-04 đã được A kiểm lại từng bước
bằng toán học, tất cả đều đúng.

### Một ghi chú nhỏ của A (không chặn, không phải lỗi)

`CM-01` bước 2 và bước 4, cùng `CM-04` bước 3, ghi căn cứ là `DL_NEN_5` — phát biểu hiện tại
của `DL_NEN_5` là *"…các cặp góc **so le trong** bằng nhau"*, trong khi lập luận ở các bước đó
dùng biến thể **trong cùng phía** (hai góc trong cùng phía có tổng 180°). Kết luận toán học vẫn
đúng; chỉ là câu căn cứ nêu chưa khớp đúng biến thể. Cách sửa gọn nhất là thêm `DL_NEN_8`
"Hai đường thẳng song song bị cắt bởi một cát tuyến thì hai góc trong cùng phía bù nhau" rồi
trỏ `CAN_CU` sang đó. Để sau buổi báo cáo vì phải sửa cả danh sách `DL_NEN` trong `SeedBTests`.

### Các gate còn lại

- [x] **A chạy AC US-13/14/17/18/25** — 16/16 đạt, bảng ở trên, ngày 09/10/2026, không có regression.
- [x] C rà soát US-03 ngày 08/10/2026; B duyệt CM-01..04 ngày 09/10/2026: đã ký phiếu [content-review-B.md](../plans/261008-0132-quan-b-ap-dung-tdd/reports/content-review-B.md) và nạp `DA_RA_SOAT`.
- [x] **Seed lặp (NFR-08)** — `scripts/seed.sh` chạy hai lần liên tiếp trên database dev: 181 nút / 419 quan hệ không đổi, mỗi lần ~23 giây.
- [x] **`scripts/seed.ps1` trên Windows** — chạy được trên **Windows PowerShell 5.1**, nạp đủ 8 file, `exit 0`. Đường seed trên Windows coi như đã nghiệm thu.
- [x] **payload ≤ 2 MB (NFR-14)** — đo bằng HTTP: trang HTML nặng nhất là Thư viện **64,4 KB**; toàn bộ tài nguyên tĩnh tải một lần (Bootstrap, jQuery, KaTeX, Cytoscape, site.css, site.js) **≈ 1,05 MB** chưa nén. Trang nặng nhất (Bản đồ kiến thức) tổng **≈ 1,1 MB**.
- [x] **Dấu hiệu tiếp cận kiểm được bằng máy (NFR-11)** — `<html lang="vi">`, `meta viewport`, liên kết "Bỏ qua, đến nội dung chính", **đúng một thẻ `<main>`** mỗi trang (sau khi B sửa 6 view ngày 09/10), `:focus-visible` có trong `site.css`, có `@media` cho màn hình nhỏ, vùng bấm nút ≥ 44px. SVG trang chi tiết có `role="img"`, `<title>`, `<desc>` và `aria-labelledby`.
- [ ] **Nhìn bằng mắt trên trình duyệt** — còn mở. A kiểm bằng HTTP nên không thay được việc mở trình duyệt. Việc còn lại (khoảng 2 phút): DevTools → 360px xem có tràn ngang không; Tab qua toàn trang xem viền focus có thấy không; xem công thức KaTeX đã render thành ký hiệu toán; Network tab bật throttling "Fast 4G" đo thời gian tải ≤ 3s. Ghi lại tên và phiên bản trình duyệt vào đây.
- [ ] **Drill sao lưu/khôi phục trên Windows** — **không chạy được**, không phải vì máy thiếu Windows mà vì `scripts/backup.ps1`, `backup-common.ps1`, `restore.ps1` khai báo `#Requires -Version 7.2`. Máy demo chỉ có Windows PowerShell 5.1 và báo: `ScriptRequiresUnmatchedPSVersion`. Muốn đóng gate này phải cài PowerShell 7 (`winget install Microsoft.PowerShell`) rồi chạy lại. Bản drill mới nhất đang có trong [backup-drill-B.json](../plans/261008-0132-quan-b-ap-dung-tdd/reports/backup-drill-B.json) là của máy B (macOS).
- [ ] A cập nhật README và kiểm tra setup máy sạch ≤ 15 phút; C hoàn thiện demo/recovery; chưa đóng US-26/27.

Không sửa file/sharedcontract thuộc A/C để né dependency. URL B tới trang hình/Bản đồ/ngân hàng bài tập giữ đúng README; các đích còn khung phải được owner hoàn thiện. Archive/log backup nằm trong `backups/` gitignored, owner-only; không chia sẻ raw archive như báo cáo test.
