# GeoQuad – Hướng dẫn thực hiện cho Claude Code

> **Dành cho Claude Code.** File này là đặc tả để sinh mã cho dự án GeoQuad (project mini chương Neo4j), bám đúng tài liệu `GeoQuad_SRS.docx` (Release 1: toàn bộ yêu cầu Must và Should).
> Dự án chia thành **PHẦN 0** (khung chung, cả nhóm làm cùng lúc), ba phần **A, B, C** độc lập (mỗi thành viên một phần) và **PHẦN TÍCH HỢP** cuối cùng.
>
> Cách dùng:
> - Kickoff (cả nhóm, một máy): *"Đọc README.md và thực hiện PHẦN 0. Không làm phần A, B, C."*
> - Mỗi thành viên, làm từng story: *"Đọc README.md và làm story US-10 (phần A). Chỉ tạo/sửa file thuộc phạm vi phần A ở mục 3.1."*
> - Phần seed của mình trong US-03/US-04: *"Đọc README.md và làm phần seed của thành viên A trong US-03."*
>
> Mã story (`US-xx`), yêu cầu (`FR-xx`), quy tắc (`BR-xx`), phi chức năng (`NFR-xx`) giữ nguyên như trong SRS và Jira.

---

## 1. Tổng quan

**GeoQuad** là ứng dụng web giúp học sinh lớp 1–12 học hình học phẳng chủ đề **tứ giác**: tra cứu kiến thức, bản đồ "hình nào là trường hợp đặc biệt của hình nào", máy tính hình học, gợi ý nên dùng dấu hiệu nhận biết nào, chứng minh mẫu, tình huống thực tế và thực hành đo đạc, bài tập có chấm tự động, lộ trình học, tiến độ, gợi ý bài kế tiếp, quản trị bài tập. Toàn bộ dữ liệu nằm trong **Neo4j** (yêu cầu bắt buộc của môn học).

- Thời gian: **07/10 – 08/10/2026**, làm đủ phạm vi Release 1 của SRS (27 user story, 101 điểm).
- Chạy local, không triển khai lên mạng.
- Giao diện tiếng Việt. Tên lớp, biến, thuộc tính viết **tiếng Việt không dấu** (ví dụ `TaiKhoan`, `dinhNghia`) cho khớp với SRS.
- Release 2 (US-28 đến US-34) **không làm** trong đợt này.

### 1.1 Công nghệ (cố định, không tự đổi)

| Thành phần | Lựa chọn |
|---|---|
| Web | ASP.NET Core MVC, C#, **.NET 8** (`net8.0`) — nếu cả nhóm cùng có .NET 10 thì đổi đồng loạt trong PHẦN 0 |
| Giao diện | Razor Views, Bootstrap 5, KaTeX (công thức), Cytoscape.js (bản đồ kiến thức). **Đóng gói trong `wwwroot/lib` bằng LibMan** (SRS mục 5.4), commit cả thư viện để chạy được khi mạng yếu |
| CSDL | Neo4j 5 Community (Docker image `neo4j:5-community`) |
| Truy cập dữ liệu | NuGet `Neo4j.Driver` (bản 5.x), **chỉ dùng Cypher có tham số** |
| Xác thực | Cookie Authentication của ASP.NET Core + `PasswordHasher<T>` (không dùng ASP.NET Identity, không EF, không SQL) |
| Kiểm thử | xUnit |
| Chạy | Docker Compose (Neo4j + web); khi phát triển chạy Neo4j bằng Docker và web bằng `dotnet run` |

### 1.2 Nguyên tắc chia việc

Mỗi thành viên sở hữu **một mảng chức năng trọn vẹn**: tự viết seed dữ liệu Neo4j cho mảng của mình, tự viết Cypher, Repository, Service, Controller, View và unit test. Ba mảng **không sửa code của nhau**, chỉ dùng chung:

1. Khung do PHẦN 0 tạo (`IGraphDb`, `ICurrentUser`, `IAuthHelper`, layout, đăng nhập).
2. **Lược đồ đồ thị** ở mục 4 — "hợp đồng dữ liệu".
3. **Đường dẫn URL** ở mục 3.2 — để liên kết sang trang của người khác.

| | Thành viên A – Tra cứu & trực quan | Thành viên B – Áp dụng kiến thức | Thành viên C – Học tập cá nhân |
|---|---|---|---|
| Story chức năng | US-09, US-10, US-11, US-12, US-15, US-16 | US-13, US-14, US-17, US-18, US-25 | US-08, US-19, US-20, US-21, US-22, US-23, US-24 |
| FR | FR-10, 11, 12, 13, 30, 31 | FR-20, 21, 40, 41, 70 | FR-04, 50, 51, 52, 60, 61, 62 |
| Điểm chức năng | 23 | 24 | 24 |
| Seed (trong US-03, US-04) | Tính chất, công thức; 10 bài tập cấp 1; 3 bối cảnh + 9 tình huống thực tế | Điều kiện, dấu hiệu, định lý nền; 4 chứng minh mẫu | Rà soát nội dung US-03; 20 bài tập cấp 2–3; dữ liệu làm bài mẫu |
| Việc chung | Giải thích US-01, US-06 trong kickoff; kiểm thử phần B; README chạy từ máy sạch | Giải thích US-02, US-07; kiểm thử phần C; script sao lưu/khôi phục | Giải thích US-05; kiểm thử phần A; kịch bản demo |

Story chung: mỗi story có **một người chịu trách nhiệm** (điều phối, kiểm tra tiêu chí chấp nhận, đóng story trên Jira); việc thật của từng người nằm ở sub-task.

| Story chung | Người chịu trách nhiệm |
|---|---|
| US-01 Khởi tạo dự án, US-05 Đăng ký, US-06 Đăng nhập | C |
| US-02 Seed khung, US-03 Seed định lý, US-27 README/sao lưu/demo | B |
| US-04 Seed bài tập/tình huống/chứng minh, US-07 Chế độ khách, US-26 Kiểm thử | A |

Tổng điểm đứng tên (story riêng + story chung): A 34, B 34, C 33.

Phần seed được chia để cân độ khó: B có story chức năng khó nhất (US-13) nên nhận phần seed vừa phải; A có story chức năng nhẹ hơn nên nhận thêm seed tình huống thực tế (dữ liệu cho US-17 của B). Mọi seed làm xong trong Sprint 1 (07/10), nên sang 08/10 không ai phải chờ dữ liệu của ai.

---

## 2. Quy ước bắt buộc cho mọi phần

1. **Chỉ sửa file trong phạm vi của phần mình** (mục 3.1). Được *gọi* (không sửa) code của PHẦN 0. Nếu thấy cần thay đổi chung, **không sửa**, mà ghi vào cuối `docs/cypher/<phần>.md` mục "Đề xuất thay đổi chung" và làm cách tạm trong phạm vi của mình.
2. **Cypher luôn có tham số** (`$lop`, `$ma`…), không nối chuỗi từ dữ liệu người dùng (NFR-05).
3. Câu Cypher đặt thành hằng `const string` trong lớp Repository, có chú thích FR/UC tương ứng.
4. **Seed chạy lặp lại không sinh trùng**: `MERGE` theo khóa `ma` (với `Lop` là `so`) (NFR-08).
5. **Seed không phụ thuộc thứ tự chạy giữa A, B, C**: khi cần tham chiếu nút của mảng khác, `MERGE` nút đó theo **nhãn gốc + `ma`** (ví dụ `MERGE (t:DinhLy {ma:'TC_HBH_2'})`), không gắn nhãn phụ, không gán thuộc tính. Mảng sở hữu nút đó `MERGE` cùng khóa rồi `SET` nhãn phụ và thuộc tính. Nút "tạm" chưa có `trangThai` nên các truy vấn hiển thị (lọc `trangThai = 'DA_RA_SOAT'`) tự bỏ qua.
6. Mọi nút nội dung do seed tạo có `nguon` (ví dụ `'CT GDPT 2018 – Toán 8'`) và `trangThai: 'DA_RA_SOAT'` (BR-13; nhóm đã rà soát theo Phụ lục B của SRS).
7. **Lọc theo lớp (BR-04)**: truy vấn nhận `$lop = ICurrentUser.LopHienThi` (bằng lớp của người dùng; bằng 12 khi bật "Xem trước kiến thức nâng cao"). Nội dung có lớp lớn hơn `ICurrentUser.Lop` phải hiển thị nhãn **"Nâng cao"**.
8. **Khách (BR-09)**: không ghi bất kỳ dữ liệu học tập nào (`DA_HOC`, `DA_LAM`); nút cần đăng nhập thì mời đăng nhập.
9. Logic thuần (chấm điểm, tính toán, kiểm tra hợp lệ, thoát ký tự, vẽ SVG…) viết trong lớp không phụ thuộc Neo4j để unit test được.
10. Giao diện (NFR-03, NFR-04, NFR-11): chữ nội dung ≥ 16px, nút ≥ 44×44px (cấp 1 ≥ 48×48px), đáp ứng từ chiều rộng 360px, không chỉ dùng màu để truyền thông tin, SVG có `<title>`; công thức bằng KaTeX (NFR-12); thông báo lỗi tiếng Việt nói rõ cách sửa.
11. Mỗi phần có **`docs/cypher/<phần>.md`** liệt kê mọi truy vấn Cypher của phần đó, **giải thích từng ký hiệu** (`()`, `[]`, `:`, `->`, `*1..`, `{}`, `$`, `|`…) và kết quả mong đợi trên dữ liệu seed. Dùng để cả nhóm học Neo4j và trình bày khi demo.
12. Xong mỗi story: `dotnet build`, `dotnet test`, chạy seed hai lần, mở trang kiểm tra mọi tiêu chí chấp nhận (AC).

---

## 3. Cấu trúc thư mục và phạm vi sở hữu

```
GeoQuad/
├── GeoQuad.sln
├── docker-compose.yml                         [0]
├── README.md                                  [0/TH]
├── neo4j/
│   ├── seed/
│   │   ├── 00-schema.cypher                   [0]  ràng buộc + chỉ mục
│   │   ├── 01-khung.cypher                    [0]  cấp, lớp, 7 hình, yếu tố, đặc biệt hóa, cần biết trước
│   │   ├── 10-kienthuc-A.cypher               [A]  tính chất, công thức            (US-03)
│   │   ├── 11-baitap-A.cypher                 [A]  bài tập BT-001..BT-010          (US-04)
│   │   ├── 12-tinhhuong-A.cypher              [A]  bối cảnh, tình huống thực tế    (US-04)
│   │   ├── 20-dinhly-B.cypher                 [B]  điều kiện, dấu hiệu, định lý nền (US-03)
│   │   ├── 21-chungminh-B.cypher              [B]  chứng minh mẫu                  (US-04)
│   │   └── 30-baitap-C.cypher                 [C]  bài tập BT-011..BT-030          (US-04)
│   └── dev/
│       └── 31-lam-bai-mau-C.cypher            [C]  lịch sử làm bài giả để thử tiến độ
├── scripts/
│   ├── seed.ps1, seed.sh                      [0]  chạy mọi file neo4j/seed theo thứ tự tên
│   └── backup.ps1/.sh, restore.ps1/.sh        [TH] sao lưu/khôi phục (US-27, NFR-07)
├── docs/
│   ├── cypher/A-tra-cuu.md                    [A]
│   ├── cypher/B-ap-dung.md                    [B]
│   ├── cypher/C-hoc-tap.md                    [C]
│   ├── kiem-thu.md                            [TH] kết quả kiểm thử chéo (US-26)
│   └── demo.md                                [TH] kịch bản demo (US-27)
├── src/GeoQuad.Web/
│   ├── Program.cs, appsettings*.json, libman.json, Dockerfile   [0]
│   ├── Infrastructure/                        [0]  Neo4j, Auth, DemoAccountSeeder
│   ├── Controllers/HomeController.cs, TaiKhoanController.cs     [0]
│   ├── Views/Shared, Views/Home, Views/TaiKhoan                 [0]
│   ├── Areas/KienThuc/**                      [A]  thư viện, chi tiết, tìm kiếm, bản đồ, máy tính hình học
│   ├── Areas/ApDung/**                        [B]  gợi ý định lý, chứng minh mẫu, tình huống, thực hành đo đạc
│   ├── Areas/QuanTri/**                       [B]  quản trị bài tập
│   ├── Areas/HocTap/**                        [C]  bài tập, làm bài, lộ trình, tiến độ, gợi ý, hồ sơ
│   └── wwwroot/css/site.css, wwwroot/lib      [0]
└── tests/GeoQuad.Tests/
    ├── Chung/**                               [0]
    ├── A_TraCuu/**                            [A]
    ├── B_ApDung/**                            [B]
    └── C_HocTap/**                            [C]
```

Mỗi Area có: `Controllers/`, `Models/`, `Repositories/`, `Services/`, `Views/`, `_ViewImports.cshtml`, `_ViewStart.cshtml` và file đăng ký dịch vụ `<Ten>Module.cs` (ví dụ `public static IServiceCollection AddKienThuc(this IServiceCollection s)`). PHẦN 0 tạo sẵn khung rỗng của 4 Area và gọi sẵn `AddKienThuc()`, `AddApDung()`, `AddQuanTri()`, `AddHocTap()` trong `Program.cs`, nên A, B, C **không sửa `Program.cs`**. CSS riêng của mỗi phần đặt trong view (`@section Styles`) hoặc `Areas/<Ten>/...` qua thư mục tĩnh của Area, không sửa `site.css`.

### 3.1 Bảng phạm vi

| Phần | Được tạo/sửa |
|---|---|
| 0 | Mọi file đánh dấu `[0]` |
| A | `Areas/KienThuc/**`, `neo4j/seed/10-kienthuc-A.cypher`, `neo4j/seed/11-baitap-A.cypher`, `neo4j/seed/12-tinhhuong-A.cypher`, `tests/GeoQuad.Tests/A_TraCuu/**`, `docs/cypher/A-tra-cuu.md` |
| B | `Areas/ApDung/**`, `Areas/QuanTri/**`, `neo4j/seed/20-dinhly-B.cypher`, `neo4j/seed/21-chungminh-B.cypher`, `tests/GeoQuad.Tests/B_ApDung/**`, `docs/cypher/B-ap-dung.md` |
| C | `Areas/HocTap/**`, `neo4j/seed/30-baitap-C.cypher`, `neo4j/dev/31-lam-bai-mau-C.cypher`, `tests/GeoQuad.Tests/C_HocTap/**`, `docs/cypher/C-hoc-tap.md` |
| TH (tích hợp) | File `[TH]` theo phân công ở PHẦN TÍCH HỢP; sửa lỗi thì mỗi người sửa trong phạm vi của mình |

### 3.2 Đường dẫn (hợp đồng URL)

Route: `{area:exists}/{controller=Home}/{action=Index}/{id?}` và `{controller=Home}/{action=Index}/{id?}`.

| URL | Chủ | Màn hình SRS |
|---|---|---|
| `/` | 0 | SCR-01 Trang chủ |
| `/TaiKhoan/DangKy`, `/TaiKhoan/DangNhap`, `/TaiKhoan/DangXuat` | 0 | SCR-02, SCR-03 |
| `/Home/ChonLop` (POST `lop`), `/Home/NangCao` (POST `bat`) | 0 | Bộ chọn lớp của khách; công tắc "Xem trước kiến thức nâng cao" |
| `/KienThuc/ThuVien?loai=&cap=&lop=` | A | SCR-04 Thư viện kiến thức |
| `/KienThuc/ThuVien/ChiTiet/{ma}` | A | SCR-05 Chi tiết khái niệm |
| `/KienThuc/ThuVien/DaHieu/{ma}` (POST) | A | "Em đã hiểu" |
| `/KienThuc/ThuVien/TimKiem?q=` | A | SCR-06 Kết quả tìm kiếm |
| `/KienThuc/BanDo`, `/KienThuc/BanDo/DuLieu` | A | SCR-07 Bản đồ kiến thức |
| `/KienThuc/MayTinh?hinh=&daiLuong=` | A | SCR-10 Máy tính hình học |
| `/ApDung/GoiY` | B | SCR-08 Nên dùng định lý nào? |
| `/ApDung/ChungMinh/Xem/{ma}` | B | SCR-09 Chứng minh mẫu |
| `/ApDung/TinhHuong?boiCanh=`, `/ApDung/TinhHuong/Xem/{ma}` | B | SCR-11, SCR-12 |
| `/ApDung/TinhHuong/DoDac/{ma}` (POST) | B | SCR-12 phần thực hành đo đạc |
| `/QuanTri/BaiTap`, `/QuanTri/BaiTap/Them`, `/QuanTri/BaiTap/Sua/{ma}` | B | SCR-18, SCR-19 |
| `/HocTap/BaiTap?cap=&lop=&loai=&doKho=&khaiNiem=` | C | SCR-13 Danh sách bài tập |
| `/HocTap/BaiTap/Lam/{ma}`, `/HocTap/BaiTap/KeTiep/{ma}` | C | SCR-14 Làm bài tập |
| `/HocTap/LoTrinh?muc={ma}` | C | SCR-15 Lộ trình học |
| `/HocTap/TienDo`, `/HocTap/TienDo/GoiY` | C | SCR-16 Tiến độ của em |
| `/HocTap/HoSo` | C | SCR-17 Hồ sơ |

Trang chưa làm xong vẫn trả về trang "Đang xây dựng" (PHẦN 0 tạo sẵn), không lỗi 404.

---

## 4. Lược đồ đồ thị Neo4j (hợp đồng dữ liệu chung, theo SRS mục 4)

### 4.1 Nhãn nút

| Nhãn | Thuộc tính | Ghi chú |
|---|---|---|
| `TaiKhoan` | `id` (UUID), `tenDangNhap` (chữ thường, duy nhất), `matKhauBam`, `bietDanh`, `vaiTro` (`HOC_SINH`/`QUAN_TRI`), `ngayTao`, `soLanSai`, `khoaDen` | Tạo bởi ứng dụng |
| `CapHoc` | `ma` (`CAP_1`, `CAP_2`, `CAP_3`), `ten` | Cấp 1 = lớp 1–5, cấp 2 = lớp 6–9, cấp 3 = lớp 10–12 (BR-03) |
| `Lop` | `so` (1–12, duy nhất), `ten` (`"Lớp 8"`) | |
| `KhaiNiem` | `ma`, `ten`, `loai` (`HINH`/`YEU_TO`), `dinhNghia`, `ghiChuTieuHoc?`, `nguon`, `trangThai` | 7 hình + 6 yếu tố |
| `DinhLy` (+ nhãn phụ `TinhChat` hoặc `DauHieu`; định lý nền chỉ có `DinhLy`) | `ma`, `noiDung`, `nguon`, `trangThai` | |
| `DieuKien` | `ma`, `noiDung` | |
| `CongThuc` | `ma`, `ten`, `bieuThuc` (LaTeX), `bienSo` (danh sách), `daiLuong` (`CHU_VI`/`DIEN_TICH`/`DUONG_CHEO`), `nguon`, `trangThai` | `daiLuong` dùng cho máy tính hình học |
| `BaiTap` | `ma`, `loai` (`TRAC_NGHIEM`/`DAP_AN_SO`/`CHUNG_MINH`), `de`, `phuongAn` (4 chuỗi), `dapAnDung` (`A`–`D`), `dapAnSo`, `saiSo` (mặc định 0.01), `donVi`, `giaiThich`, `loiGiaiMau`, `doKho` (1–3), `hienThi` (bool), `nguon`, `trangThai` | |
| `TinhHuong` | `ma`, `ten`, `moTa`, `loiGiai`, `thucHanh` (bool), `nguon`, `trangThai` | |
| `BoiCanh` | `ma`, `ten` | `BC_NHA_CUA`, `BC_MANH_VUON`, `BC_DO_DUNG` |
| `ChungMinh` | `ma`, `ten`, `giaThiet`, `ketLuan`, `nguon`, `trangThai` | |
| `Buoc` | `ma` (ví dụ `CM-01-B1`), `noiDung` | |

### 4.2 Quan hệ (chiều luôn từ nút phụ thuộc → nút được tham chiếu)

| Quan hệ | Từ → Tới | Thuộc tính | Ý nghĩa |
|---|---|---|---|
| `HOC_LOP` | TaiKhoan → Lop | | Đúng một quan hệ (BR-03) |
| `THUOC_CAP` | Lop → CapHoc | | |
| `THUOC_LOP` | KhaiNiem, DinhLy, CongThuc, BaiTap → Lop | | Lớp xuất hiện lần đầu |
| `LA_TRUONG_HOP_DAC_BIET_CUA` | KhaiNiem → KhaiNiem | | Hình vuông → Hình chữ nhật (BR-11) |
| `CAN_BIET_TRUOC` | KhaiNiem → KhaiNiem | | |
| `CO_TINH_CHAT` | KhaiNiem → DinhLy:TinhChat | | |
| `CO_CONG_THUC` | KhaiNiem → CongThuc | | |
| `YEU_CAU_LA` | DinhLy:DauHieu → KhaiNiem | | Hình nền (BR-07) |
| `YEU_CAU_CO` | DinhLy:DauHieu → DieuKien | | |
| `KHANG_DINH` | DinhLy:DauHieu → KhaiNiem | | |
| `LIEN_QUAN_DEN` | BaiTap, TinhHuong → KhaiNiem | | ≥ 1 (BR-08) |
| `SU_DUNG` | BaiTap → DinhLy, CongThuc | | |
| `AP_DUNG` | TinhHuong → DinhLy, CongThuc | | |
| `TRONG_BOI_CANH` | TinhHuong → BoiCanh | | |
| `CHUNG_MINH_CHO` | ChungMinh → DinhLy | | |
| `CO_BUOC` | ChungMinh → Buoc | `thuTu` | |
| `CAN_CU` | Buoc → DinhLy | | |
| `DA_HOC` | TaiKhoan → KhaiNiem | `luc` | |
| `DA_LAM` | TaiKhoan → BaiTap | `luc`, `dung`, `dapAnDaChon`, `thoiGianGiay` | Mỗi lần làm là một quan hệ mới |

---

## 5. PHẦN 0 – Khung chung (US-01, US-02, US-05, US-06, US-07; cả nhóm, chiều 07/10)

> Làm **một lần, trên máy của C** (người chịu trách nhiệm US-01, US-05, US-06), cả ba người ngồi cùng; A kiểm tra AC của US-07, B kiểm tra US-02. Xong thì đẩy lên `main`; mỗi người tạo nhánh `feature/A-tra-cuu`, `feature/B-ap-dung`, `feature/C-hoc-tap`. Sau đó mỗi người giải thích lại phần được phân công cho hai người kia (A: US-01, US-06; B: US-02, US-07; C: US-05).

### US-01 · Khởi tạo dự án, Docker Compose, kết nối Neo4j (3 điểm) – NFR-08, NFR-13
- Tạo `GeoQuad.sln`, `src/GeoQuad.Web` (MVC), `tests/GeoQuad.Tests` (xUnit, tham chiếu dự án web).
- `docker-compose.yml`: dịch vụ `neo4j` (image `neo4j:5-community`, cổng `7474`, `7687`, `NEO4J_AUTH=neo4j/geoquad123`, volume dữ liệu có tên, mount `./neo4j/seed:/seed:ro`, `./neo4j/dev:/dev-seed:ro`, `./backups:/backups`); dịch vụ `web` (build `src/GeoQuad.Web/Dockerfile`, cổng `8080`, kết nối `bolt://neo4j:7687`, `depends_on` neo4j). `docker compose up` khởi động đủ cả hai.
- `appsettings.json`: mục `Neo4j` (`Uri`, `User`, `Password`), mục `HocTap:NguongCanOn` = 0.6.
- `libman.json`: Bootstrap 5, KaTeX (kèm auto-render), Cytoscape.js vào `wwwroot/lib`, commit cả thư viện.
- Đăng ký `IDriver` singleton và `IGraphDb`:
  ```csharp
  public interface IGraphDb
  {
      Task<IReadOnlyList<IRecord>> ReadAsync(string cypher, object? parameters = null);
      Task<IReadOnlyList<IRecord>> WriteAsync(string cypher, object? parameters = null);
      Task WriteTransactionAsync(Func<IAsyncQueryRunner, Task> work); // nhiều câu trong một giao dịch
  }
  ```
  Dùng `session.ExecuteReadAsync` / `ExecuteWriteAsync`. Thêm `GraphDbErrors.IsUniqueViolation(Exception)` nhận diện mã `Neo.ClientError.Schema.ConstraintValidationFailed`.
- `scripts/seed.ps1`, `scripts/seed.sh`: chạy lần lượt mọi file `neo4j/seed/*.cypher` theo thứ tự tên bằng `docker compose exec -T neo4j cypher-shell -u neo4j -p geoquad123 -f /seed/<file>`; in tên file; dừng nếu lỗi; tham số tùy chọn `-Dev` / `--dev` để chạy thêm `/dev-seed/*.cypher`.
- Trang chủ (SCR-01): ô tìm kiếm lớn (GET `/KienThuc/ThuVien/TimKiem?q=`), thẻ lối tắt (Bản đồ kiến thức, Nên dùng định lý nào?, Hình học quanh ta, Bài tập, Máy tính hình học), lời chào học sinh, dòng nhỏ "Kết nối Neo4j: OK – N nút".
- `_Layout.cshtml`: thanh điều hướng (Thư viện, Bản đồ, Máy tính, Nên dùng định lý nào?, Hình học quanh ta, Bài tập, Lộ trình, Tiến độ, Hồ sơ, Quản trị chỉ admin), bộ chọn lớp (khách), công tắc "Xem trước kiến thức nâng cao", nạp KaTeX auto-render cho `$...$` / `$$...$$`, `@RenderSection("Styles"/"Scripts", false)`.
- Tạo sẵn 4 Area, controller và view "Đang xây dựng" cho mọi URL ở mục 3.2, `_ViewStart.cshtml` trỏ layout chung, `<Ten>Module.cs` rỗng đã được gọi trong `Program.cs`; 3 file `docs/cypher/*.md` có tiêu đề và mục "Đề xuất thay đổi chung"; mỗi thư mục test có một test mẫu chạy được.

**AC:** `docker compose up` khởi động đủ Neo4j và web; trang chủ truy vấn được Neo4j.

### US-02 · Seed khung, ràng buộc, chỉ mục (3 điểm) – NFR-08, NFR-10
- `00-schema.cypher`: ràng buộc duy nhất (`IF NOT EXISTS`) cho `TaiKhoan.id`, `TaiKhoan.tenDangNhap`, `Lop.so`, `CapHoc.ma`, `KhaiNiem.ma`, `DinhLy.ma`, `DieuKien.ma`, `CongThuc.ma`, `BaiTap.ma`, `TinhHuong.ma`, `BoiCanh.ma`, `ChungMinh.ma`, `Buoc.ma`; chỉ mục toàn văn (SRS mục 4.5):
  ```cypher
  CREATE FULLTEXT INDEX kienThucTimKiem IF NOT EXISTS
  FOR (n:KhaiNiem|DinhLy|CongThuc)
  ON EACH [n.ten, n.dinhNghia, n.noiDung, n.bieuThuc]
  OPTIONS { indexConfig: { `fulltext.analyzer`: 'standard-folding' } };
  ```
- `01-khung.cypher`: 3 `CapHoc`, 12 `Lop` + `THUOC_CAP`, 7 hình và 6 yếu tố (`KhaiNiem` + `THUOC_LOP`, `dinhNghia`, `nguon`, `trangThai`), `ghiChuTieuHoc` cho Hình thang ("Ở tiểu học: hình thang có một cặp cạnh đối song song") và Hình bình hành (BR-11), 7 quan hệ `LA_TRUONG_HOP_DAC_BIET_CUA`, các quan hệ `CAN_BIET_TRUOC` — dữ liệu ở **Phụ lục D.1, D.2**.

**AC:** chạy seed hai lần, số nút không đổi; có ràng buộc UNIQUE và chỉ mục full-text (`SHOW CONSTRAINTS`, `SHOW INDEXES`).

### US-05 · Đăng ký (3 điểm) – FR-01, UC-01, BR-01, BR-02, BR-03, BR-12
Biểu mẫu (SCR-02): tên đăng nhập 4–20 ký tự `[a-z0-9_]` (đưa về chữ thường), mật khẩu ≥ 8 ký tự, nhập lại, biệt danh, lớp 1–12, ô xác nhận "Em đã đọc thông báo quyền riêng tư và được cha mẹ/người giám hộ đồng ý (nếu dưới 16 tuổi)" kèm liên kết tới trang thông báo quyền riêng tư ngắn gọn (`/Home/QuyenRiengTu`). Không hỏi email/số điện thoại (NFR-06). Lỗi hiển thị cạnh từng trường, giữ dữ liệu đã nhập trừ mật khẩu.
```cypher
MATCH (l:Lop {so: $lop})
CREATE (tk:TaiKhoan {id: randomUUID(), tenDangNhap: $ten, matKhauBam: $bam,
                     bietDanh: $bietDanh, vaiTro: 'HOC_SINH', ngayTao: datetime(), soLanSai: 0})
CREATE (tk)-[:HOC_LOP]->(l)
RETURN tk.id AS id
```
Trùng tên (lỗi ràng buộc) → "Tên đăng nhập đã tồn tại". Đăng ký xong tự đăng nhập, về trang chủ.

**AC:** tên trùng bị từ chối; mật khẩu lưu dạng băm; không tích xác nhận thì không đăng ký được.

### US-06 · Đăng nhập, đăng xuất, phân quyền (3 điểm) – FR-02, UC-02
- Sai thì báo chung "Tên đăng nhập hoặc mật khẩu không đúng"; sai 5 lần liên tiếp khóa 5 phút (`soLanSai`, `khoaDen`), báo thời gian chờ. Đúng thì đặt lại `soLanSai = 0`.
- Claims: `NameIdentifier` = id, `Name` = biệt danh, `Role` = vaiTro, `"lop"` = số lớp. Học sinh về trang chủ, quản trị viên về `/QuanTri/BaiTap`.
- `IAuthHelper` (Infrastructure/Auth) để các phần khác dùng lại, không tự viết lại:
  ```csharp
  public sealed record TaiKhoanAuth(string Id, string TenDangNhap, string BietDanh, string VaiTro, int Lop);
  public interface IAuthHelper
  {
      string BamMatKhau(string matKhau);
      bool KiemTraMatKhau(string matKhauBam, string matKhau);
      Task DangNhapAsync(HttpContext http, TaiKhoanAuth tk);   // tạo lại cookie (dùng cả khi đổi lớp/biệt danh)
      Task DangXuatAsync(HttpContext http);
  }
  ```
- `DemoAccountSeeder` (chạy khi khởi động ở Development, tạo nếu chưa có): `admin` / `Admin@123` (QUAN_TRI, lớp 12), `hocsinh8` / `Hocsinh@123` (lớp 8), `hocsinh4` / `Hocsinh@123` (lớp 4).

**AC:** sai 5 lần liên tiếp thì khóa 5 phút; trang Quản trị từ chối người không phải quản trị viên.

### US-07 · Chế độ khách (1 điểm) – FR-03, BR-04, BR-09
- `ICurrentUser` (scoped):
  ```csharp
  public interface ICurrentUser
  {
      bool DaDangNhap { get; }
      string? TaiKhoanId { get; }   // null nếu là khách
      string? BietDanh { get; }
      bool LaQuanTri { get; }
      int Lop { get; }              // học sinh: claim "lop"; khách: cookie gq_lop; mặc định 8
      bool XemNangCao { get; }      // cookie gq_nangcao
      int LopHienThi { get; }       // XemNangCao ? 12 : Lop — dùng làm tham số $lop
  }
  ```
- Khách dùng được tra cứu, máy tính, gợi ý, tình huống, bài tập; bộ chọn lớp lưu cookie `gq_lop`. Partial view dùng chung `_MoiDangNhap.cshtml` ("Đăng nhập để lưu tiến độ của em") để các phần hiển thị khi khách bấm chức năng cần lưu.

**AC:** khách làm bài không lưu kết quả và được mời đăng nhập (kiểm tra lại ở US-20).

### Kiểm tra PHẦN 0
`docker compose up -d neo4j` → `scripts/seed` (2 lần) → `dotnet run --project src/GeoQuad.Web` → trang chủ "Kết nối Neo4j: OK"; đăng ký, đăng nhập, khóa sau 5 lần sai, `admin` vào được `/QuanTri/BaiTap`; mọi URL ở mục 3.2 mở trang "Đang xây dựng"; `dotnet test` qua.

---

## 6. Seed nội dung chung (US-03, US-04; mỗi người một phần, tối 07/10)

### US-03 · Tính chất, dấu hiệu, điều kiện, công thức, định lý nền (5 điểm) – NFR-09
| Người | File | Nội dung |
|---|---|---|
| A | `10-kienthuc-A.cypher` | 12 tính chất (D.5): `MERGE (t:DinhLy {ma})` → `SET t:TinhChat, t.noiDung, t.nguon, t.trangThai`; `CO_TINH_CHAT`; `THUOC_LOP`. 12 công thức (D.6): `MERGE (c:CongThuc {ma})` → `SET ten, bieuThuc` (LaTeX, ví dụ `P = 2(a + b)`, `d = \sqrt{a^2 + b^2}`), `bienSo` (ví dụ `['a','b']`, hình bình hành `['a','b','h']`, hình thoi diện tích `['d1','d2']`, hình thang `['a','b','h']`, tứ giác `['a','b','c','d']`), `daiLuong`; `CO_CONG_THUC`; `THUOC_LOP` |
| B | `20-dinhly-B.cypher` | 14 điều kiện (D.3); 20 dấu hiệu (D.4): `MERGE (d:DinhLy {ma})` → `SET d:DauHieu, ...`; `YEU_CAU_LA`, `YEU_CAU_CO`, `KHANG_DINH`, `THUOC_LOP`. 6 định lý nền (D.7): `DinhLy` không nhãn phụ, có `THUOC_LOP` |
| C | — | **Rà soát chéo**: chạy hai file trên, đối chiếu từng mục với Phụ lục D/SRS Phụ lục B, kiểm tra bằng truy vấn ở AC, ghi kết quả vào `docs/cypher/C-hoc-tap.md` mục "Rà soát US-03" |

**AC:** đủ 14 điều kiện, 20 dấu hiệu, 12 tính chất, 12 công thức (đếm bằng `MATCH (d:DauHieu) RETURN count(d)`…); mỗi dấu hiệu có đúng 1 `YEU_CAU_LA`, 1 `KHANG_DINH`, ≥ 1 `YEU_CAU_CO`; mỗi mục có `nguon` và `trangThai`; chạy hai lần không trùng.

### US-04 · Bài tập (≥ 30), tình huống (9), chứng minh mẫu (4) (5 điểm) – NFR-09, BR-08
| Người | File | Nội dung |
|---|---|---|
| A | `11-baitap-A.cypher` | **BT-001 đến BT-010**, cấp 1 (lớp 1–5): nhận biết hình, chu vi, diện tích hình chữ nhật, hình vuông, hình bình hành, hình thoi, hình thang; 5 trắc nghiệm + 5 đáp án số; gồm BT-001 (D.10) |
| A | `12-tinhhuong-A.cypher` | 3 bối cảnh + 9 tình huống (D.8): `moTa`, **`loiGiai`** (2–4 câu "vì sao đúng", ngôn ngữ dễ hiểu), `thucHanh`, `nguon`, `trangThai`, `TRONG_BOI_CANH`, `AP_DUNG` (mã ở cột "Kiến thức"; dấu hiệu `DH_...` tham chiếu theo quy ước 2.5 vì thuộc B), `LIEN_QUAN_DEN` tới hình liên quan |
| B | `21-chungminh-B.cypher` | 4 chứng minh mẫu (D.9): `ChungMinh` với `giaThiet`, `ketLuan`, `nguon`, `trangThai`; **3–6 bước** đúng toán học mỗi bài (`Buoc`, `CO_BUOC {thuTu}` từ 1), `CAN_CU` tới các mã ở cột "Căn cứ" (tính chất `TC_...` tham chiếu theo quy ước 2.5) |
| C | `30-baitap-C.cypher` | **BT-011 đến BT-030**: cấp 2 (lớp 6–9) 15 bài — tính chất, dấu hiệu, tổng các góc, đường chéo (Pythagore), diện tích, **≥ 5 bài liên quan Hình thoi**; cấp 3 (lớp 10–12) 5 bài — vectơ, tọa độ trong hình bình hành; ≥ 7 trắc nghiệm, ≥ 7 đáp án số, ≥ 3 chứng minh (có `loiGiaiMau`); gồm BT-014, BT-027 (D.10) |

Mỗi bài tập: `THUOC_LOP`, ≥ 1 `LIEN_QUAN_DEN` (khái niệm của PHẦN 0, dùng `MATCH`), `SU_DUNG` tới định lý/công thức theo quy ước 2.5, `hienThi: true`, độ khó 1–3. **Tự kiểm tra lại mọi phép tính**; đáp án số làm tròn 2 chữ số, có `saiSo`, `donVi`.

**AC:** `MATCH (b:BaiTap) RETURN count(b)` = 30, phủ 3 cấp và 3 loại; mỗi bài/tình huống có ≥ 1 `LIEN_QUAN_DEN`; mỗi chứng minh có bước `thuTu` liên tục từ 1; chạy hai lần không trùng.

---

## 7. PHẦN A – Tra cứu & trực quan (Thành viên A, 23 điểm)

### US-09 · Duyệt thư viện kiến thức (2 điểm) – FR-10, UC-03, SCR-04
`/KienThuc/ThuVien`: tab loại nội dung (Hình · Tính chất · Dấu hiệu · Công thức), lọc cấp và lớp, thẻ nội dung có nhãn lớp, nhãn "Nâng cao" (quy ước 2.7), huy hiệu "Đã học" với hình học sinh đã đánh dấu. Mỗi thẻ liên kết tới chi tiết của hình liên quan.
```cypher
MATCH (n)-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE ((n:KhaiNiem AND n.loai = 'HINH') OR n:TinhChat OR n:DauHieu OR n:CongThuc)
  AND n.trangThai = 'DA_RA_SOAT' AND l.so <= $lop
  AND ($lopLoc IS NULL OR l.so = $lopLoc)
  AND ($capLoc IS NULL OR cap.ma = $capLoc)
  AND ($loai IS NULL
       OR ($loai = 'HINH' AND n:KhaiNiem) OR ($loai = 'TINH_CHAT' AND n:TinhChat)
       OR ($loai = 'DAU_HIEU' AND n:DauHieu) OR ($loai = 'CONG_THUC' AND n:CongThuc))
OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(n)
OPTIONAL MATCH (n)-[:KHANG_DINH]->(dich:KhaiNiem)
RETURN n.ma AS ma, coalesce(n.ten, n.noiDung) AS tieuDe, n.bieuThuc AS bieuThuc,
       [x IN labels(n) WHERE x IN ['KhaiNiem','TinhChat','DauHieu','CongThuc']][0] AS loai,
       l.so AS lop, cap.ma AS cap,
       coalesce(CASE WHEN n:KhaiNiem THEN n.ma END, h.ma, dich.ma) AS maHinh,
       EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(n) } AS daHoc
ORDER BY lop, loai, tieuDe
```
**AC:** học sinh lớp 4 không thấy nội dung lớp 6 (Hình thang cân) trừ khi bật xem trước nâng cao; bật lên thì thấy kèm nhãn "Nâng cao".

### US-10 · Chi tiết khái niệm và "Em đã hiểu" (5 điểm) – FR-11, UC-03, SCR-05
`/KienThuc/ThuVien/ChiTiet/{ma}`: tab Định nghĩa · Tính chất · Dấu hiệu nhận biết · Công thức (KaTeX) · Ví dụ thực tế · Bài tập; hình SVG minh họa cho 7 hình; ghi chú tiểu học (BR-11) với học sinh cấp 1; liên kết "tổng quát hơn" / "đặc biệt hơn"; tab rỗng thì ẩn; mã không tồn tại → trang 404 thân thiện; nút "Em đã hiểu" (khách bấm → `_MoiDangNhap`).

Truy vấn chính:
```cypher
MATCH (k:KhaiNiem {ma: $ma})-[:THUOC_LOP]->(l:Lop)
OPTIONAL MATCH (k)-[:CO_TINH_CHAT]->(t:TinhChat {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lt:Lop)
  WHERE lt.so <= $lop
WITH k, l, collect(DISTINCT t {.ma, .noiDung, lop: lt.so}) AS tinhChat
OPTIONAL MATCH (k)-[:CO_CONG_THUC]->(c:CongThuc {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lc:Lop)
  WHERE lc.so <= $lop
WITH k, l, tinhChat, collect(DISTINCT c {.ma, .ten, .bieuThuc, lop: lc.so}) AS congThuc
OPTIONAL MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(k)
OPTIONAL MATCH (d)-[:THUOC_LOP]->(ld:Lop)
WITH k, l, tinhChat, congThuc,
     [x IN collect(DISTINCT {ma: d.ma, noiDung: d.noiDung, lop: ld.so})
        WHERE x.ma IS NOT NULL AND x.lop <= $lop] AS dauHieu
OPTIONAL MATCH (k)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(cha:KhaiNiem)
OPTIONAL MATCH (con:KhaiNiem)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(k)
RETURN k {.ma, .ten, .loai, .dinhNghia, .ghiChuTieuHoc} AS khaiNiem, l.so AS lop,
       tinhChat, congThuc, dauHieu,
       collect(DISTINCT cha {.ma, .ten}) AS tongQuatHon,
       collect(DISTINCT con {.ma, .ten}) AS dacBietHon,
       EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(k) } AS daHoc
```
Tình huống và bài tập liên quan (truy vấn riêng để không nhân dòng; liên kết sang `/ApDung/TinhHuong/Xem/{ma}` và `/HocTap/BaiTap/Lam/{ma}`):
```cypher
MATCH (k:KhaiNiem {ma: $ma})
OPTIONAL MATCH (th:TinhHuong {trangThai:'DA_RA_SOAT'})-[:LIEN_QUAN_DEN]->(k)
WITH k, collect(DISTINCT th {.ma, .ten}) AS tinhHuong
OPTIONAL MATCH (bt:BaiTap {hienThi: true})-[:LIEN_QUAN_DEN]->(k)
OPTIONAL MATCH (bt)-[:THUOC_LOP]->(lb:Lop)
WITH tinhHuong, [x IN collect(DISTINCT {ma: bt.ma, de: left(bt.de, 80), doKho: bt.doKho, lop: lb.so})
                 WHERE x.ma IS NOT NULL AND x.lop <= $lop] AS baiTap
RETURN tinhHuong, baiTap[0..5] AS baiTap
```
"Em đã hiểu" (POST, chỉ học sinh, chống CSRF):
```cypher
MATCH (tk:TaiKhoan {id: $tk}), (k:KhaiNiem {ma: $ma})
MERGE (tk)-[h:DA_HOC]->(k) ON CREATE SET h.luc = datetime()
```
**AC:** "Hình chữ nhật" hiển thị định nghĩa, tính chất, dấu hiệu nhận biết, công thức chu vi, diện tích đúng định dạng toán; học sinh lớp 4 mở "Hình thoi" không thấy tính chất lớp 8; mục rỗng không hiển thị; bấm "Em đã hiểu" (học sinh) ghi nhận đã học, bấm hai lần chỉ có một `DA_HOC`.

### US-11 · Tìm kiếm có/không dấu (3 điểm) – FR-12, UC-04, SCR-06, NFR-14
Tách từ khóa thành từ, **thoát ký tự đặc biệt Lucene** (`+ - && || ! ( ) { } [ ] ^ " ~ * ? : \ /`) từng từ, nối bằng ` AND `; không có kết quả thì thử ` OR `. Từ khóa rỗng: không gửi, nhắc nhập.
```cypher
CALL db.index.fulltext.queryNodes('kienThucTimKiem', $q) YIELD node, score
WHERE node.trangThai = 'DA_RA_SOAT'
MATCH (node)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop
OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(node)
OPTIONAL MATCH (node)-[:KHANG_DINH]->(dich:KhaiNiem)
RETURN labels(node) AS nhan, node.ma AS ma, coalesce(node.ten, node.noiDung) AS tieuDe,
       coalesce(node.dinhNghia, node.noiDung, node.bieuThuc) AS doanTrich,
       l.so AS lop, score,
       coalesce(CASE WHEN node:KhaiNiem THEN node.ma END, h.ma, dich.ma) AS maHinh
ORDER BY score DESC LIMIT 30
```
Kết quả gom theo loại (Khái niệm / Tính chất & dấu hiệu / Công thức), có đoạn trích và nhãn lớp. Không có kết quả → gợi ý kiểm tra chính tả và nút "Duyệt thư viện".

**AC:** "hinh thoi" và "hình thoi" đều đưa "Hình thoi" lên đầu; ký tự `(`, `*`, `"` không gây lỗi. Unit test cho hàm thoát ký tự và hàm tạo chuỗi truy vấn. Nếu analyzer `standard-folding` không bỏ được dấu (kể cả "đ"), ghi vào "Đề xuất thay đổi chung" và tạm thêm thuộc tính `tenKhongDau` trong seed của A.

### US-12 · Bản đồ kiến thức (5 điểm) – FR-13, UC-05, SCR-07
`/KienThuc/BanDo`: Cytoscape.js (bố cục `breadthfirst`, mũi tên từ hình đặc biệt tới hình tổng quát), **màu nút theo cấp học kèm chú giải** (và chữ ghi lớp, không chỉ dùng màu), lọc theo lớp, chọn nút hiện khung tóm tắt (định nghĩa, lớp) + "Xem chi tiết"; học sinh thấy nút đã học có viền đậm và "✓". Lớp chưa có quan hệ nào (ví dụ lớp 1) → các nút rời và lời giải thích. Thêm ô "Vì sao … là …?".

Dữ liệu `/KienThuc/BanDo/DuLieu?lop=`:
```cypher
MATCH (a:KhaiNiem {loai:'HINH', trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(la:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE la.so <= $lop
OPTIONAL MATCH (a)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(b:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
  WHERE lb.so <= $lop
RETURN a.ma AS ma, a.ten AS ten, a.dinhNghia AS dinhNghia, la.so AS lop, cap.ma AS cap,
       collect(b.ma) AS laDacBietCua,
       EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(a) } AS daHoc
```
Mọi hình tổng quát hơn (nhiều bước):
```cypher
MATCH (:KhaiNiem {ma: $ma})-[:LA_TRUONG_HOP_DAC_BIET_CUA*1..]->(t:KhaiNiem)
RETURN DISTINCT t.ma AS ma, t.ten AS ten
```
"Vì sao hình vuông là hình thang?":
```cypher
MATCH p = shortestPath((a:KhaiNiem {ma: $tu})-[:LA_TRUONG_HOP_DAC_BIET_CUA*]->(b:KhaiNiem {ma: $den}))
RETURN [n IN nodes(p) | n.ten] AS chuoi
```
**AC:** hình vuông nối tới hình chữ nhật và hình thoi, đồ thị không có chu trình; lọc theo lớp hoạt động (lớp 3 không có hình thang cân); chọn nút mở được chi tiết; "Vì sao Hình vuông là Hình thang?" ra chuỗi 4 hình.

### US-15 · Máy tính hình học (5 điểm) – FR-30, UC-08, SCR-10, BR-10
`/KienThuc/MayTinh`: chọn hình (hình chữ nhật, hình vuông, hình bình hành, hình thoi, hình thang, hình thang cân), chọn đại lượng, nhập số đo và đơn vị. **Danh sách đại lượng và công thức lấy từ Neo4j**: công thức của chính hình đó hoặc của hình tổng quát gần nhất (hình vuông dùng chu vi hình vuông, không dùng chu vi tứ giác):
```cypher
MATCH p = (h:KhaiNiem {ma: $hinh})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..]->(g:KhaiNiem)
          -[:CO_CONG_THUC]->(c:CongThuc {trangThai:'DA_RA_SOAT'})
MATCH (c)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop
WITH c, g, length(p) AS khoangCach ORDER BY khoangCach
WITH c.daiLuong AS daiLuong,
     head(collect({ma: c.ma, ten: c.ten, bieuThuc: c.bieuThuc, bienSo: c.bienSo, tuHinh: g.ten})) AS congThuc
RETURN daiLuong, congThuc
ORDER BY daiLuong
```
Tính toán trong lớp thuần `MayTinhHinhHoc` (chọn theo `ma` công thức), làm tròn 2 chữ số, hiển thị công thức KaTeX, **các bước thay số**, kết quả kèm đơn vị (cm, cm², m…). Kiểm tra BR-10: độ dài > 0; bốn cạnh tứ giác thỏa mỗi cạnh nhỏ hơn tổng ba cạnh còn lại; báo lý do cụ thể. Học sinh cấp 1 chỉ thấy đại lượng có công thức lớp ≤ 5 (truy vấn đã lọc theo `$lop`).

**AC:** hình chữ nhật 5 × 3: chu vi 16, diện tích 15, đường chéo ≈ 5,83 (cm), kèm công thức và các bước; hình thoi đường chéo 12 và 16: diện tích 96 cm²; số đo không hợp lệ bị báo lỗi rõ ràng. Unit test ≥ 8 trường hợp.

### US-16 · Vẽ hình theo số liệu (3 điểm) – FR-31, UC-08
Lớp thuần `VeHinhSvg` sinh SVG tỉ lệ theo số đo vừa nhập ở US-15: đỉnh A, B, C, D, nhãn độ dài cạnh, ký hiệu góc vuông, đường chéo nét đứt khi tính đường chéo; hình bình hành dùng chiều cao `h` (không có `h` thì góc mặc định 60°), hình thoi dựng từ hai đường chéo, hình thang dựng từ hai đáy và chiều cao (hình thang cân đặt cân giữa). Co giãn vừa khung 320×240 giữ đúng tỉ lệ. Có `<title>` và `<desc>` (NFR-11).

**AC:** hình SVG có nhãn đỉnh, cạnh, góc và mô tả thay thế; hình chữ nhật 5 × 3 có tỉ lệ cạnh đúng 5:3. Unit test kiểm tra tọa độ và nhãn.

---

## 8. PHẦN B – Áp dụng kiến thức (Thành viên B, 24 điểm)

### US-13 · Gợi ý định lý / dấu hiệu cần dùng (8 điểm) – FR-20, UC-06, SCR-08, BR-07
`/ApDung/GoiY`: (1) "Tứ giác ABCD đã biết là" (danh sách hình, mặc định Tứ giác); (2) tích điều kiện đã có (danh sách `DieuKien`); (3) "Cần chứng minh ABCD là". Kết quả: mỗi dấu hiệu một thẻ, ✓ điều kiện đã có, ✗ điều kiện còn thiếu, lớp, liên kết "Xem chứng minh mẫu" (nếu có) và "Bài tập liên quan" (`/HocTap/BaiTap?khaiNiem={dich}`).

Kiểm tra trước (luồng 5a): `$nen = $dich` → "Hình đã biết chính là hình cần chứng minh";
```cypher
RETURN EXISTS {
  MATCH (:KhaiNiem {ma: $nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*1..]->(:KhaiNiem {ma: $dich})
} AS suyRaTrucTiep
```
đúng → "Mọi {hình đã biết} đều là {hình cần chứng minh}" và dừng.

Truy vấn gợi ý:
```cypher
MATCH (dh:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(:KhaiNiem {ma: $dich})
MATCH (dh)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop
MATCH (dh)-[:YEU_CAU_LA]->(nen:KhaiNiem)
WHERE nen.ma = $nen
   OR EXISTS { MATCH (:KhaiNiem {ma: $nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*1..]->(nen) }
OPTIONAL MATCH (dh)-[:YEU_CAU_CO]->(dk:DieuKien)
WITH dh, nen, collect(dk) AS dks
OPTIONAL MATCH (cm:ChungMinh)-[:CHUNG_MINH_CHO]->(dh)
WITH dh, nen, dks, head(collect(cm.ma)) AS maChungMinh
WITH dh, nen, maChungMinh,
     [d IN dks WHERE d.ma IN $co     | d.noiDung] AS daCo,
     [d IN dks WHERE NOT d.ma IN $co | d.noiDung] AS conThieu
RETURN dh.ma AS ma, dh.noiDung AS dauHieu, nen.ten AS hinhNen, daCo, conThieu,
       size(conThieu) AS soThieu, maChungMinh
ORDER BY soThieu, ma
```
Luồng 5b (chỉ chạy khi truy vấn gợi ý không có kết quả): đề xuất chuỗi tối đa 3 bước, mỗi bước là một dấu hiệu, dùng **quantified path pattern** của Neo4j 5 (cần Neo4j ≥ 5.9; image `neo4j:5-community` hiện tại đáp ứng):
```cypher
MATCH p = (:KhaiNiem {ma: $nen})
          ((:KhaiNiem)<-[:YEU_CAU_LA]-(d:DauHieu)-[:KHANG_DINH]->(:KhaiNiem)){2,3}
          (:KhaiNiem {ma: $dich})
WHERE all(x IN d WHERE x.trangThai = 'DA_RA_SOAT')
RETURN [x IN d | x.noiDung] AS cacBuoc, size(d) AS soBuoc
ORDER BY soBuoc LIMIT 3
```
(`d` là danh sách các dấu hiệu trên đường đi. Ví dụ tứ giác → hình vuông: "Tứ giác có ba góc vuông là hình chữ nhật" rồi "Hình chữ nhật có hai cạnh kề bằng nhau là hình vuông". Nếu vẫn không có thì "Chưa có dấu hiệu phù hợp, em thử xem Bản đồ kiến thức".) Học sinh lớp < 8 thấy "Dấu hiệu nhận biết được học ở lớp 8" và gợi ý bật "Xem trước kiến thức nâng cao".

**AC:** hình bình hành + "Có một góc vuông" + đích hình chữ nhật → `DH_HCN_2` đứng đầu, thiếu 0; hình bình hành, không tích gì, đích hình thoi → 4 dấu hiệu (`DH_THOI_1..4`), mỗi cái thiếu 1 điều kiện (`DH_THOI_1` có hình nền là Tứ giác, vẫn hợp lệ theo BR-07); tứ giác + "Các cạnh đối bằng nhau" → hình bình hành: `DH_HBH_2` thiếu 0; hình vuông → hình chữ nhật: thông báo suy ra trực tiếp. Unit test cho lớp dựng mô hình kết quả (sắp xếp, ✓/✗, thông báo 5a/5b).

### US-14 · Chứng minh mẫu từng bước (3 điểm) – FR-21, UC-07, SCR-09
`/ApDung/ChungMinh/Xem/{ma}`: định lý, giả thiết, kết luận, hình vẽ SVG, các bước đánh số; mỗi căn cứ là liên kết (tính chất → trang chi tiết của hình có tính chất đó; dấu hiệu/định lý nền → hiển thị nội dung trong hộp bật lên).
```cypher
MATCH (cm:ChungMinh {ma: $ma})-[:CHUNG_MINH_CHO]->(dl:DinhLy)
MATCH (cm)-[r:CO_BUOC]->(b:Buoc)
OPTIONAL MATCH (b)-[:CAN_CU]->(cc:DinhLy)
OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT]->(cc)
WITH cm, dl, r, b, collect(DISTINCT cc {.ma, .noiDung, maHinh: h.ma}) AS canCu
ORDER BY r.thuTu
RETURN cm {.ma, .ten, .giaThiet, .ketLuan} AS chungMinh, dl {.ma, .noiDung} AS dinhLy,
       collect({thuTu: r.thuTu, noiDung: b.noiDung, canCu: canCu}) AS cacBuoc
```
Định lý chưa có chứng minh mẫu → không hiển thị nút (truy vấn US-13 trả `maChungMinh` null).

**AC:** CM-01 "Hình bình hành có một góc vuông là hình chữ nhật" hiển thị các bước đúng thứ tự, mỗi bước có căn cứ là liên kết; ẩn nút nếu chưa có chứng minh mẫu.

### US-17 · Tình huống thực tế gắn với kiến thức (3 điểm) – FR-40, UC-09, SCR-11, SCR-12
Dữ liệu tình huống do A nạp ở US-04 (`12-tinhhuong-A.cypher`, xong trong Sprint 1); B chỉ đọc theo lược đồ mục 4. `/ApDung/TinhHuong`: ba bối cảnh dạng thẻ lớn (Nhà cửa; Mảnh vườn, thửa đất; Đồ dùng, sân chơi), danh sách tình huống có nhãn lớp, ẩn tình huống cần kiến thức lớp cao hơn; bối cảnh không có tình huống phù hợp → thông báo và gợi ý bối cảnh khác.
```cypher
MATCH (th:TinhHuong {trangThai:'DA_RA_SOAT'})-[:TRONG_BOI_CANH]->(bc:BoiCanh)
WHERE $boiCanh IS NULL OR bc.ma = $boiCanh
OPTIONAL MATCH (th)-[:AP_DUNG]->(x)-[:THUOC_LOP]->(l:Lop)
WITH bc, th, max(l.so) AS lopCan
WHERE lopCan IS NULL OR lopCan <= $lop
RETURN bc.ma AS maBoiCanh, bc.ten AS boiCanh,
       collect(th {.ma, .ten, .thucHanh, lopCan: lopCan}) AS tinhHuong, count(th) AS soTinhHuong
ORDER BY boiCanh
```
`/ApDung/TinhHuong/Xem/{ma}`: mô tả, hình minh họa SVG, "Kiến thức dùng ở đây" (liên kết), "Vì sao cách này đúng" (`loiGiai`), nút "Thử với số đo của em" khi `thucHanh = true` (US-18).
```cypher
MATCH (th:TinhHuong {ma: $ma})-[:TRONG_BOI_CANH]->(bc:BoiCanh)
OPTIONAL MATCH (th)-[:AP_DUNG]->(x)
OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(x)
OPTIONAL MATCH (x)-[:KHANG_DINH]->(dich:KhaiNiem)
WITH th, bc, collect(DISTINCT {ma: x.ma, noiDung: coalesce(x.noiDung, x.ten), bieuThuc: x.bieuThuc,
                               maHinh: coalesce(h.ma, dich.ma)}) AS ds
RETURN th {.*} AS tinhHuong, bc.ten AS boiCanh, [k IN ds WHERE k.ma IS NOT NULL] AS kienThuc
```
**AC:** "Kiểm tra khung cửa có vuông góc không" hiển thị liên kết tới `DH_HCN_3` "Hình bình hành có hai đường chéo bằng nhau là hình chữ nhật" (và `DH_HBH_2`) kèm phần giải thích; học sinh lớp 3 không thấy TH-01.

### US-18 · Thực hành đo đạc (5 điểm) – FR-41, UC-10, SCR-12, BR-10
Biểu mẫu trên trang tình huống có `thucHanh = true`: AB, BC, CD, DA, AC, BD, đơn vị, sai số cho phép (mặc định 1%). Lớp thuần `KiemTraHinhDang` (giả định tứ giác lồi):
1. Hợp lệ: mọi số đo > 0; các tam giác ABC, ACD, ABD, BCD thỏa bất đẳng thức tam giác. Không lập được → báo lỗi, gợi ý đo lại.
2. So sánh trong phạm vi sai số tương đối: nếu bốn cạnh bằng nhau → **hình thoi** (`DH_THOI_1`), thêm hai đường chéo bằng nhau → **hình vuông** (`DH_HV_5`); ngược lại nếu cạnh đối bằng nhau → **hình bình hành** (`DH_HBH_2`), thêm đường chéo bằng nhau → **hình chữ nhật** (`DH_HCN_3`), thêm hai cạnh kề bằng nhau → **hình vuông** (`DH_HV_1`).
3. Không khớp → "Chưa xác định được hình đặc biệt" và chỉ ra cặp số đo lệch nhiều nhất (tỉ lệ %).

Nội dung các dấu hiệu đã dùng lấy từ Neo4j để hiển thị và liên kết:
```cypher
MATCH (d:DauHieu)-[:KHANG_DINH]->(k:KhaiNiem)
WHERE d.ma IN $maDauHieu
RETURN d.ma AS ma, d.noiDung AS noiDung, k.ma AS maHinh, k.ten AS tenHinh
```
**AC:** AB = CD = 2,00 m; BC = DA = 0,90 m; AC = BD = 2,19 m (sai số 1%) → "hình chữ nhật" kèm dấu hiệu "hình bình hành có hai đường chéo bằng nhau"; số đo không lập được tứ giác → báo lỗi. Unit test ≥ 8 trường hợp (hình chữ nhật, hình vuông theo hai nhánh, hình thoi, hình bình hành, không xác định, không hợp lệ, biên sai số).

### US-25 · Quản lý bài tập: thêm, sửa, ẩn (5 điểm) – FR-70, UC-15, SCR-18, SCR-19, BR-05, BR-08
`/QuanTri/BaiTap` (`[Authorize(Roles = "QUAN_TRI")]`): bảng có lọc theo lớp/loại/trạng thái và tìm theo mã hoặc nội dung đề; nút Thêm, Sửa, Ẩn/Hiện.
```cypher
MATCH (bt:BaiTap)
WHERE ($tuKhoa IS NULL OR toLower(bt.ma) CONTAINS toLower($tuKhoa) OR toLower(bt.de) CONTAINS toLower($tuKhoa))
  AND ($loai IS NULL OR bt.loai = $loai)
OPTIONAL MATCH (bt)-[:THUOC_LOP]->(l:Lop)
OPTIONAL MATCH (:TaiKhoan)-[d:DA_LAM]->(bt)
WITH bt, l, count(d) AS soLanLam
WHERE $lopLoc IS NULL OR l.so = $lopLoc
RETURN bt.ma AS ma, left(bt.de, 80) AS de, bt.loai AS loai, l.so AS lop, bt.doKho AS doKho,
       bt.hienThi AS hienThi, soLanLam
ORDER BY ma
```
Biểu mẫu soạn bài (SCR-19): mã, đề (hỗ trợ KaTeX), loại, 4 phương án + đáp án đúng (trắc nghiệm), đáp án số + sai số + đơn vị (đáp án số), lời giải mẫu (chứng minh), giải thích, lớp, độ khó, khái niệm liên quan (chọn nhiều), định lý/công thức liên quan (chọn nhiều), **bản xem trước giống giao diện học sinh**.

Thêm (một giao dịch qua `WriteTransactionAsync`):
```cypher
MATCH (l:Lop {so: $lop})
CREATE (bt:BaiTap {ma: $ma, loai: $loai, de: $de, phuongAn: $phuongAn, dapAnDung: $dapAnDung,
                   dapAnSo: $dapAnSo, saiSo: $saiSo, donVi: $donVi, giaiThich: $giaiThich,
                   loiGiaiMau: $loiGiaiMau, doKho: $doKho, hienThi: true,
                   nguon: 'Nhóm GeoQuad', trangThai: 'DA_RA_SOAT'})
MERGE (bt)-[:THUOC_LOP]->(l)
```
Sửa: cập nhật thuộc tính rồi dựng lại quan hệ (cùng giao dịch):
```cypher
MATCH (bt:BaiTap {ma: $ma})
SET bt += $thuocTinh
WITH bt
OPTIONAL MATCH (bt)-[r:THUOC_LOP|LIEN_QUAN_DEN|SU_DUNG]->()
DELETE r
```
```cypher
MATCH (bt:BaiTap {ma: $ma}), (l:Lop {so: $lop})
MERGE (bt)-[:THUOC_LOP]->(l)
WITH bt
UNWIND $khaiNiem AS maKn
MATCH (k:KhaiNiem {ma: maKn})
MERGE (bt)-[:LIEN_QUAN_DEN]->(k)
```
```cypher
MATCH (bt:BaiTap {ma: $ma})
UNWIND $suDung AS m
MATCH (x) WHERE (x:DinhLy OR x:CongThuc) AND x.ma = m
MERGE (bt)-[:SU_DUNG]->(x)
```
Ẩn/hiện: `MATCH (bt:BaiTap {ma: $ma}) SET bt.hienThi = $hienThi` (không xóa, giữ lịch sử `DA_LAM`).

Lớp kiểm tra hợp lệ (có unit test): trắc nghiệm đủ 4 phương án và **đúng 1** đáp án `A`–`D`; đáp án số có giá trị, sai số > 0, đơn vị; chứng minh có lời giải mẫu; ≥ 1 khái niệm (BR-08); mã trùng → "Mã bài đã tồn tại".

**AC:** trắc nghiệm không có đúng một đáp án đúng, chưa gắn khái niệm, hoặc mã trùng → bị từ chối kèm thông báo; bài ẩn không hiện với học sinh nhưng thống kê cũ không đổi; học sinh mở trang quản trị bị từ chối.

---

## 9. PHẦN C – Học tập cá nhân (Thành viên C, 24 điểm)

### US-08 · Cập nhật hồ sơ, xóa tài khoản (2 điểm) – FR-04, UC-14, SCR-17, NFR-06
`/HocTap/HoSo` (bắt buộc đăng nhập): sửa biệt danh, lớp; đổi mật khẩu (nhập mật khẩu cũ, mới, nhập lại); đoạn thông báo quyền riêng tư; **xóa tài khoản** (xác nhận bằng mật khẩu).
```cypher
MATCH (tk:TaiKhoan {id: $tk}), (l:Lop {so: $lop})
OPTIONAL MATCH (tk)-[r:HOC_LOP]->()
DELETE r
MERGE (tk)-[:HOC_LOP]->(l)
SET tk.bietDanh = $bietDanh
```
Sau khi đổi lớp/biệt danh gọi `IAuthHelper.DangNhapAsync` để cấp lại cookie. Đổi mật khẩu dùng `IAuthHelper.KiemTraMatKhau` / `BamMatKhau`, rồi `SET tk.matKhauBam = $bam`.
```cypher
MATCH (tk:TaiKhoan {id: $tk}) DETACH DELETE tk   // xóa tài khoản và mọi DA_LAM, DA_HOC, HOC_LOP
```
**AC:** đổi lớp 7 → 8 thì Thư viện hiển thị nội dung lớp 8 mặc định; đổi mật khẩu yêu cầu mật khẩu cũ đúng; xóa tài khoản thì đăng xuất và không còn nút `TaiKhoan` đó.

### US-19 · Ngân hàng bài tập (2 điểm) – FR-50, SCR-13
`/HocTap/BaiTap`: bộ lọc cấp, lớp, dạng, độ khó, khái niệm (kết hợp được); cột "Đã làm / Đã đúng" cho học sinh; nhãn "Nâng cao".
```cypher
MATCH (bt:BaiTap {hienThi: true})-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE l.so <= $lop
  AND ($lopLoc IS NULL OR l.so = $lopLoc)
  AND ($capLoc IS NULL OR cap.ma = $capLoc)
  AND ($loai IS NULL OR bt.loai = $loai)
  AND ($doKho IS NULL OR bt.doKho = $doKho)
  AND ($khaiNiem IS NULL OR EXISTS { MATCH (bt)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma: $khaiNiem}) })
OPTIONAL MATCH (:TaiKhoan {id: $tk})-[d:DA_LAM]->(bt)
WITH bt, l, count(d) AS soLan, any(x IN collect(d) WHERE x.dung) AS daDung
RETURN bt.ma AS ma, left(bt.de, 120) AS de, bt.loai AS loai, bt.doKho AS doKho, l.so AS lop, soLan, daDung
ORDER BY lop, doKho, ma
```
**AC:** bộ lọc kết hợp được nhiều tiêu chí (ví dụ lớp 8 + đáp án số + Hình thoi).

### US-20 · Làm bài và chấm tự động (5 điểm) – FR-51, UC-11, SCR-14, BR-05, BR-09
`/HocTap/BaiTap/Lam/{ma}`: đề (KaTeX, hình nếu có), trắc nghiệm 4 nút lớn A–D, đáp án số là ô nhập. **Không gửi đáp án đúng xuống trình duyệt trước khi nộp.** Chưa chọn/nhập → nhắc, không chấm.
```cypher
MATCH (bt:BaiTap {ma: $ma, hienThi: true})
OPTIONAL MATCH (bt)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
OPTIONAL MATCH (bt)-[:SU_DUNG]->(x)
RETURN bt {.*} AS baiTap, collect(DISTINCT k {.ma, .ten}) AS khaiNiem,
       collect(DISTINCT x {.ma, ten: coalesce(x.ten, x.noiDung), bieuThuc: x.bieuThuc}) AS suDung
```
Lớp thuần `ChamBai` (BR-05): trắc nghiệm đúng khi chọn đúng chữ cái; đáp án số chấp nhận dấu phẩy hoặc dấu chấm, đúng khi `|nhập − dapAnSo| ≤ saiSo`. Kết quả: đúng/sai, đáp án, giải thích, định lý/công thức liên quan (liên kết chi tiết hình), nút "Bài kế tiếp" (`/HocTap/BaiTap/KeTiep/{ma}`: học sinh → bài đầu tiên của US-24; khách → bài kế tiếp cùng lớp theo mã). Học sinh thì lưu lần làm; khách hiện `_MoiDangNhap`:
```cypher
MATCH (tk:TaiKhoan {id: $tk}), (bt:BaiTap {ma: $ma})
CREATE (tk)-[:DA_LAM {luc: datetime(), dung: $dung, dapAnDaChon: $dapAn, thoiGianGiay: $giay}]->(bt)
```
(`thoiGianGiay` đo từ lúc mở trang đến lúc nộp.)

**AC:** trắc nghiệm đáp án B: chọn B báo đúng, chọn khác báo sai và có giải thích; bài đáp án 96 (sai số 0,01): "96,0" đúng, "95" sai; học sinh đăng nhập được lưu lần làm (xuất hiện ở Tiến độ), khách thì không. Unit test `ChamBai` ≥ 6 trường hợp.

### US-21 · Lời giải mẫu bài tự luận (2 điểm) – FR-52, UC-11 luồng 2a
Bài `CHUNG_MINH`: hiển thị đề và nút "Xem lời giải mẫu" (không chấm). Lời giải mẫu hiện `loiGiaiMau`; nếu bài dùng một định lý đã có chứng minh mẫu thì thêm liên kết sang `/ApDung/ChungMinh/Xem/{ma}`:
```cypher
MATCH (bt:BaiTap {ma: $ma})
OPTIONAL MATCH (bt)-[:SU_DUNG]->(:DinhLy)<-[:CHUNG_MINH_CHO]-(cm:ChungMinh {trangThai:'DA_RA_SOAT'})
RETURN bt.loiGiaiMau AS loiGiaiMau, collect(DISTINCT cm {.ma, .ten}) AS chungMinhMau
```
**AC:** bài chứng minh không chấm tự động và có nút xem lời giải mẫu; BT-027 có liên kết sang CM-01.

### US-22 · Lộ trình học (5 điểm) – FR-60, UC-12, SCR-15
`/HocTap/LoTrinh?muc={ma}` (bắt buộc đăng nhập). Chọn khái niệm mục tiêu (mặc định hình có lớp lớn nhất ≤ lớp học sinh); các bước từ nền tảng đến mục tiêu, bước đã học có ✓, thanh phần trăm hoàn thành, mỗi bước liên kết `/KienThuc/ThuVien/ChiTiet/{ma}` và có nút "Em đã hiểu".
```cypher
MATCH (muc:KhaiNiem {ma: $muc})
MATCH p = (muc)-[:CAN_BIET_TRUOC*0..]->(k:KhaiNiem)-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND k.trangThai = 'DA_RA_SOAT'
WITH k, max(length(p)) AS doSau
OPTIONAL MATCH (:TaiKhoan {id: $tk})-[h:DA_HOC]->(k)
RETURN k.ma AS ma, k.ten AS ten, k.loai AS loai, doSau, h IS NOT NULL AS daHoc
ORDER BY doSau DESC, ma
```
"Em đã hiểu" (POST trong Area HocTap, Cypher tự viết trong Repository của C):
```cypher
MATCH (tk:TaiKhoan {id: $tk}), (k:KhaiNiem {ma: $ma})
MERGE (tk)-[h:DA_HOC]->(k) ON CREATE SET h.luc = datetime()
```
Mục tiêu không có khái niệm tiên quyết → "Em có thể học ngay khái niệm này".

**AC:** `hocsinh8`, mục tiêu Hình vuông: Hình chữ nhật và Hình thoi đứng trước Hình vuông, khái niệm nền của chúng (Góc vuông, Cạnh đối, Cạnh kề…) đứng trước chúng, Hình vuông ở cuối; danh sách mục tiêu của `hocsinh4` không có Hình thang cân; đánh dấu một bước, tải lại vẫn ✓.

### US-23 · Tiến độ và khái niệm cần ôn (5 điểm) – FR-61, UC-13, SCR-16, BR-06
`/HocTap/TienDo` (bắt buộc đăng nhập): số lần làm, số bài, tỉ lệ đúng; **biểu đồ thanh ngang** tỉ lệ đúng theo khái niệm (HTML/CSS, kèm số %); danh sách "Cần ôn lại"; nút "Làm bài gợi ý" (US-24). Chưa làm bài nào → lời mời làm bài đầu tiên; không có khái niệm cần ôn → chúc mừng và gợi ý bài khó hơn hoặc lộ trình.
```cypher
MATCH (:TaiKhoan {id: $tk})-[d:DA_LAM]->(bt:BaiTap)
RETURN count(d) AS soLanLam, count(DISTINCT bt) AS soBai,
       sum(CASE WHEN d.dung THEN 1 ELSE 0 END) AS soDung
```
```cypher
MATCH (:TaiKhoan {id: $tk})-[d:DA_LAM]->(:BaiTap)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
RETURN k.ma AS ma, k.ten AS ten, count(d) AS soLan,
       toFloat(sum(CASE WHEN d.dung THEN 1 ELSE 0 END)) / count(d) AS tiLe
ORDER BY tiLe, ten
```
Cần ôn lại (BR-06; `$nguong` đọc từ `HocTap:NguongCanOn`):
```cypher
MATCH (:TaiKhoan {id: $tk})-[lam:DA_LAM]->(:BaiTap)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WITH k, lam ORDER BY lam.luc DESC
WITH k, collect(lam)[0..5] AS gan
WHERE size(gan) >= 3
WITH k, size(gan) AS soLan, toFloat(size([x IN gan WHERE x.dung])) / size(gan) AS tiLe
WHERE tiLe < $nguong
RETURN k.ma AS ma, k.ten AS ten, soLan, tiLe
ORDER BY tiLe, ten
```
**AC:** sau khi chạy `31-lam-bai-mau-C.cypher`, `hocsinh8` (5 lần gần nhất về Hình thoi đúng 2) thấy "Hình thoi" trong "Cần ôn lại"; khái niệm có ít hơn 3 lần làm không bị xếp vào; khách bị chuyển tới đăng nhập.

### US-24 · Gợi ý bài tập kế tiếp (3 điểm) – FR-62, UC-13
`/HocTap/TienDo/GoiY`: tối đa 5 bài chưa làm đúng, thuộc khái niệm cần ôn (kết quả US-23), lớp ≤ lớp học sinh, độ khó tăng dần, không gồm bài chứng minh.
```cypher
MATCH (bt:BaiTap)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WHERE k.ma IN $canOn AND bt.hienThi = true AND bt.loai <> 'CHUNG_MINH'
MATCH (bt)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop
  AND NOT EXISTS { MATCH (:TaiKhoan {id: $tk})-[d:DA_LAM]->(bt) WHERE d.dung = true }
RETURN DISTINCT bt.ma AS ma, left(bt.de, 120) AS de, bt.doKho AS doKho
ORDER BY doKho, ma LIMIT 5
```
Không có khái niệm cần ôn (UC-13 luồng 3a) → chúc mừng, gợi ý 5 bài chưa làm đúng xếp theo độ khó giảm dần (bài khó trước) và liên kết Lộ trình học.

**AC:** gợi ý tối đa 5 bài, không có bài học sinh đã làm đúng, đúng lớp, độ khó tăng dần.

---

## 10. PHẦN TÍCH HỢP (US-26, US-27; cuối ngày 08/10)

### US-26 · Kiểm thử, sửa lỗi, hoàn thiện giao diện (5 điểm)
- Kiểm thử chéo theo vòng: **A kiểm thử phần B, B kiểm thử phần C, C kiểm thử phần A**: chạy hết AC của từng story, tự chạy lại ít nhất 2 câu Cypher của bạn trên Neo4j Browser, ghi kết quả và lỗi vào `docs/kiem-thu.md` (bảng: story, AC, đạt/không, ghi chú).
- Mỗi người sửa lỗi trong phạm vi của mình; kiểm tra hiển thị ở chiều rộng 360px và trên máy tính.

**AC:** mọi AC của story Must đạt; không có lỗi nghiêm trọng ở luồng đăng ký, tra cứu, gợi ý, làm bài.

### US-27 · README, sao lưu/khôi phục, kịch bản demo (2 điểm) – NFR-07, NFR-13
- **A:** cập nhật README phần "Chạy dự án từ máy sạch" (cài đặt, `docker compose up`, seed, tài khoản demo) và thử trên một máy chưa cài.
- **B:** `scripts/backup.*` và `scripts/restore.*` dùng `neo4j-admin database dump` / `load` (dừng Neo4j khi dump/load với bản Community, file vào `./backups`); thử khôi phục thành công một lần.
- **C:** `docs/demo.md` theo kịch bản mục 12, tập chạy và bấm giờ.

**AC:** người ngoài chạy được hệ thống từ máy sạch theo README trong 15 phút; khôi phục dữ liệu thử thành công.

---

## 11. Lịch 2 ngày

| Thời gian | Cả nhóm | A | B | C |
|---|---|---|---|---|
| 07/10 chiều | PHẦN 0 trên một máy; mỗi người giải thích lại phần được phân công | | | |
| 07/10 tối | | Seed US-03 + US-04 phần A (gồm tình huống thực tế); viết và chạy thử Cypher của mọi story A trên Neo4j Browser (`http://localhost:7474`), ghi vào `docs/cypher/A-tra-cuu.md` | Như bên cạnh, phần B | Rà soát US-03; seed US-04 phần C; Cypher của phần C |
| 08/10 sáng | | US-09, US-10, US-11 | US-13, US-14 | US-19, US-20, US-21 |
| 08/10 đầu chiều | | US-12, US-15, US-16 | US-17, US-18, US-25 | US-08, US-22, US-23, US-24 |
| 08/10 cuối chiều | US-26 kiểm thử chéo, merge vào `main` (không xung đột vì mỗi người một thư mục), US-27, tập demo | | | |

## 12. Kịch bản demo (10–12 phút)

1. Khách, chọn lớp 8: Thư viện → Hình chữ nhật (A); tìm "hinh thoi" (A).
2. Bản đồ kiến thức; "Vì sao Hình vuông là Hình thang?" (A).
3. Máy tính hình học: hình chữ nhật 5 × 3 kèm hình vẽ (A).
4. Nên dùng định lý nào? hình bình hành + một góc vuông → hình chữ nhật → chứng minh mẫu CM-01 (B).
5. Hình học quanh ta → kiểm tra khung cửa → thực hành đo đạc với số đo 2,00 / 0,90 / 2,19 (B).
6. Đăng nhập `hocsinh8`: làm 2 bài (C), xem lời giải mẫu BT-027 (C), lộ trình Hình vuông (C), tiến độ và bài gợi ý (C), đổi biệt danh (C).
7. Đăng nhập `admin`: thêm, sửa, ẩn một bài tập (B).
8. Neo4j Browser: mỗi người chạy một câu Cypher của mình và giải thích.

## 13. Definition of Done

- [ ] `docker compose up -d neo4j`, `scripts/seed` (2 lần), `dotnet run` chạy được từ máy sạch.
- [ ] `dotnet build` không lỗi, `dotnet test` qua.
- [ ] Mọi AC của story đạt; giao diện dùng được ở chiều rộng 360px.
- [ ] Không có câu Cypher nối chuỗi dữ liệu người dùng.
- [ ] Không sửa file ngoài phạm vi.
- [ ] `docs/cypher/<phần>.md` có đủ truy vấn kèm giải thích ký hiệu và kết quả mong đợi.
- [ ] Một thành viên khác đã xem mã (review chéo).

---

## Phụ lục D. Dữ liệu nội dung (từ SRS Phụ lục B)

Phân lớp là đề xuất, cần đối chiếu Chương trình GDPT 2018 và sách giáo khoa nhóm chọn (SRS OI-02, OI-05). Biểu thức công thức dưới đây viết dạng văn bản; khi seed, chuyển sang LaTeX.

### D.1 Hình (KhaiNiem loai HINH) và yếu tố (KhaiNiem loai YEU_TO) — PHẦN 0
| ma | ten | Lớp | Cần biết trước (CAN_BIET_TRUOC) | dinhNghia |
|---|---|---|---|---|
| TU_GIAC | Tứ giác | 3 | — | Hình gồm bốn đoạn thẳng nối tiếp AB, BC, CD, DA khép kín, trong đó không có hai đoạn liên tiếp nào nằm trên cùng một đường thẳng. |
| HINH_THANG | Hình thang | 5 | TU_GIAC, HAI_DT_SONG_SONG | Tứ giác có hai cạnh đối song song. |
| HINH_THANG_CAN | Hình thang cân | 6 | HINH_THANG, DUONG_CHEO | Hình thang có hai góc kề một đáy bằng nhau. |
| HINH_BINH_HANH | Hình bình hành | 4 | TU_GIAC, HAI_DT_SONG_SONG, CANH_DOI | Tứ giác có các cạnh đối song song. |
| HINH_CHU_NHAT | Hình chữ nhật | 1 | GOC_VUONG, CANH_DOI | Tứ giác có bốn góc vuông. |
| HINH_THOI | Hình thoi | 4 | CANH_KE, HAI_DT_VUONG_GOC | Tứ giác có bốn cạnh bằng nhau. |
| HINH_VUONG | Hình vuông | 1 | HINH_CHU_NHAT, HINH_THOI | Tứ giác có bốn góc vuông và bốn cạnh bằng nhau. |

| ma (yếu tố) | ten | Lớp | dinhNghia (gợi ý viết) |
|---|---|---|---|
| CANH_KE | Cạnh kề | 3 | Viết định nghĩa ngắn, dễ hiểu |
| CANH_DOI | Cạnh đối | 3 | Viết định nghĩa ngắn, dễ hiểu |
| DUONG_CHEO | Đường chéo | 3 | Viết định nghĩa ngắn, dễ hiểu |
| GOC_VUONG | Góc vuông | 3 | Viết định nghĩa ngắn, dễ hiểu |
| HAI_DT_SONG_SONG | Hai đường thẳng song song | 4 | Viết định nghĩa ngắn, dễ hiểu |
| HAI_DT_VUONG_GOC | Hai đường thẳng vuông góc | 4 | Viết định nghĩa ngắn, dễ hiểu |

### D.2 Quan hệ LA_TRUONG_HOP_DAC_BIET_CUA — PHẦN 0
| Từ (hình đặc biệt) | Tới (hình tổng quát) |
|---|---|
| HINH_THANG (Hình thang) | TU_GIAC (Tứ giác) |
| HINH_THANG_CAN (Hình thang cân) | HINH_THANG (Hình thang) |
| HINH_BINH_HANH (Hình bình hành) | HINH_THANG (Hình thang) |
| HINH_CHU_NHAT (Hình chữ nhật) | HINH_BINH_HANH (Hình bình hành) |
| HINH_THOI (Hình thoi) | HINH_BINH_HANH (Hình bình hành) |
| HINH_VUONG (Hình vuông) | HINH_CHU_NHAT (Hình chữ nhật) |
| HINH_VUONG (Hình vuông) | HINH_THOI (Hình thoi) |

### D.3 Điều kiện (DieuKien) — PHẦN B
| ma | noiDung |
|---|---|
| DK_MOT_CAP_CANH_DOI_SONG | Có một cặp cạnh đối song song |
| DK_CANH_DOI_SONG | Các cạnh đối song song |
| DK_CANH_DOI_BANG | Các cạnh đối bằng nhau |
| DK_MOT_CAP_SONG_SONG_BANG | Có một cặp cạnh đối vừa song song vừa bằng nhau |
| DK_GOC_DOI_BANG | Các góc đối bằng nhau |
| DK_CHEO_CAT_TRUNG_DIEM | Hai đường chéo cắt nhau tại trung điểm của mỗi đường |
| DK_BA_GOC_VUONG | Có ba góc vuông |
| DK_MOT_GOC_VUONG | Có một góc vuông |
| DK_CHEO_BANG_NHAU | Hai đường chéo bằng nhau |
| DK_BON_CANH_BANG | Bốn cạnh bằng nhau |
| DK_HAI_CANH_KE_BANG | Hai cạnh kề bằng nhau |
| DK_CHEO_VUONG_GOC | Hai đường chéo vuông góc |
| DK_CHEO_PHAN_GIAC | Một đường chéo là đường phân giác của một góc |
| DK_HAI_GOC_KE_DAY_BANG | Hai góc kề một đáy bằng nhau |

### D.4 Dấu hiệu nhận biết (DinhLy:DauHieu) — PHẦN B
| ma | noiDung | YEU_CAU_LA | YEU_CAU_CO | KHANG_DINH | Lớp |
|---|---|---|---|---|---|
| DH_HT_1 | Tứ giác có hai cạnh đối song song là hình thang. | TU_GIAC | DK_MOT_CAP_CANH_DOI_SONG | HINH_THANG | 8 |
| DH_HTC_1 | Hình thang có hai góc kề một đáy bằng nhau là hình thang cân. | HINH_THANG | DK_HAI_GOC_KE_DAY_BANG | HINH_THANG_CAN | 8 |
| DH_HTC_2 | Hình thang có hai đường chéo bằng nhau là hình thang cân. | HINH_THANG | DK_CHEO_BANG_NHAU | HINH_THANG_CAN | 8 |
| DH_HBH_1 | Tứ giác có các cạnh đối song song là hình bình hành. | TU_GIAC | DK_CANH_DOI_SONG | HINH_BINH_HANH | 8 |
| DH_HBH_2 | Tứ giác có các cạnh đối bằng nhau là hình bình hành. | TU_GIAC | DK_CANH_DOI_BANG | HINH_BINH_HANH | 8 |
| DH_HBH_3 | Tứ giác có hai cạnh đối song song và bằng nhau là hình bình hành. | TU_GIAC | DK_MOT_CAP_SONG_SONG_BANG | HINH_BINH_HANH | 8 |
| DH_HBH_4 | Tứ giác có các góc đối bằng nhau là hình bình hành. | TU_GIAC | DK_GOC_DOI_BANG | HINH_BINH_HANH | 8 |
| DH_HBH_5 | Tứ giác có hai đường chéo cắt nhau tại trung điểm của mỗi đường là hình bình hành. | TU_GIAC | DK_CHEO_CAT_TRUNG_DIEM | HINH_BINH_HANH | 8 |
| DH_HCN_1 | Tứ giác có ba góc vuông là hình chữ nhật. | TU_GIAC | DK_BA_GOC_VUONG | HINH_CHU_NHAT | 8 |
| DH_HCN_2 | Hình bình hành có một góc vuông là hình chữ nhật. | HINH_BINH_HANH | DK_MOT_GOC_VUONG | HINH_CHU_NHAT | 8 |
| DH_HCN_3 | Hình bình hành có hai đường chéo bằng nhau là hình chữ nhật. | HINH_BINH_HANH | DK_CHEO_BANG_NHAU | HINH_CHU_NHAT | 8 |
| DH_THOI_1 | Tứ giác có bốn cạnh bằng nhau là hình thoi. | TU_GIAC | DK_BON_CANH_BANG | HINH_THOI | 8 |
| DH_THOI_2 | Hình bình hành có hai cạnh kề bằng nhau là hình thoi. | HINH_BINH_HANH | DK_HAI_CANH_KE_BANG | HINH_THOI | 8 |
| DH_THOI_3 | Hình bình hành có hai đường chéo vuông góc với nhau là hình thoi. | HINH_BINH_HANH | DK_CHEO_VUONG_GOC | HINH_THOI | 8 |
| DH_THOI_4 | Hình bình hành có một đường chéo là đường phân giác của một góc là hình thoi. | HINH_BINH_HANH | DK_CHEO_PHAN_GIAC | HINH_THOI | 8 |
| DH_HV_1 | Hình chữ nhật có hai cạnh kề bằng nhau là hình vuông. | HINH_CHU_NHAT | DK_HAI_CANH_KE_BANG | HINH_VUONG | 8 |
| DH_HV_2 | Hình chữ nhật có hai đường chéo vuông góc với nhau là hình vuông. | HINH_CHU_NHAT | DK_CHEO_VUONG_GOC | HINH_VUONG | 8 |
| DH_HV_3 | Hình chữ nhật có một đường chéo là đường phân giác của một góc là hình vuông. | HINH_CHU_NHAT | DK_CHEO_PHAN_GIAC | HINH_VUONG | 8 |
| DH_HV_4 | Hình thoi có một góc vuông là hình vuông. | HINH_THOI | DK_MOT_GOC_VUONG | HINH_VUONG | 8 |
| DH_HV_5 | Hình thoi có hai đường chéo bằng nhau là hình vuông. | HINH_THOI | DK_CHEO_BANG_NHAU | HINH_VUONG | 8 |

### D.5 Tính chất (DinhLy:TinhChat) — PHẦN A
| ma | Hình (CO_TINH_CHAT) | noiDung | Lớp |
|---|---|---|---|
| TC_TG_1 | TU_GIAC | Tổng bốn góc của một tứ giác bằng 360°. | 8 |
| TC_HT_1 | HINH_THANG | Hai góc kề một cạnh bên của hình thang có tổng bằng 180°. | 8 |
| TC_HTC_1 | HINH_THANG_CAN | Hai cạnh bên của hình thang cân bằng nhau. | 8 |
| TC_HTC_2 | HINH_THANG_CAN | Hai đường chéo của hình thang cân bằng nhau. | 8 |
| TC_HBH_1 | HINH_BINH_HANH | Các cạnh đối của hình bình hành bằng nhau. | 8 |
| TC_HBH_2 | HINH_BINH_HANH | Các góc đối của hình bình hành bằng nhau. | 8 |
| TC_HBH_3 | HINH_BINH_HANH | Hai đường chéo của hình bình hành cắt nhau tại trung điểm của mỗi đường. | 8 |
| TC_HCN_1 | HINH_CHU_NHAT | Hình chữ nhật có bốn góc vuông. | 8 |
| TC_HCN_2 | HINH_CHU_NHAT | Hai đường chéo của hình chữ nhật bằng nhau. | 8 |
| TC_THOI_1 | HINH_THOI | Hai đường chéo của hình thoi vuông góc với nhau. | 8 |
| TC_THOI_2 | HINH_THOI | Hai đường chéo của hình thoi là các đường phân giác của các góc của hình thoi. | 8 |
| TC_HV_1 | HINH_VUONG | Hình vuông có tất cả tính chất của hình chữ nhật và của hình thoi. | 8 |

### D.6 Công thức (CongThuc) — PHẦN A
| ma | Hình (CO_CONG_THUC) | ten | Biểu thức | Lớp |
|---|---|---|---|---|
| CT_TG_CHUVI | TU_GIAC | Chu vi tứ giác | P = a + b + c + d | 3 |
| CT_HCN_CV | HINH_CHU_NHAT | Chu vi hình chữ nhật | P = 2(a + b) | 3 |
| CT_HCN_DT | HINH_CHU_NHAT | Diện tích hình chữ nhật | S = a · b | 3 |
| CT_HV_CV | HINH_VUONG | Chu vi hình vuông | P = 4a | 3 |
| CT_HV_DT | HINH_VUONG | Diện tích hình vuông | S = a² | 3 |
| CT_HBH_CV | HINH_BINH_HANH | Chu vi hình bình hành | P = 2(a + b) | 4 |
| CT_HBH_DT | HINH_BINH_HANH | Diện tích hình bình hành | S = a · h (đáy nhân chiều cao) | 4 |
| CT_THOI_CV | HINH_THOI | Chu vi hình thoi | P = 4a | 4 |
| CT_THOI_DT | HINH_THOI | Diện tích hình thoi | S = (d₁ · d₂) / 2 | 4 |
| CT_HT_DT | HINH_THANG | Diện tích hình thang | S = (a + b) · h / 2 | 5 |
| CT_HCN_CHEO | HINH_CHU_NHAT | Đường chéo hình chữ nhật | d = √(a² + b²) | 8 |
| CT_HV_CHEO | HINH_VUONG | Đường chéo hình vuông | d = a√2 | 8 |

### D.7 Định lý nền (DinhLy, không nhãn phụ) — PHẦN B
| ma | noiDung | Lớp |
|---|---|---|
| DL_NEN_1 | Tổng ba góc của một tam giác bằng 180°. | 7 |
| DL_NEN_2 | Hai tam giác bằng nhau theo các trường hợp c.c.c, c.g.c, g.c.g. | 7 |
| DL_NEN_3 | Định lí Pythagore: trong tam giác vuông, bình phương cạnh huyền bằng tổng bình phương hai cạnh góc vuông. | 8 |
| DL_NEN_4 | Đường trung bình của tam giác song song với cạnh thứ ba và bằng nửa cạnh ấy. | 8 |
| DL_NEN_5 | Hai đường thẳng song song bị cắt bởi một cát tuyến thì các cặp góc so le trong bằng nhau. | 7 |
| DL_NEN_6 | Tứ giác ABCD là hình bình hành khi và chỉ khi vectơ AB bằng vectơ DC (lớp 10). | 10 |

### D.8 Bối cảnh và tình huống thực tế — PHẦN B
| BoiCanh.ma | ten |
|---|---|
| BC_NHA_CUA | Nhà cửa |
| BC_MANH_VUON | Mảnh vườn, thửa đất |
| BC_DO_DUNG | Đồ dùng, sân chơi |

| ma | Bối cảnh | ten | moTa | Kiến thức (AP_DUNG) | thucHanh |
|---|---|---|---|---|---|
| TH-01 | BC_NHA_CUA | Kiểm tra khung cửa có vuông góc không | Thợ mộc đo hai cặp cạnh đối và hai đường chéo của khung cửa. | DH_HBH_2, DH_HCN_3 | true |
| TH-02 | BC_NHA_CUA | Tính số viên gạch lát nền phòng hình chữ nhật | Diện tích nền chia cho diện tích một viên gạch, cộng hao hụt. | CT_HCN_DT | false |
| TH-03 | BC_NHA_CUA | Lát sân bằng gạch hình thoi | Tính diện tích mỗi viên gạch thoi từ hai đường chéo để ước lượng số viên. | CT_THOI_DT | false |
| TH-04 | BC_MANH_VUON | Mua bao nhiêu mét lưới rào vườn hình chữ nhật | Chu vi mảnh vườn quyết định độ dài lưới rào. | CT_HCN_CV | false |
| TH-05 | BC_MANH_VUON | Chia thửa ruộng hình thang thành luống rau | Diện tích thửa ruộng hình thang theo hai đáy và chiều cao. | CT_HT_DT | false |
| TH-06 | BC_MANH_VUON | Diện tích thửa đất hình bình hành (đừng nhân hai cạnh kề) | Phân biệt cạnh bên với chiều cao khi tính diện tích. | CT_HBH_DT | false |
| TH-07 | BC_DO_DUNG | Cổng xếp và giá phơi đồ: hình đổi dạng nhưng chu vi không đổi | Các thanh bằng nhau tạo hình thoi/hình bình hành; đổi góc làm diện tích thay đổi. | TC_HBH_1, CT_HBH_CV | false |
| TH-08 | BC_DO_DUNG | Sân cầu lông: chu vi vạch kẻ và đường chéo | Tính độ dài vạch kẻ và đường chéo sân hình chữ nhật. | CT_HCN_CV, CT_HCN_CHEO | false |
| TH-09 | BC_DO_DUNG | Kiểm tra mặt bàn có vuông không | Đo bốn cạnh và hai đường chéo mặt bàn. | DH_THOI_1, DH_HV_5 | true |

Hình liên quan (`LIEN_QUAN_DEN`): lấy hình của từng mã kiến thức (tính chất/công thức → hình sở hữu; dấu hiệu → hình kết luận).

### D.9 Chứng minh mẫu — PHẦN B
| ma | ten | CHUNG_MINH_CHO | Căn cứ (CAN_CU của các bước) |
|---|---|---|---|
| CM-01 | Hình bình hành có một góc vuông là hình chữ nhật | DH_HCN_2 | TC_HBH_2, DL_NEN_5 |
| CM-02 | Hình bình hành có hai đường chéo bằng nhau là hình chữ nhật | DH_HCN_3 | TC_HBH_1, DL_NEN_2, DL_NEN_5 |
| CM-03 | Hình bình hành có hai đường chéo vuông góc là hình thoi | DH_THOI_3 | TC_HBH_3, DL_NEN_2 |
| CM-04 | Tứ giác có các cạnh đối bằng nhau là hình bình hành | DH_HBH_2 | DL_NEN_2, DL_NEN_5 |

### D.10 Bài tập mẫu — PHẦN C
| ma | Lớp | Loại | Độ khó | Đề | Đáp án | Liên quan |
|---|---|---|---|---|---|---|
| BT-001 | 3 | Trắc nghiệm | 1 | Một hình chữ nhật có chiều dài 8 cm, chiều rộng 5 cm. Chu vi hình chữ nhật là: A. 13 cm; B. 26 cm; C. 40 cm; D. 20 cm. | B | HINH_CHU_NHAT, CT_HCN_CV |
| BT-014 | 8 | Đáp án số | 2 | Hình thoi ABCD có hai đường chéo AC = 12 cm và BD = 16 cm. Tính diện tích hình thoi (đơn vị cm²). | 96 | HINH_THOI, CT_THOI_DT |
| BT-027 | 8 | Chứng minh | 2 | Cho hình bình hành ABCD có góc A bằng 90°. Chứng minh ABCD là hình chữ nhật. | Xem lời giải mẫu (CM-01) | HINH_CHU_NHAT, DH_HCN_2 |

BT-001: `loai TRAC_NGHIEM`, `phuongAn ["13 cm","26 cm","40 cm","20 cm"]`, `dapAnDung "B"`. BT-014: `loai DAP_AN_SO`, `dapAnSo 96`, `saiSo 0.01`, `donVi "cm²"`. BT-027: `loai CHUNG_MINH`, `loiGiaiMau` viết lại từ các bước của CM-01.
