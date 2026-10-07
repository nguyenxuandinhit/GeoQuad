# Cypher – Phần A · Tra cứu & trực quan

> Khung do PHẦN 0 tạo (US-01). Thành viên A ghi vào đây **mọi** truy vấn Cypher của phần mình.
> Story của phần A: US-09, US-10, US-11, US-12, US-15, US-16.

Quy ước bắt buộc (README mục 2, điểm 11): mỗi truy vấn có **ba** mục —
câu Cypher, **giải thích từng ký hiệu** xuất hiện trong câu đó, và **kết quả mong đợi**
trên dữ liệu seed. Tài liệu này dùng để cả nhóm học Neo4j và trình bày khi demo.

## Bảng ký hiệu Cypher (tham khảo chung)

| Ký hiệu | Nghĩa |
|---|---|
| `()` | một nút, ví dụ `(k:KhaiNiem)` là nút có nhãn `KhaiNiem`, đặt tên tạm là `k` |
| `:Nhan` | nhãn của nút (`KhaiNiem`, `DinhLy`…) hoặc kiểu của quan hệ |
| `[]` | một quan hệ, ví dụ `-[:THUOC_LOP]->` |
| `->` | chiều của quan hệ (từ nút phụ thuộc tới nút được tham chiếu) |
| `{}` | bộ thuộc tính / bản đồ khóa-giá trị, ví dụ `{ma: $ma}` |
| `$ten` | **tham số** — dữ liệu người dùng luôn truyền qua đây, không nối chuỗi (NFR-05) |
| `*1..` | đi theo quan hệ từ 1 bước trở lên (đường đi độ dài thay đổi) |
| `\|` | "hoặc", ví dụ `-[:CO_TINH_CHAT\|CO_CONG_THUC]->` |
| `MATCH` / `OPTIONAL MATCH` | tìm mẫu; `OPTIONAL` thì không có cũng vẫn trả dòng (giá trị `null`) |
| `MERGE` | tìm theo khóa, chưa có thì tạo — nhờ vậy seed chạy lại không trùng (NFR-08) |
| `WITH` | chuyển kết quả sang bước tiếp theo (như một "đường ống") |
| `UNWIND` | trải một danh sách thành từng dòng |

## Truy vấn theo story

<!-- Mẫu cho mỗi truy vấn:

### US-xx · <tên màn hình> (FR-xx, UC-xx)

```cypher
...
```

**Giải thích ký hiệu:** …

**Kết quả mong đợi trên dữ liệu seed:** …
-->

### US-03 · Seed 12 tính chất (`neo4j/seed/10-kienthuc-A.cypher`)

```cypher
UNWIND [
  {ma: 'TC_TG_1', hinh: 'TU_GIAC', lop: 8,
   noiDung: 'Tổng bốn góc của một tứ giác bằng 360°.'},
  ...
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
```

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `UNWIND [ {...}, {...} ] AS row` | trải danh sách 12 bản đồ thành 12 dòng, mỗi dòng gọi là `row` |
| `row.ma`, `row.hinh` | lấy giá trị theo khóa trong bản đồ của dòng đang xử lý |
| `MERGE (t:DinhLy {ma: row.ma})` | tìm nút nhãn `DinhLy` có `ma` bằng giá trị đó; chưa có thì tạo. Nhờ `MERGE` mà chạy seed lần hai không sinh nút trùng (NFR-08) |
| `SET t:TinhChat` | gắn **thêm** nhãn phụ `TinhChat`; nút giữ cả hai nhãn `DinhLy` và `TinhChat` (lược đồ mục 4.1) |
| `SET t.noiDung = ...` | đặt thuộc tính; chạy lại thì ghi đè đúng giá trị cũ nên kết quả không đổi |
| `+ toString(row.lop)` | nối chuỗi; `toString` đổi số 8 thành `'8'` để ghép vào `nguon` |
| `WITH t, row` | chuyển `t` và `row` sang bước sau, như một đường ống; không có `WITH` thì `MATCH` phía dưới không thấy chúng |
| `MATCH (h:KhaiNiem {ma: row.hinh})` | **tìm** nút hình do PHẦN 0 tạo. Dùng `MATCH` chứ không `MERGE` vì `01-khung.cypher` chạy trước (thứ tự tên file) |
| `(h)-[:CO_TINH_CHAT]->(t)` | quan hệ **có chiều**: từ hình tới tính chất của hình đó |
| `-[:THUOC_LOP]->(l)` | tính chất thuộc lớp xuất hiện lần đầu, dùng để lọc theo lớp (BR-04) |
| `;` | kết thúc một câu; `cypher-shell` chạy từng câu một |

**Kết quả mong đợi trên dữ liệu seed**

```cypher
MATCH (t:TinhChat) RETURN count(t) AS soTinhChat;          // 12
MATCH (h:KhaiNiem)-[:CO_TINH_CHAT]->(t:TinhChat)
RETURN h.ma AS hinh, collect(t.ma) AS tinhChat ORDER BY hinh;
```

| hinh | tinhChat |
|---|---|
| HINH_BINH_HANH | TC_HBH_1, TC_HBH_2, TC_HBH_3 |
| HINH_CHU_NHAT | TC_HCN_1, TC_HCN_2 |
| HINH_THANG | TC_HT_1 |
| HINH_THANG_CAN | TC_HTC_1, TC_HTC_2 |
| HINH_THOI | TC_THOI_1, TC_THOI_2 |
| HINH_VUONG | TC_HV_1 |
| TU_GIAC | TC_TG_1 |

Mỗi tính chất có đúng 1 `CO_TINH_CHAT` và 1 `THUOC_LOP`; cả 12 nút đều có `nguon` và
`trangThai = 'DA_RA_SOAT'` (BR-13).

### US-03 · Seed 12 công thức (`neo4j/seed/10-kienthuc-A.cypher`)

```cypher
UNWIND [
  {ma: 'CT_HCN_DT', hinh: 'HINH_CHU_NHAT', lop: 3, daiLuong: 'DIEN_TICH',
   ten: 'Diện tích hình chữ nhật', bieuThuc: 'S = a \\cdot b', bienSo: ['a', 'b']},
  {ma: 'CT_HCN_CHEO', hinh: 'HINH_CHU_NHAT', lop: 8, daiLuong: 'DUONG_CHEO',
   ten: 'Đường chéo hình chữ nhật', bieuThuc: 'd = \\sqrt{a^2 + b^2}', bienSo: ['a', 'b']},
  ...
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
```

**Giải thích ký hiệu** (ngoài các ký hiệu đã nói ở trên)

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `bienSo: ['a', 'b']` | `[]` là **danh sách**; Neo4j lưu được danh sách chuỗi làm thuộc tính. Máy tính hình học (US-15) đọc danh sách này để biết cần hỏi học sinh những số đo nào |
| `\\cdot`, `\\sqrt`, `\\frac` | LaTeX cho KaTeX dựng hình (NFR-12). **Phải viết hai dấu gạch chéo**: trong chuỗi Cypher, `\f`, `\t`, `\n`, `\b`, `\r` là ký tự điều khiển, nên `\frac` sẽ bị hiểu thành form feed + `rac` và mất dấu gạch chéo |
| `daiLuong` | `CHU_VI` / `DIEN_TICH` / `DUONG_CHEO` — dùng để lọc ở `/KienThuc/MayTinh?daiLuong=` |
| `(h)-[:CO_CONG_THUC]->(c)` | chiều từ hình tới công thức của hình đó |

**Kết quả mong đợi trên dữ liệu seed**

```cypher
MATCH (c:CongThuc) RETURN count(c) AS soCongThuc;          // 12
MATCH (c:CongThuc)
RETURN c.ma AS ma, c.bieuThuc AS bieuThuc, c.bienSo AS bienSo, c.daiLuong AS daiLuong
ORDER BY c.daiLuong, c.ma;
```

| ma | bieuThuc | bienSo | daiLuong |
|---|---|---|---|
| CT_HBH_CV | `P = 2(a + b)` | a, b | CHU_VI |
| CT_HCN_CV | `P = 2(a + b)` | a, b | CHU_VI |
| CT_HV_CV | `P = 4a` | a | CHU_VI |
| CT_TG_CHUVI | `P = a + b + c + d` | a, b, c, d | CHU_VI |
| CT_THOI_CV | `P = 4a` | a | CHU_VI |
| CT_HBH_DT | `S = a \cdot h` | a, h | DIEN_TICH |
| CT_HCN_DT | `S = a \cdot b` | a, b | DIEN_TICH |
| CT_HT_DT | `S = \frac{(a + b) \cdot h}{2}` | a, b, h | DIEN_TICH |
| CT_HV_DT | `S = a^2` | a | DIEN_TICH |
| CT_THOI_DT | `S = \frac{d_1 \cdot d_2}{2}` | d1, d2 | DIEN_TICH |
| CT_HCN_CHEO | `d = \sqrt{a^2 + b^2}` | a, b | DUONG_CHEO |
| CT_HV_CHEO | `d = a\sqrt{2}` | a | DUONG_CHEO |

`bienSo` liệt kê **đúng các biến có trong `bieuThuc` của riêng công thức đó**, nên máy tính
hình học không hỏi số đo không cần (ví dụ tính diện tích hình bình hành chỉ hỏi `a` và `h`).

Kiểm tra LaTeX lưu đúng (một dấu gạch chéo, không bị mất `\f`):

```cypher
MATCH (c:CongThuc {ma: 'CT_HT_DT'}) RETURN size(c.bieuThuc) AS doDai;   // 29
```

### US-03 · Hai câu kiểm tra của AC

```cypher
// Đủ số lượng
MATCH (t:TinhChat) RETURN count(t) AS soTinhChat;    // 12
MATCH (c:CongThuc) RETURN count(c) AS soCongThuc;    // 12

// Mọi mục có nguon và trangThai (BR-13)
MATCH (n) WHERE n:TinhChat OR n:CongThuc
WITH n WHERE n.nguon IS NULL OR n.trangThai <> 'DA_RA_SOAT'
RETURN count(n) AS soThieu;                          // 0
```

`WHERE n:TinhChat OR n:CongThuc` — `n:Nhan` dùng như một biểu thức đúng/sai để kiểm tra
nút có nhãn đó hay không. `<>` là "khác". `IS NULL` kiểm tra thuộc tính chưa được đặt.

Chạy `scripts/seed` hai lần: tổng số nút giữ nguyên **55** (28 của PHẦN 0 + 12 tính chất +
12 công thức + 3 tài khoản demo), tổng quan hệ giữ nguyên **96**.

### US-04 · Seed 10 bài tập cấp 1 (`neo4j/seed/11-baitap-A.cypher`)

Hai câu tạo nút (một cho mỗi `loai`), rồi hai câu nối quan hệ.

```cypher
// 5 bài trắc nghiệm
UNWIND [
  {ma: 'BT-001', lop: 3, doKho: 1,
   de: 'Một hình chữ nhật có chiều dài 8 cm, chiều rộng 5 cm. Chu vi hình chữ nhật là:',
   phuongAn: ['13 cm', '26 cm', '40 cm', '20 cm'], dapAnDung: 'B',
   giaiThich: '…'},
  ...
] AS row
MERGE (b:BaiTap {ma: row.ma})
SET b.loai = 'TRAC_NGHIEM', b.de = row.de, b.phuongAn = row.phuongAn,
    b.dapAnDung = row.dapAnDung, b.giaiThich = row.giaiThich, b.doKho = row.doKho,
    b.hienThi = true,
    b.nguon = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    b.trangThai = 'DA_RA_SOAT'
WITH b, row
MATCH (l:Lop {so: row.lop})
MERGE (b)-[:THUOC_LOP]->(l);

// LIEN_QUAN_DEN — mỗi bài tập ít nhất một khái niệm (BR-08)
UNWIND [
  {ma: 'BT-002', khaiNiem: ['HINH_VUONG', 'HINH_CHU_NHAT', 'HINH_THOI']},
  ...
] AS row
UNWIND row.khaiNiem AS maKhaiNiem
MATCH (b:BaiTap {ma: row.ma}), (k:KhaiNiem {ma: maKhaiNiem})
MERGE (b)-[:LIEN_QUAN_DEN]->(k);
```

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `phuongAn: ['13 cm', …]` | `[]` là danh sách 4 chuỗi; `dapAnDung` là một chữ A–D, ứng với vị trí trong danh sách |
| `b.hienThi = true` | giá trị lô-gic; quản trị viên ẩn bài bằng cách đặt `false` (US-25) |
| `UNWIND row.khaiNiem AS maKhaiNiem` | **UNWIND lồng**: dòng ngoài cho một bài tập, dòng trong trải danh sách khái niệm của bài đó → một bài tạo được nhiều quan hệ |
| `MATCH (b:BaiTap {…}), (k:KhaiNiem {…})` | dấu phẩy nối hai mẫu độc lập trong cùng một `MATCH`; cả hai phải tìm thấy thì mới sang bước sau |
| `MERGE (b)-[:LIEN_QUAN_DEN]->(k)` | tạo quan hệ nếu chưa có; chạy seed lần hai không sinh quan hệ trùng |
| `MERGE (b)-[:SU_DUNG]->(c)` | bài tập dùng tới công thức nào — dùng cho gợi ý bài kế tiếp (US-24) |

**Kết quả mong đợi trên dữ liệu seed**

```cypher
MATCH (b:BaiTap) RETURN b.loai AS loai, count(b) AS so ORDER BY loai;
```

| loai | so |
|---|---|
| DAP_AN_SO | 5 |
| TRAC_NGHIEM | 5 |

```cypher
MATCH (b:BaiTap)-[:THUOC_LOP]->(l:Lop)
RETURN b.ma AS ma, b.loai AS loai, l.so AS lop, b.doKho AS doKho ORDER BY ma;
```

| ma | loai | lop | doKho | chủ đề |
|---|---|---|---|---|
| BT-001 | TRAC_NGHIEM | 3 | 1 | chu vi hình chữ nhật (D.10) |
| BT-002 | TRAC_NGHIEM | 1 | 1 | nhận biết hình vuông |
| BT-003 | TRAC_NGHIEM | 4 | 1 | nhận biết hình thoi |
| BT-004 | TRAC_NGHIEM | 3 | 1 | diện tích hình vuông |
| BT-005 | TRAC_NGHIEM | 5 | 2 | diện tích hình thang |
| BT-006 | DAP_AN_SO | 3 | 1 | diện tích hình chữ nhật |
| BT-007 | DAP_AN_SO | 3 | 1 | chu vi hình vuông |
| BT-008 | DAP_AN_SO | 4 | 2 | diện tích hình bình hành |
| BT-009 | DAP_AN_SO | 4 | 2 | diện tích hình thoi |
| BT-010 | DAP_AN_SO | 4 | 1 | chu vi hình thoi |

Cả 10 bài đều thuộc `CAP_1`. Đáp án trắc nghiệm rải đều (A×1, B×2, D×2) để học sinh không
đoán được theo vị trí. Năm bài đáp án số có `dapAnSo`, `saiSo = 0.01` và `donVi`.

Câu tự kiểm tra phép tính (tính lại độc lập rồi so với đáp án trong đề):

```cypher
WITH [{ma: 'BT-001', tinh: 2*(8+5), dung: 26.0},
      {ma: 'BT-009', tinh: 8*5/2.0, dung: 20.0}] AS ds
UNWIND ds AS r
RETURN r.ma, CASE WHEN toFloat(r.tinh) = r.dung THEN 'KHOP' ELSE 'LECH' END AS ketQua;
```

Cả 8 bài có phép tính đều cho `KHOP` (BT-002 và BT-003 là bài nhận biết hình, không có phép tính).

### US-04 · Seed 3 bối cảnh và 9 tình huống (`neo4j/seed/12-tinhhuong-A.cypher`)

```cypher
UNWIND [
  {ma: 'TH-01', boiCanh: 'BC_NHA_CUA', thucHanh: true, lop: 8,
   ten: 'Kiểm tra khung cửa có vuông góc không',
   moTa: 'Thợ mộc đo hai cặp cạnh đối và hai đường chéo của khung cửa.',
   loiGiai: 'Khi hai cặp cạnh đối … bằng nhau thì khung cửa đã là hình bình hành. …'},
  ...
] AS row
MERGE (th:TinhHuong {ma: row.ma})
SET th.ten = row.ten, th.moTa = row.moTa, th.loiGiai = row.loiGiai,
    th.thucHanh = row.thucHanh,
    th.nguon = 'CT GDPT 2018 – Toán ' + toString(row.lop),
    th.trangThai = 'DA_RA_SOAT'
WITH th, row
MATCH (bc:BoiCanh {ma: row.boiCanh})
MERGE (th)-[:TRONG_BOI_CANH]->(bc);

// AP_DUNG tới dấu hiệu của phần B — quy ước README mục 2, điểm 5
UNWIND [
  {th: 'TH-01', ma: 'DH_HBH_2'}, {th: 'TH-01', ma: 'DH_HCN_3'},
  {th: 'TH-09', ma: 'DH_THOI_1'}, {th: 'TH-09', ma: 'DH_HV_5'}
] AS row
MATCH (th:TinhHuong {ma: row.th})
MERGE (d:DinhLy {ma: row.ma})
MERGE (th)-[:AP_DUNG]->(d);
```

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `thucHanh: true` | tình huống có phần thực hành đo đạc (US-18). Chỉ TH-01 và TH-09 là `true` |
| `MERGE (d:DinhLy {ma: row.ma})` | **điểm quan trọng**: dấu hiệu `DH_…` thuộc phần B, có thể chưa được seed. Chỉ `MERGE` theo **nhãn gốc `DinhLy` + `ma`**, không gắn nhãn phụ `DauHieu`, không gán thuộc tính. Khi `20-dinhly-B.cypher` chạy, nó `MERGE` cùng khóa rồi `SET` nhãn và thuộc tính — nhờ vậy seed của A, B, C **không phụ thuộc thứ tự chạy** |
| `(th)-[:AP_DUNG]->(k)` | tình huống áp dụng kiến thức nào; `k` có thể là `CongThuc` hoặc `DinhLy` nên file chia thành ba câu, mỗi câu một nhãn, tránh quét toàn bộ nút |
| `(th)-[:LIEN_QUAN_DEN]->(h)` | hình liên quan: tính chất/công thức → hình sở hữu; dấu hiệu → hình ở cột `KHANG_DINH` |

**Kết quả mong đợi trên dữ liệu seed**

```cypher
MATCH (th:TinhHuong)-[:TRONG_BOI_CANH]->(bc:BoiCanh)
RETURN bc.ma AS boiCanh, collect(th.ma) AS tinhHuong ORDER BY boiCanh;
```

| boiCanh | tinhHuong |
|---|---|
| BC_DO_DUNG | TH-07, TH-08, TH-09 |
| BC_MANH_VUON | TH-04, TH-05, TH-06 |
| BC_NHA_CUA | TH-01, TH-02, TH-03 |

```cypher
MATCH (th:TinhHuong)-[:AP_DUNG]->(k) RETURN th.ma AS th, collect(k.ma) AS kienThuc ORDER BY th;
```

| th | kienThuc (AP_DUNG) | hình (LIEN_QUAN_DEN) |
|---|---|---|
| TH-01 | DH_HBH_2, DH_HCN_3 | HINH_BINH_HANH, HINH_CHU_NHAT |
| TH-02 | CT_HCN_DT | HINH_CHU_NHAT |
| TH-03 | CT_THOI_DT | HINH_THOI |
| TH-04 | CT_HCN_CV | HINH_CHU_NHAT |
| TH-05 | CT_HT_DT | HINH_THANG |
| TH-06 | CT_HBH_DT | HINH_BINH_HANH |
| TH-07 | TC_HBH_1, CT_HBH_CV | HINH_BINH_HANH |
| TH-08 | CT_HCN_CV, CT_HCN_CHEO | HINH_CHU_NHAT |
| TH-09 | DH_THOI_1, DH_HV_5 | HINH_THOI, HINH_VUONG |

Kiểm tra nút "tạm" của phần B đúng quy ước (chỉ có thuộc tính `ma`, chưa có nhãn `DauHieu`):

```cypher
MATCH (d:DinhLy) WHERE d.ma STARTS WITH 'DH_'
RETURN d.ma AS ma, labels(d) AS nhan, keys(d) AS thuocTinh ORDER BY ma;
```

| ma | nhan | thuocTinh |
|---|---|---|
| DH_HBH_2 | DinhLy | ma |
| DH_HCN_3 | DinhLy | ma |
| DH_HV_5 | DinhLy | ma |
| DH_THOI_1 | DinhLy | ma |

Vì nút tạm chưa có `trangThai`, truy vấn hiển thị tự bỏ qua:
`MATCH (d:DinhLy) WHERE d.trangThai = 'DA_RA_SOAT' RETURN count(d)` cho **12** — đúng 12 tính
chất của US-03. Sau khi phần B chạy seed, bốn nút này sẽ có đủ nhãn `DauHieu` và thuộc tính.

### US-04 · Ba câu kiểm tra của AC

```cypher
// Mỗi bài tập và mỗi tình huống có ít nhất một LIEN_QUAN_DEN (BR-08)
MATCH (b:BaiTap)     WHERE NOT (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem) RETURN count(b) AS so;    // 0
MATCH (th:TinhHuong) WHERE NOT (th)-[:LIEN_QUAN_DEN]->(:KhaiNiem) RETURN count(th) AS so;  // 0

// Trắc nghiệm đủ 4 phương án, đáp án hợp lệ
MATCH (b:BaiTap {loai: 'TRAC_NGHIEM'})
WHERE size(b.phuongAn) <> 4 OR NOT b.dapAnDung IN ['A', 'B', 'C', 'D']
RETURN count(b) AS so;                                                                     // 0
```

`NOT (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem)` — dùng một **mẫu** làm điều kiện đúng/sai: đúng khi
không tồn tại quan hệ nào khớp. `IN [...]` kiểm tra giá trị có nằm trong danh sách.

Chạy `scripts/seed` hai lần: tổng số nút giữ nguyên **81** (55 sau US-03 + 10 bài tập +
9 tình huống + 3 bối cảnh + 4 nút dấu hiệu tạm của phần B), tổng quan hệ giữ nguyên **160**.

## Đề xuất thay đổi chung

> README mục 2, điểm 1: nếu thấy cần sửa file của PHẦN 0 hoặc của phần khác thì **không sửa**,
> mà ghi đề xuất vào đây và làm cách tạm trong phạm vi phần A.

| Ngày | Đề xuất | Lý do | Cách tạm đang dùng | Trạng thái |
|---|---|---|---|---|
| | | | | |
