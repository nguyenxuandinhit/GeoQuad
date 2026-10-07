using System.Security.Claims;
using GeoQuad.Web.Infrastructure;
using GeoQuad.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace GeoQuad.Tests.Chung;

/// <summary>US-07: chế độ khách và lớp hiển thị (FR-03, BR-04, BR-09).</summary>
public class CurrentUserTests
{
    /// <summary>Dựng ICurrentUser từ một HttpContext giả, không cần Neo4j.</summary>
    private static ICurrentUser TaoNguoiDung(
        bool daDangNhap = false,
        string? lopClaim = null,
        string vaiTro = VaiTro.HocSinh,
        string? cookieLop = null,
        string? cookieNangCao = null)
    {
        var http = new DefaultHttpContext();

        var cookies = new List<string>();
        if (cookieLop is not null)
        {
            cookies.Add($"{GqCookie.Lop}={cookieLop}");
        }

        if (cookieNangCao is not null)
        {
            cookies.Add($"{GqCookie.NangCao}={cookieNangCao}");
        }

        if (cookies.Count > 0)
        {
            http.Request.Headers.Cookie = string.Join("; ", cookies);
        }

        if (daDangNhap)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "tk-1"),
                new(ClaimTypes.Name, "Minh An"),
                new(ClaimTypes.Role, vaiTro)
            };

            if (lopClaim is not null)
            {
                claims.Add(new Claim(GqClaim.Lop, lopClaim));
            }

            http.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        return new CurrentUser(new HttpContextAccessor { HttpContext = http });
    }

    [Fact]
    public void KhachKhongCoTaiKhoanId()
    {
        var nd = TaoNguoiDung();

        Assert.False(nd.DaDangNhap);
        Assert.Null(nd.TaiKhoanId);   // BR-09: không có chỗ để ghi dữ liệu học tập
        Assert.Null(nd.BietDanh);
        Assert.False(nd.LaQuanTri);
    }

    [Fact]
    public void KhachMacDinhLop8()
        => Assert.Equal(8, TaoNguoiDung().Lop);

    [Theory]
    [InlineData("4", 4)]
    [InlineData("12", 12)]
    [InlineData("1", 1)]
    public void KhachDungLopTrongCookie(string cookie, int mong)
        => Assert.Equal(mong, TaoNguoiDung(cookieLop: cookie).Lop);

    [Theory]
    [InlineData("0")]
    [InlineData("13")]
    [InlineData("tam")]
    [InlineData("")]
    public void CookieLopKhongHopLeThiVeMacDinh(string cookie)
        => Assert.Equal(QuyUoc.LopMacDinh, TaoNguoiDung(cookieLop: cookie).Lop);

    [Fact]
    public void HocSinhDungLopTrongClaim()
    {
        // Cookie của khách không được lấn lớp của học sinh đã đăng nhập.
        var nd = TaoNguoiDung(daDangNhap: true, lopClaim: "4", cookieLop: "11");

        Assert.True(nd.DaDangNhap);
        Assert.Equal("tk-1", nd.TaiKhoanId);
        Assert.Equal("Minh An", nd.BietDanh);
        Assert.Equal(4, nd.Lop);
    }

    [Fact]
    public void NhanDienQuanTri()
    {
        Assert.True(TaoNguoiDung(daDangNhap: true, lopClaim: "12", vaiTro: VaiTro.QuanTri).LaQuanTri);
        Assert.False(TaoNguoiDung(daDangNhap: true, lopClaim: "8").LaQuanTri);
    }

    [Fact]
    public void KhongBatNangCaoThiLopHienThiBangLop()
    {
        var nd = TaoNguoiDung(cookieLop: "4");

        Assert.False(nd.XemNangCao);
        Assert.Equal(4, nd.Lop);
        Assert.Equal(4, nd.LopHienThi);
    }

    [Fact]
    public void BatNangCaoThiLopHienThiBang12()
    {
        var nd = TaoNguoiDung(cookieLop: "4", cookieNangCao: "1");

        Assert.True(nd.XemNangCao);
        Assert.Equal(4, nd.Lop);          // lớp thật không đổi, dùng để gắn nhãn "Nâng cao"
        Assert.Equal(12, nd.LopHienThi);  // nhưng truy vấn lấy tới lớp 12
    }

    [Fact]
    public void BatNangCaoApDungCaChoHocSinh()
    {
        var nd = TaoNguoiDung(daDangNhap: true, lopClaim: "8", cookieNangCao: "1");

        Assert.Equal(8, nd.Lop);
        Assert.Equal(12, nd.LopHienThi);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("true")]
    [InlineData("")]
    public void CookieNangCaoChiNhanGiaTri1(string gia)
        => Assert.False(TaoNguoiDung(cookieNangCao: gia).XemNangCao);
}
