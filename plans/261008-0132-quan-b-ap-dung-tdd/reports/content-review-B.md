# Review nội dung seed B — kết quả rà soát US-03 và US-04

Đã có kết luận rà soát độc lập của thành viên C (ngày 08/10/2026, xem `docs/cypher/C-hoc-tap.md`)
và thành viên B đã rà soát 4 chứng minh mẫu CM-01..CM-04. Toàn bộ 14 điều kiện, 20 dấu hiệu,
6 định lý nền và 4 chứng minh mẫu đã chuyển sang `DA_RA_SOAT`.

## Phạm vi đã kiểm tra

- 14 điều kiện, 20 dấu hiệu, 6 định lý nền: đối chiếu SRS B.3/B.4/B.7 và CT GDPT 2018; C xác nhận đạt toán học và cấu trúc graph.
- Bốn chứng minh: giả thiết, thứ tự 3–6 bước, tính đúng của suy luận, từng căn cứ và hình vẽ ABCD lồi. Đặc biệt CM-04 đã rà soát căn cứ DL_NEN_2 (tam giác bằng nhau c.c.c) và DL_NEN_5 (góc so le trong suy ra hai cặp cạnh đối song song theo định nghĩa hình bình hành).
- CM-01/02/04 sử dụng quan hệ góc trong cùng phía và dấu hiệu hai đường thẳng song song.
- CM-03 sử dụng các cặp tam giác vuông bằng nhau quanh giao điểm O, tránh dùng chính dấu hiệu đang chứng minh làm căn cứ.
- Lớp theo phụ lục: dấu hiệu lớp 8; định lý nền lớp 7/8/10; proof lớp 8.

## Chứng cứ review

| Người review | Ngày | Tài liệu/phiên bản nguồn | Kết luận và sửa cần làm |
|---|---|---|---|
| Nguyễn Xuân Định (C) | 08/10/2026 | SRS B.3/B.4/B.7, CT GDPT 2018 | Đạt toàn bộ 14 điều kiện, 20 dấu hiệu, 6 định lý nền; cập nhật nguồn và chuyển DA_RA_SOAT. |
| Nguyễn Anh Quân (B) | 09/10/2026 | SRS B.9, CT GDPT 2018 – Toán 8 | Đạt 4 chứng minh mẫu CM-01..CM-04 và các bước; chuyển DA_RA_SOAT. |

Toàn bộ seed B (`20-dinhly-B.cypher`, `21-chungminh-B.cypher`) đã được chuyển `DA_RA_SOAT`.
