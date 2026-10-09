# Kịch bản demo nhóm GeoQuad

Thời lượng kịch bản: 10–12 phút theo README. Chuẩn bị môi trường và dữ liệu trước giờ demo; người A phụ trách setup sạch 15 phút, người B chuẩn bị backup/restore, C điều phối ghi kết quả. Buổi rehearsal thật chưa diễn ra.

## Chuẩn bị

- Chọn cùng commit, kiểm tra `docker compose up -d neo4j`, chạy `scripts/seed` hai lần và khởi động web; ghi phiên bản/ports thực tế.
- Dùng database demo có thể bỏ hoặc bản sao riêng; không reset database dùng chung.
- Chuẩn bị trình duyệt desktop và kiểm tra nhanh ở chiều rộng 360 px.
- Đăng nhập sẵn tài khoản demo `hocsinh8` và `admin`; xác minh dữ liệu trước buổi chạy.
- `BT-027`/`CM-01` đang `NHAP` ở seed B hiện tại. Chỉ trình bày xem lời giải mẫu nếu B đã rà soát, đổi trạng thái hợp lệ và chạy lại kiểm tra; nếu chưa, trình bày cổng chặn và ghi nhận là blocker, không giả lập nội dung công khai.

## Luồng 10–12 phút

| Thời gian | Chủ trì | Nội dung | Kỳ vọng |
|---|---|---|---|
| 1,5 phút | A | Khách chọn lớp 8, mở Thư viện/Hình chữ nhật, tìm “hình thoi”. | Bộ lọc, kết quả và lớp hiển thị đúng. |
| 1 phút | A | Mở Bản đồ kiến thức, hỏi “Vì sao Hình vuông là Hình thang?”. | Quan hệ trường hợp đặc biệt hiện rõ. |
| 1 phút | A | Máy tính hình học: hình chữ nhật 5 × 3. | Kết quả và hình vẽ phù hợp. |
| 1,5 phút | B | “Nên dùng định lý nào?” với hình bình hành + một góc vuông; mở chứng minh mẫu CM-01 nếu đã được duyệt. | Dấu hiệu đúng đứng đầu; proof chỉ xuất hiện khi trạng thái cho phép. |
| 1,5 phút | B | “Hình học quanh ta”: kiểm tra khung cửa; số đo 2,00 / 0,90 / 2,19. | Tình huống và thực hành đo đạc hoạt động. |
| 3 phút | C | Đăng nhập `hocsinh8`; làm một bài trắc nghiệm và một bài số; xem kết quả, lộ trình Hình vuông, tiến độ, bài gợi ý; đổi biệt danh. | GET không lộ đáp án, server chấm, lịch sử riêng được cập nhật, bài kế tiếp đúng lớp. |
| 1 phút | B | Đăng nhập `admin`, thêm/sửa/ẩn một bài demo. | Quản trị và trạng thái hiển thị đúng. |
| 1 phút | A/B/C | Mỗi người chạy một câu Cypher của phần mình trong Neo4j Browser và giải thích kết quả. | Browser trả output; ghi nhận truy vấn, người chạy và kết quả thật. |

## Phục hồi và bằng chứng

- Nếu seed/DB hoặc tài khoản demo lỗi, dừng luồng phụ thuộc; chỉ khôi phục bản sao demo đã được chuẩn bị và xác minh, không chạy reset trên DB dùng chung.
- Ghi thời lượng rehearsal thực tế, người thao tác từng phần, ảnh desktop/360 px, output Neo4j Browser, kết quả backup/restore và blocker vào `docs/kiem-thu.md` sau buổi chạy.
- Chưa điền trước thời lượng hoặc kết quả rehearsal chưa diễn ra.