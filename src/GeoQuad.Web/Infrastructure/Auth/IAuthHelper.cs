namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>
/// Tiện ích xác thực dùng chung (PHẦN 0 sở hữu). A, B, C gọi lại, không tự viết lại.
/// </summary>
public interface IAuthHelper
{
    /// <summary>Băm mật khẩu bằng <c>PasswordHasher</c> của ASP.NET Core (BR-12).</summary>
    string BamMatKhau(string matKhau);

    /// <summary>Đối chiếu mật khẩu người dùng nhập với chuỗi băm đã lưu.</summary>
    bool KiemTraMatKhau(string matKhauBam, string matKhau);

    /// <summary>Tạo lại cookie đăng nhập (dùng cả khi đổi lớp hoặc đổi biệt danh).</summary>
    Task DangNhapAsync(HttpContext http, TaiKhoanAuth tk);

    Task DangXuatAsync(HttpContext http);
}
