// ============================================================================
// GeoQuad · 01-khung.cypher · PHẦN 0 · US-02 (NFR-08)
// Cấp học, lớp, 7 hình tứ giác, 6 yếu tố, quan hệ đặc biệt hóa và cần biết trước.
// Dữ liệu lấy từ README Phụ lục D.1 và D.2.
// Mọi câu dùng MERGE theo khóa (`Lop.so`, `CapHoc.ma`, `KhaiNiem.ma`) nên chạy
// lặp lại không sinh nút trùng (NFR-08).
// ----------------------------------------------------------------------------
// Giải thích ký hiệu:
//   UNWIND [...] AS row   trải danh sách thành từng dòng để xử lý lần lượt
//   MERGE (n:Nhan {khoa}) tìm nút theo khóa, chưa có thì tạo
//   ON CREATE SET         chỉ đặt khi nút vừa được tạo
//   SET                   đặt (ghi đè) thuộc tính mỗi lần chạy
//   (a)-[:QUAN_HE]->(b)   quan hệ có chiều từ a tới b
//   {} dấu ngoặc nhọn     bộ thuộc tính / bản đồ khóa-giá trị
// ============================================================================

// ---- 3 cấp học (BR-03) -----------------------------------------------------
UNWIND [
  {ma: 'CAP_1', ten: 'Cấp 1 (lớp 1–5)'},
  {ma: 'CAP_2', ten: 'Cấp 2 (lớp 6–9)'},
  {ma: 'CAP_3', ten: 'Cấp 3 (lớp 10–12)'}
] AS row
MERGE (c:CapHoc {ma: row.ma})
SET c.ten = row.ten;

// ---- 12 lớp và quan hệ THUOC_CAP (BR-03) -----------------------------------
// lớp 1–5 → CAP_1, lớp 6–9 → CAP_2, lớp 10–12 → CAP_3
UNWIND range(1, 12) AS so
MERGE (l:Lop {so: so})
SET l.ten = 'Lớp ' + toString(so)
WITH l, so,
     CASE
       WHEN so <= 5 THEN 'CAP_1'
       WHEN so <= 9 THEN 'CAP_2'
       ELSE 'CAP_3'
     END AS maCap
MATCH (c:CapHoc {ma: maCap})
MERGE (l)-[:THUOC_CAP]->(c);

// ---- 7 hình tứ giác (KhaiNiem loai HINH) — Phụ lục D.1 ---------------------
UNWIND [
  {ma: 'TU_GIAC', ten: 'Tứ giác', lop: 3,
   dinhNghia: 'Hình gồm bốn đoạn thẳng nối tiếp AB, BC, CD, DA khép kín, trong đó không có hai đoạn liên tiếp nào nằm trên cùng một đường thẳng.',
   ghiChuTieuHoc: null},
  {ma: 'HINH_THANG', ten: 'Hình thang', lop: 5,
   dinhNghia: 'Tứ giác có hai cạnh đối song song.',
   ghiChuTieuHoc: 'Ở tiểu học: hình thang có một cặp cạnh đối song song'},
  {ma: 'HINH_THANG_CAN', ten: 'Hình thang cân', lop: 6,
   dinhNghia: 'Hình thang có hai góc kề một đáy bằng nhau.',
   ghiChuTieuHoc: null},
  {ma: 'HINH_BINH_HANH', ten: 'Hình bình hành', lop: 4,
   dinhNghia: 'Tứ giác có các cạnh đối song song.',
   ghiChuTieuHoc: 'Ở tiểu học: hình bình hành có hai cặp cạnh đối song song và bằng nhau; chưa coi hình chữ nhật, hình thoi, hình vuông là hình bình hành'},
  {ma: 'HINH_CHU_NHAT', ten: 'Hình chữ nhật', lop: 1,
   dinhNghia: 'Tứ giác có bốn góc vuông.',
   ghiChuTieuHoc: null},
  {ma: 'HINH_THOI', ten: 'Hình thoi', lop: 4,
   dinhNghia: 'Tứ giác có bốn cạnh bằng nhau.',
   ghiChuTieuHoc: null},
  {ma: 'HINH_VUONG', ten: 'Hình vuông', lop: 1,
   dinhNghia: 'Tứ giác có bốn góc vuông và bốn cạnh bằng nhau.',
   ghiChuTieuHoc: null}
] AS row
MERGE (k:KhaiNiem {ma: row.ma})
SET k.ten           = row.ten,
    k.loai          = 'HINH',
    k.dinhNghia     = row.dinhNghia,
    k.ghiChuTieuHoc = row.ghiChuTieuHoc,
    k.nguon         = 'Nhóm GeoQuad biên soạn theo SRS Phụ lục B.1; '
                      + 'đối chiếu CT GDPT 2018 – Toán ' + toString(row.lop)
                      + '; thành viên C rà soát 08/10/2026',
    k.trangThai     = 'DA_RA_SOAT'
WITH k, row
MATCH (l:Lop {so: row.lop})
MERGE (k)-[:THUOC_LOP]->(l);

// ---- 6 yếu tố (KhaiNiem loai YEU_TO) — Phụ lục D.1 -------------------------
UNWIND [
  {ma: 'CANH_KE', ten: 'Cạnh kề', lop: 3,
   dinhNghia: 'Hai cạnh của tứ giác có chung một đỉnh.'},
  {ma: 'CANH_DOI', ten: 'Cạnh đối', lop: 3,
   dinhNghia: 'Hai cạnh của tứ giác không có đỉnh nào chung.'},
  {ma: 'DUONG_CHEO', ten: 'Đường chéo', lop: 3,
   dinhNghia: 'Đoạn thẳng nối hai đỉnh không kề nhau của tứ giác.'},
  {ma: 'GOC_VUONG', ten: 'Góc vuông', lop: 3,
   dinhNghia: 'Góc có số đo bằng 90 độ.'},
  {ma: 'HAI_DT_SONG_SONG', ten: 'Hai đường thẳng song song', lop: 4,
   dinhNghia: 'Hai đường thẳng cùng nằm trong một mặt phẳng và không có điểm chung.'},
  {ma: 'HAI_DT_VUONG_GOC', ten: 'Hai đường thẳng vuông góc', lop: 4,
   dinhNghia: 'Hai đường thẳng cắt nhau tạo thành một góc vuông.'}
] AS row
MERGE (k:KhaiNiem {ma: row.ma})
SET k.ten       = row.ten,
    k.loai      = 'YEU_TO',
    k.dinhNghia = row.dinhNghia,
    k.nguon     = 'Nhóm GeoQuad tự soạn định nghĩa (SRS Phụ lục B.1 không có cột định nghĩa); '
                  + 'đối chiếu CT GDPT 2018 – Toán ' + toString(row.lop),
    k.trangThai = 'DA_RA_SOAT'
WITH k, row
MATCH (l:Lop {so: row.lop})
MERGE (k)-[:THUOC_LOP]->(l);

// ---- 7 quan hệ LA_TRUONG_HOP_DAC_BIET_CUA (BR-11) — Phụ lục D.2 -----------
// Chiều: hình đặc biệt → hình tổng quát.
UNWIND [
  {tu: 'HINH_THANG',      toi: 'TU_GIAC'},
  {tu: 'HINH_THANG_CAN',  toi: 'HINH_THANG'},
  {tu: 'HINH_BINH_HANH',  toi: 'HINH_THANG'},
  {tu: 'HINH_CHU_NHAT',   toi: 'HINH_BINH_HANH'},
  {tu: 'HINH_THOI',       toi: 'HINH_BINH_HANH'},
  {tu: 'HINH_VUONG',      toi: 'HINH_CHU_NHAT'},
  {tu: 'HINH_VUONG',      toi: 'HINH_THOI'}
] AS row
MATCH (dacBiet:KhaiNiem {ma: row.tu}), (tongQuat:KhaiNiem {ma: row.toi})
MERGE (dacBiet)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(tongQuat);

// ---- Quan hệ CAN_BIET_TRUOC — cột "Cần biết trước" của Phụ lục D.1 --------
UNWIND [
  {tu: 'HINH_THANG',      toi: ['TU_GIAC', 'HAI_DT_SONG_SONG']},
  {tu: 'HINH_THANG_CAN',  toi: ['HINH_THANG', 'DUONG_CHEO']},
  {tu: 'HINH_BINH_HANH',  toi: ['TU_GIAC', 'HAI_DT_SONG_SONG', 'CANH_DOI']},
  {tu: 'HINH_CHU_NHAT',   toi: ['GOC_VUONG', 'CANH_DOI']},
  {tu: 'HINH_THOI',       toi: ['CANH_KE', 'HAI_DT_VUONG_GOC']},
  {tu: 'HINH_VUONG',      toi: ['HINH_CHU_NHAT', 'HINH_THOI']}
] AS row
UNWIND row.toi AS maTruoc
MATCH (sau:KhaiNiem {ma: row.tu}), (truoc:KhaiNiem {ma: maTruoc})
MERGE (sau)-[:CAN_BIET_TRUOC]->(truoc);
