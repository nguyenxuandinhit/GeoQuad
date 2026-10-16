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

    /// <summary>
    /// Học sinh: claim "lop"; khách: cookie <c>gq_lop</c>; mặc định 8.
    /// Cũng là tham số <c>$lop</c> khi lọc nội dung (BR-04): em chỉ thấy nội dung
    /// từ lớp 1 tới lớp của mình.
    /// </summary>
    int Lop { get; }
}
