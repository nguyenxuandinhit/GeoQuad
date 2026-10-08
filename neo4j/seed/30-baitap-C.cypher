// GeoQuad US-04 — Thành viên C: BT-011 đến BT-030.
// Chạy sau 01-khung.cypher; có thể chạy trước seed A/B vì tham chiếu DinhLy/CongThuc dùng MERGE.
// Chạy lại an toàn: mọi nút/quan hệ đều MERGE theo khóa ổn định.

UNWIND [
  {ma:'BT-011', lop:8, cap:'CAP_2', loai:'TRAC_NGHIEM', doKho:1,
   de:'Tổng số đo bốn góc trong của một tứ giác bằng bao nhiêu?',
   phuongAn:['180°','270°','360°','540°'], dapAnDung:'C',
   giaiThich:'Tổng các góc trong của mọi tứ giác bằng 360°.',
   khaiNiem:['TU_GIAC'], dinhLy:['TC_TG_1']},
  {ma:'BT-012', lop:8, cap:'CAP_2', loai:'TRAC_NGHIEM', doKho:2,
   de:'Trong hình thoi, hai đường chéo có quan hệ nào sau đây?',
   phuongAn:['Bằng nhau trong mọi trường hợp','Vuông góc với nhau','Song song với nhau','Không cắt nhau'], dapAnDung:'B',
   giaiThich:'Hai đường chéo của hình thoi vuông góc với nhau.',
   khaiNiem:['HINH_THOI'], dinhLy:['TC_THOI_1']},
  {ma:'BT-013', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:2,
   de:'Hình thoi ABCD có cạnh dài 7 cm. Tính chu vi (cm).',
   dapAnSo:28, saiSo:0.01, donVi:'cm',
   giaiThich:'Chu vi hình thoi là 4 lần độ dài một cạnh: 4 × 7 = 28 cm.',
   khaiNiem:['HINH_THOI'], congThuc:['CT_THOI_CV']},
  {ma:'BT-014', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:2,
   de:'Hình thoi ABCD có hai đường chéo AC = 12 cm và BD = 16 cm. Tính diện tích hình thoi (đơn vị cm²).',
   dapAnSo:96, saiSo:0.01, donVi:'cm²',
   giaiThich:'Diện tích hình thoi bằng nửa tích hai đường chéo: 12 × 16 ÷ 2 = 96 cm².',
   khaiNiem:['HINH_THOI'], congThuc:['CT_THOI_DT']},
  {ma:'BT-015', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:2,
   de:'Hình vuông có cạnh 5 cm. Tính độ dài đường chéo, làm tròn đến hai chữ số thập phân (cm).',
   dapAnSo:7.07, saiSo:0.01, donVi:'cm',
   giaiThich:'Đường chéo hình vuông cạnh a là a√2; 5√2 ≈ 7,07 cm.',
   khaiNiem:['HINH_VUONG'], congThuc:['CT_HV_CHEO']},
  {ma:'BT-016', lop:8, cap:'CAP_2', loai:'TRAC_NGHIEM', doKho:1,
   de:'Hai đường chéo của hình chữ nhật có tính chất nào?',
   phuongAn:['Vuông góc trong mọi trường hợp','Bằng nhau','Song song','Một đường luôn dài gấp đôi đường kia'], dapAnDung:'B',
   giaiThich:'Hai đường chéo của hình chữ nhật bằng nhau.',
   khaiNiem:['HINH_CHU_NHAT'], dinhLy:['TC_HCN_2']},
  {ma:'BT-017', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:2,
   de:'Hình bình hành có đáy 12 cm và chiều cao tương ứng 7 cm. Tính diện tích (cm²).',
   dapAnSo:84, saiSo:0.01, donVi:'cm²',
   giaiThich:'Diện tích hình bình hành bằng đáy nhân chiều cao: 12 × 7 = 84 cm².',
   khaiNiem:['HINH_BINH_HANH'], congThuc:['CT_HBH_DT']},
  {ma:'BT-018', lop:8, cap:'CAP_2', loai:'TRAC_NGHIEM', doKho:1,
   de:'Hình chữ nhật có chiều dài 8 cm và chiều rộng 6 cm. Đường chéo dài bao nhiêu?',
   phuongAn:['9 cm','10 cm','12 cm','14 cm'], dapAnDung:'B',
   giaiThich:'Đường chéo là cạnh huyền của tam giác vuông có hai cạnh góc vuông 8 cm và 6 cm. Theo định lý Pythagore, d = √(8² + 6²) = 10 cm.',
   khaiNiem:['HINH_CHU_NHAT'], dinhLy:['DL_NEN_3'], congThuc:['CT_HCN_CHEO']},
  {ma:'BT-019', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:2,
   de:'Hình thang có hai đáy dài 8 cm và 14 cm, chiều cao 5 cm. Tính diện tích (cm²).',
   dapAnSo:55, saiSo:0.01, donVi:'cm²',
   giaiThich:'Diện tích là (8 + 14) × 5 ÷ 2 = 55 cm².',
   khaiNiem:['HINH_THANG'], congThuc:['CT_HT_DT']},
  {ma:'BT-020', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:1,
   de:'Hình thoi có cạnh dài 6 cm. Tính chu vi (cm).',
   dapAnSo:24, saiSo:0.01, donVi:'cm',
   giaiThich:'Bốn cạnh hình thoi bằng nhau nên chu vi là 4 × 6 = 24 cm.',
   khaiNiem:['HINH_THOI'], congThuc:['CT_THOI_CV']},
  {ma:'BT-021', lop:8, cap:'CAP_2', loai:'CHUNG_MINH', doKho:2,
   de:'Cho hình bình hành ABCD có hai đường chéo AC và BD cắt nhau tại O. Chứng minh AO = OC và BO = OD.',
   loiGiaiMau:'1. AB = CD vì hai cạnh đối của hình bình hành bằng nhau. 2. ∠ABO = ∠CDO vì AB ∥ CD và BD là cát tuyến; ∠BAO = ∠DCO vì AB ∥ CD và AC là cát tuyến. 3. Suy ra tam giác ABO bằng tam giác CDO theo trường hợp góc-cạnh-góc. 4. Do đó AO = OC và BO = OD. Căn cứ: TC_HBH_1, DL_NEN_2, DL_NEN_5.',
   khaiNiem:['HINH_BINH_HANH'], dinhLy:['TC_HBH_1','TC_HBH_3','DL_NEN_2','DL_NEN_5']},
  {ma:'BT-022', lop:8, cap:'CAP_2', loai:'CHUNG_MINH', doKho:3,
   de:'Cho hình thoi ABCD. Chứng minh hai đường chéo AC và BD vuông góc với nhau.',
   loiGiaiMau:'1. Gọi O là giao điểm hai đường chéo. Vì ABCD là hình thoi nên AB = AD. 2. Hình thoi là hình bình hành, nên hai đường chéo cắt nhau tại trung điểm mỗi đường: BO = DO. 3. Xét tam giác ABO và ADO: AB = AD, BO = DO, AO là cạnh chung. Hai tam giác bằng nhau theo c.c.c; do đó ∠AOB = ∠AOD. 4. Hai góc này kề bù nên mỗi góc bằng 90°, vậy AC ⟂ BD. Căn cứ: TC_HBH_3, DL_NEN_2.',
   khaiNiem:['HINH_THOI'], dinhLy:['TC_HBH_3','DL_NEN_2']},
  {ma:'BT-023', lop:8, cap:'CAP_2', loai:'DAP_AN_SO', doKho:3,
   de:'Hình thoi có cạnh 13 cm và một đường chéo dài 10 cm. Tính độ dài đường chéo còn lại (cm).',
   dapAnSo:24, saiSo:0.01, donVi:'cm',
   giaiThich:'Hai đường chéo hình thoi vuông góc và cắt nhau tại trung điểm. Nửa đường chéo đã biết là 5 cm; nửa đường chéo kia bằng √(13² − 5²) = 12 cm, nên đường chéo còn lại dài 24 cm.',
   khaiNiem:['HINH_THOI'], dinhLy:['TC_THOI_1','TC_HBH_3','DL_NEN_3']},
  {ma:'BT-024', lop:10, cap:'CAP_3', loai:'TRAC_NGHIEM', doKho:2,
   de:'Hình bình hành ABCD có A(1, 1), B(4, 1), D(2, 3). Tọa độ đỉnh C là gì?',
   phuongAn:['(4, 3)','(5, 3)','(5, 2)','(3, 5)'], dapAnDung:'B',
   giaiThich:'Vì vectơ AB = vectơ DC, suy ra C = B + D − A = (5, 3).',
   khaiNiem:['HINH_BINH_HANH'], dinhLy:['DL_NEN_6']},
  {ma:'BT-025', lop:9, cap:'CAP_2', loai:'DAP_AN_SO', doKho:2,
   de:'Một hình thang có một góc kề cạnh bên bằng 112°. Tính số đo góc còn lại kề cùng cạnh bên đó (độ).',
   dapAnSo:68, saiSo:0.01, donVi:'°',
   giaiThich:'Hai góc kề một cạnh bên của hình thang có tổng 180°, nên góc còn lại là 180° − 112° = 68°.',
   khaiNiem:['HINH_THANG'], dinhLy:['TC_HT_1']},
  {ma:'BT-026', lop:10, cap:'CAP_3', loai:'TRAC_NGHIEM', doKho:2,
   de:'Trong hình bình hành ABCD, đẳng thức vectơ nào luôn đúng?',
   phuongAn:['Vectơ AB = vectơ CD','Vectơ AB = vectơ DC','Vectơ AC = vectơ BD','Vectơ AD = vectơ CB'], dapAnDung:'B',
   giaiThich:'Trong hình bình hành, hai vectơ AB và DC cùng hướng, cùng độ dài nên bằng nhau.',
   khaiNiem:['HINH_BINH_HANH'], dinhLy:['DL_NEN_6']},
  {ma:'BT-027', lop:8, cap:'CAP_2', loai:'CHUNG_MINH', doKho:2,
   de:'Cho hình bình hành ABCD có góc A bằng 90°. Chứng minh ABCD là hình chữ nhật.',
   loiGiaiMau:'1. Vì ABCD là hình bình hành nên AB ∥ CD và AD ∥ BC. 2. Góc A bằng 90°, tức AB ⟂ AD. 3. Do các cặp cạnh đối song song, các góc kề bù và các góc đối bằng nhau; suy ra cả bốn góc đều vuông. 4. Tứ giác ABCD có bốn góc vuông nên là hình chữ nhật. Căn cứ: DH_HCN_2.',
   khaiNiem:['HINH_BINH_HANH','HINH_CHU_NHAT'], dinhLy:['DH_HCN_2']},
  {ma:'BT-028', lop:10, cap:'CAP_3', loai:'DAP_AN_SO', doKho:2,
   de:'Trong hình bình hành ABCD, A(1, 2), B(5, 2), D(2, 6). Tìm hoành độ của C.',
   dapAnSo:6, saiSo:0.01, donVi:'',
   giaiThich:'ABCD là hình bình hành nên vectơ AB = vectơ DC. Từ tọa độ suy ra C = B + D − A = (6, 6), vậy hoành độ của C là 6.',
   khaiNiem:['HINH_BINH_HANH'], dinhLy:['DL_NEN_6']},
  {ma:'BT-029', lop:11, cap:'CAP_3', loai:'TRAC_NGHIEM', doKho:2,
   de:'Trong hình bình hành ABCD, biểu diễn vectơ AC theo hai vectơ cạnh xuất phát từ A.',
   phuongAn:['Vectơ AB − vectơ AD','Vectơ AB + vectơ AD','Vectơ AD − vectơ AB','Vectơ AB + vectơ DC'], dapAnDung:'B',
   giaiThich:'Quy tắc hình bình hành cho vectơ AC = vectơ AB + vectơ AD.',
   khaiNiem:['HINH_BINH_HANH'], dinhLy:['DL_NEN_6']},
  {ma:'BT-030', lop:12, cap:'CAP_3', loai:'DAP_AN_SO', doKho:3,
   de:'Hình bình hành có hai vectơ cạnh kề u = (3, 0) và v = (1, 4). Tính diện tích hình bình hành (đơn vị vuông).',
   dapAnSo:12, saiSo:0.01, donVi:'đơn vị vuông',
   giaiThich:'Diện tích bằng trị tuyệt đối định thức của hai vectơ cạnh: |3×4 − 0×1| = 12 đơn vị vuông.',
   khaiNiem:['HINH_BINH_HANH'], dinhLy:['DL_NEN_6']}
] AS row
MATCH (l:Lop {so: row.lop})-[:THUOC_CAP]->(:CapHoc {ma: row.cap})
MATCH (k:KhaiNiem)
WHERE k.ma IN row.khaiNiem
WITH row, l, collect(k) AS khaiNiem
WHERE size(khaiNiem) = size(row.khaiNiem)
MERGE (b:BaiTap {ma: row.ma})
SET b.loai = row.loai,
    b.de = row.de,
    b.phuongAn = row.phuongAn,
    b.dapAnDung = row.dapAnDung,
    b.dapAnSo = row.dapAnSo,
    b.saiSo = row.saiSo,
    b.donVi = row.donVi,
    b.giaiThich = row.giaiThich,
    b.loiGiaiMau = row.loiGiaiMau,
    b.doKho = row.doKho,
    b.hienThi = true,
    b.nguon = CASE WHEN row.ma IN ['BT-014','BT-027']
                   THEN 'GeoQuad SRS Phụ lục B.10; README Phụ lục D.10'
                   ELSE 'Nhóm C tự biên soạn theo README mục 6 và Phụ lục D'
              END,
    b.trangThai = 'DA_RA_SOAT'
MERGE (b)-[:THUOC_LOP]->(l)
FOREACH (k IN khaiNiem |
  MERGE (b)-[:LIEN_QUAN_DEN]->(k)
)
FOREACH (maDinhLy IN coalesce(row.dinhLy, []) |
  MERGE (d:DinhLy {ma: maDinhLy})
  MERGE (b)-[:SU_DUNG]->(d)
)
FOREACH (maCongThuc IN coalesce(row.congThuc, []) |
  MERGE (c:CongThuc {ma: maCongThuc})
  MERGE (b)-[:SU_DUNG]->(c)
);
