using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.B_ApDung.Doubles;

public sealed record CurrentUserGia(int Lop = 8, bool XemNangCao = false, bool LaQuanTri = false) : ICurrentUser
{
    public bool DaDangNhap { get; init; }
    public string? TaiKhoanId { get; init; }
    public string? BietDanh { get; init; }
    public int LopHienThi => XemNangCao ? 12 : Lop;
}
