using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>
/// Cài đặt <see cref="IAuthHelper"/> trên Cookie Authentication và
/// <see cref="PasswordHasher{TUser}"/> (không dùng ASP.NET Identity, không EF, không SQL).
/// </summary>
public sealed class AuthHelper : IAuthHelper
{
    private readonly PasswordHasher<TaiKhoanAuth> _hasher = new();

    public string BamMatKhau(string matKhau) => _hasher.HashPassword(null!, matKhau);

    public bool KiemTraMatKhau(string matKhauBam, string matKhau)
    {
        if (string.IsNullOrEmpty(matKhauBam) || string.IsNullOrEmpty(matKhau))
        {
            return false;
        }

        try
        {
            var kq = _hasher.VerifyHashedPassword(null!, matKhauBam, matKhau);
            return kq is PasswordVerificationResult.Success
                       or PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            // Chuỗi băm trong CSDL bị hỏng → coi như sai mật khẩu.
            return false;
        }
    }

    public async Task DangNhapAsync(HttpContext http, TaiKhoanAuth tk)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, tk.Id),
            new(ClaimTypes.Name, tk.BietDanh),
            new(ClaimTypes.Role, tk.VaiTro),
            new(GqClaim.Lop, tk.Lop.ToString())
        };

        var danhTinh = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(danhTinh),
            new AuthenticationProperties { IsPersistent = true });
    }

    public Task DangXuatAsync(HttpContext http)
        => http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
