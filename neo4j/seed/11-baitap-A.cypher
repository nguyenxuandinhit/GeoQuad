// ============================================================================
// GeoQuad · 11-baitap-A.cypher · PHẦN A · US-04 (NFR-09, BR-08)
// 10 bài tập cấp 1 (lớp 1–5): BT-001 … BT-010.
// 5 bài trắc nghiệm (BT-001…BT-005) + 5 bài đáp án số (BT-006…BT-010).
// BT-001 lấy từ README Phụ lục D.10; chín bài còn lại nhóm biên soạn theo
// CT GDPT 2018 và đã kiểm tra lại từng phép tính.
//
// Chủ đề phủ theo US-04: nhận biết hình, chu vi, diện tích hình chữ nhật,
// hình vuông, hình bình hành, hình thoi, hình thang.
//
// Khóa là `ma`, mọi câu dùng MERGE nên chạy lặp lại không sinh nút trùng (NFR-08).
// Nút KhaiNiem (PHẦN 0) và CongThuc (10-kienthuc-A.cypher) đã có trước theo thứ tự
// tên file nên dùng MATCH.
// ----------------------------------------------------------------------------
// Giải thích ký hiệu:
//   UNWIND [...] AS row       trải danh sách thành từng dòng
//   MERGE (b:BaiTap {ma})     tìm bài tập theo `ma`, chưa có thì tạo
//   []                        danh sách, ví dụ phuongAn: ['13 cm', '26 cm', …]
//   UNWIND row.khaiNiem AS m  trải danh sách con để tạo nhiều quan hệ từ một dòng
//   (b)-[:LIEN_QUAN_DEN]->(k) quan hệ có chiều: bài tập → khái niệm liên quan
//
// giaiThich dùng $…$ cho KaTeX nhưng không dùng lệnh LaTeX có dấu gạch chéo ngược:
// học sinh cấp 1 đọc lời diễn giải tiếng Việt dễ hơn, còn công thức dạng LaTeX đã
// nằm ở nút CongThuc (xem quan hệ SU_DUNG).
// ============================================================================

// ---- 5 bài trắc nghiệm (loai TRAC_NGHIEM) ---------------------------------
// phuongAn có đúng 4 chuỗi, dapAnDung là một trong 'A'…'D'.
UNWIND [
  {ma: 'BT-001', lop: 3, doKho: 1,
   de: 'Một hình chữ nhật có chiều dài 8 cm, chiều rộng 5 cm. Chu vi hình chữ nhật là:',
   phuongAn: ['13 cm', '26 cm', '40 cm', '20 cm'],
   dapAnDung: 'B',
   giaiThich: 'Chu vi hình chữ nhật bằng hai lần tổng chiều dài và chiều rộng: $P = 2(8 + 5) = 26$ (cm). Nếu em chọn 13 cm là em quên nhân 2; nếu chọn 40 cm là em tính diện tích.'},

  {ma: 'BT-002', lop: 1, doKho: 1,
   de: 'Hình nào vừa có bốn góc vuông, vừa có bốn cạnh bằng nhau?',
   phuongAn: ['Hình chữ nhật', 'Hình thoi', 'Hình thang', 'Hình vuông'],
   dapAnDung: 'D',
   giaiThich: 'Hình vuông có bốn góc vuông và bốn cạnh bằng nhau. Hình chữ nhật có bốn góc vuông nhưng bốn cạnh không nhất thiết bằng nhau; hình thoi có bốn cạnh bằng nhau nhưng góc không nhất thiết vuông.'},

  {ma: 'BT-003', lop: 4, doKho: 1,
   de: 'Hình thoi là tứ giác có đặc điểm nào sau đây?',
   phuongAn: ['Bốn cạnh bằng nhau', 'Bốn góc vuông', 'Chỉ có một cặp cạnh đối song song', 'Hai góc kề một đáy bằng nhau'],
   dapAnDung: 'A',
   giaiThich: 'Theo định nghĩa, hình thoi là tứ giác có bốn cạnh bằng nhau. Bốn góc vuông là đặc điểm của hình chữ nhật; chỉ một cặp cạnh đối song song là hình thang.'},

  {ma: 'BT-004', lop: 3, doKho: 1,
   de: 'Một hình vuông có cạnh 6 cm. Diện tích hình vuông đó là:',
   phuongAn: ['12 cm²', '24 cm²', '30 cm²', '36 cm²'],
   dapAnDung: 'D',
   giaiThich: 'Diện tích hình vuông bằng cạnh nhân cạnh: $S = 6 × 6 = 36$ (cm²). Số 24 cm² là chu vi của hình vuông này, không phải diện tích.'},

  {ma: 'BT-005', lop: 5, doKho: 2,
   de: 'Một hình thang có hai đáy dài 6 cm và 10 cm, chiều cao 4 cm. Diện tích hình thang đó là:',
   phuongAn: ['16 cm²', '32 cm²', '40 cm²', '64 cm²'],
   dapAnDung: 'B',
   giaiThich: 'Diện tích hình thang bằng tổng hai đáy nhân chiều cao rồi chia 2: $(6 + 10) × 4 : 2 = 64 : 2 = 32$ (cm²). Nếu em chọn 64 cm² là em quên chia 2.'}
] AS row
MERGE (b:BaiTap {ma: row.ma})
SET b.loai      = 'TRAC_NGHIEM',
    b.de        = row.de,
    b.phuongAn  = row.phuongAn,
    b.dapAnDung = row.dapAnDung,
    b.giaiThich = row.giaiThich,
    b.doKho     = row.doKho,
    b.hienThi   = true,
    b.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    b.trangThai = 'DA_RA_SOAT'
WITH b, row
MATCH (l:Lop {so: row.lop})
MERGE (b)-[:THUOC_LOP]->(l);

// ---- 5 bài đáp án số (loai DAP_AN_SO) -------------------------------------
// dapAnSo làm tròn 2 chữ số; saiSo mặc định 0.01; donVi luôn có để chấm đúng.
UNWIND [
  {ma: 'BT-006', lop: 3, doKho: 1, dapAnSo: 36.0, donVi: 'cm²',
   de: 'Một hình chữ nhật có chiều dài 9 cm, chiều rộng 4 cm. Tính diện tích hình chữ nhật đó (đơn vị cm²).',
   giaiThich: 'Diện tích hình chữ nhật bằng chiều dài nhân chiều rộng: $S = 9 × 4 = 36$ (cm²).'},

  {ma: 'BT-007', lop: 3, doKho: 1, dapAnSo: 28.0, donVi: 'cm',
   de: 'Một hình vuông có cạnh 7 cm. Tính chu vi hình vuông đó (đơn vị cm).',
   giaiThich: 'Chu vi hình vuông bằng 4 lần độ dài cạnh: $P = 4 × 7 = 28$ (cm).'},

  {ma: 'BT-008', lop: 4, doKho: 2, dapAnSo: 60.0, donVi: 'cm²',
   de: 'Một hình bình hành có cạnh đáy 12 cm và chiều cao tương ứng 5 cm. Tính diện tích hình bình hành đó (đơn vị cm²).',
   giaiThich: 'Diện tích hình bình hành bằng đáy nhân chiều cao: $S = 12 × 5 = 60$ (cm²). Em nhớ dùng chiều cao, không dùng cạnh bên.'},

  {ma: 'BT-009', lop: 4, doKho: 2, dapAnSo: 20.0, donVi: 'cm²',
   de: 'Một hình thoi có hai đường chéo dài 8 cm và 5 cm. Tính diện tích hình thoi đó (đơn vị cm²).',
   giaiThich: 'Diện tích hình thoi bằng tích hai đường chéo rồi chia 2: $8 × 5 : 2 = 40 : 2 = 20$ (cm²).'},

  {ma: 'BT-010', lop: 4, doKho: 1, dapAnSo: 24.0, donVi: 'cm',
   de: 'Một hình thoi có cạnh 6 cm. Tính chu vi hình thoi đó (đơn vị cm).',
   giaiThich: 'Hình thoi có bốn cạnh bằng nhau nên chu vi bằng 4 lần cạnh: $P = 4 × 6 = 24$ (cm).'}
] AS row
MERGE (b:BaiTap {ma: row.ma})
SET b.loai      = 'DAP_AN_SO',
    b.de        = row.de,
    b.dapAnSo   = row.dapAnSo,
    b.saiSo     = 0.01,
    b.donVi     = row.donVi,
    b.giaiThich = row.giaiThich,
    b.doKho     = row.doKho,
    b.hienThi   = true,
    b.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    b.trangThai = 'DA_RA_SOAT'
WITH b, row
MATCH (l:Lop {so: row.lop})
MERGE (b)-[:THUOC_LOP]->(l);

// ---- LIEN_QUAN_DEN: mỗi bài tập có ít nhất một khái niệm (BR-08) ----------
UNWIND [
  {ma: 'BT-001', khaiNiem: ['HINH_CHU_NHAT']},
  {ma: 'BT-002', khaiNiem: ['HINH_VUONG', 'HINH_CHU_NHAT', 'HINH_THOI']},
  {ma: 'BT-003', khaiNiem: ['HINH_THOI']},
  {ma: 'BT-004', khaiNiem: ['HINH_VUONG']},
  {ma: 'BT-005', khaiNiem: ['HINH_THANG']},
  {ma: 'BT-006', khaiNiem: ['HINH_CHU_NHAT']},
  {ma: 'BT-007', khaiNiem: ['HINH_VUONG']},
  {ma: 'BT-008', khaiNiem: ['HINH_BINH_HANH']},
  {ma: 'BT-009', khaiNiem: ['HINH_THOI', 'DUONG_CHEO']},
  {ma: 'BT-010', khaiNiem: ['HINH_THOI']}
] AS row
UNWIND row.khaiNiem AS maKhaiNiem
MATCH (b:BaiTap {ma: row.ma}), (k:KhaiNiem {ma: maKhaiNiem})
MERGE (b)-[:LIEN_QUAN_DEN]->(k);

// ---- SU_DUNG: công thức mà bài tập dùng tới --------------------------------
// BT-002 và BT-003 là bài nhận biết hình theo định nghĩa nên không dùng công thức nào.
UNWIND [
  {ma: 'BT-001', congThuc: ['CT_HCN_CV']},
  {ma: 'BT-004', congThuc: ['CT_HV_DT']},
  {ma: 'BT-005', congThuc: ['CT_HT_DT']},
  {ma: 'BT-006', congThuc: ['CT_HCN_DT']},
  {ma: 'BT-007', congThuc: ['CT_HV_CV']},
  {ma: 'BT-008', congThuc: ['CT_HBH_DT']},
  {ma: 'BT-009', congThuc: ['CT_THOI_DT']},
  {ma: 'BT-010', congThuc: ['CT_THOI_CV']}
] AS row
UNWIND row.congThuc AS maCongThuc
MATCH (b:BaiTap {ma: row.ma}), (c:CongThuc {ma: maCongThuc})
MERGE (b)-[:SU_DUNG]->(c);
