// ============================================================================
// GeoQuad · 00-schema.cypher · PHẦN 0 · US-02 (NFR-08, NFR-10)
// Ràng buộc duy nhất và chỉ mục toàn văn. Dùng IF NOT EXISTS nên chạy lại
// nhiều lần vẫn an toàn.
// ----------------------------------------------------------------------------
// Giải thích ký hiệu:
//   (n:TaiKhoan)  một nút có nhãn TaiKhoan; n là tên tạm để nói về nút đó
//   REQUIRE ... IS UNIQUE  không cho hai nút cùng nhãn có cùng giá trị thuộc tính
//   :KhaiNiem|DinhLy|CongThuc  "hoặc": chỉ mục áp cho cả ba nhãn
//   ON EACH [...]  danh sách thuộc tính được đưa vào chỉ mục toàn văn
// ============================================================================

// ---- Ràng buộc duy nhất ----------------------------------------------------

// TaiKhoan: id là UUID do ứng dụng sinh, tenDangNhap là chữ thường duy nhất (BR-01).
CREATE CONSTRAINT taiKhoan_id IF NOT EXISTS
FOR (n:TaiKhoan) REQUIRE n.id IS UNIQUE;

CREATE CONSTRAINT taiKhoan_tenDangNhap IF NOT EXISTS
FOR (n:TaiKhoan) REQUIRE n.tenDangNhap IS UNIQUE;

// Lop khóa theo `so` (1–12), các nhãn nội dung khác khóa theo `ma`.
CREATE CONSTRAINT lop_so IF NOT EXISTS
FOR (n:Lop) REQUIRE n.so IS UNIQUE;

CREATE CONSTRAINT capHoc_ma IF NOT EXISTS
FOR (n:CapHoc) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT khaiNiem_ma IF NOT EXISTS
FOR (n:KhaiNiem) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT dinhLy_ma IF NOT EXISTS
FOR (n:DinhLy) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT dieuKien_ma IF NOT EXISTS
FOR (n:DieuKien) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT congThuc_ma IF NOT EXISTS
FOR (n:CongThuc) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT baiTap_ma IF NOT EXISTS
FOR (n:BaiTap) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT tinhHuong_ma IF NOT EXISTS
FOR (n:TinhHuong) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT boiCanh_ma IF NOT EXISTS
FOR (n:BoiCanh) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT chungMinh_ma IF NOT EXISTS
FOR (n:ChungMinh) REQUIRE n.ma IS UNIQUE;

CREATE CONSTRAINT buoc_ma IF NOT EXISTS
FOR (n:Buoc) REQUIRE n.ma IS UNIQUE;

// ---- Chỉ mục toàn văn cho tìm kiếm (SRS mục 4.5, US-11) --------------------
// standard-folding bỏ dấu khi lập chỉ mục nên "hinh thoi" cũng tìm ra "Hình thoi".
CREATE FULLTEXT INDEX kienThucTimKiem IF NOT EXISTS
FOR (n:KhaiNiem|DinhLy|CongThuc)
ON EACH [n.ten, n.dinhNghia, n.noiDung, n.bieuThuc]
OPTIONS { indexConfig: { `fulltext.analyzer`: 'standard-folding' } };
