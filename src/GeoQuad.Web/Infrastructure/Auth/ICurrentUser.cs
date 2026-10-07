namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>
/// Người đang dùng GeoQuad: học sinh đã đăng nhập hoặc khách (US-07, FR-03, BR-04, BR-09).
/// Đăng ký theo vòng đời scoped; A, B, C dùng lại, không tự đọc cookie/claim.
/// </summary>
public interface ICurrentUser
{
    bool DaDangNhap { get; }

    /// <summary>Null nếu là khách — khách không được ghi dữ liệu học tập (BR-09).</summary>
    string? TaiKhoanId { get; }

    string? BietDanh { get; }

    bool LaQuanTri { get; }

    /// <summary>Học sinh: claim "lop"; khách: cookie <c>gq_lop</c>; mặc định 8.</summary>
    int Lop { get; }

    /// <summary>Công tắc "Xem trước kiến thức nâng cao" (cookie <c>gq_nangcao</c>).</summary>
    bool XemNangCao { get; }

    /// <summary>
    /// Lớp dùng làm tham số <c>$lop</c> khi lọc nội dung (BR-04):
    /// bằng 12 khi bật xem trước nâng cao, ngược lại bằng <see cref="Lop"/>.
    /// </summary>
    int LopHienThi { get; }
}
