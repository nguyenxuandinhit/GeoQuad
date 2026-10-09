namespace GeoQuad.Web.Areas.HocTap.Repositories;

/// <summary>Bản ghi thông tin tài khoản dùng cho chức năng hồ sơ (US-08).</summary>
public sealed record TaiKhoanHoSoBanGhi(
    string Id,
    string TenDangNhap,
    string BietDanh,
    string MatKhauBam,
    string VaiTro,
    int Lop,
    DateTimeOffset? NgayTao);

/// <summary>Truy cập dữ liệu hồ sơ cá nhân trên Neo4j (US-08).</summary>
public interface IHoSoRepository
{
    /// <summary>Lấy thông tin tài khoản theo Id.</summary>
    Task<TaiKhoanHoSoBanGhi?> TimTheoIdAsync(string id);

    /// <summary>Cập nhật biệt danh và quan hệ HOC_LOP (FR-04, US-08).</summary>
    Task<bool> CapNhatThongTinAsync(string id, string bietDanh, int lop);

    /// <summary>Cập nhật mật khẩu băm mới (FR-04, US-08).</summary>
    Task<bool> CapNhatMatKhauAsync(string id, string matKhauBam);

    /// <summary>Xóa tài khoản và ngắt toàn bộ quan hệ (NFR-06, US-08).</summary>
    Task<bool> XoaTaiKhoanAsync(string id);
}
