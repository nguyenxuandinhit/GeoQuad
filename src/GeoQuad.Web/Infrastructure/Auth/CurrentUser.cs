using System.Security.Claims;
using GeoQuad.Web.Infrastructure;

namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>
/// Cài đặt <see cref="ICurrentUser"/> đọc từ claim của cookie đăng nhập và cookie của khách.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly HttpContext? _http;

    public CurrentUser(IHttpContextAccessor accessor) => _http = accessor.HttpContext;

    private ClaimsPrincipal? NguoiDung => _http?.User;

    public bool DaDangNhap => NguoiDung?.Identity?.IsAuthenticated == true;

    public string? TaiKhoanId
        => DaDangNhap ? NguoiDung!.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    public string? BietDanh => DaDangNhap ? NguoiDung!.Identity!.Name : null;

    public bool LaQuanTri => DaDangNhap && NguoiDung!.IsInRole(VaiTro.QuanTri);

    public int Lop
    {
        get
        {
            var tho = DaDangNhap
                ? NguoiDung!.FindFirstValue(GqClaim.Lop)
                : _http?.Request.Cookies[GqCookie.Lop];

            return int.TryParse(tho, out var lop) && QuyTacTaiKhoan.LopHopLe(lop)
                ? lop
                : QuyUoc.LopMacDinh;
        }
    }

    public bool XemNangCao => _http?.Request.Cookies[GqCookie.NangCao] == "1";

    public int LopHienThi => XemNangCao ? QuyUoc.LopLonNhat : Lop;
}
