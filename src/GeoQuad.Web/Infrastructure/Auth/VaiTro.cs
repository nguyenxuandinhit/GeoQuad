namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>Vai trò của tài khoản (thuộc tính <c>TaiKhoan.vaiTro</c>).</summary>
public static class VaiTro
{
    public const string HocSinh = "HOC_SINH";
    public const string QuanTri = "QUAN_TRI";
}

/// <summary>Tên claim riêng của GeoQuad.</summary>
public static class GqClaim
{
    /// <summary>Số lớp của học sinh (BR-03, BR-04).</summary>
    public const string Lop = "lop";
}
