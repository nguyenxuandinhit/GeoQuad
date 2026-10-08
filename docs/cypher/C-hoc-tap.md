# Cypher phần C — Học tập cá nhân

Tài liệu này lưu các truy vấn phần C để chạy trên Neo4j 5, cùng ý nghĩa ký hiệu và kết quả dự kiến. Tham số người dùng luôn truyền riêng qua driver, không ghép chuỗi. Mọi bài tập phải được nối tới ít nhất một `KhaiNiem` bằng `LIEN_QUAN_DEN`.

Đối chiếu SRS: FR-50/51/60 là Must; FR-04/52/61/62 là Should. BR-05 yêu cầu bài số nằm trong sai số (mặc định 0,01) **và đơn vị khớp đề**; BR-06 xét tối đa 5 lượt gần nhất, tối thiểu 3, ngưỡng dưới 60%; BR-08 yêu cầu gắn ít nhất một khái niệm; BR-09 cấm lưu tiến độ cho khách. Phụ lục D.10 xác nhận BT-014 (đáp án 96, lớp 8) và BT-027 (lời giải mẫu CM-01); các chi tiết này đã phản ánh trong seed/truy vấn.

SRS nói ngân hàng khoảng 30 bài được soạn trong Sprint 2, còn README/Jira đặt seed BT-011..030 vào ngày 07/10 (Sprint 1). Nội dung C hiện theo phạm vi README/Jira; lịch giữa các tài liệu chưa thống nhất.

SRS FR-04 mô tả đổi biệt danh/lớp/mật khẩu; màn hình SCR-17 còn liệt kê chức năng xóa tài khoản. README cũng có truy vấn xóa, nên phần C giữ chức năng này và yêu cầu xác nhận mật khẩu trước khi xóa.

## Rà soát US-03

README phân công C chạy seed của A (`10-kienthuc-A.cypher`) và B (`20-dinhly-B.cypher`), đối chiếu SRS Phụ lục B/README Phụ lục D rồi ghi kết quả ở đây. Trong checkout hiện tại chưa có hai file seed đó, `00-schema.cypher`, `01-khung.cypher`, Docker Compose hoặc Neo4j. Docker daemon từ môi trường hiện tại trả `permission denied`, cổng Neo4j `7687` cũng chưa lắng nghe. Vì vậy các truy vấn dưới đây đã được chuẩn bị nhưng **chưa chạy được trên cơ sở dữ liệu**; không đánh dấu nội dung/seed US-03 đã đạt trước khi có seed A và B.

### Đếm các nhóm nội dung US-03

```cypher
MATCH (n:DauHieu {trangThai:'DA_RA_SOAT'}) RETURN count(n) AS soDauHieu;
MATCH (n:TinhChat {trangThai:'DA_RA_SOAT'}) RETURN count(n) AS soTinhChat;
MATCH (n:CongThuc {trangThai:'DA_RA_SOAT'}) RETURN count(n) AS soCongThuc;
MATCH (n:DinhLy {trangThai:'DA_RA_SOAT'})
WHERE NOT n:DauHieu AND NOT n:TinhChat
RETURN count(n) AS soDieuKienDinhLyNen;
MATCH (n:DieuKien) RETURN count(n) AS soDieuKien;
```

Kết quả theo tiêu chí US-03: `20`, `12`, `12`, `6` định lý nền; riêng `DieuKien` là `14`. Sáu định lý nền là mục D.7; chúng không thay thế 14 nút `DieuKien`.

### Mỗi dấu hiệu phải đủ các quan hệ

```cypher
MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})
OPTIONAL MATCH (d)-[la:YEU_CAU_LA]->(:KhaiNiem)
WITH d, count(DISTINCT la) AS soNen
OPTIONAL MATCH (d)-[co:YEU_CAU_CO]->(:DieuKien)
WITH d, soNen, count(DISTINCT co) AS soDieuKien
OPTIONAL MATCH (d)-[k:KHANG_DINH]->(:KhaiNiem)
WITH d, soNen, soDieuKien, count(DISTINCT k) AS soKetLuan
WHERE soNen <> 1 OR soDieuKien < 1 OR soKetLuan <> 1
RETURN d.ma AS ma, soNen, soDieuKien, soKetLuan;
```

Kết quả mong đợi là 0 dòng. Truy vấn kiểm tra từng nút dấu hiệu có đúng một hình nền, ít nhất một điều kiện và đúng một kết luận. Đối chiếu thêm đủ `nguon` và `trangThai`:

```cypher
MATCH (n)
WHERE (n:DauHieu OR n:TinhChat OR n:CongThuc OR n:DieuKien)
  AND (n.nguon IS NULL OR n.trangThai IS NULL)
RETURN labels(n) AS nhan, n.ma AS ma, n.nguon AS nguon, n.trangThai AS trangThai;
```

Kết quả mong đợi là 0 dòng. Để kiểm tra seed lặp lại, ghi lại các số đếm trước/sau khi chạy từng seed A/B lần thứ hai; chúng phải giữ nguyên và các truy vấn quan hệ ở trên vẫn trả về 0 dòng. Khi nhận được seed A/B và môi trường Neo4j, ghi ngày, lệnh chạy, các số đếm và lỗi đã sửa tại đây.

## US-04 — Kiểm tra seed bài tập C

Seed C có 20 bài: 7 trắc nghiệm, 10 đáp án số, 3 bài chứng minh có lời giải; 6 bài nối Hình thoi và có BT-014/BT-027 theo D.10. Bốn nút `ChungMinh` mẫu của US-04 cha thuộc seed B (`21-chungminh-B.cypher`). Kiểm tra tĩnh ngày 08/10/2026 đã đối chiếu mã, cấp/lớp, loại, công thức tính, đáp án, liên kết khái niệm và dữ liệu làm bài mẫu. BT-026 được sửa để chỉ có một phương án đúng. Có thể chạy lại kiểm tra bằng `python tests/GeoQuad.Tests/C_HocTap/seed_contract.py`. Đây chưa phải kết quả chạy trên Neo4j: checkout thiếu `00-schema.cypher`, `01-khung.cypher`, Docker Compose và seed A/B; Docker daemon hiện chưa chạy.

Seed C cần chạy **sau `01-khung.cypher`** vì `Lop`, `CapHoc` và `KhaiNiem` được lấy bằng `MATCH`, không tự tạo nút nền. Tham chiếu tới `DinhLy`/`CongThuc` của A/B dùng `MERGE` theo mã, nên có thể chạy trước hoặc sau seed A/B.

```cypher
MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
RETURN count(b) AS soBai,
       count(CASE WHEN b.loai = 'TRAC_NGHIEM' THEN 1 END) AS tracNghiem,
       count(CASE WHEN b.loai = 'DAP_AN_SO' THEN 1 END) AS dapAnSo,
       count(CASE WHEN b.loai = 'CHUNG_MINH' THEN 1 END) AS chungMinh;
```

Kết quả dự kiến cho riêng C: `20`, `7`, `10`, `3`. Sau khi seed A chạy, toàn nhóm dự kiến có 30 bài và đủ ba loại.

Kiểm tra phân bố cấp và cấu trúc từng bài:

```cypher
MATCH (b:BaiTap)-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
RETURN cap.ma AS cap, count(DISTINCT b) AS soBai
ORDER BY cap;

MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
OPTIONAL MATCH (b)-[:THUOC_LOP]->(l:Lop)
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
OPTIONAL MATCH (b)-[:SU_DUNG]->(x)
WITH b, count(DISTINCT l) AS soLop, count(DISTINCT k) AS soKhaiNiem,
     count(DISTINCT x) AS soKienThuc
WHERE soLop <> 1 OR soKhaiNiem < 1 OR soKienThuc < 1
   OR b.nguon IS NULL OR b.trangThai <> 'DA_RA_SOAT'
   OR b.hienThi <> true OR b.doKho < 1 OR b.doKho > 3
RETURN b.ma AS ma, soLop, soKhaiNiem, soKienThuc,
       b.nguon AS nguon, b.trangThai AS trangThai;
```

Kết quả dự kiến: `CAP_2 = 15`, `CAP_3 = 5`; truy vấn chẩn đoán trả 0 dòng. Ba bài chứng minh phải có `loiGiaiMau`; bảy bài trắc nghiệm phải có bốn phương án và một đáp án A–D; mười bài số phải có `dapAnSo`, `saiSo = 0.01`, `donVi` (BT-028 không có đơn vị).

```cypher
MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WITH b, collect(DISTINCT k.ma) AS khaiNiem
WHERE size(khaiNiem) = 0
RETURN b.ma AS ma;
```

Kết quả dự kiến là 0 dòng. Kiểm tra ít nhất năm bài gắn Hình thoi:

```cypher
MATCH (b:BaiTap)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:'HINH_THOI'})
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
RETURN count(DISTINCT b) AS soBaiHinhThoi;
```

Kết quả dự kiến ít nhất `5` (seed C hiện gắn sáu bài). Kiểm tra bài số/proof đặc tả:

```cypher
MATCH (b:BaiTap {ma:'BT-014'})-[:SU_DUNG]->(c:CongThuc {ma:'CT_THOI_DT'})
RETURN b.dapAnSo AS dapAn, b.saiSo AS saiSo, b.donVi AS donVi, c.ma AS congThuc;

MATCH (b:BaiTap {ma:'BT-027'})-[:SU_DUNG]->(d:DinhLy {ma:'DH_HCN_2'})
OPTIONAL MATCH (cm:ChungMinh {ma:'CM-01'})-[:CHUNG_MINH_CHO]->(d)
RETURN b.loai AS loai, b.loiGiaiMau IS NOT NULL AS coLoiGiai,
       d.ma AS dauHieu, cm.ma AS chungMinhMau;
```

Kết quả dự kiến: BT-014 có `96`, sai số `0.01`, `cm²`, công thức `CT_THOI_DT`; BT-027 là `CHUNG_MINH`, có lời giải và liên kết tới `CM-01` sau khi seed B được nạp.

Sau khi chạy seed C lần thứ nhất và thứ hai, dùng cùng truy vấn đếm dưới đây. Hai lần phải trả cùng số nút và số quan hệ xuất phát từ các bài C:

```cypher
MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
OPTIONAL MATCH (b)-[r]->()
RETURN count(DISTINCT b) AS soBai, count(DISTINCT r) AS soQuanHe;
```

Chạy dữ liệu dev **sau khi ứng dụng Development tạo tài khoản demo** `hocsinh8` (`tenDangNhap`; `id` của tài khoản là UUID). Nên kiểm tra trên tài khoản demo chưa có lượt làm khác, vì các lượt làm mới có thể đổi tập 5 lần gần nhất. Seed dev dùng `MERGE` theo `maLan`, chạy hai lần vẫn có 7 lượt mẫu, trong đó 5 lượt liên quan Hình thoi và 2 lượt đúng:

```cypher
MATCH (:TaiKhoan {tenDangNhap:'hocsinh8'})-[d:DA_LAM]->(:BaiTap)
WHERE d.maLan STARTS WITH 'DEV-C-'
RETURN count(d) AS soLanMau;

MATCH (:TaiKhoan {tenDangNhap:'hocsinh8'})-[d:DA_LAM]->
      (:BaiTap)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:'HINH_THOI'})
WHERE d.maLan STARTS WITH 'DEV-C-'
WITH d ORDER BY d.luc DESC
WITH collect(d)[0..5] AS gan
RETURN size(gan) AS soLanGanNhat,
       size([x IN gan WHERE x.dung]) AS soLanDung;
```

Kết quả dự kiến: `soLanMau = 7`, `soLanGanNhat = 5`, `soLanDung = 2`. Bài BT-017 có hai lượt mẫu (đúng với `84`, sai với `80`) để kiểm tra khái niệm mới có dưới ba lượt không bị xếp vào danh sách cần ôn.

## US-08 — Cập nhật hồ sơ

Đổi biệt danh/lớp; `$tk`, `$lop`, `$bietDanh` là tham số, không lấy bằng nối chuỗi. Ứng dụng phải gọi `IAuthHelper.DangNhapAsync` sau khi lưu để cấp lại cookie theo lớp mới.

```cypher
MATCH (tk:TaiKhoan {id:$tk}), (l:Lop {so:$lop})
OPTIONAL MATCH (tk)-[r:HOC_LOP]->()
DELETE r
MERGE (tk)-[:HOC_LOP]->(l)
SET tk.bietDanh = $bietDanh
RETURN tk.id AS id, tk.bietDanh AS bietDanh, l.so AS lop;
```

Đổi mật khẩu: trước truy vấn cần xác thực mật khẩu cũ bằng `IAuthHelper.KiemTraMatKhau`, sau đó băm mật khẩu mới bằng `BamMatKhau`.

```cypher
MATCH (tk:TaiKhoan {id:$tk})
SET tk.matKhauBam = $bam
RETURN tk.id AS id;
```

README còn mở rộng US-08 thành xóa tài khoản; câu sau xóa các quan hệ học tập cùng nút tài khoản và phải chỉ chạy sau xác nhận mật khẩu.

```cypher
MATCH (tk:TaiKhoan {id:$tk})
DETACH DELETE tk;
```

Kiểm tra: sau đổi lớp 7 → 8, `HOC_LOP` chỉ còn trỏ tới lớp 8; mật khẩu sai không được đổi; sau xóa, tài khoản và các quan hệ `DA_LAM`, `DA_HOC`, `HOC_LOP` không còn.

## US-19 — Lọc ngân hàng bài tập

```cypher
MATCH (bt:BaiTap {hienThi:true})-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE l.so <= $lop
  AND ($lopLoc IS NULL OR l.so = $lopLoc)
  AND ($capLoc IS NULL OR cap.ma = $capLoc)
  AND ($loai IS NULL OR bt.loai = $loai)
  AND ($doKho IS NULL OR bt.doKho = $doKho)
  AND ($khaiNiem IS NULL OR EXISTS {
    MATCH (bt)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:$khaiNiem})
  })
OPTIONAL MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM]->(bt)
WITH bt, l, count(d) AS soLan,
     any(x IN collect(d) WHERE x.dung) AS daDung
RETURN bt.ma AS ma, left(bt.de,120) AS de, bt.loai AS loai,
       bt.doKho AS doKho, l.so AS lop, soLan, daDung
ORDER BY lop, doKho, ma;
```

Truyền `null` cho bộ lọc không chọn. Kết quả mong đợi khi lọc lớp 8 + đáp án số + Hình thoi gồm BT-013, BT-014, BT-020 và BT-023; bài ở lớp cao hơn lớp hiện tại và bài ẩn không xuất hiện.

## US-20 — Lấy đề, chấm và lưu lần làm

Lấy dữ liệu để hiển thị; tuyệt đối không gửi `dapAnDung`, `dapAnSo` hoặc lời giải xuống trang trước khi nộp.

```cypher
MATCH (bt:BaiTap {ma:$ma, hienThi:true})
OPTIONAL MATCH (bt)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
OPTIONAL MATCH (bt)-[:SU_DUNG]->(x)
RETURN bt.ma AS ma, bt.loai AS loai, bt.de AS de,
       bt.phuongAn AS phuongAn, bt.donVi AS donVi,
       bt.doKho AS doKho,
       collect(DISTINCT k {.ma,.ten}) AS khaiNiem,
       collect(DISTINCT x {.ma,.ten,.noiDung,.bieuThuc}) AS kienThucSuDung;
```

Projection trên cố ý bỏ đáp án và lời giải. Sau khi nhận bài nộp, backend mới đọc đáp án để chấm và dựng phản hồi:

```cypher
MATCH (bt:BaiTap {ma:$ma, hienThi:true})
RETURN bt.loai AS loai, bt.dapAnDung AS dapAnDung,
       bt.dapAnSo AS dapAnSo, bt.saiSo AS saiSo,
       bt.giaiThich AS giaiThich, bt.loiGiaiMau AS loiGiaiMau;
```

Lưu bài đã chấm cho học sinh đăng nhập (chỉ gọi khi `ICurrentUser` có tài khoản). Khách không chạy truy vấn ghi này. Mỗi lần nộp tạo một quan hệ mới để giữ lịch sử.

```cypher
MATCH (tk:TaiKhoan {id:$tk}), (bt:BaiTap {ma:$ma})
CREATE (tk)-[:DA_LAM {
  luc:datetime(), dung:$dung, dapAnDaChon:$dapAn, thoiGianGiay:$giay
}]->(bt)
RETURN bt.ma AS ma, $dung AS dung;
```

Chấm điểm là logic thuần `ChamBai`, không phải Cypher: trắc nghiệm so chữ cái; bài số phải khớp đơn vị yêu cầu, chấp nhận dấu phẩy hoặc dấu chấm và đúng khi `abs(nhap - dapAnSo) <= saiSo`. Kết quả mong đợi theo README/SRS: BT-014 nhập `96,0 cm²` là đúng; nhập `95 cm²` là sai; sai đơn vị phải bị từ chối; khách không có quan hệ `DA_LAM` mới.

## US-21 — Xem lời giải mẫu

```cypher
MATCH (bt:BaiTap {ma:$ma, loai:'CHUNG_MINH', hienThi:true})
OPTIONAL MATCH (bt)-[:SU_DUNG]->(:DinhLy)<-[:CHUNG_MINH_CHO]-
               (cm:ChungMinh {trangThai:'DA_RA_SOAT'})
RETURN bt.loiGiaiMau AS loiGiaiMau,
       collect(DISTINCT cm {.ma,.ten}) AS chungMinhMau;
```

Kết quả BT-027 có nội dung lời giải và `CM-01`; bài chứng minh không đi qua bộ chấm tự động.

## US-22 — Lộ trình và đánh dấu đã học

```cypher
MATCH (muc:KhaiNiem {ma:$muc})
MATCH p = (muc)-[:CAN_BIET_TRUOC*0..]->(k:KhaiNiem)-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND k.trangThai = 'DA_RA_SOAT'
WITH k, max(length(p)) AS doSau
OPTIONAL MATCH (:TaiKhoan {id:$tk})-[h:DA_HOC]->(k)
RETURN k.ma AS ma, k.ten AS ten, k.loai AS loai,
       doSau, h IS NOT NULL AS daHoc
ORDER BY doSau DESC, ma;
```

`$lop` là lớp của học sinh; đường đi chỉ lấy kiến thức đã rà soát và có lớp không vượt quá lớp đó. Mục tiêu không có tiên quyết vẫn xuất hiện với độ sâu 0.

```cypher
MATCH (tk:TaiKhoan {id:$tk}), (k:KhaiNiem {ma:$ma})
MERGE (tk)-[h:DA_HOC]->(k)
ON CREATE SET h.luc = datetime()
RETURN k.ma AS ma;
```

Kết quả mong đợi: Hình chữ nhật và Hình thoi đứng trước Hình vuông cho `hocsinh8`; Hình thang cân không xuất hiện với `hocsinh4`; đánh dấu rồi tải lại giữ `daHoc = true`.

## US-23 — Tiến độ và phần cần ôn

Tổng số lần làm, số bài khác nhau và số lần đúng:

```cypher
MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM]->(bt:BaiTap)
RETURN count(d) AS soLanLam,
       count(DISTINCT bt) AS soBai,
       sum(CASE WHEN d.dung THEN 1 ELSE 0 END) AS soDung;
```

Tỉ lệ đúng theo khái niệm:

```cypher
MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM]->(:BaiTap)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
RETURN k.ma AS ma, k.ten AS ten, count(d) AS soLan,
       toFloat(sum(CASE WHEN d.dung THEN 1 ELSE 0 END)) / count(d) AS tiLe
ORDER BY tiLe, ten;
```

Khái niệm cần ôn: chỉ xét tối đa 5 lần gần nhất, cần ít nhất 3 lần; `$nguong` mặc định 0.6 từ `HocTap:NguongCanOn`.

```cypher
MATCH (:TaiKhoan {id:$tk})-[lam:DA_LAM]->(:BaiTap)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WITH k, lam ORDER BY lam.luc DESC
WITH k, collect(lam)[0..5] AS gan
WHERE size(gan) >= 3
WITH k, size(gan) AS soLan,
     toFloat(size([x IN gan WHERE x.dung])) / size(gan) AS tiLe
WHERE tiLe < $nguong
RETURN k.ma AS ma, k.ten AS ten, soLan, tiLe
ORDER BY tiLe, ten;
```

Sau seed `31-lam-bai-mau-C.cypher`, Hình thoi của `hocsinh8` có 2/5 lần đúng nên xuất hiện; Hình bình hành mới có 2 lần nên không xuất hiện. Khách được chuyển tới đăng nhập trước khi truy vấn thống kê.

## US-24 — Gợi ý bài tiếp theo

```cypher
MATCH (bt:BaiTap)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WHERE k.ma IN $canOn AND bt.hienThi = true AND bt.loai <> 'CHUNG_MINH'
MATCH (bt)-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop
  AND NOT EXISTS {
    MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM]->(bt)
    WHERE d.dung = true
  }
RETURN DISTINCT bt.ma AS ma, left(bt.de,120) AS de, bt.doKho AS doKho
ORDER BY doKho, ma LIMIT 5;
```

Kết quả mong đợi tối đa 5 bài; không có bài chứng minh, không có bài học sinh đã làm đúng, lớp không vượt quá lớp học sinh, độ khó tăng dần. Nếu danh sách cần ôn rỗng, nhánh thay thế hiển thị lời chúc và tối đa 5 bài chưa làm đúng có độ khó giảm dần cùng liên kết lộ trình.

## Giải thích ký hiệu Cypher

| Ký hiệu | Ý nghĩa trong truy vấn phần C |
|---|---|
| `(b:BaiTap)` | Nút tên biến `b`, nhãn `BaiTap`; `{ma:$ma}` là điều kiện thuộc tính. |
| `-[r:DA_LAM]->` | Quan hệ có hướng, biến `r`, loại `DA_LAM`; dấu mũi tên chỉ chiều tài khoản → bài tập. |
| `-[:LIEN_QUAN_DEN]->` | Quan hệ không cần lấy biến, nối bài tới khái niệm. |
| `{ma: row.ma}` | Map thuộc tính dùng để tìm/tạo theo khóa `ma`. |
| `$tk`, `$lop`, `$ma` | Tham số bind riêng; không phải nội suy chuỗi. |
| `MERGE` | Tìm mẫu đã có hoặc tạo mẫu còn thiếu; dùng để seed có thể chạy lại. |
| `OPTIONAL MATCH` | Ghép nếu có; không làm mất bài khi học sinh chưa có lịch sử. |
| `EXISTS { MATCH ... }` | Kiểm tra có tồn tại đường/mẫu thỏa điều kiện. |
| `*0..` | Đường đi có độ dài từ 0 trở lên, nên gồm cả chính khái niệm mục tiêu. |
| `[]` trong `collect(...)[0..5]` | Danh sách kết quả; `[0..5]` lấy tối đa năm phần tử đầu. |
| `|` trong `[x IN gan WHERE ...]` | Phân cách biểu thức đầu ra của list comprehension với biến/điều kiện. |
| `WITH` | Chuyển tiếp, nhóm hoặc tính toán các giá trị cho phần kế tiếp của truy vấn. |
| `CASE WHEN` | Điều kiện để cộng số lần đúng mà vẫn giữ các lần sai trong tổng số. |
| `FOREACH (... IN ... | ...)` | Lặp thao tác cập nhật seed cho từng mã liên quan trong danh sách. |
| `DISTINCT` | Loại trùng khi nhiều đường quan hệ cùng đưa về một nút. |
| `IS NULL` / `IS NOT NULL` | Kiểm tra bộ lọc chưa chọn hoặc dữ liệu có tồn tại. |

## Đề xuất thay đổi chung

- Neo4j không bảo đảm thứ tự bên trong `collect()` nếu không sắp xếp trước; truy vấn US-23 sắp xếp các lượt theo `luc` trước khi lấy 5 lượt gần nhất.
- Seed mẫu dùng thuộc tính phụ `DA_LAM.maLan` để `MERGE` idempotent. Thuộc tính này chỉ định danh dữ liệu dev; lượt làm thật vẫn là quan hệ mới như BR-09.
- Cần seed nền và seed A/B trước khi kiểm tra các AC có liên quan đến cấp/lớp, tính chất, công thức, dấu hiệu và CM-01.
