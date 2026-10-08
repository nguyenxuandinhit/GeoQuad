// ============================================================================
// GeoQuad · 10-kienthuc-A.cypher · PHẦN A · US-03 (NFR-09)
// 12 tính chất (DinhLy:TinhChat) và 12 công thức (CongThuc) của bảy hình tứ giác.
// Dữ liệu lấy từ README Phụ lục D.5 và D.6.
//
// Khóa là `ma`, mọi câu dùng MERGE nên chạy lặp lại không sinh nút trùng (NFR-08).
// Nút KhaiNiem và Lop do PHẦN 0 tạo (01-khung.cypher chạy trước theo thứ tự tên)
// nên ở đây dùng MATCH, không MERGE.
// ----------------------------------------------------------------------------
// Giải thích ký hiệu:
//   UNWIND [...] AS row     trải danh sách thành từng dòng để xử lý lần lượt
//   MERGE (t:DinhLy {ma})   tìm nút DinhLy theo `ma`, chưa có thì tạo
//   SET t:TinhChat          gắn thêm nhãn phụ TinhChat cho nút đã có nhãn DinhLy
//   WITH t, row             chuyển `t` và `row` sang bước tiếp theo
//   (h)-[:CO_TINH_CHAT]->(t)  quan hệ có chiều: hình → tính chất của hình đó
//   {} dấu ngoặc nhọn       bộ thuộc tính / bản đồ khóa-giá trị
//   []                      danh sách, ví dụ bienSo: ['a','b']
//   \\                      trong chuỗi Cypher cho ra một dấu gạch chéo ngược của
//                           LaTeX. Bắt buộc viết \\ vì Cypher coi \f, \t, \n, \b, \r là ký tự
//                           điều khiển: viết \frac thì "\f" bị hiểu thành form feed và mất.
// ============================================================================

// ---- 12 tính chất (DinhLy + nhãn phụ TinhChat) — Phụ lục D.5 ---------------
// Mỗi tính chất: CO_TINH_CHAT từ hình tương ứng, THUOC_LOP tới lớp xuất hiện lần đầu.
UNWIND [
  {ma: 'TC_TG_1',    hinh: 'TU_GIAC',         lop: 8,
   noiDung: 'Tổng bốn góc của một tứ giác bằng 360°.'},
  {ma: 'TC_HT_1',    hinh: 'HINH_THANG',      lop: 8,
   noiDung: 'Hai góc kề một cạnh bên của hình thang có tổng bằng 180°.'},
  {ma: 'TC_HTC_1',   hinh: 'HINH_THANG_CAN',  lop: 8,
   noiDung: 'Hai cạnh bên của hình thang cân bằng nhau.'},
  {ma: 'TC_HTC_2',   hinh: 'HINH_THANG_CAN',  lop: 8,
   noiDung: 'Hai đường chéo của hình thang cân bằng nhau.'},
  {ma: 'TC_HBH_1',   hinh: 'HINH_BINH_HANH',  lop: 8,
   noiDung: 'Các cạnh đối của hình bình hành bằng nhau.'},
  {ma: 'TC_HBH_2',   hinh: 'HINH_BINH_HANH',  lop: 8,
   noiDung: 'Các góc đối của hình bình hành bằng nhau.'},
  {ma: 'TC_HBH_3',   hinh: 'HINH_BINH_HANH',  lop: 8,
   noiDung: 'Hai đường chéo của hình bình hành cắt nhau tại trung điểm của mỗi đường.'},
  {ma: 'TC_HCN_1',   hinh: 'HINH_CHU_NHAT',   lop: 8,
   noiDung: 'Hình chữ nhật có bốn góc vuông.'},
  {ma: 'TC_HCN_2',   hinh: 'HINH_CHU_NHAT',   lop: 8,
   noiDung: 'Hai đường chéo của hình chữ nhật bằng nhau.'},
  {ma: 'TC_THOI_1',  hinh: 'HINH_THOI',       lop: 8,
   noiDung: 'Hai đường chéo của hình thoi vuông góc với nhau.'},
  {ma: 'TC_THOI_2',  hinh: 'HINH_THOI',       lop: 8,
   noiDung: 'Hai đường chéo của hình thoi là các đường phân giác của các góc của hình thoi.'},
  {ma: 'TC_HV_1',    hinh: 'HINH_VUONG',      lop: 8,
   noiDung: 'Hình vuông có tất cả tính chất của hình chữ nhật và của hình thoi.'}
] AS row
MERGE (t:DinhLy {ma: row.ma})
SET t:TinhChat,
    t.noiDung   = row.noiDung,
    t.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    t.trangThai = 'DA_RA_SOAT'
WITH t, row
MATCH (h:KhaiNiem {ma: row.hinh})
MERGE (h)-[:CO_TINH_CHAT]->(t)
WITH t, row
MATCH (l:Lop {so: row.lop})
MERGE (t)-[:THUOC_LOP]->(l);

// ---- 12 công thức (CongThuc) — Phụ lục D.6 --------------------------------
// bieuThuc viết bằng LaTeX để KaTeX dựng hình (NFR-12).
// bienSo liệt kê đúng các biến xuất hiện trong bieuThuc — máy tính hình học (US-15)
// chỉ hỏi những số đo thực sự cần.
// daiLuong là CHU_VI / DIEN_TICH / DUONG_CHEO, dùng để lọc ở máy tính hình học.
UNWIND [
  {ma: 'CT_TG_CHUVI', hinh: 'TU_GIAC',        lop: 3, daiLuong: 'CHU_VI',
   ten: 'Chu vi tứ giác',          bieuThuc: 'P = a + b + c + d',
   bienSo: ['a', 'b', 'c', 'd']},
  {ma: 'CT_HCN_CV',   hinh: 'HINH_CHU_NHAT',  lop: 3, daiLuong: 'CHU_VI',
   ten: 'Chu vi hình chữ nhật',    bieuThuc: 'P = 2(a + b)',
   bienSo: ['a', 'b']},
  {ma: 'CT_HCN_DT',   hinh: 'HINH_CHU_NHAT',  lop: 3, daiLuong: 'DIEN_TICH',
   ten: 'Diện tích hình chữ nhật', bieuThuc: 'S = a \\cdot b',
   bienSo: ['a', 'b']},
  {ma: 'CT_HV_CV',    hinh: 'HINH_VUONG',     lop: 3, daiLuong: 'CHU_VI',
   ten: 'Chu vi hình vuông',       bieuThuc: 'P = 4a',
   bienSo: ['a']},
  {ma: 'CT_HV_DT',    hinh: 'HINH_VUONG',     lop: 3, daiLuong: 'DIEN_TICH',
   ten: 'Diện tích hình vuông',    bieuThuc: 'S = a^2',
   bienSo: ['a']},
  {ma: 'CT_HBH_CV',   hinh: 'HINH_BINH_HANH', lop: 4, daiLuong: 'CHU_VI',
   ten: 'Chu vi hình bình hành',   bieuThuc: 'P = 2(a + b)',
   bienSo: ['a', 'b']},
  {ma: 'CT_HBH_DT',   hinh: 'HINH_BINH_HANH', lop: 4, daiLuong: 'DIEN_TICH',
   ten: 'Diện tích hình bình hành', bieuThuc: 'S = a \\cdot h',
   bienSo: ['a', 'h']},
  {ma: 'CT_THOI_CV',  hinh: 'HINH_THOI',      lop: 4, daiLuong: 'CHU_VI',
   ten: 'Chu vi hình thoi',        bieuThuc: 'P = 4a',
   bienSo: ['a']},
  {ma: 'CT_THOI_DT',  hinh: 'HINH_THOI',      lop: 4, daiLuong: 'DIEN_TICH',
   ten: 'Diện tích hình thoi',     bieuThuc: 'S = \\frac{d_1 \\cdot d_2}{2}',
   bienSo: ['d1', 'd2']},
  {ma: 'CT_HT_DT',    hinh: 'HINH_THANG',     lop: 5, daiLuong: 'DIEN_TICH',
   ten: 'Diện tích hình thang',    bieuThuc: 'S = \\frac{(a + b) \\cdot h}{2}',
   bienSo: ['a', 'b', 'h']},
  {ma: 'CT_HCN_CHEO', hinh: 'HINH_CHU_NHAT',  lop: 8, daiLuong: 'DUONG_CHEO',
   ten: 'Đường chéo hình chữ nhật', bieuThuc: 'd = \\sqrt{a^2 + b^2}',
   bienSo: ['a', 'b']},
  {ma: 'CT_HV_CHEO',  hinh: 'HINH_VUONG',     lop: 8, daiLuong: 'DUONG_CHEO',
   ten: 'Đường chéo hình vuông',   bieuThuc: 'd = a\\sqrt{2}',
   bienSo: ['a']}
] AS row
MERGE (c:CongThuc {ma: row.ma})
SET c.ten       = row.ten,
    c.bieuThuc  = row.bieuThuc,
    c.bienSo    = row.bienSo,
    c.daiLuong  = row.daiLuong,
    c.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    c.trangThai = 'DA_RA_SOAT'
WITH c, row
MATCH (h:KhaiNiem {ma: row.hinh})
MERGE (h)-[:CO_CONG_THUC]->(c)
WITH c, row
MATCH (l:Lop {so: row.lop})
MERGE (c)-[:THUOC_LOP]->(l);

// ---- Tính chất bổ sung (ngoài Phụ lục D/B) --------------------------------
// Lý do bổ sung: Phụ lục B không có mục nào về ĐỐI XỨNG, trong khi trục đối xứng và
// tâm đối xứng là một phần của chương tứ giác ở SGK Toán 8; thiếu cả tổng góc ngoài
// và đường trung bình hình thang. AC của US-03 ghi "đủ 12 tính chất" — hiểu là mức
// tối thiểu, nên bổ sung chỉ làm dày thêm, không bỏ mục nào của Phụ lục B.
UNWIND [
  {ma: 'TC_TG_2',    hinh: 'TU_GIAC',         lop: 8,
   noiDung: 'Tổng bốn góc ngoài của một tứ giác, mỗi đỉnh lấy một góc ngoài, bằng 360°.'},
  {ma: 'TC_HT_2',    hinh: 'HINH_THANG',      lop: 8,
   noiDung: 'Đường trung bình của hình thang song song với hai đáy và bằng nửa tổng hai đáy.'},
  {ma: 'TC_HTC_3',   hinh: 'HINH_THANG_CAN',  lop: 8,
   noiDung: 'Hình thang cân có một trục đối xứng là đường thẳng đi qua trung điểm của hai đáy.'},
  {ma: 'TC_HBH_4',   hinh: 'HINH_BINH_HANH',  lop: 8,
   noiDung: 'Giao điểm hai đường chéo của hình bình hành là tâm đối xứng của hình bình hành đó.'},
  {ma: 'TC_HCN_3',   hinh: 'HINH_CHU_NHAT',   lop: 8,
   noiDung: 'Hình chữ nhật có hai trục đối xứng là hai đường thẳng đi qua trung điểm của hai cặp cạnh đối, và có tâm đối xứng là giao điểm hai đường chéo.'},
  {ma: 'TC_THOI_3',  hinh: 'HINH_THOI',       lop: 8,
   noiDung: 'Hình thoi có hai trục đối xứng là hai đường chéo và có tâm đối xứng là giao điểm hai đường chéo.'},
  {ma: 'TC_HV_2',    hinh: 'HINH_VUONG',      lop: 8,
   noiDung: 'Hình vuông có bốn trục đối xứng: hai đường chéo và hai đường thẳng đi qua trung điểm của hai cặp cạnh đối; tâm đối xứng là giao điểm hai đường chéo.'},
  {ma: 'TC_HV_3',    hinh: 'HINH_VUONG',      lop: 8,
   noiDung: 'Hai đường chéo của hình vuông bằng nhau, vuông góc với nhau và là các đường phân giác của các góc của hình vuông.'}
] AS row
MERGE (t:DinhLy {ma: row.ma})
SET t:TinhChat,
    t.noiDung   = row.noiDung,
    t.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop) + ' (bổ sung ngoài Phụ lục B)',
    t.trangThai = 'DA_RA_SOAT'
WITH t, row
MATCH (h:KhaiNiem {ma: row.hinh})
MERGE (h)-[:CO_TINH_CHAT]->(t)
WITH t, row
MATCH (l:Lop {so: row.lop})
MERGE (t)-[:THUOC_LOP]->(l);

// ---- Công thức bổ sung (ngoài Phụ lục D/B) --------------------------------
// Lý do: hình thang thiếu chu vi (phải mượn công thức tứ giác) và thiếu đường trung
// bình; hình thoi không có đại lượng đường chéo nào, dù bài BT-023 của C hỏi đúng
// phép tính đó.
//
// KHÔNG thêm công thức đường trung bình hình thang vào đây: máy tính hình học mượn
// công thức của hình tổng quát gần nhất, nên hình thoi và hình chữ nhật sẽ mượn luôn
// công thức đó và hỏi học sinh "hai đáy" — với các hình ấy hai cạnh song song bằng
// nhau nên nhập chiều dài và chiều rộng là ra kết quả sai. Kiến thức này giữ ở dạng
// tính chất TC_HT_2 để vẫn có trong thư viện.
UNWIND [
  {ma: 'CT_HT_CV',     hinh: 'HINH_THANG', lop: 5, daiLuong: 'CHU_VI',
   ten: 'Chu vi hình thang',          bieuThuc: 'P = a + b + c + d',
   bienSo: ['a', 'b', 'c', 'd']},
  {ma: 'CT_THOI_CHEO', hinh: 'HINH_THOI',  lop: 8, daiLuong: 'DUONG_CHEO',
   ten: 'Đường chéo còn lại của hình thoi',
   bieuThuc: 'd_2 = 2\\sqrt{a^2 - \\frac{d_1^2}{4}}',
   bienSo: ['a', 'd1']}
] AS row
MERGE (c:CongThuc {ma: row.ma})
SET c.ten       = row.ten,
    c.bieuThuc  = row.bieuThuc,
    c.bienSo    = row.bienSo,
    c.daiLuong  = row.daiLuong,
    c.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop) + ' (bổ sung ngoài Phụ lục B)',
    c.trangThai = 'DA_RA_SOAT'
WITH c, row
MATCH (h:KhaiNiem {ma: row.hinh})
MERGE (h)-[:CO_CONG_THUC]->(c)
WITH c, row
MATCH (l:Lop {so: row.lop})
MERGE (c)-[:THUOC_LOP]->(l);
