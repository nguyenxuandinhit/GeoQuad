using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.B_ApDung.Doubles;

public sealed record CurrentUserGia(int Lop = 8, bool LaQuanTri = false) : ICurrentUser
{
    public bool DaDangNhap { get; init; }
    public string? TaiKhoanId { get; init; }
    public string? BietDanh { get; init; }
}
