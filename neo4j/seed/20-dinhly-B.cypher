// GeoQuad · B · bản nháp theo SRS B.3/B.4/B.7.
// Chờ C review SGK/chương trình: không tự công bố DA_RA_SOAT.
// MERGE theo nhãn gốc + ma bảo toàn placeholder/quan hệ A.
// UNWIND trải bảng; WITH chuyển biến; SET cập nhật nội dung; THUOC_LOP là lớp đầu tiên.
// Nguồn dưới đây là nguồn dự kiến, chưa phải chứng cứ review độc lập.

UNWIND [
  {ma: 'DK_MOT_CAP_CANH_DOI_SONG', noiDung: 'Có một cặp cạnh đối song song'},
  {ma: 'DK_CANH_DOI_SONG', noiDung: 'Các cạnh đối song song'},
  {ma: 'DK_CANH_DOI_BANG', noiDung: 'Các cạnh đối bằng nhau'},
  {ma: 'DK_MOT_CAP_SONG_SONG_BANG', noiDung: 'Có một cặp cạnh đối vừa song song vừa bằng nhau'},
  {ma: 'DK_GOC_DOI_BANG', noiDung: 'Các góc đối bằng nhau'},
  {ma: 'DK_CHEO_CAT_TRUNG_DIEM', noiDung: 'Hai đường chéo cắt nhau tại trung điểm của mỗi đường'},
  {ma: 'DK_BA_GOC_VUONG', noiDung: 'Có ba góc vuông'},
  {ma: 'DK_MOT_GOC_VUONG', noiDung: 'Có một góc vuông'},
  {ma: 'DK_CHEO_BANG_NHAU', noiDung: 'Hai đường chéo bằng nhau'},
  {ma: 'DK_BON_CANH_BANG', noiDung: 'Bốn cạnh bằng nhau'},
  {ma: 'DK_HAI_CANH_KE_BANG', noiDung: 'Hai cạnh kề bằng nhau'},
  {ma: 'DK_CHEO_VUONG_GOC', noiDung: 'Hai đường chéo vuông góc'},
  {ma: 'DK_CHEO_PHAN_GIAC', noiDung: 'Một đường chéo là đường phân giác của một góc'},
  {ma: 'DK_HAI_GOC_KE_DAY_BANG', noiDung: 'Hai góc kề một đáy bằng nhau'}
] AS row
MERGE (dk:DieuKien {ma: row.ma})
SET dk.ten = row.noiDung, dk.noiDung = row.noiDung,
    dk.nguon = 'CT GDPT 2018 – Toán 8 (chờ C đối chiếu SGK)', dk.trangThai = 'NHAP'
WITH dk MATCH (l:Lop {so:8})
MERGE (dk)-[:THUOC_LOP]->(l);

UNWIND [
  {ma: 'DH_HT_1', noiDung: 'Tứ giác có hai cạnh đối song song là hình thang.', nen: 'TU_GIAC', dk: 'DK_MOT_CAP_CANH_DOI_SONG', dich: 'HINH_THANG', lop: 8},
  {ma: 'DH_HTC_1', noiDung: 'Hình thang có hai góc kề một đáy bằng nhau là hình thang cân.', nen: 'HINH_THANG', dk: 'DK_HAI_GOC_KE_DAY_BANG', dich: 'HINH_THANG_CAN', lop: 8},
  {ma: 'DH_HTC_2', noiDung: 'Hình thang có hai đường chéo bằng nhau là hình thang cân.', nen: 'HINH_THANG', dk: 'DK_CHEO_BANG_NHAU', dich: 'HINH_THANG_CAN', lop: 8},
  {ma: 'DH_HBH_1', noiDung: 'Tứ giác có các cạnh đối song song là hình bình hành.', nen: 'TU_GIAC', dk: 'DK_CANH_DOI_SONG', dich: 'HINH_BINH_HANH', lop: 8},
  {ma: 'DH_HBH_2', noiDung: 'Tứ giác có các cạnh đối bằng nhau là hình bình hành.', nen: 'TU_GIAC', dk: 'DK_CANH_DOI_BANG', dich: 'HINH_BINH_HANH', lop: 8},
  {ma: 'DH_HBH_3', noiDung: 'Tứ giác có hai cạnh đối song song và bằng nhau là hình bình hành.', nen: 'TU_GIAC', dk: 'DK_MOT_CAP_SONG_SONG_BANG', dich: 'HINH_BINH_HANH', lop: 8},
  {ma: 'DH_HBH_4', noiDung: 'Tứ giác có các góc đối bằng nhau là hình bình hành.', nen: 'TU_GIAC', dk: 'DK_GOC_DOI_BANG', dich: 'HINH_BINH_HANH', lop: 8},
  {ma: 'DH_HBH_5', noiDung: 'Tứ giác có hai đường chéo cắt nhau tại trung điểm của mỗi đường là hình bình hành.', nen: 'TU_GIAC', dk: 'DK_CHEO_CAT_TRUNG_DIEM', dich: 'HINH_BINH_HANH', lop: 8},
  {ma: 'DH_HCN_1', noiDung: 'Tứ giác có ba góc vuông là hình chữ nhật.', nen: 'TU_GIAC', dk: 'DK_BA_GOC_VUONG', dich: 'HINH_CHU_NHAT', lop: 8},
  {ma: 'DH_HCN_2', noiDung: 'Hình bình hành có một góc vuông là hình chữ nhật.', nen: 'HINH_BINH_HANH', dk: 'DK_MOT_GOC_VUONG', dich: 'HINH_CHU_NHAT', lop: 8},
  {ma: 'DH_HCN_3', noiDung: 'Hình bình hành có hai đường chéo bằng nhau là hình chữ nhật.', nen: 'HINH_BINH_HANH', dk: 'DK_CHEO_BANG_NHAU', dich: 'HINH_CHU_NHAT', lop: 8},
  {ma: 'DH_THOI_1', noiDung: 'Tứ giác có bốn cạnh bằng nhau là hình thoi.', nen: 'TU_GIAC', dk: 'DK_BON_CANH_BANG', dich: 'HINH_THOI', lop: 8},
  {ma: 'DH_THOI_2', noiDung: 'Hình bình hành có hai cạnh kề bằng nhau là hình thoi.', nen: 'HINH_BINH_HANH', dk: 'DK_HAI_CANH_KE_BANG', dich: 'HINH_THOI', lop: 8},
  {ma: 'DH_THOI_3', noiDung: 'Hình bình hành có hai đường chéo vuông góc với nhau là hình thoi.', nen: 'HINH_BINH_HANH', dk: 'DK_CHEO_VUONG_GOC', dich: 'HINH_THOI', lop: 8},
  {ma: 'DH_THOI_4', noiDung: 'Hình bình hành có một đường chéo là đường phân giác của một góc là hình thoi.', nen: 'HINH_BINH_HANH', dk: 'DK_CHEO_PHAN_GIAC', dich: 'HINH_THOI', lop: 8},
  {ma: 'DH_HV_1', noiDung: 'Hình chữ nhật có hai cạnh kề bằng nhau là hình vuông.', nen: 'HINH_CHU_NHAT', dk: 'DK_HAI_CANH_KE_BANG', dich: 'HINH_VUONG', lop: 8},
  {ma: 'DH_HV_2', noiDung: 'Hình chữ nhật có hai đường chéo vuông góc với nhau là hình vuông.', nen: 'HINH_CHU_NHAT', dk: 'DK_CHEO_VUONG_GOC', dich: 'HINH_VUONG', lop: 8},
  {ma: 'DH_HV_3', noiDung: 'Hình chữ nhật có một đường chéo là đường phân giác của một góc là hình vuông.', nen: 'HINH_CHU_NHAT', dk: 'DK_CHEO_PHAN_GIAC', dich: 'HINH_VUONG', lop: 8},
  {ma: 'DH_HV_4', noiDung: 'Hình thoi có một góc vuông là hình vuông.', nen: 'HINH_THOI', dk: 'DK_MOT_GOC_VUONG', dich: 'HINH_VUONG', lop: 8},
  {ma: 'DH_HV_5', noiDung: 'Hình thoi có hai đường chéo bằng nhau là hình vuông.', nen: 'HINH_THOI', dk: 'DK_CHEO_BANG_NHAU', dich: 'HINH_VUONG', lop: 8}
] AS row
MERGE (d:DinhLy {ma: row.ma})
SET d:DauHieu, d.ten = row.noiDung, d.noiDung = row.noiDung,
    d.nguon = 'CT GDPT 2018 – Toán 8 (chờ C đối chiếu SGK)', d.trangThai = 'NHAP'
WITH d,row
MATCH (nen:KhaiNiem {ma:row.nen}), (dich:KhaiNiem {ma:row.dich}),
      (dk:DieuKien {ma:row.dk}), (l:Lop {so:row.lop})
MERGE (d)-[:YEU_CAU_LA]->(nen)
MERGE (d)-[:KHANG_DINH]->(dich)
MERGE (d)-[:YEU_CAU_CO]->(dk)
MERGE (d)-[:THUOC_LOP]->(l);

UNWIND [
  {ma: 'DL_NEN_1', noiDung: 'Tổng ba góc của một tam giác bằng 180°.', lop: 7},
  {ma: 'DL_NEN_2', noiDung: 'Hai tam giác bằng nhau theo các trường hợp c.c.c, c.g.c, g.c.g.', lop: 7},
  {ma: 'DL_NEN_3', noiDung: 'Định lí Pythagore: trong tam giác vuông, bình phương cạnh huyền bằng tổng bình phương hai cạnh góc vuông.', lop: 8},
  {ma: 'DL_NEN_4', noiDung: 'Đường trung bình của tam giác song song với cạnh thứ ba và bằng nửa cạnh ấy.', lop: 8},
  {ma: 'DL_NEN_5', noiDung: 'Hai đường thẳng song song bị cắt bởi một cát tuyến thì các cặp góc so le trong bằng nhau.', lop: 7},
  {ma: 'DL_NEN_6', noiDung: 'Tứ giác ABCD là hình bình hành khi và chỉ khi vectơ AB bằng vectơ DC (lớp 10).', lop: 10}
] AS row
MERGE (d:DinhLy {ma: row.ma})
SET d.ten = row.noiDung, d.noiDung = row.noiDung,
    d.nguon = 'CT GDPT 2018 – Toán ' + toString(row.lop) + ' (chờ C đối chiếu SGK)', d.trangThai = 'NHAP'
WITH d,row MATCH (l:Lop {so:row.lop})
MERGE (d)-[:THUOC_LOP]->(l);
