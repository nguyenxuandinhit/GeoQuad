# GeoQuad - Bản đối chiếu Toán 8, tập một (Chương III, Tứ giác)

Tài liệu này là **đề xuất cần nhóm duyệt**, không phải nội dung tự động được công nhận đã rà soát. Chưa có script Cypher và chưa cấp mã chính thức cho nội dung mới. Giữ nguyên mọi mã đã có.

Kết quả chính: 7/7 định nghĩa hình và 20/20 dấu hiệu khớp với sách. Vấn đề chính là (1) nhãn `DA_RA_SOAT` gán cho nhiều mục chưa ai đối chiếu, (2) 7 tính chất lớp 8 không thấy trong Bài 10-14, (3) vài chi tiết còn thiếu.

## A. Phạm vi đã kiểm tra

- **Sách:** Toán 8, tập một, bộ Kết nối tri thức với cuộc sống, Chương III, Bài 10-14. Năm xuất bản chưa xác định được vì chưa có trang bản quyền.
- **Đã đọc:** 12 ảnh. Số trang in chỉ thấy ở Ảnh 1 (tr.48-49), Ảnh 4 (tr.54-55) và Ảnh 7 (tr.58-59). Tên tệp ảnh suy ra theo thứ tự đính kèm; Ảnh 4 đã mở kiểm tra và khớp.
- **Tệp đối chiếu:** `01-khung.cypher`, `10-kienthuc-A.cypher`, `20-dinhly-B.cypher`. Các tệp 00, 11, 12, 21, 30 chỉ được dò xem mã nào đang được tham chiếu, không rà nội dung.
- **Xác nhận được:** nội dung các trang trong ảnh.
- **Chưa chắc:**
  - "Không thấy" không có nghĩa là "sách không có", vì chưa đọc hết chương (bài tập cuối chương, một số trang Bài 10, Luyện tập chung).
  - Số trang in còn thiếu ở nhiều ảnh, để trống chứ không đoán.
  - Chưa tìm trên mạng.

## B. Bảng đối chiếu

### B1. Nội dung đã khớp sách (DA_CO_PHU_HOP, cần bổ sung nguồn cụ thể)

| Nhóm | Vị trí trong sách |
|---|---|
| 7 định nghĩa hình (`01-khung`) | TU_GIAC: Ảnh 1 tr.49. HINH_THANG, HINH_THANG_CAN: Ảnh 3. HINH_BINH_HANH: Ảnh 6. HINH_CHU_NHAT: Ảnh 9. HINH_THOI: Ảnh 10. HINH_VUONG: Ảnh 12. |
| Yếu tố DUONG_CHEO | Ảnh 1 tr.49. Sách nói "hai đỉnh đối nhau", tương đương "không kề nhau". |
| 11 tính chất | TC_TG_1 (Ảnh 2), TC_HT_1 (Ảnh 3, Ví dụ 1, không phải định lí), TC_HTC_1 (Ảnh 3), TC_HTC_2 (Ảnh 4 tr.54), TC_HBH_1-3 (Ảnh 7 tr.58), TC_HCN_2 (Ảnh 9, thiếu một vế, xem B2 #7), TC_THOI_1-2 (Ảnh 12), TC_HV_1 (Ảnh 11, ô chú thích) |
| 20 dấu hiệu DH_* | Toàn bộ khớp, giữ đúng hình ban đầu. DH_HCN_1, DH_HV_4 và DH_HV_5 trong sách là mục "Chú ý", không phải định lí đánh số. DH_HT_1, DH_HTC_1, DH_HBH_1 và DH_THOI_1 chính là các định nghĩa. |
| 7 quan hệ LA_TRUONG_HOP_DAC_BIET_CUA | Khớp. Riêng HINH_BINH_HANH -> HINH_THANG là suy luận từ sách (Tranh luận Ảnh 7 tr.59, ô chú thích Ảnh 9), sách chưa nói thẳng trong các trang đã đọc. |

### B2. Đề xuất ưu tiên (14 mục)

| # | Mã / loại / hình | Hiện có | Kết luận | Đề xuất | Nguồn | Lý do, ưu tiên | Ảnh hưởng |
|---|---|---|---|---|---|---|---|
| 1 | 23 mã có `nguon` ghi "(bổ sung ngoài Phụ lục B)", trừ TC_HV_3 (danh sách ở C) | Đang có `trangThai = DA_RA_SOAT` | CAN_SUA | Đổi sang trạng thái chưa duyệt cho tới khi nhóm rà | `10-kienthuc-A`: ghi chú khối lớp 9-12 nói "KHÔNG ghi là đã rà soát" nhưng code vẫn gán DA_RA_SOAT | Mục chưa ai đối chiếu sẽ hiển thị như đã duyệt. **Cao** | CAN_XEM_XET |
| 2 | TC_TG_2, TC_HT_2, TC_HTC_3, TC_HBH_4, TC_HCN_3, TC_THOI_3, TC_HV_2 / tính chất | Tổng góc ngoài, đường trung bình hình thang, trục và tâm đối xứng, ghi lớp 8 | CHUA_DU_CAN_CU | Giữ nội dung, chưa khẳng định nguồn "Toán 8" | Mục "Kiến thức, kĩ năng" đầu Bài 10-14 (Ảnh 1, 3, 6, 9, 10) không nhắc các nội dung này; chú thích trong file nói chúng thuộc chương này nhưng sách này chưa xác nhận | Lập luận toán không thấy sai, nhưng chưa có căn cứ nguồn và lớp. **Cao** | CAN_XEM_XET |
| 3 | TU_GIAC / khái niệm | Không nói tới tứ giác lồi | DE_XUAT_MOI | Thêm định nghĩa tứ giác lồi và quy ước "tứ giác" mặc định là tứ giác lồi | Bài 10, mục 1, Ảnh 1 tr.49 | Sách coi hình lõm và hình tự cắt cũng là tứ giác, rồi quy ước dùng tứ giác lồi. **Trung bình-cao** | CAN_XEM_XET |
| 4 | Mục mới / dấu hiệu / HINH_THANG_CAN | Chưa có | DE_XUAT_MOI | Hình thang có hai cạnh bên bằng nhau và hai đáy không bằng nhau là hình thang cân | Bài 11, Luyện tập chung (Ảnh 5); Tranh luận Ảnh 7 tr.59 | Bẫy kinh điển: chỉ cạnh bên bằng nhau thì chưa đủ. Sách xếp đây là ví dụ đã chứng minh. **Trung bình-cao** | CAN_XEM_XET (nhiều điều kiện) |
| 5 | Mục mới / dấu hiệu / HINH_CHU_NHAT | Chưa có | DE_XUAT_MOI | Tứ giác có hai đường chéo bằng nhau và cắt nhau tại trung điểm mỗi đường là hình chữ nhật | Bài 13, Ví dụ 2, Ảnh 9 | Hai DieuKien đã có sẵn. **Trung bình** | CAN_XEM_XET (nhiều điều kiện) |
| 6 | TC_HV_3 / tính chất | Bằng nhau, vuông góc, phân giác | CAN_SUA | Thêm vế "cắt nhau tại trung điểm của mỗi đường" | Bài 14, Định lí 3, Ảnh 11 | Sách nêu đủ 4 ý. **Trung bình** | CHI_DU_LIEU |
| 7 | TC_HCN_2 / tính chất | Chỉ "bằng nhau" | CAN_SUA | Thêm vế "cắt nhau tại trung điểm của mỗi đường" | Bài 13, Định lí 1, Ảnh 9 | Sách nêu hai ý trong cùng định lí. **Trung bình** | CHI_DU_LIEU |
| 8 | Mục mới / tính chất / HINH_BINH_HANH | Chưa có | DE_XUAT_MOI | Hai góc kề một cạnh bất kì của hình bình hành bù nhau | Bài 12, Nhận xét, Ảnh 7 tr.58 | **Thấp-trung bình** | CHI_DU_LIEU |
| 9 | TC_HCN_1 / tính chất | "Hình chữ nhật có bốn góc vuông" | CAN_XEM_XET | Đúng, nhưng chỉ lặp lại định nghĩa | Sách đặt tính chất ở Định lí 1 (đường chéo) | **Thấp** | CAN_XEM_XET |
| 10 | Quan hệ giữa các hình | HINH_CHU_NHAT chỉ nối tới HINH_BINH_HANH | CAN_XEM_XET | Không sửa. Chỉ ghi nhận sách nói hình chữ nhật có mọi tính chất của hình thang cân | Ô chú thích Bài 13, Ảnh 9 | Bản đồ hình có thể thiếu nhánh này. **Thấp-trung bình** | CAN_XEM_XET |
| 11 | Mọi mục ở B1 / nguồn | `nguon` chung chung | CAN_BO_SUNG_NGUON | Điền bài, trang, ảnh theo bảng ở C | Xem C | Làm theo máy móc được. **Cao** | CHI_DU_LIEU |
| 12 | 14 công thức (CT_*) | Chu vi, diện tích, đường chéo | CHUA_DU_CAN_CU | Không đề xuất thêm hay sửa | Các trang đã đọc không có công thức nào | Tránh CAN_SUA_CODE. **Thấp** | CHI_DU_LIEU |
| 13 | `ghiChuTieuHoc` và lớp tiểu học | Mô tả cách hiểu ở tiểu học | CHUA_DU_CAN_CU | Cần sách Toán 3-5 | Chưa có ảnh | **Trung bình** | CHI_DU_LIEU |
| 14 | DL_NEN_3, 4, 6 | Pythagore, đường trung bình tam giác, vectơ | CHUA_DU_CAN_CU | Chưa thấy trong các trang đã đọc; DL_NEN_1, 2, 5 phù hợp cách sách dùng | Ảnh 2, 5, 7 | **Thấp** | CHI_DU_LIEU |

**Còn lại (chưa ưu tiên):**

- Ví dụ 2 Bài 11 (hình thang có góc ACD = BDC thì là hình thang cân).
- Ví dụ 2 Bài 14 (góc đối bằng và hai cạnh kề bằng thì là hình thoi).
- Ví dụ 3 Bài 14 (ba góc vuông và hai cạnh kề bằng nhau thì là hình vuông).
- Nhận xét về trung tuyến ứng với cạnh huyền (có thể thêm làm DL_NEN).
- DH_HBH_3 ghi "hai cạnh đối", sách ghi "một cặp cạnh đối".
- Các khái niệm đỉnh, góc, hai đỉnh đối nhau chưa có trong yếu tố.

## C. Dữ liệu bàn giao cho AI lập trình

Trường `danhSachMa` được thêm cho các hành động theo nhóm. Định dạng này chỉ là đề xuất bàn giao, không phải schema Neo4j mới.

```json
[
  {
    "hanhDong": "SUA_NOI_DUNG",
    "maHienCo": "TC_HV_3",
    "loai": "tính chất",
    "maHinh": "HINH_VUONG",
    "noiDungDeXuat": "Hai đường chéo của hình vuông bằng nhau, vuông góc với nhau, cắt nhau tại trung điểm của mỗi đường và là các đường phân giác của các góc của hình vuông.",
    "lopTrongSach": 8,
    "lopHienCo": 8,
    "nguon": {"tenSach": "Toán 8, tập một", "boSach": "Kết nối tri thức với cuộc sống", "phienBan": null, "baiMuc": "Bài 14, mục 2, Định lí 3", "trangIn": null, "trangPdf": null, "urlHoacTenTep": "1792111051248_image.png (Ảnh 11)"},
    "lyDo": "Sách nêu thêm vế cắt nhau tại trung điểm; bản hiện có thiếu vế này.",
    "mucAnhHuong": "CHI_DU_LIEU",
    "diemCanXacMinh": []
  },
  {
    "hanhDong": "SUA_NOI_DUNG",
    "maHienCo": "TC_HCN_2",
    "loai": "tính chất",
    "maHinh": "HINH_CHU_NHAT",
    "noiDungDeXuat": "Hai đường chéo của hình chữ nhật bằng nhau và cắt nhau tại trung điểm của mỗi đường.",
    "lopTrongSach": 8,
    "lopHienCo": 8,
    "nguon": {"tenSach": "Toán 8, tập một", "boSach": "Kết nối tri thức với cuộc sống", "phienBan": null, "baiMuc": "Bài 13, mục 1, Định lí 1", "trangIn": null, "trangPdf": null, "urlHoacTenTep": "1792110996646_image.png (Ảnh 9)"},
    "lyDo": "Sách nêu hai ý trong cùng một định lí; bản hiện có chỉ giữ ý thứ nhất.",
    "mucAnhHuong": "CHI_DU_LIEU",
    "diemCanXacMinh": ["Bài tập trong 30-baitap-C có tham chiếu mã này; kiểm tra không phụ thuộc vào việc nội dung chỉ có một vế."]
  },
  {
    "hanhDong": "DE_XUAT_MOI",
    "maHienCo": null,
    "loai": "khái niệm",
    "maHinh": "TU_GIAC",
    "noiDungDeXuat": "Tứ giác lồi là tứ giác mà hai đỉnh thuộc một cạnh bất kì luôn nằm về một phía của đường thẳng đi qua hai đỉnh còn lại. Quy ước của sách: khi nói tứ giác mà không chú thích gì thêm thì hiểu là tứ giác lồi.",
    "lopTrongSach": 8,
    "lopHienCo": null,
    "nguon": {"tenSach": "Toán 8, tập một", "boSach": "Kết nối tri thức với cuộc sống", "phienBan": null, "baiMuc": "Bài 10, mục 1", "trangIn": 49, "trangPdf": null, "urlHoacTenTep": "1792110919031_image.png (Ảnh 1)"},
    "lyDo": "Sách coi hình lõm và hình tự cắt cũng là tứ giác, rồi quy ước dùng tứ giác lồi cho phần còn lại của chương.",
    "mucAnhHuong": "CAN_XEM_XET",
    "diemCanXacMinh": ["Có thể chỉ thêm ghi chú vào TU_GIAC thay vì tạo nút KhaiNiem mới; để AI lập trình và nhóm quyết định."]
  },
  {
    "hanhDong": "DE_XUAT_MOI",
    "maHienCo": null,
    "loai": "dấu hiệu",
    "maHinh": "HINH_THANG_CAN",
    "noiDungDeXuat": "Hình thang có hai cạnh bên bằng nhau và hai đáy không bằng nhau là hình thang cân.",
    "hinhBanDau": "HINH_THANG",
    "cacDieuKien": ["Hai cạnh bên bằng nhau", "Hai đáy không bằng nhau"],
    "hinhKetLuan": "HINH_THANG_CAN",
    "lopTrongSach": 8,
    "lopHienCo": null,
    "nguon": {"tenSach": "Toán 8, tập một", "boSach": "Kết nối tri thức với cuộc sống", "phienBan": null, "baiMuc": "Bài 11, Luyện tập chung, Ví dụ; đối chiếu Tranh luận Bài 12", "trangIn": null, "trangPdf": null, "urlHoacTenTep": "1792110937027_image.png (Ảnh 5); 1792110952869_image.png (Ảnh 7, tr.59)"},
    "lyDo": "Thiếu điều kiện hai đáy không bằng nhau sẽ cho kết luận sai (hình bình hành có hai cạnh bên bằng nhau).",
    "mucAnhHuong": "CAN_XEM_XET",
    "diemCanXacMinh": ["Sách trình bày đây là ví dụ đã chứng minh, không phải định lí đánh số.", "Hai DieuKien chưa có mã; kiểm tra tính năng gợi ý dấu hiệu có xử lý nhiều YEU_CAU_CO không."]
  },
  {
    "hanhDong": "DE_XUAT_MOI",
    "maHienCo": null,
    "loai": "dấu hiệu",
    "maHinh": "HINH_CHU_NHAT",
    "noiDungDeXuat": "Tứ giác có hai đường chéo bằng nhau và cắt nhau tại trung điểm của mỗi đường là hình chữ nhật.",
    "hinhBanDau": "TU_GIAC",
    "cacDieuKien": ["DK_CHEO_BANG_NHAU", "DK_CHEO_CAT_TRUNG_DIEM"],
    "hinhKetLuan": "HINH_CHU_NHAT",
    "lopTrongSach": 8,
    "lopHienCo": null,
    "nguon": {"tenSach": "Toán 8, tập một", "boSach": "Kết nối tri thức với cuộc sống", "phienBan": null, "baiMuc": "Bài 13, mục 2, Ví dụ 2", "trangIn": null, "trangPdf": null, "urlHoacTenTep": "1792110996646_image.png (Ảnh 9)"},
    "lyDo": "Dấu hiệu có trong sách, hai điều kiện đã có sẵn trong 20-dinhly-B.",
    "mucAnhHuong": "CAN_XEM_XET",
    "diemCanXacMinh": ["Sách trình bày là ví dụ đã chứng minh; kiểm tra tính năng gợi ý xử lý nhiều điều kiện."]
  },
  {
    "hanhDong": "DE_XUAT_MOI",
    "maHienCo": null,
    "loai": "tính chất",
    "maHinh": "HINH_BINH_HANH",
    "noiDungDeXuat": "Trong hình bình hành, hai góc kề một cạnh bất kì bù nhau (tổng bằng 180°).",
    "lopTrongSach": 8,
    "lopHienCo": null,
    "nguon": {"tenSach": "Toán 8, tập một", "boSach": "Kết nối tri thức với cuộc sống", "phienBan": null, "baiMuc": "Bài 12, mục 1, Nhận xét sau Định lí 1", "trangIn": 58, "trangPdf": null, "urlHoacTenTep": "1792110952869_image.png (Ảnh 7)"},
    "lyDo": "Sách nêu thành nhận xét riêng; GeoQuad chỉ có cho hình thang (TC_HT_1).",
    "mucAnhHuong": "CHI_DU_LIEU",
    "diemCanXacMinh": []
  },
  {
    "hanhDong": "XEM_LAI_TRANG_THAI",
    "maHienCo": null,
    "danhSachMa": ["TC_TG_2","TC_HT_2","TC_HTC_3","TC_HBH_4","TC_HCN_3","TC_THOI_3","TC_HV_2","TC_TG_3","TC_HCN_4","TC_HV_4","TC_HTC_4","TC_THOI_4","TC_HBH_5","TC_TG_4","TC_HBH_6","TC_HBH_7","TC_HBH_8","TC_HCN_5","TC_TG_5","TC_HCN_6","TC_HV_5","CT_HT_CV","CT_THOI_CHEO"],
    "loai": "nhiều loại",
    "maHinh": null,
    "noiDungDeXuat": "Đổi trangThai sang giá trị dành cho nội dung chưa duyệt cho tới khi nhóm đối chiếu với nguồn.",
    "lopTrongSach": null,
    "lopHienCo": null,
    "nguon": {"tenSach": null, "boSach": null, "phienBan": null, "baiMuc": null, "trangIn": null, "trangPdf": null, "urlHoacTenTep": "10-kienthuc-A.cypher"},
    "lyDo": "nguon ghi (bổ sung ngoài Phụ lục B) nên chưa được thành viên C rà soát, nhưng code gán DA_RA_SOAT.",
    "mucAnhHuong": "CAN_XEM_XET",
    "diemCanXacMinh": ["Các tệp Cypher chỉ dùng DA_RA_SOAT; AI lập trình cần xác định giá trị trạng thái nào ứng dụng đang dùng để ẩn.", "7 mã đầu (lớp 8) còn chưa thấy trong Bài 10-14, xem mục 2 của B2."]
  }
]
```

Nguồn cụ thể cho các mục khớp (mục 11 của B2):

```json
{
  "tenSach": "Toán 8, tập một",
  "boSach": "Kết nối tri thức với cuộc sống",
  "phienBan": null,
  "cacNhom": [
    {"baiMuc": "Bài 10, mục 1", "tenTep": "1792110919031_image.png (Ảnh 1)", "trangIn": 49, "danhSachMa": ["TU_GIAC", "DUONG_CHEO"]},
    {"baiMuc": "Bài 10, mục 2, Định lí", "tenTep": "1792110923122_image.png (Ảnh 2)", "trangIn": null, "danhSachMa": ["TC_TG_1"]},
    {"baiMuc": "Bài 11, mục 1 (định nghĩa, Ví dụ 1) và mục 2 (Định lí 1)", "tenTep": "1792110926931_image.png (Ảnh 3)", "trangIn": null, "danhSachMa": ["HINH_THANG", "HINH_THANG_CAN", "DH_HT_1", "DH_HTC_1", "TC_HT_1", "TC_HTC_1"]},
    {"baiMuc": "Bài 11, Định lí 2 và Định lí 3", "tenTep": "1792110930722_image.png (Ảnh 4)", "trangIn": 54, "danhSachMa": ["TC_HTC_2", "DH_HTC_2"]},
    {"baiMuc": "Bài 12, mục 1, định nghĩa", "tenTep": "1792110947498_image.png (Ảnh 6)", "trangIn": null, "danhSachMa": ["HINH_BINH_HANH", "DH_HBH_1"]},
    {"baiMuc": "Bài 12, Định lí 1", "tenTep": "1792110952869_image.png (Ảnh 7)", "trangIn": 58, "danhSachMa": ["TC_HBH_1", "TC_HBH_2", "TC_HBH_3"]},
    {"baiMuc": "Bài 12, mục 2, Định lí 2", "tenTep": "1792110952869_image.png (Ảnh 7)", "trangIn": 59, "danhSachMa": ["DH_HBH_2", "DH_HBH_3"]},
    {"baiMuc": "Bài 12, mục 2, Định lí 3", "tenTep": "1792110957455_image.png (Ảnh 8)", "trangIn": null, "danhSachMa": ["DH_HBH_4", "DH_HBH_5"]},
    {"baiMuc": "Bài 13, định nghĩa, Chú ý, Định lí 1 và Định lí 2", "tenTep": "1792110996646_image.png (Ảnh 9)", "trangIn": null, "danhSachMa": ["HINH_CHU_NHAT", "TC_HCN_2", "DH_HCN_1", "DH_HCN_2", "DH_HCN_3"]},
    {"baiMuc": "Bài 14, mục 1, định nghĩa hình thoi", "tenTep": "1792111036147_image.png (Ảnh 10)", "trangIn": null, "danhSachMa": ["HINH_THOI", "DH_THOI_1"]},
    {"baiMuc": "Bài 14, Định lí 1 và Định lí 2 của hình thoi, định nghĩa hình vuông", "tenTep": "1792111567534_image.png (Ảnh 12)", "trangIn": null, "danhSachMa": ["TC_THOI_1", "TC_THOI_2", "DH_THOI_2", "DH_THOI_3", "DH_THOI_4", "HINH_VUONG"]},
    {"baiMuc": "Bài 14, mục 2, ô chú thích, Định lí 3, Định lí 4 và Chú ý", "tenTep": "1792111051248_image.png (Ảnh 11)", "trangIn": null, "danhSachMa": ["TC_HV_1", "TC_HV_3", "DH_HV_1", "DH_HV_2", "DH_HV_3", "DH_HV_4", "DH_HV_5"]}
  ]
}
```

## D. Danh sách cần nhóm xác nhận

- **Trạng thái:** 23 mục chưa ai đối chiếu đang hiển thị như đã duyệt, trong đó có cả khối lớp 9-12 mà chính ghi chú trong file nói không được ghi là đã rà soát. Nhóm quyết định ẩn hay giữ.
- **7 tính chất lớp 8 (đối xứng, góc ngoài, đường trung bình hình thang):** cần chụp mục lục và bài tập cuối chương Toán 8 tập một để kiểm tra thêm. Có nghi ngờ phần đối xứng thuộc Toán 6, nhưng chưa kiểm chứng; cần chụp sách Toán 6 để kiểm.
- **Tứ giác lồi:** thêm khái niệm mới hay chỉ ghi chú vào TU_GIAC. Việc này ảnh hưởng thư viện và các tính chất dùng chữ "tứ giác".
- **Bản đồ hình:** sách coi hình chữ nhật có mọi tính chất của hình thang cân, nhưng GeoQuad không có quan hệ HCN tới HTC. Không sửa trong lượt này, nhóm quyết định có thêm hay không.
- **Dấu hiệu nhiều điều kiện:** hai đề xuất mới (hình thang cân, hình chữ nhật) có hai điều kiện mỗi mục; AI lập trình cần kiểm tra tính năng gợi ý có xử lý được nhiều YEU_CAU_CO, và cấp mã cho DieuKien mới.
- **Công thức:** lượt này không có đề xuất nào cần CAN_SUA_CODE.
- **Năm tái bản và `ghiChuTieuHoc`:** cần trang bản quyền và sách tiểu học.
