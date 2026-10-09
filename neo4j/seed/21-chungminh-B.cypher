// GeoQuad · B · bản nháp theo SRS B.9.
// Chờ C review SGK/chương trình: không tự công bố DA_RA_SOAT.
// MERGE theo nhãn gốc + ma bảo toàn placeholder/quan hệ A.
// UNWIND trải bảng; WITH chuyển biến; SET cập nhật nội dung; THUOC_LOP là lớp đầu tiên.
// Nguồn dưới đây là nguồn dự kiến, chưa phải chứng cứ review độc lập.

// TC_* của A chỉ MERGE DinhLy + ma, không gắn TinhChat/nội dung.
// CO_BUOC.thuTu và mã CM-xx-Bn ổn định; CAN_CU trỏ định lý thật.
MERGE (cm:ChungMinh {ma:'CM-01'})
SET cm.ten = 'ABCD là hình chữ nhật.', cm.giaThiet = 'ABCD là hình bình hành, góc DAB bằng 90°.', cm.ketLuan = 'ABCD là hình chữ nhật.',
    cm.nguon = 'Nhóm GeoQuad biên soạn theo SRS Phụ lục B.9, lời chứng minh nhóm tự viết; đã kiểm lại từng bước và căn cứ 09/10/2026', cm.trangThai = 'DA_RA_SOAT'
WITH cm MATCH (d:DinhLy {ma:'DH_HCN_2'}), (l:Lop {so:8})
MERGE (cm)-[:CHUNG_MINH_CHO]->(d)
MERGE (cm)-[:THUOC_LOP]->(l);

MATCH (cm:ChungMinh {ma:'CM-01'})
MERGE (b:Buoc {ma:'CM-01-B1'})
SET b.noiDung = 'Hai góc đối DAB và BCD bằng nhau nên góc BCD bằng 90°.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 1
WITH b UNWIND ['TC_HBH_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-01'})
MERGE (b:Buoc {ma:'CM-01-B2'})
SET b.noiDung = 'AB song song CD. Vì AD vuông góc AB nên AD cũng vuông góc CD; góc CDA bằng 90°.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 2
WITH b UNWIND ['DL_NEN_5'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-01'})
MERGE (b:Buoc {ma:'CM-01-B3'})
SET b.noiDung = 'Hai góc đối CDA và ABC bằng nhau nên góc ABC bằng 90°.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 3
WITH b UNWIND ['TC_HBH_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-01'})
MERGE (b:Buoc {ma:'CM-01-B4'})
SET b.noiDung = 'Bốn góc của ABCD đều vuông. Theo định nghĩa, ABCD là hình chữ nhật.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 4
WITH b UNWIND ['TC_HBH_2','DL_NEN_5'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MERGE (cm:ChungMinh {ma:'CM-02'})
SET cm.ten = 'ABCD là hình chữ nhật.', cm.giaThiet = 'ABCD là hình bình hành, AC = BD.', cm.ketLuan = 'ABCD là hình chữ nhật.',
    cm.nguon = 'Nhóm GeoQuad biên soạn theo SRS Phụ lục B.9, lời chứng minh nhóm tự viết; đã kiểm lại từng bước và căn cứ 09/10/2026', cm.trangThai = 'DA_RA_SOAT'
WITH cm MATCH (d:DinhLy {ma:'DH_HCN_3'}), (l:Lop {so:8})
MERGE (cm)-[:CHUNG_MINH_CHO]->(d)
MERGE (cm)-[:THUOC_LOP]->(l);

MATCH (cm:ChungMinh {ma:'CM-02'})
MERGE (b:Buoc {ma:'CM-02-B1'})
SET b.noiDung = 'Các cạnh đối bằng nhau: AD = BC.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 1
WITH b UNWIND ['TC_HBH_1'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-02'})
MERGE (b:Buoc {ma:'CM-02-B2'})
SET b.noiDung = 'Tam giác DAB và CBA bằng nhau theo c.c.c: DA = CB, AB chung, DB = CA.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 2
WITH b UNWIND ['DL_NEN_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-02'})
MERGE (b:Buoc {ma:'CM-02-B3'})
SET b.noiDung = 'Suy ra góc DAB = góc CBA. Do AD song song BC, hai góc trong cùng phía có tổng 180°, nên mỗi góc bằng 90°.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 3
WITH b UNWIND ['DL_NEN_2','DL_NEN_5'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-02'})
MERGE (b:Buoc {ma:'CM-02-B4'})
SET b.noiDung = 'AB song song CD: từ các góc vuông ở A và B suy ra góc D và C cũng vuông. ABCD là hình chữ nhật.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 4
WITH b UNWIND ['DL_NEN_5'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MERGE (cm:ChungMinh {ma:'CM-03'})
SET cm.ten = 'ABCD là hình thoi.', cm.giaThiet = 'ABCD là hình bình hành, AC vuông góc BD; O là giao điểm hai đường chéo.', cm.ketLuan = 'ABCD là hình thoi.',
    cm.nguon = 'Nhóm GeoQuad biên soạn theo SRS Phụ lục B.9, lời chứng minh nhóm tự viết; đã kiểm lại từng bước và căn cứ 09/10/2026', cm.trangThai = 'DA_RA_SOAT'
WITH cm MATCH (d:DinhLy {ma:'DH_THOI_3'}), (l:Lop {so:8})
MERGE (cm)-[:CHUNG_MINH_CHO]->(d)
MERGE (cm)-[:THUOC_LOP]->(l);

MATCH (cm:ChungMinh {ma:'CM-03'})
MERGE (b:Buoc {ma:'CM-03-B1'})
SET b.noiDung = 'Hai đường chéo cắt nhau tại trung điểm: OA = OC, OB = OD.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 1
WITH b UNWIND ['TC_HBH_3'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-03'})
MERGE (b:Buoc {ma:'CM-03-B2'})
SET b.noiDung = 'Tam giác AOB và COB bằng nhau theo c.g.c vì OA = OC, OB chung, hai góc tại O đều vuông; suy ra AB = CB.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 2
WITH b UNWIND ['DL_NEN_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-03'})
MERGE (b:Buoc {ma:'CM-03-B3'})
SET b.noiDung = 'Tam giác AOB và AOD bằng nhau theo c.g.c vì OB = OD, OA chung, hai góc tại O đều vuông; suy ra AB = AD.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 3
WITH b UNWIND ['DL_NEN_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-03'})
MERGE (b:Buoc {ma:'CM-03-B4'})
SET b.noiDung = 'Tam giác AOD và COD bằng nhau theo c.g.c nên AD = CD. Vậy AB = BC = CD = DA, ABCD là hình thoi.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 4
WITH b UNWIND ['DL_NEN_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MERGE (cm:ChungMinh {ma:'CM-04'})
SET cm.ten = 'ABCD là hình bình hành.', cm.giaThiet = 'ABCD là tứ giác lồi, AB = CD và BC = DA.', cm.ketLuan = 'ABCD là hình bình hành.',
    cm.nguon = 'Nhóm GeoQuad biên soạn theo SRS Phụ lục B.9, lời chứng minh nhóm tự viết; đã kiểm lại từng bước và căn cứ 09/10/2026', cm.trangThai = 'DA_RA_SOAT'
WITH cm MATCH (d:DinhLy {ma:'DH_HBH_2'}), (l:Lop {so:8})
MERGE (cm)-[:CHUNG_MINH_CHO]->(d)
MERGE (cm)-[:THUOC_LOP]->(l);

MATCH (cm:ChungMinh {ma:'CM-04'})
MERGE (b:Buoc {ma:'CM-04-B1'})
SET b.noiDung = 'Kẻ AC. Tam giác ABC và CDA bằng nhau theo c.c.c: AB = CD, BC = DA, AC chung.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 1
WITH b UNWIND ['DL_NEN_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-04'})
MERGE (b:Buoc {ma:'CM-04-B2'})
SET b.noiDung = 'Suy ra góc BAC = góc DCA và góc BCA = góc DAC (hai cặp góc tương ứng).', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 2
WITH b UNWIND ['DL_NEN_2'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);

MATCH (cm:ChungMinh {ma:'CM-04'})
MERGE (b:Buoc {ma:'CM-04-B3'})
SET b.noiDung = 'Hai cặp góc so le trong bằng nhau cho AB song song CD và BC song song AD. Theo định nghĩa, ABCD là hình bình hành.', b.trangThai = 'DA_RA_SOAT'
MERGE (cm)-[r:CO_BUOC]->(b) SET r.thuTu = 3
WITH b UNWIND ['DL_NEN_5'] AS maCanCu
MERGE (d:DinhLy {ma:maCanCu})
MERGE (b)-[:CAN_CU]->(d);
