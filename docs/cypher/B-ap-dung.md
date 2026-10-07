# Cypher – Phần B · Áp dụng kiến thức

> Khung do PHẦN 0 tạo (US-01). Thành viên B ghi vào đây **mọi** truy vấn Cypher của phần mình.
> Story của phần B: US-13, US-14, US-17, US-18, US-25.

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

*(Chưa có truy vấn — thành viên B bổ sung khi làm từng story.)*

## Đề xuất thay đổi chung

> README mục 2, điểm 1: nếu thấy cần sửa file của PHẦN 0 hoặc của phần khác thì **không sửa**,
> mà ghi đề xuất vào đây và làm cách tạm trong phạm vi phần B.

| Ngày | Đề xuất | Lý do | Cách tạm đang dùng | Trạng thái |
|---|---|---|---|---|
| | | | | |
