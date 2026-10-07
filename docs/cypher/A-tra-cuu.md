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

## Đề xuất thay đổi chung

> README mục 2, điểm 1: nếu thấy cần sửa file của PHẦN 0 hoặc của phần khác thì **không sửa**,
> mà ghi đề xuất vào đây và làm cách tạm trong phạm vi phần A.

| Ngày | Đề xuất | Lý do | Cách tạm đang dùng | Trạng thái |
|---|---|---|---|---|
| | | | | |
