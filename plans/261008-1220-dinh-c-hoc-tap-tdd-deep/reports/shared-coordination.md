# Việc chung C: kickoff và điều phối

Ngày ghi nhận: 08/10/2026. C chưa đánh dấu đã trình bày buổi kickoff hay được A/B xác nhận các hạng mục chung; trang này là biên bản chuẩn bị, evidence nhóm cần bổ sung sau buổi trao đổi thật.

## Kickoff US-05 — nội dung C cần giải thích

1. Tài khoản có `TaiKhoan.id` UUID; tên đăng nhập UNIQUE, không dùng username làm graph ID.
2. `HOC_LOP` là quan hệ đến `Lop`; truy vấn profile lấy lớp từ quan hệ, đổi lớp phải cập nhật quan hệ/cookie.
3. Mật khẩu được băm bằng `PasswordHasher<T>`, không lưu/ghi log plaintext; `IAuthHelper` thực hiện đăng nhập/đăng xuất.
4. Tên đăng nhập trùng phải bị constraint từ chối và UI báo lỗi có thể hiểu; retry không được tạo hai tài khoản.
5. Mọi Cypher tham số hóa; không ghép input người dùng.

**Query minh họa cần chạy khi trình bày:**

```cypher
MATCH (tk:TaiKhoan {tenDangNhap: $tenDangNhap})-[:HOC_LOP]->(l:Lop)
RETURN tk.id AS id, tk.tenDangNhap AS tenDangNhap, l.so AS lop
```

`:TaiKhoan(tenDangNhap)` cần UNIQUE theo `00-schema.cypher`; chụp output đã ẩn UUID nếu cần và ghi tên người nghe, câu hỏi, thời gian. Đây chưa phải evidence buổi kickoff.

## Điều phối US-01, US-05, US-06

| Hạng mục chung | Owner cần xác nhận | Bằng chứng hiện có | Còn cần |
|---|---|---|---|
| US-01 compose web + Neo4j, trang chủ query được | C điều phối; owner khung theo README | Local app/Neo4j từng chạy trong phiên trước | Buổi chạy từ checkout sạch và evidence app/query; chưa nhận thay README A |
| US-05 unique, consent, hash, lỗi trùng | C điều phối; phần C profile/auth đã có | Controller/AuthHelper, tests C | Kickoff và AC đăng ký consent/duplicate/hash trong branch tích hợp |
| US-06 lock 5 lần/5 phút, admin gate | C điều phối; A sở hữu auth chung | CSV ghi task owner, chưa có evidence ở đây | Owner cung cấp query/test và review matrix quyền |
| US-03 nội dung | C rà seed B; A sở hữu seed A | [`C-hoc-tap.md`](../../../docs/cypher/C-hoc-tap.md) ghi đếm lại 14/20/12/12/6 PASS | Finding A lớp/nguồn và người rà soát thứ hai |

Không gửi yêu cầu ra ngoài nhóm từ repository. Khi C trình bày hoặc owner gửi evidence, cập nhật biên bản này và `docs/kiem-thu.md` với tên/giờ/output chính xác.
