// ============================================================================
// GeoQuad · 12-tinhhuong-A.cypher · PHẦN A · US-04 (NFR-09, BR-08)
// 3 bối cảnh và 9 tình huống thực tế (TH-01 … TH-09).
// Dữ liệu lấy từ README Phụ lục D.8; `loiGiai` do nhóm viết (2–4 câu "vì sao đúng",
// ngôn ngữ dễ hiểu cho học sinh). Đây là dữ liệu cho US-17 và US-18 của phần B.
//
// Khóa là `ma`, mọi câu dùng MERGE nên chạy lặp lại không sinh nút trùng (NFR-08).
// ----------------------------------------------------------------------------
// Quy ước tham chiếu nút của mảng khác (README mục 2, điểm 5):
//   - KhaiNiem (PHẦN 0), CongThuc và TinhChat (phần A) đã có trước theo thứ tự tên
//     file nên dùng MATCH.
//   - Dấu hiệu DH_… thuộc phần B, có thể chưa được seed. Ở đây chỉ
//     `MERGE (d:DinhLy {ma: …})` theo **nhãn gốc + ma**, KHÔNG gắn nhãn phụ DauHieu
//     và KHÔNG gán thuộc tính. Khi 20-dinhly-B.cypher chạy, nó MERGE cùng khóa rồi
//     SET nhãn phụ và thuộc tính. Nút "tạm" chưa có `trangThai` nên các truy vấn
//     hiển thị (lọc trangThai = 'DA_RA_SOAT') tự bỏ qua.
// ----------------------------------------------------------------------------
// Giải thích ký hiệu:
//   UNWIND [...] AS row        trải danh sách thành từng dòng
//   MERGE (th:TinhHuong {ma})  tìm tình huống theo `ma`, chưa có thì tạo
//   (th)-[:TRONG_BOI_CANH]->(bc)  quan hệ có chiều: tình huống → bối cảnh
//   (th)-[:AP_DUNG]->(k)       tình huống áp dụng kiến thức nào
//   (th)-[:LIEN_QUAN_DEN]->(h) tình huống liên quan tới hình nào
//   thucHanh: true             tình huống có phần thực hành đo đạc (US-18)
// ============================================================================

// ---- 3 bối cảnh (BoiCanh) — Phụ lục D.8 -----------------------------------
UNWIND [
  {ma: 'BC_NHA_CUA',   ten: 'Nhà cửa'},
  {ma: 'BC_MANH_VUON', ten: 'Mảnh vườn, thửa đất'},
  {ma: 'BC_DO_DUNG',   ten: 'Đồ dùng, sân chơi'}
] AS row
MERGE (bc:BoiCanh {ma: row.ma})
SET bc.ten = row.ten;

// ---- 9 tình huống thực tế (TinhHuong) — Phụ lục D.8 -----------------------
// Mỗi tình huống có TRONG_BOI_CANH tới đúng một bối cảnh.
UNWIND [
  {ma: 'TH-01', boiCanh: 'BC_NHA_CUA', thucHanh: true, lop: 8,
   ten: 'Kiểm tra khung cửa có vuông góc không',
   moTa: 'Thợ mộc đo hai cặp cạnh đối và hai đường chéo của khung cửa.',
   loiGiai: 'Khi hai cặp cạnh đối của khung cửa bằng nhau thì khung cửa đã là hình bình hành. Hình bình hành chưa chắc có góc vuông, nên phải đo thêm hai đường chéo. Nếu hai đường chéo cũng bằng nhau thì khung cửa là hình chữ nhật, tức là bốn góc đều vuông. Chỉ đo bốn cạnh thôi thì chưa đủ kết luận.'},

  {ma: 'TH-02', boiCanh: 'BC_NHA_CUA', thucHanh: false, lop: 3,
   ten: 'Tính số viên gạch lát nền phòng hình chữ nhật',
   moTa: 'Diện tích nền chia cho diện tích một viên gạch, cộng hao hụt.',
   loiGiai: 'Nền phòng và viên gạch đều là hình chữ nhật nên diện tích bằng chiều dài nhân chiều rộng. Lấy diện tích nền chia cho diện tích một viên gạch là ra số viên cần dùng. Phải đổi hai diện tích về cùng một đơn vị trước khi chia. Cuối cùng cộng thêm vài phần trăm hao hụt vì khi lát sẽ phải cắt gạch ở mép tường.'},

  {ma: 'TH-03', boiCanh: 'BC_NHA_CUA', thucHanh: false, lop: 4,
   ten: 'Lát sân bằng gạch hình thoi',
   moTa: 'Tính diện tích mỗi viên gạch thoi từ hai đường chéo để ước lượng số viên.',
   loiGiai: 'Viên gạch hình thoi có diện tích bằng tích hai đường chéo chia 2, chứ không phải cạnh nhân cạnh. Đo hai đường chéo của một viên là tính được diện tích một viên. Lấy diện tích sân chia cho diện tích một viên thì ra số viên gạch cần mua.'},

  {ma: 'TH-04', boiCanh: 'BC_MANH_VUON', thucHanh: false, lop: 3,
   ten: 'Mua bao nhiêu mét lưới rào vườn hình chữ nhật',
   moTa: 'Chu vi mảnh vườn quyết định độ dài lưới rào.',
   loiGiai: 'Lưới rào chạy quanh mép vườn nên độ dài lưới bằng chu vi, không liên quan tới diện tích. Chu vi hình chữ nhật bằng hai lần tổng chiều dài và chiều rộng. Nếu vườn có một cửa ra vào thì trừ bớt chiều rộng của cửa.'},

  {ma: 'TH-05', boiCanh: 'BC_MANH_VUON', thucHanh: false, lop: 5,
   ten: 'Chia thửa ruộng hình thang thành luống rau',
   moTa: 'Diện tích thửa ruộng hình thang theo hai đáy và chiều cao.',
   loiGiai: 'Thửa ruộng hình thang có diện tích bằng tổng hai đáy nhân chiều cao rồi chia 2. Chiều cao là khoảng cách giữa hai đáy, đo vuông góc với đáy, không phải độ dài cạnh bên. Có diện tích rồi thì chia cho diện tích mỗi luống là biết trồng được bao nhiêu luống.'},

  {ma: 'TH-06', boiCanh: 'BC_MANH_VUON', thucHanh: false, lop: 4,
   ten: 'Diện tích thửa đất hình bình hành (đừng nhân hai cạnh kề)',
   moTa: 'Phân biệt cạnh bên với chiều cao khi tính diện tích.',
   loiGiai: 'Diện tích hình bình hành bằng đáy nhân chiều cao, không phải nhân hai cạnh kề. Chiều cao luôn ngắn hơn hoặc bằng cạnh bên, nên nhân hai cạnh kề sẽ cho kết quả lớn hơn diện tích thật. Muốn có chiều cao thì kéo một đoạn vuông góc từ một đỉnh xuống đáy rồi đo đoạn đó.'},

  {ma: 'TH-07', boiCanh: 'BC_DO_DUNG', thucHanh: false, lop: 4,
   ten: 'Cổng xếp và giá phơi đồ: hình đổi dạng nhưng chu vi không đổi',
   moTa: 'Các thanh bằng nhau tạo hình thoi/hình bình hành; đổi góc làm diện tích thay đổi.',
   loiGiai: 'Các thanh của cổng xếp luôn giữ nguyên độ dài nên các cạnh đối vẫn bằng nhau và hình vẫn là hình bình hành, chu vi vì thế không đổi. Nhưng khi em xếp cổng lại, góc hẹp đi làm chiều cao giảm, nên diện tích giảm theo. Đó là lý do cùng một chu vi vẫn có thể cho nhiều diện tích khác nhau.'},

  {ma: 'TH-08', boiCanh: 'BC_DO_DUNG', thucHanh: false, lop: 8,
   ten: 'Sân cầu lông: chu vi vạch kẻ và đường chéo',
   moTa: 'Tính độ dài vạch kẻ và đường chéo sân hình chữ nhật.',
   loiGiai: 'Vạch kẻ quanh sân là chu vi hình chữ nhật, bằng hai lần tổng chiều dài và chiều rộng. Đường chéo sân tính theo định lý Pythagore: bình phương đường chéo bằng tổng bình phương chiều dài và chiều rộng. Người ta thường đo hai đường chéo để kiểm tra sân đã kẻ vuông góc chưa, vì hai đường chéo của hình chữ nhật phải bằng nhau.'},

  {ma: 'TH-09', boiCanh: 'BC_DO_DUNG', thucHanh: true, lop: 8,
   ten: 'Kiểm tra mặt bàn có vuông không',
   moTa: 'Đo bốn cạnh và hai đường chéo mặt bàn.',
   loiGiai: 'Đo bốn cạnh thấy bằng nhau thì mặt bàn là hình thoi, nhưng hình thoi chưa chắc có góc vuông. Phải đo thêm hai đường chéo: nếu hai đường chéo bằng nhau thì hình thoi đó là hình vuông. Nếu bốn cạnh bằng nhau mà hai đường chéo lệch nhau thì mặt bàn bị méo thành hình thoi nghiêng.'}
] AS row
MERGE (th:TinhHuong {ma: row.ma})
SET th.ten       = row.ten,
    th.moTa      = row.moTa,
    th.loiGiai   = row.loiGiai,
    th.thucHanh  = row.thucHanh,
    th.nguon     = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    th.trangThai = 'DA_RA_SOAT'
WITH th, row
MATCH (bc:BoiCanh {ma: row.boiCanh})
MERGE (th)-[:TRONG_BOI_CANH]->(bc);

// ---- AP_DUNG tới công thức của phần A (đã có, dùng MATCH) -----------------
UNWIND [
  {th: 'TH-02', ma: 'CT_HCN_DT'},
  {th: 'TH-03', ma: 'CT_THOI_DT'},
  {th: 'TH-04', ma: 'CT_HCN_CV'},
  {th: 'TH-05', ma: 'CT_HT_DT'},
  {th: 'TH-06', ma: 'CT_HBH_DT'},
  {th: 'TH-07', ma: 'CT_HBH_CV'},
  {th: 'TH-08', ma: 'CT_HCN_CV'},
  {th: 'TH-08', ma: 'CT_HCN_CHEO'}
] AS row
MATCH (th:TinhHuong {ma: row.th}), (c:CongThuc {ma: row.ma})
MERGE (th)-[:AP_DUNG]->(c);

// ---- AP_DUNG tới tính chất của phần A (đã có, dùng MATCH) -----------------
UNWIND [
  {th: 'TH-07', ma: 'TC_HBH_1'}
] AS row
MATCH (th:TinhHuong {ma: row.th}), (t:DinhLy {ma: row.ma})
MERGE (th)-[:AP_DUNG]->(t);

// ---- AP_DUNG tới dấu hiệu của phần B (quy ước mục 2, điểm 5) -------------
// Chỉ MERGE theo nhãn gốc DinhLy + ma; phần B sẽ SET nhãn phụ DauHieu và thuộc tính.
UNWIND [
  {th: 'TH-01', ma: 'DH_HBH_2'},
  {th: 'TH-01', ma: 'DH_HCN_3'},
  {th: 'TH-09', ma: 'DH_THOI_1'},
  {th: 'TH-09', ma: 'DH_HV_5'}
] AS row
MATCH (th:TinhHuong {ma: row.th})
MERGE (d:DinhLy {ma: row.ma})
MERGE (th)-[:AP_DUNG]->(d);

// ---- LIEN_QUAN_DEN: hình của từng mã kiến thức (BR-08, ghi chú Phụ lục D.8) ----
// Tính chất và công thức → hình sở hữu; dấu hiệu → hình kết luận (KHANG_DINH):
//   DH_HBH_2 → HINH_BINH_HANH, DH_HCN_3 → HINH_CHU_NHAT,
//   DH_THOI_1 → HINH_THOI,     DH_HV_5  → HINH_VUONG.
UNWIND [
  {th: 'TH-01', khaiNiem: ['HINH_BINH_HANH', 'HINH_CHU_NHAT']},
  {th: 'TH-02', khaiNiem: ['HINH_CHU_NHAT']},
  {th: 'TH-03', khaiNiem: ['HINH_THOI']},
  {th: 'TH-04', khaiNiem: ['HINH_CHU_NHAT']},
  {th: 'TH-05', khaiNiem: ['HINH_THANG']},
  {th: 'TH-06', khaiNiem: ['HINH_BINH_HANH']},
  {th: 'TH-07', khaiNiem: ['HINH_BINH_HANH']},
  {th: 'TH-08', khaiNiem: ['HINH_CHU_NHAT']},
  {th: 'TH-09', khaiNiem: ['HINH_THOI', 'HINH_VUONG']}
] AS row
UNWIND row.khaiNiem AS maKhaiNiem
MATCH (th:TinhHuong {ma: row.th}), (k:KhaiNiem {ma: maKhaiNiem})
MERGE (th)-[:LIEN_QUAN_DEN]->(k);
