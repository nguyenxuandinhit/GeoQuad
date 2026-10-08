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

### US-09 · Duyệt thư viện kiến thức (FR-10, UC-03, SCR-04)

Hằng `CypherDuyet` trong `Areas/KienThuc/Repositories/ThuVienRepository.cs`.

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

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `(n)` không có nhãn | nút bất kỳ — vì một thẻ có thể là `KhaiNiem`, `TinhChat`, `DauHieu` hay `CongThuc`; điều kiện nhãn nằm trong `WHERE` |
| `-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)` | **đường đi hai chặng**: nội dung → lớp → cấp. Nhờ vậy lọc theo cấp mà không cần lưu cấp trên từng nút |
| `n:TinhChat` trong `WHERE` | `n:Nhan` dùng như biểu thức đúng/sai: "nút này có nhãn đó không" |
| `$lop`, `$loai`, `$capLoc`, `$lopLoc`, `$tk` | **tham số**; dữ liệu từ URL không bao giờ nối vào chuỗi truy vấn (NFR-05) |
| `$lopLoc IS NULL OR l.so = $lopLoc` | mẫu "lọc tuỳ chọn": tham số null thì bỏ qua điều kiện, khỏi phải ghép câu động |
| `l.so <= $lop` | lọc theo lớp (BR-04). `$lop` = `ICurrentUser.LopHienThi`, bằng 12 khi bật xem trước nâng cao |
| `n.trangThai = 'DA_RA_SOAT'` | chỉ hiện nội dung đã rà soát (BR-13). Nhờ điều kiện này, nút "tạm" của mảng khác (chưa có `trangThai`) tự bị bỏ qua |
| `OPTIONAL MATCH` | như `MATCH` nhưng không có cũng vẫn giữ dòng, các biến thành `null`. Dùng vì tính chất/công thức mới có hình sở hữu, còn hình thì không |
| `-[:CO_TINH_CHAT\|CO_CONG_THUC]->` | `\|` là **"hoặc"** giữa hai kiểu quan hệ: đi theo cả hai trong một bước |
| `labels(n)` | trả về danh sách nhãn của nút, ví dụ `['DinhLy','TinhChat']` |
| `[x IN labels(n) WHERE x IN [...]][0]` | **list comprehension**: lọc danh sách nhãn rồi lấy phần tử đầu — để biến nhiều nhãn thành một chữ `loai` cho giao diện |
| `coalesce(a, b, c)` | lấy giá trị **không null đầu tiên**. `coalesce(n.ten, n.noiDung)` vì hình/công thức có `ten`, còn tính chất/dấu hiệu có `noiDung` |
| `CASE WHEN n:KhaiNiem THEN n.ma END` | không có `ELSE` nên trả `null` khi không phải khái niệm, để `coalesce` chuyển sang `h.ma` rồi `dich.ma` |
| `EXISTS { MATCH ... }` | **truy vấn con**: đúng/sai, không nhân thêm dòng. Với khách thì `$tk` là null nên không khớp tài khoản nào → `daHoc` luôn false (BR-09) |
| `ORDER BY lop, loai, tieuDe` | sắp xếp theo lớp trước để nội dung dễ trước, khó sau |

**Kết quả mong đợi trên dữ liệu seed** (chưa có dữ liệu phần B nên tab Dấu hiệu rỗng)

Khách lớp 4, **không** bật xem trước nâng cao (`$lop = 4`, các tham số lọc đều null) — **14 mục**:

| loai | các mục |
|---|---|
| KhaiNiem | HINH_CHU_NHAT (1), HINH_VUONG (1), TU_GIAC (3), HINH_BINH_HANH (4), HINH_THOI (4) |
| CongThuc | CT_HCN_CV, CT_HCN_DT, CT_HV_CV, CT_HV_DT, CT_TG_CHUVI (lớp 3); CT_HBH_CV, CT_HBH_DT, CT_THOI_CV, CT_THOI_DT (lớp 4) |

Không có `HINH_THANG_CAN` (lớp 6), không mục nào mang nhãn "Nâng cao" — **đúng AC**.

Khách lớp 4, **bật** xem trước nâng cao (`$lop = 12`) — **31 mục**:

| loai | số mục |
|---|---|
| KhaiNiem | 7 |
| TinhChat | 12 |
| CongThuc | 12 |
| DauHieu | 0 (chờ `20-dinhly-B.cypher`) |

`HINH_THANG_CAN` xuất hiện, lớp 6 > lớp 4 nên mang nhãn **"Nâng cao"** — **đúng AC**.
17 thẻ mang nhãn "Nâng cao" (mọi nội dung lớp 6 và lớp 8).

Huy hiệu "Đã học" — tạo thử một quan hệ rồi xem:

```cypher
MATCH (tk:TaiKhoan {tenDangNhap: 'hocsinh8'}), (k:KhaiNiem {ma: 'HINH_THOI'})
MERGE (tk)-[h:DA_HOC]->(k) ON CREATE SET h.luc = datetime();
```

`hocsinh8` mở tab Hình thì chỉ "Hình thoi" có huy hiệu **✓ Đã học**; khách mở cùng trang thì
không thẻ nào có huy hiệu. Xoá thử nghiệm: `MATCH (:TaiKhoan)-[h:DA_HOC]->() DELETE h;`

**Nhãn "Nâng cao" tính ở đâu?** Không tính trong Cypher. Truy vấn trả `lop` của từng mục;
`ThuVienQuyTac.LaNangCao(lopNoiDung, lopHocSinh)` so với `ICurrentUser.Lop` (lớp **thật**, không
phải `LopHienThi`). Tách như vậy để unit test được mà không cần Neo4j (quy ước 2, điểm 9).

### US-10 · Chi tiết khái niệm (FR-11, UC-03, SCR-05)

Hằng `CypherChiTiet` trong `Areas/KienThuc/Repositories/ChiTietRepository.cs`.

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

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `t {.ma, .noiDung, lop: lt.so}` | **map projection**: lấy ra một bản đồ chỉ gồm vài thuộc tính của `t`, cộng thêm khoá `lop` lấy từ nút khác. Gọn hơn liệt kê `t.ma AS …, t.noiDung AS …` |
| `collect(...)` | gom nhiều dòng thành **một danh sách** — biến quan hệ một-nhiều thành một dòng duy nhất |
| `DISTINCT` trong `collect` | bỏ trùng; cần vì các `OPTIONAL MATCH` phía sau nhân số dòng lên |
| `WITH k, l, collect(...) AS tinhChat` | `WITH` vừa là "đường ống" vừa là chỗ **gom nhóm**: cái gì không nằm trong hàm gom thì thành khoá nhóm |
| `{trangThai:'DA_RA_SOAT'}` ngay trong mẫu | lọc gọn hơn viết `WHERE t.trangThai = …`; nhờ nó nút "tạm" của mảng khác tự bị bỏ qua |
| `WHERE lt.so <= $lop` sau `OPTIONAL MATCH` | `WHERE` thuộc về `OPTIONAL MATCH` đó: không khớp thì trả `null` chứ **không** loại cả khái niệm |
| `collect` bỏ qua `null` | khi `OPTIONAL MATCH` không khớp, `t {...}` là `null` nên danh sách ra rỗng `[]` → tab rỗng, giao diện ẩn tab |
| `[x IN collect(...) WHERE x.ma IS NOT NULL AND x.lop <= $lop]` | với dấu hiệu phải dùng **bản đồ literal** `{ma: d.ma, …}` (luôn khác null kể cả khi `d` null), nên lọc lại bằng list comprehension |
| `(k)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(cha)` | đi **xuôi** chiều: hình tổng quát hơn |
| `(con)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(k)` | đi **ngược** chiều: hình đặc biệt hơn. Cùng một quan hệ, đổi chiều đọc là ra hai danh sách |
| `EXISTS { MATCH ... }` | đúng/sai, không nhân dòng; `$tk` null (khách) → luôn false (BR-09) |

Truy vấn tình huống và bài tập để **riêng** (`CypherLienQuan`) đúng như README: nếu ghép vào
câu trên thì mỗi tình huống × mỗi bài tập sẽ nhân số dòng lên.

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

`left(bt.de, 80)` lấy 80 ký tự đầu của đề để làm dòng xem trước. `baiTap[0..5]` là **slice**
danh sách: lấy phần tử 0 đến 4, tức nhiều nhất 5 bài.

**Kết quả mong đợi trên dữ liệu seed**

`/KienThuc/ThuVien/ChiTiet/HINH_CHU_NHAT`, học sinh lớp 8:

| Tab | Nội dung |
|---|---|
| Định nghĩa | Tứ giác có bốn góc vuông. |
| Tính chất (2) | TC_HCN_1, TC_HCN_2 (lớp 8) |
| Công thức (3) | `P = 2(a + b)` (lớp 3), `S = a \cdot b` (lớp 3), `d = \sqrt{a^2 + b^2}` (lớp 8) |
| Ví dụ thực tế (4) | TH-01, TH-02, TH-04, TH-08 |
| Bài tập (3) | BT-001, BT-006, BT-002 |
| Dấu hiệu nhận biết | **ẩn** — chờ `20-dinhly-B.cypher` của phần B |

Đúng AC về định dạng toán: công thức trả về nguyên chuỗi LaTeX, view bọc `$$…$$` để KaTeX dựng.

`/KienThuc/ThuVien/ChiTiet/HINH_THOI` — minh hoạ lọc theo lớp (BR-04):

| Người xem | Tab hiện |
|---|---|
| Học sinh **lớp 4** | Định nghĩa · Công thức (2) · Ví dụ thực tế (2) · Bài tập (4) — **tab Tính chất bị ẩn** vì TC_THOI_1 và TC_THOI_2 đều lớp 8 |
| Học sinh **lớp 8** | thêm Tính chất (2) |

→ **đúng AC** "học sinh lớp 4 mở Hình thoi không thấy tính chất lớp 8" và "mục rỗng không hiển thị".

`HINH_VUONG` có hai liên kết **tổng quát hơn** (Hình chữ nhật, Hình thoi) và không có "đặc biệt
hơn" — đúng với bảy quan hệ `LA_TRUONG_HOP_DAC_BIET_CUA` ở Phụ lục D.2.

Kiểm tra tab Dấu hiệu sẽ hiện khi phần B seed xong — tạo thử một nút rồi xoá:

```cypher
MATCH (k:KhaiNiem {ma:'HINH_CHU_NHAT'}), (l:Lop {so:8})
CREATE (d:DinhLy:DauHieu {ma:'DH_TMP_TEST', noiDung:'Tứ giác có ba góc vuông là hình chữ nhật.',
                          nguon:'tam', trangThai:'DA_RA_SOAT'})
CREATE (d)-[:KHANG_DINH]->(k) CREATE (d)-[:THUOC_LOP]->(l);
-- xem trang, rồi dọn:
MATCH (d:DinhLy {ma:'DH_TMP_TEST'}) DETACH DELETE d;
```

Tab **"Dấu hiệu nhận biết (1)"** xuất hiện đúng vị trí giữa Tính chất và Công thức.

### US-10 · "Em đã hiểu" (POST, chỉ học sinh, chống CSRF)

```cypher
MATCH (tk:TaiKhoan {id: $tk}), (k:KhaiNiem {ma: $ma})
MERGE (tk)-[h:DA_HOC]->(k) ON CREATE SET h.luc = datetime()
RETURN k.ma AS ma
```

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `MATCH (tk:…), (k:…)` | dấu phẩy nối hai mẫu rời; **cả hai** phải tìm thấy, nếu không câu không trả dòng nào → ứng dụng biết mã sai và trả 404 |
| `MERGE (tk)-[h:DA_HOC]->(k)` | chưa có thì tạo, đã có thì dùng lại → **bấm hai lần chỉ có một quan hệ** |
| `ON CREATE SET h.luc = datetime()` | chỉ đặt thời điểm ở lần tạo đầu; bấm lại không ghi đè `luc` |
| `RETURN k.ma AS ma` | thêm vào so với README để phân biệt "ghi xong" với "mã không tồn tại" |

**Kết quả mong đợi**

| Hành động | Kết quả |
|---|---|
| **Khách** bấm "Em đã hiểu" | chuyển về `…/ChiTiet/HINH_THOI?moi=True`, hiện khối `_MoiDangNhap` ("Đăng nhập để lưu tiến độ của em"); **không** sinh quan hệ nào (BR-09) |
| `hocsinh8` bấm lần 1 | tạo `DA_HOC` có `luc` |
| `hocsinh8` bấm lần 2 | vẫn **đúng 1** quan hệ `DA_HOC`; nút đổi thành "✓ Em đã hiểu bài này", tiêu đề có huy hiệu "✓ Em đã hiểu" |
| Mã sai (`/ChiTiet/KHONG_CO_MA_NAY`) | HTTP **404** với trang "Không tìm thấy trang này" của PHẦN 0 |

Câu kiểm tra:

```cypher
MATCH (tk:TaiKhoan)-[h:DA_HOC]->(k:KhaiNiem)
RETURN tk.tenDangNhap AS ten, k.ma AS khaiNiem, count(h) AS soQuanHe, h.luc IS NOT NULL AS coLuc;
-- hocsinh8 | HINH_THOI | 1 | TRUE
MATCH ()-[h:DA_HOC]->() DELETE h;   -- dọn sau khi thử
```

**Hình SVG** không sinh bằng Cypher. `HinhVeSvg.Ve(ma)` dựng từ bảng toạ độ cố định trong mã
nguồn, có `<title>` và `<desc>` (NFR-11), đường chéo vẽ **nét đứt** và ký hiệu góc vuông để không
chỉ dựa vào màu. Tách ra lớp thuần nên unit test được bằng cách phân tích XML (quy ước 2, điểm 9).

### US-11 · Tìm kiếm có/không dấu (FR-12, UC-04, SCR-06, NFR-14)

Hằng `CypherTim` trong `Areas/KienThuc/Repositories/TimKiemRepository.cs`.

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

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `CALL ... YIELD node, score` | gọi **thủ tục** có sẵn của Neo4j; `YIELD` khai báo các cột nó trả về để dùng ở bước sau |
| `db.index.fulltext.queryNodes('tên', $q)` | tìm trong chỉ mục toàn văn `kienThucTimKiem` tạo ở `00-schema.cypher` |
| `$q` | **chuỗi truy vấn Lucene**, dựng ở `TimKiemQuyTac` rồi truyền vào như tham số — không nối chuỗi (NFR-05) |
| `score` | điểm khớp do Lucene tính; càng cao càng sát từ khoá |
| `LIMIT 30` | giới hạn 30 kết quả để trang không quá dài |

**Chuỗi Lucene được dựng thế nào**

`TimKiemQuyTac` thoát 19 ký tự đặc biệt của Lucene (`+ - && || ! ( ) { } [ ] ^ " ~ * ? : \ /`)
cho **từng từ**, rồi nối bằng ` AND `. Không có kết quả thì thử lại bằng ` OR `.

| Em gõ | Chuỗi gửi vào `$q` |
|---|---|
| `hinh thoi` | `hinh AND thoi` |
| `hinh thoi` (lần hai, nếu rỗng) | `hinh OR thoi` |
| `hinh(1) thoi*` | `hinh\(1\) AND thoi\*` |
| (rỗng) | **không gửi truy vấn**, chỉ nhắc em nhập |

**Kết quả mong đợi trên dữ liệu seed**

`hinh thoi` và `hình thoi` cho **cùng 6 mục** (analyzer `standard-folding` bỏ dấu):

| Nhóm | Mục |
|---|---|
| Khái niệm (1) | Hình thoi (lớp 4) |
| Tính chất & dấu hiệu (3) | TC_THOI_1, TC_THOI_2, TC_HV_1 (lớp 8) |
| Công thức (2) | Chu vi hình thoi, Diện tích hình thoi (lớp 4) |

> **Rủi ro README nêu đã kiểm tra xong:** analyzer `standard-folding` **bỏ được cả chữ "đ"**.
> Thử `queryNodes('kienThucTimKiem', 'duong cheo')` cho `DUONG_CHEO` ("Đường chéo") ở hạng
> đầu. Vậy **không cần** thêm thuộc tính `tenKhongDau` như phương án tạm của README.

### US-12 · Bản đồ kiến thức (FR-13, UC-05, SCR-07)

Ba câu trong `Areas/KienThuc/Repositories/BanDoRepository.cs`.

```cypher
-- 1. Dữ liệu nút và quan hệ cho Cytoscape.js
MATCH (a:KhaiNiem {loai:'HINH', trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(la:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE la.so <= $lop
OPTIONAL MATCH (a)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(b:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
  WHERE lb.so <= $lop
RETURN a.ma AS ma, a.ten AS ten, a.dinhNghia AS dinhNghia, la.so AS lop, cap.ma AS cap,
       collect(b.ma) AS laDacBietCua,
       EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(a) } AS daHoc
```

Điều kiện `lb.so <= $lop` ở `OPTIONAL MATCH` rất quan trọng: nó bỏ luôn mũi tên trỏ tới hình
**không hiển thị**, nên lọc theo lớp không để lại mũi tên "treo".

```cypher
-- 2. Mọi hình tổng quát hơn, đi nhiều bước
MATCH (:KhaiNiem {ma: $ma})-[:LA_TRUONG_HOP_DAC_BIET_CUA*1..]->(t:KhaiNiem)
RETURN DISTINCT t.ma AS ma, t.ten AS ten
```

```cypher
-- 3. "Vì sao Hình vuông là Hình thang?"
MATCH p = shortestPath((a:KhaiNiem {ma: $tu})-[:LA_TRUONG_HOP_DAC_BIET_CUA*]->(b:KhaiNiem {ma: $den}))
RETURN [n IN nodes(p) | n.ten] AS chuoi
```

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `*1..` | đi theo quan hệ **từ 1 bước trở lên**, không giới hạn số bước — "đường đi độ dài thay đổi" |
| `*` | như `*1..` nhưng không chặn đầu nào cả |
| `*0..` | kể cả **0 bước**, tức chính nút đó cũng được tính (dùng ở US-15) |
| `DISTINCT` | bỏ kết quả trùng; cần vì có nhiều đường đi tới cùng một hình (hình vuông tới hình bình hành qua hình chữ nhật **và** qua hình thoi) |
| `p = (...)` | đặt tên cho **đường đi**, không chỉ cho nút |
| `shortestPath(...)` | lấy đường đi **ngắn nhất** giữa hai nút; nhanh hơn nhiều so với liệt kê mọi đường |
| `nodes(p)` | danh sách các nút trên đường đi, theo thứ tự |
| `[n IN nodes(p) \| n.ten]` | **list comprehension**: `\|` ở đây không phải "hoặc" mà là dấu tách giữa biến và biểu thức, nghĩa là "với mỗi n lấy n.ten" |

**Kết quả mong đợi trên dữ liệu seed**

Lọc tới lớp 12 — **7 nút, 7 mũi tên** (đúng bảy quan hệ của Phụ lục D.2):

| Hình | Lớp | Là trường hợp đặc biệt của |
|---|---|---|
| Hình chữ nhật | 1 | Hình bình hành |
| Hình vuông | 1 | **Hình thoi, Hình chữ nhật** |
| Tứ giác | 3 | — |
| Hình bình hành | 4 | Hình thang |
| Hình thoi | 4 | Hình bình hành |
| Hình thang | 5 | Tứ giác |
| Hình thang cân | 6 | Hình thang |

→ **đúng AC** "hình vuông nối tới hình chữ nhật và hình thoi".

Lọc tới **lớp 3**: chỉ còn Hình chữ nhật, Hình vuông, Tứ giác và một mũi tên
(Hình vuông → Hình chữ nhật). **Không có Hình thang cân** — đúng AC.

Đồ thị **không có chu trình**, kiểm bằng:

```cypher
MATCH (a:KhaiNiem)-[:LA_TRUONG_HOP_DAC_BIET_CUA*1..]->(a) RETURN count(*) AS soChuTrinh;  -- 0
```

Ô "Vì sao Hình vuông là Hình thang?":

```cypher
MATCH p = shortestPath((a:KhaiNiem {ma:'HINH_VUONG'})-[:LA_TRUONG_HOP_DAC_BIET_CUA*]->(b:KhaiNiem {ma:'HINH_THANG'}))
RETURN [n IN nodes(p) | n.ten] AS chuoi;
-- ["Hình vuông", "Hình chữ nhật", "Hình bình hành", "Hình thang"]
```

→ **chuỗi 4 hình**, đúng AC. Giao diện ghép thành câu: "Hình vuông là một trường hợp đặc biệt
của hình chữ nhật; Hình chữ nhật là một trường hợp đặc biệt của hình bình hành; Hình bình hành
là một trường hợp đặc biệt của hình thang."

### US-15 · Danh sách đại lượng và công thức của máy tính (FR-30, UC-08, SCR-10)

Hằng `CypherDaiLuong` trong `Areas/KienThuc/Repositories/MayTinhRepository.cs`.

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

**Giải thích ký hiệu**

| Ký hiệu trong câu | Nghĩa |
|---|---|
| `*0..` | **kể cả 0 bước**: công thức của chính hình đó cũng được tính, không chỉ của hình tổng quát |
| `length(p)` | số quan hệ trên đường đi — ở đây là "hình này cách hình sở hữu công thức mấy bước" |
| `ORDER BY khoangCach` trong `WITH` | sắp xếp **trước khi gom**, nhờ vậy `collect` giữ thứ tự gần → xa |
| `head(collect(...))` | lấy phần tử đầu của danh sách, tức công thức của hình **gần nhất** |
| `WITH c.daiLuong AS daiLuong, head(...)` | `daiLuong` không nằm trong hàm gom nên trở thành **khoá nhóm**: mỗi đại lượng lấy đúng một công thức |

Đó là mẹo để "hình vuông dùng chu vi hình vuông, không dùng chu vi tứ giác".

**Kết quả mong đợi trên dữ liệu seed** (lớp 12)

| `$hinh` | CHU_VI | DIEN_TICH | DUONG_CHEO |
|---|---|---|---|
| HINH_CHU_NHAT | CT_HCN_CV (a, b) | CT_HCN_DT (a, b) | CT_HCN_CHEO (a, b) |
| HINH_VUONG | **CT_HV_CV** (a) | CT_HV_DT (a) | CT_HV_CHEO (a) |
| HINH_BINH_HANH | CT_HBH_CV (a, b) | CT_HBH_DT (a, h) | — |
| HINH_THOI | CT_THOI_CV (a) | CT_THOI_DT (d1, d2) | — |
| HINH_THANG | CT_TG_CHUVI (a, b, c, d) *từ Tứ giác* | CT_HT_DT (a, b, h) | — |
| HINH_THANG_CAN | CT_TG_CHUVI *từ Tứ giác* | CT_HT_DT *từ Hình thang* | — |

Hình vuông lấy `CT_HV_CV` (khoảng cách 0) chứ không lấy `CT_TG_CHUVI` (khoảng cách 4) —
**đúng yêu cầu của US-15**. Hình thang cân không có công thức riêng nên mượn của hình thang
và tứ giác, giao diện ghi rõ "Công thức này là của hình thang — hình thang cân là một trường
hợp đặc biệt nên dùng được".

Với `$lop = 4` thì HINH_CHU_NHAT chỉ còn **Chu vi và Diện tích** (`CT_HCN_CHEO` là lớp 8),
đúng yêu cầu "học sinh cấp 1 chỉ thấy đại lượng có công thức lớp ≤ 5".

Câu lấy danh sách hình chọn được:

```cypher
MATCH (h:KhaiNiem {loai:'HINH', trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND h.ma IN $maChoPhep
RETURN h.ma AS ma, h.ten AS ten
ORDER BY l.so, h.ten
```

`IN $maChoPhep` nhận danh sách sáu hình máy tính hỗ trợ, truyền vào như **một tham số** là
danh sách — không ghép chuỗi.

**Phần tính toán và vẽ hình không dùng Cypher.** `MayTinhHinhHoc` (US-15) tính và kiểm tra
BR-10, `VeHinhSvg` (US-16) dựng SVG theo số đo; cả hai là lớp thuần nên unit test được mà
không cần Neo4j (quy ước 2, điểm 9).

## Đề xuất thay đổi chung

> README mục 2, điểm 1: nếu thấy cần sửa file của PHẦN 0 hoặc của phần khác thì **không sửa**,
> mà ghi đề xuất vào đây và làm cách tạm trong phạm vi phần A.

| Ngày | Đề xuất | Lý do | Cách tạm đang dùng | Trạng thái |
|---|---|---|---|---|
| 08/10 | **Cả nhóm chốt**: README mục 2 điểm 6 và SRS mục 4 **xung đột** về `trangThai` ban đầu của seed. Cần chọn một hướng rồi sửa tài liệu còn lại cho khớp | SRS mục 4: *"Trạng thái nội dung đặt ban đầu là **NHAP**; chuyển sang DA_RA_SOAT khi đã đối chiếu nguồn và có người thứ hai rà soát (NFR-09)"*. README lại ghi seed thẳng `DA_RA_SOAT` vì "nhóm đã rà soát". Seed của A theo README, seed của B theo SRS → lệch nhau | Không có cách tạm. A **không** nới điều kiện lọc, vì nới là vi phạm BR-13/NFR-09 và sẽ hiện nội dung chưa rà soát cho học sinh | 💬 Cần nhóm chốt |
| 08/10 | **Phần C**: hoàn tất việc "Rà soát US-03" rồi **đổi `trangThai` trong chính file seed của B** từ `'NHAP'` sang `'DA_RA_SOAT'` (hoặc B tự đổi sau khi C xác nhận) | US-03 giao C việc "Rà soát chéo: chạy hai file trên, đối chiếu từng mục với Phụ lục D/SRS Phụ lục B". `nguon` của B đã ghi sẵn "(chờ C đối chiếu SGK)". Đây là bước quy trình **chưa làm**, không phải lỗi mã của B | Chờ. Tuyệt đối **không** đổi bằng Cypher thủ công trên Neo4j Browser — chạy lại seed là mất | ⏳ Chờ C |
| 08/10 | **Phần B**: cho các test trong `tests/.../B_ApDung/Integration/**` và `Http/MayChuThuTests` **bỏ qua** (`Assert.Skip` / `ITestOutputHelper` + điều kiện) khi chưa có `GQ_B_INTEGRATION=1`, thay vì ném `InvalidOperationException` | Hiện `dotnet test` trên máy sạch cho **18 test đỏ**, vi phạm Definition of Done của README ("`dotnet test` qua") và mục 2 điểm 12 | A không chạm; khi cần xác nhận phần A thì lọc `--filter FullyQualifiedName~A_TraCuu` | ⏳ Chờ B |
| 08/10 | **Phần B**: 4 test `BackupFlowTests` gọi `bash`/WSL thật nên đỏ trên máy Windows không có WSL hoạt động | Test phụ thuộc môi trường, không phải lỗi logic | A không chạm | ⏳ Chờ B |
| 08/10 | **Hợp đồng dữ liệu**: `20-dinhly-B.cypher` thêm quan hệ `THUOC_LOP` cho `DieuKien`, trong khi lược đồ README mục 4.2 không liệt kê quan hệ này cho `DieuKien` | Nếu giữ thì nên bổ sung vào mục 4.2 để A và C biết mà dùng | A không dùng `DieuKien` nên không ảnh hưởng | 💬 Cần nhóm chốt |

### Chi tiết: xung đột README vs SRS về `trangThai`

**Hai tài liệu nói khác nhau**, đây là gốc của mọi hệ quả bên dưới:

| Tài liệu | Quy định |
|---|---|
| **SRS mục 4** | "Trạng thái nội dung đặt ban đầu là **NHAP**; chuyển sang `DA_RA_SOAT` khi đã đối chiếu nguồn và có người thứ hai rà soát (NFR-09)" |
| **SRS NFR-09** | "Mọi tính chất, dấu hiệu, công thức có nguồn đối chiếu và được **ít nhất một người khác** trong nhóm rà soát (lý tưởng: giáo viên Toán) trước khi đặt `DA_RA_SOAT`" |
| **README mục 2 điểm 6** | "Mọi nút nội dung do seed tạo có `nguon` và `trangThai: 'DA_RA_SOAT'` (BR-13; **nhóm đã rà soát** theo Phụ lục B của SRS)" |

Seed của A (`10-`, `11-`, `12-`) theo README → `DA_RA_SOAT`.
Seed của B (`20-`, `21-`) theo SRS → `NHAP`, `nguon` ghi "(chờ C đối chiếu SGK)".

**Cả hai đều có cơ sở.** SRS là tài liệu được chấm nên cách của B chặt chẽ hơn về quy trình;
README thì giả định việc rà soát đã xong từ trước. Nhóm cần chốt một hướng:

- **Hướng 1 (theo SRS)**: giữ `NHAP`, C làm xong rà soát US-03 rồi sửa `trangThai` **trong file
  seed**. Đúng NFR-09, nhưng trước khi C xong thì demo trống — nên phải làm ngay.
- **Hướng 2 (theo README)**: coi việc rà soát Phụ lục B đã hoàn tất, seed thẳng `DA_RA_SOAT`,
  và sửa lại câu trong SRS mục 4 cho khớp. Nhanh, nhưng phải giải thích được với thầy rằng
  NFR-09 đã thỏa (ai là "người thứ hai" đã rà soát).

### Hệ quả đo được khi còn `NHAP` (ảnh hưởng rộng nhất)

Đo trên dữ liệu sau khi seed cả A và B (133 nút / 297 quan hệ):

| Nhãn | Số nút | `trangThai` |
|---|---|---|
| KhaiNiem, CongThuc, BaiTap, TinhHuong, DinhLy:TinhChat | 56 | `DA_RA_SOAT` ✅ |
| DieuKien (14), DinhLy:DauHieu (20), DinhLy nền (6), ChungMinh (4), Buoc | 44+ | **`NHAP`** ❌ |

Hệ quả đo được trên trình duyệt:

| Màn hình | Chủ | Hiện trạng |
|---|---|---|
| `/KienThuc/ThuVien?loai=DAU_HIEU` | A | **0 thẻ** |
| `/KienThuc/ThuVien/ChiTiet/HINH_CHU_NHAT` | A | tab "Dấu hiệu nhận biết" **vẫn ẩn** |
| `/KienThuc/ThuVien/TimKiem?q=dau+hieu` | A | **0 mục** |
| `/ApDung/GoiY` | B | không có điều kiện nào để chọn |
| `/ApDung/ChungMinh/Xem/CM-01` | B | **HTTP 404** |
| `/ApDung/TinhHuong/Xem/TH-01`, `TH-09` | B | **HTTP 404** — hai tình huống này `AP_DUNG` tới dấu hiệu đang `NHAP`, và `TinhHuongRepository` có mệnh đề *fail closed*; danh sách còn **7/9** |

Câu kiểm tra sau khi B sửa (kỳ vọng tất cả bằng 0):

```cypher
MATCH (n) WHERE (n:DinhLy OR n:DieuKien OR n:ChungMinh OR n:Buoc)
  AND coalesce(n.trangThai,'') <> 'DA_RA_SOAT'
RETURN count(n) AS soChuaRaSoat;
```
