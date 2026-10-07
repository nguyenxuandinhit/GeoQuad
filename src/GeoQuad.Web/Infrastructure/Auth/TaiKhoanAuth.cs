namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>Thông tin tài khoản dùng để tạo cookie đăng nhập.</summary>
public sealed record TaiKhoanAuth(string Id, string TenDangNhap, string BietDanh, string VaiTro, int Lop);
